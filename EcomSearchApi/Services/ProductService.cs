using EcomSearchApi.Models;
using EcomSearchApi.Repositories;

namespace EcomSearchApi.Services;

// Service handling product business logic and dual-write synchronization
public class ProductService(
    IProductRepository productRepo,
    IProductSearchRepository searchRepo) : IProductService
{
    // Retrieves all products from repository
    public async Task<List<Product>> GetAllAsync()
    {
        return await productRepo.GetAllAsync();
    }

    // Retrieves a single product by identifier
    public async Task<Product?> GetByIdAsync(int id)
    {
        return await productRepo.GetByIdAsync(id);
    }

    // Creates product in relational database and syncs to Elasticsearch
    public async Task<Product> CreateAsync(Product product)
    {
        await productRepo.AddAsync(product);
        await searchRepo.IndexProductAsync(product);
        return product;
    }

    // Updates product in relational database and refreshes Elasticsearch index
    public async Task<Product?> UpdateAsync(int id, Product updated)
    {
        updated.Id = id;
        var existing = await productRepo.UpdateAsync(updated);
        if (existing == null)
        {
            return null;
        }

        await searchRepo.IndexProductAsync(existing);
        return existing;
    }

    // Deletes product from relational database and removes from Elasticsearch
    public async Task<bool> DeleteAsync(int id)
    {
        var deleted = await productRepo.DeleteAsync(id);
        if (!deleted)
        {
            return false;
        }

        await searchRepo.DeleteProductAsync(id);
        return true;
    }
}