using EcomSearchApi.Models;
using EcomSearchApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace EcomSearchApi.Controllers;

// Controller for Elasticsearch search operations
[ApiController]
[Route("api/[controller]")]
public class SearchController(ISearchService searchService) : ControllerBase
{
    // Full-text search across product name and description
    [HttpGet("fulltext")]
    public async Task<IActionResult> FullText([FromQuery] string q, [FromQuery] int size = 10)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(q)) return BadRequest("Query 'q' is required.");
            var result = await searchService.FullTextSearchAsync(q, size);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    // Filter search by category, brand, and price range
    [HttpGet("filter")]
    public async Task<IActionResult> Filter([FromQuery] string? category, [FromQuery] string? brand, [FromQuery] decimal? minPrice, [FromQuery] decimal? maxPrice)
    {
        try
        {
            var result = await searchService.FilterSearchAsync(category, brand, minPrice, maxPrice);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    // Dynamic multi-clause search allowing flexible rows of conditions
    [HttpPost("dynamic")]
    public async Task<IActionResult> Dynamic([FromBody] DynamicSearchRequest request)
    {
        try
        {
            var result = await searchService.DynamicSearchAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    // Autocomplete suggestion search based on Edge N-Gram token filter
    [HttpGet("autocomplete")]
    public async Task<IActionResult> Autocomplete([FromQuery] string prefix, [FromQuery] int size = 5)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(prefix)) return BadRequest("Prefix is required.");
            var result = await searchService.AutocompleteAsync(prefix, size);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    // Fuzzy search supporting spelling typo tolerance
    [HttpGet("fuzzy")]
    public async Task<IActionResult> Fuzzy([FromQuery] string q, [FromQuery] int size = 5)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(q)) return BadRequest("Query 'q' is required.");
            var result = await searchService.FuzzySearchAsync(q, size);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    // Aggregations and summary statistics for products
    [HttpGet("stats")]
    public async Task<IActionResult> Stats()
    {
        try
        {
            var result = await searchService.GetStatsAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }
}
