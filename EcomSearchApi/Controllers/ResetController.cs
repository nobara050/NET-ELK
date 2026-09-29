using EcomSearchApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace EcomSearchApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResetController(IResetService resetService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Reset()
    {
        var result = await resetService.ResetAllAsync();
        return Ok(result);
    }
}
