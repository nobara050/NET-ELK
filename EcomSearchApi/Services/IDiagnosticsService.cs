namespace EcomSearchApi.Services;

// Diagnostics service contract for testing error logging and exceptions
public interface IDiagnosticsService
{
    object TriggerBusinessError(string? message, string? orderId);
    void SimulateUnhandledException(string? serviceName);
    object TriggerBurstErrors(int count);
}