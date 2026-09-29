using Elastic.Clients.Elasticsearch;
using EcomSearchApi.Data;
using EcomSearchApi.Infrastructure;
using EcomSearchApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EcomSearchApi.Services;

public class ResetService(
    AppDbContext db, 
    ElasticsearchClient es, 
    ElasticIndexManager indexManager, 
    IConfiguration config, 
    ILogger<ResetService> logger) : IResetService
{
    private readonly string _indexName = config["Elasticsearch:IndexName"] ?? "products";

    public async Task<object> ResetAllAsync()
    {
        logger.LogInformation("Starting 1-Click System Reset...");

        // 1. Truncate bảng Products trong PostgreSQL
        await db.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Products\" RESTART IDENTITY CASCADE;");
        logger.LogInformation("PostgreSQL Products table truncated.");

        // 2. Tạo lại index trong Elasticsearch kèm mapping & analyzer
        await indexManager.RecreateIndexAsync();

        // 3. Seed 10 sản phẩm vào PostgreSQL
        var samples = SampleData.InitialProducts;
        foreach (var p in samples)
        {
            p.Id = 0;
        }
        await db.Products.AddRangeAsync(samples);
        await db.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} products to PostgreSQL.", samples.Count);

        // 4. Bulk Index 10 sản phẩm sang Elasticsearch với _id = product.Id
        var bulkResponse = await es.BulkAsync(b => b
            .Index(_indexName)
            .IndexMany(samples, (descriptor, product) => descriptor.Id(product.Id.ToString()))
        );

        if (!bulkResponse.IsValidResponse)
        {
            throw new InvalidOperationException($"Bulk indexing failed: {bulkResponse.DebugInformation}");
        }

        await es.Indices.RefreshAsync(_indexName);

        return new
        {
            status = "SUCCESS",
            message = "PostgreSQL and Elasticsearch reset and seeded successfully!",
            postgresCount = await db.Products.CountAsync(),
            elasticsearchCount = (await es.CountAsync(c => c.Indices(_indexName))).Count
        };
    }
}
