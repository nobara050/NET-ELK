using EcomSearchApi.Models;

namespace EcomSearchApi.Repositories;

// Repository contract for Elasticsearch indexing and search capabilities
public interface IProductSearchRepository
{
    // Indexing and synchronization operations
    Task<bool> IndexProductAsync(Product product);
    Task<bool> DeleteProductAsync(int id);
    Task BulkIndexAsync(IEnumerable<Product> products);
    Task<long> CountAsync();

    // Query and search operations
    Task<object> FullTextSearchAsync(string query, int size = 10);
    Task<object> FilterSearchAsync(string? category, string? brand, decimal? minPrice, decimal? maxPrice);
    Task<object> AdvancedSearchAsync(string? query, string? category, string? excludeBrand);
    Task<object> AutocompleteAsync(string prefix, int size = 5);
    Task<object> FuzzySearchAsync(string typo, int size = 5);
    Task<object> GetStatsAsync();
}