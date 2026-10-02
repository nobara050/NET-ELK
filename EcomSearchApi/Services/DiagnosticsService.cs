namespace EcomSearchApi.Services;

public class DiagnosticsService : IDiagnosticsService
{
    private readonly ILogger<DiagnosticsService> _logger;

    public DiagnosticsService(ILogger<DiagnosticsService> logger)
    {
        _logger = logger;
    }

    public object TriggerBusinessError(string? message, string? orderId)
    {
        var errMessage = message ?? "Payment gateway timeout while processing transaction.";
        var code = orderId ?? Guid.NewGuid().ToString("N")[..8].ToUpper();

        // Write Error log via Serilog -> Logstash (TCP :5000) -> Elasticsearch
        _logger.LogError(
            "CRITICAL_BUSINESS_ERROR: {ErrorMessage} | OrderId: {OrderId} | Server: {MachineName}",
            errMessage,
            code,
            Environment.MachineName
        );

        return new
        {
            success = true,
            level = "Error",
            message = "Đã bắn log Error thành công vào hệ thống ELK!",
            orderId = code,
            timestamp = DateTime.UtcNow
        };
    }

    public void SimulateUnhandledException(string? serviceName)
    {
        var targetService = serviceName ?? "InventorySyncService";

        try
        {
            throw new InvalidOperationException($"Cannot connect to upstream database cluster for '{targetService}'. Connection pool exhausted.");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "UNHANDLED_EXCEPTION in {ServiceName}: {ExceptionMessage}",
                targetService,
                ex.Message
            );
            throw;
        }
    }

    public object TriggerBurstErrors(int count)
    {
        var total = count > 0 ? count : 5;
        for (int i = 1; i <= total; i++)
        {
            _logger.LogError(
                "BURST_ERROR #{Index}/{Total}: High error rate detected in BackgroundWorker at {Time}",
                i,
                total,
                DateTime.UtcNow
            );
        }

        return new { message = $"Đã bắn thành công {total} log Error liên tiếp!" };
    }
}