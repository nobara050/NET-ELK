namespace EcomSearchApi.Services;

// Search service contract for Elasticsearch queries and statistics
public interface ISearchService
{
    Task<object> FullTextSearchAsync(string query, int size = 10);
    Task<object> FilterSearchAsync(string? category, string? brand, decimal? minPrice, decimal? maxPrice);
    Task<object> AdvancedSearchAsync(string? query, string? category, string? excludeBrand);
    Task<object> AutocompleteAsync(string prefix, int size = 5);
    Task<object> FuzzySearchAsync(string typo, int size = 5);
    Task<object> GetStatsAsync();
}
