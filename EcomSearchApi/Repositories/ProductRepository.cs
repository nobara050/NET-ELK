using EcomSearchApi.Data;
using EcomSearchApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EcomSearchApi.Repositories;

public class ProductRepository(AppDbContext db) : IProductRepository
{
    public async Task<List<Product>> GetAllAsync()
    {
        return await db.Products.AsNoTracking().OrderByDescending(p => p.CreatedAt).ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await db.Products.FindAsync(id);
    }

    public async Task<Product> AddAsync(Product product)
    {
        db.Products.Add(product);
        await db.SaveChangesAsync();
        return product;
    }

    public async Task<Product?> UpdateAsync(Product product)
    {
        var existing = await db.Products.FindAsync(product.Id);
        if (existing == null) return null;

        existing.Name = product.Name;
        existing.Description = product.Description;
        existing.Category = product.Category;
        existing.Brand = product.Brand;
        existing.Price = product.Price;
        existing.Stock = product.Stock;
        existing.Tags = product.Tags;

        await db.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await db.Products.FindAsync(id);
        if (existing == null) return false;

        db.Products.Remove(existing);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task TruncateAndResetAsync()
    {
        await db.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Products\" RESTART IDENTITY CASCADE;");
    }

    public async Task AddRangeAsync(IEnumerable<Product> products)
    {
        await db.Products.AddRangeAsync(products);
        await db.SaveChangesAsync();
    }

    public async Task<int> CountAsync()
    {
        return await db.Products.CountAsync();
    }
}
