using EcomSearchApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace EcomSearchApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DiagnosticsController : ControllerBase
{
    private readonly IDiagnosticsService _diagnosticsService;

    public DiagnosticsController(IDiagnosticsService diagnosticsService)
    {
        _diagnosticsService = diagnosticsService;
    }

    /// <summary>
    /// Bắn log Error giả lập lỗi nghiệp vụ (Payment/Checkout)
    /// </summary>
    [HttpPost("trigger-error")]
    public IActionResult TriggerBusinessError([FromQuery] string? message, [FromQuery] string? orderId)
    {
        var result = _diagnosticsService.TriggerBusinessError(message, orderId);
        return Ok(result);
    }

    /// <summary>
    /// Giả lập Exception chưa được bắt (Unhandled Exception / NullReference)
    /// </summary>
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

    /// <summary>
    /// Bắn nhiều log Error liên tục để test ngưỡng threshold lớn
    /// </summary>
    [HttpPost("trigger-burst-errors")]
    public IActionResult TriggerBurstErrors([FromQuery] int count = 5)
    {
        var result = _diagnosticsService.TriggerBurstErrors(count);
        return Ok(result);
    }
}