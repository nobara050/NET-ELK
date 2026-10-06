namespace EcomSearchApi.Services;

// Diagnostics service simulating application errors and monitoring events
public class DiagnosticsService(ILogger<DiagnosticsService> logger) : IDiagnosticsService
{
    // Simulates a business level error and logs details for ELK tracking
    public object TriggerBusinessError(string? message, string? orderId)
    {
        var errMessage = message ?? "Payment gateway timeout while processing transaction.";
        var code = orderId ?? Guid.NewGuid().ToString("N")[..8].ToUpper();

        logger.LogError(
            "CRITICAL_BUSINESS_ERROR: {ErrorMessage} | OrderId: {OrderId} | Server: {MachineName}",
            errMessage,
            code,
            Environment.MachineName
        );

        return new
        {
            success = true,
            level = "Error",
            message = "Business error log dispatched to ELK stack successfully.",
            orderId = code,
            timestamp = DateTime.UtcNow
        };
    }

    // Simulates an unhandled service exception with stack trace logging
    public void SimulateUnhandledException(string? serviceName)
    {
        var targetService = serviceName ?? "InventorySyncService";

        try
        {
            throw new InvalidOperationException($"Cannot connect to upstream database cluster for '{targetService}'. Connection pool exhausted.");
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "UNHANDLED_EXCEPTION in {ServiceName}: {ExceptionMessage}",
                targetService,
                ex.Message
            );
            throw;
        }
    }

    // Generates a burst of error logs to test alert ingestion rates
    public object TriggerBurstErrors(int count)
    {
        var total = count > 0 ? count : 5;
        for (int i = 1; i <= total; i++)
        {
            logger.LogError(
                "BURST_ERROR #{Index}/{Total}: High error rate detected in BackgroundWorker at {Time}",
                i,
                total,
                DateTime.UtcNow
            );
        }

        return new { message = $"Successfully emitted {total} error logs." };
    }
}