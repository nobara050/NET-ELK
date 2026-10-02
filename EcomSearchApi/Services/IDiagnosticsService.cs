namespace EcomSearchApi.Services;

public interface IDiagnosticsService
{
    object TriggerBusinessError(string? message, string? orderId);
    void SimulateUnhandledException(string? serviceName);
    object TriggerBurstErrors(int count);
}