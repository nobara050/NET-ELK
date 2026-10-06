using Elastic.Clients.Elasticsearch;
using EcomSearchApi.Infrastructure;
using EcomSearchApi.Models;
using EcomSearchApi.Repositories;

namespace EcomSearchApi.Services;

// Service coordinating full database truncation and re-seeding across stores
public class ResetService(
    IProductRepository repo, 
    ElasticsearchClient es, 
    ElasticIndexManager indexManager, 
    IConfiguration config, 
    ILogger<ResetService> logger) : IResetService
{
    private readonly string _indexName = config["Elasticsearch:IndexName"] ?? "products";

    // Resets relational data, index mapping, and populates sample documents
    public async Task<object> ResetAllAsync()
    {
        logger.LogInformation("Starting 1-Click System Reset...");

        await repo.TruncateAndResetAsync();
        logger.LogInformation("PostgreSQL Products table truncated.");

        await indexManager.RecreateIndexAsync();

        var samples = SampleData.InitialProducts;
        foreach (var p in samples)
        {
            p.Id = 0;
        }
        await repo.AddRangeAsync(samples);
        logger.LogInformation("Seeded {Count} products to PostgreSQL.", samples.Count);

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
            postgresCount = await repo.CountAsync(),
            elasticsearchCount = (await es.CountAsync(c => c.Indices(_indexName))).Count
        };
    }
}