using EcomSearchApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace EcomSearchApi.Controllers;

// Controller for resetting and re-seeding demo database and Elasticsearch index
[ApiController]
[Route("api/[controller]")]
public class ResetController(IResetService resetService) : ControllerBase
{
    // Resets PostgreSQL tables and Elasticsearch index with initial seed data
    [HttpPost]
    public async Task<IActionResult> Reset()
    {
        try
        {
            var result = await resetService.ResetAllAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }
}
