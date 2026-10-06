namespace EcomSearchApi.Models;

// Represents a single dynamic condition for Boolean search
public class SearchCondition
{
    // Clause type: "must", "filter", "should", "must_not"
    public string Clause { get; set; } = "must";

    // Field name: "name", "description", "category", "brand", "tags", "minPrice", "maxPrice"
    public string Field { get; set; } = "name";

    // Value to search/filter
    public string Value { get; set; } = string.Empty;
}

// Request payload containing dynamic conditions
public class DynamicSearchRequest
{
    public List<SearchCondition> Conditions { get; set; } = [];
    public int Size { get; set; } = 10;
}
