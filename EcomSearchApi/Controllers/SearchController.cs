using EcomSearchApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace EcomSearchApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController(ISearchService searchService) : ControllerBase
{
    [HttpGet("fulltext")]
    public async Task<IActionResult> FullText([FromQuery] string q, [FromQuery] int size = 10)
    {
        if (string.IsNullOrWhiteSpace(q)) return BadRequest("Query 'q' is required.");
        var result = await searchService.FullTextSearchAsync(q, size);
        return Ok(result);
    }

    [HttpGet("filter")]
    public async Task<IActionResult> Filter([FromQuery] string? category, [FromQuery] string? brand, [FromQuery] decimal? minPrice, [FromQuery] decimal? maxPrice)
    {
        var result = await searchService.FilterSearchAsync(category, brand, minPrice, maxPrice);
        return Ok(result);
    }

    [HttpGet("advanced")]
    public async Task<IActionResult> Advanced([FromQuery] string? q, [FromQuery] string? category, [FromQuery] string? excludeBrand)
    {
        var result = await searchService.AdvancedSearchAsync(q, category, excludeBrand);
        return Ok(result);
    }

    [HttpGet("autocomplete")]
    public async Task<IActionResult> Autocomplete([FromQuery] string prefix, [FromQuery] int size = 5)
    {
        if (string.IsNullOrWhiteSpace(prefix)) return BadRequest("Prefix is required.");
        var result = await searchService.AutocompleteAsync(prefix, size);
        return Ok(result);
    }

    [HttpGet("fuzzy")]
    public async Task<IActionResult> Fuzzy([FromQuery] string q, [FromQuery] int size = 5)
    {
        if (string.IsNullOrWhiteSpace(q)) return BadRequest("Query 'q' is required.");
        var result = await searchService.FuzzySearchAsync(q, size);
        return Ok(result);
    }

    [HttpGet("stats")]
    public async Task<IActionResult> Stats()
    {
        var result = await searchService.GetStatsAsync();
        return Ok(result);
    }
}
