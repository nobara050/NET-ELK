using Elastic.Clients.Elasticsearch;
using EcomSearchApi.Data;
using EcomSearchApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EcomSearchApi.Services;

public class ProductService(
    AppDbContext db, 
    ElasticsearchClient es, 
    IConfiguration config, 
    ILogger<ProductService> logger) : IProductService
{
    private readonly string _indexName = config["Elasticsearch:IndexName"] ?? "products";

    public async Task<List<Product>> GetAllAsync()
    {
        return await db.Products.AsNoTracking().OrderByDescending(p => p.CreatedAt).ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await db.Products.FindAsync(id);
    }

    public async Task<Product> CreateAsync(Product product)
    {
        // 1. Ghi vào PostgreSQL để sinh Id
        db.Products.Add(product);
        await db.SaveChangesAsync();

        // 2. Dual-Write sang Elasticsearch với _id = product.Id
        try
        {
            var indexResponse = await es.IndexAsync(product, i => i.Index(_indexName).Id(product.Id.ToString()));
            if (!indexResponse.IsValidResponse)
            {
                logger.LogWarning("Elasticsearch indexing failed for Product {Id}: {Error}", product.Id, indexResponse.DebugInformation);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception while indexing Product {Id} to Elasticsearch", product.Id);
        }

        return product;
    }

    public async Task<Product?> UpdateAsync(int id, Product updated)
    {
        var existing = await db.Products.FindAsync(id);
        if (existing == null) return null;

        existing.Name = updated.Name;
        existing.Description = updated.Description;
        existing.Category = updated.Category;
        existing.Brand = updated.Brand;
        existing.Price = updated.Price;
        existing.Stock = updated.Stock;
        existing.Tags = updated.Tags;

        await db.SaveChangesAsync();

        try
        {
            await es.IndexAsync(existing, i => i.Index(_indexName).Id(existing.Id.ToString()));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update Product {Id} in Elasticsearch", id);
        }

        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await db.Products.FindAsync(id);
        if (existing == null) return false;

        db.Products.Remove(existing);
        await db.SaveChangesAsync();

        try
        {
            await es.DeleteAsync<Product>(id.ToString(), d => d.Index(_indexName));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete Product {Id} from Elasticsearch", id);
        }

        return true;
    }
}
