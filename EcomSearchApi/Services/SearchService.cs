using EcomSearchApi.Repositories;

namespace EcomSearchApi.Services;

// Service executing search and aggregation operations via Elasticsearch repository
public class SearchService(IProductSearchRepository searchRepo) : ISearchService
{
    // Executes full-text search with highlighting and field boosting
    public async Task<object> FullTextSearchAsync(string query, int size = 10)
    {
        return await searchRepo.FullTextSearchAsync(query, size);
    }

    // Executes structured filtering by category, brand, and price boundaries
    public async Task<object> FilterSearchAsync(string? category, string? brand, decimal? minPrice, decimal? maxPrice)
    {
        return await searchRepo.FilterSearchAsync(category, brand, minPrice, maxPrice);
    }

    // Executes compound query combining required, optional, and excluded criteria
    public async Task<object> AdvancedSearchAsync(string? query, string? category, string? excludeBrand)
    {
        return await searchRepo.AdvancedSearchAsync(query, category, excludeBrand);
    }

    // Provides prefix-based suggestions using autocomplete Edge N-Gram analyzer
    public async Task<object> AutocompleteAsync(string prefix, int size = 5)
    {
        return await searchRepo.AutocompleteAsync(prefix, size);
    }

    // Executes fuzzy matching for tolerance against user typos
    public async Task<object> FuzzySearchAsync(string typo, int size = 5)
    {
        return await searchRepo.FuzzySearchAsync(typo, size);
    }

    // Computes aggregation metrics across categories, brands, and price fields
    public async Task<object> GetStatsAsync()
    {
        return await searchRepo.GetStatsAsync();
    }
}