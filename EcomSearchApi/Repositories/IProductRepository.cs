using EcomSearchApi.Models;

namespace EcomSearchApi.Repositories;

public interface IProductRepository
{
    Task<List<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(int id);
    Task<Product> AddAsync(Product product);
    Task<Product?> UpdateAsync(Product product);
    Task<bool> DeleteAsync(int id);
    Task TruncateAndResetAsync();
    Task AddRangeAsync(IEnumerable<Product> products);
    Task<int> CountAsync();
}