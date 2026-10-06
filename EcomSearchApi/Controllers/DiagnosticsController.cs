using EcomSearchApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace EcomSearchApi.Controllers;

// Controller for diagnostic and log generation testing
[ApiController]
[Route("api/[controller]")]
public class DiagnosticsController : ControllerBase
{
    private readonly IDiagnosticsService _diagnosticsService;

    public DiagnosticsController(IDiagnosticsService diagnosticsService)
    {
        _diagnosticsService = diagnosticsService;
    }

    // Triggers simulated business errors for ELK logging
    [HttpPost("trigger-error")]
    public IActionResult TriggerBusinessError([FromQuery] string? message, [FromQuery] string? orderId)
    {
        var result = _diagnosticsService.TriggerBusinessError(message, orderId);
        return Ok(result);
    }

    // Simulates an unhandled exception for error monitoring
    [HttpPost("simulate-exception")]
    public IActionResult SimulateException([FromQuery] string? serviceName)
    {
        try
        {
            _diagnosticsService.SimulateUnhandledException(serviceName);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                error = ex.Message,
                type = ex.GetType().Name,
                timestamp = DateTime.UtcNow
            });
        }
    }

    // Triggers multiple consecutive error logs for rate testing
    [HttpPost("trigger-burst-errors")]
    public IActionResult TriggerBurstErrors([FromQuery] int count = 5)
    {
        var result = _diagnosticsService.TriggerBurstErrors(count);
        return Ok(result);
    }
}