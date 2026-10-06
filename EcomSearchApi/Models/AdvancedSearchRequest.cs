namespace EcomSearchApi.Models;

// Request model for multi-clause Boolean search in Elasticsearch
public class AdvancedSearchRequest
{
    // MUST clauses (All must match, affects score)
    public string? MustQuery { get; set; }
    public string? MustBrand { get; set; }

    // FILTER clauses (Exact filter match, cached, does not affect score)
    public string? FilterCategory { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }

    // SHOULD clauses (Optional, boosts relevance score if matched)
    public string? ShouldTag { get; set; }
    public string? ShouldBrand { get; set; }

    // MUST_NOT clauses (Exclusions)
    public string? ExcludeBrand { get; set; }
    public string? ExcludeTag { get; set; }
}
