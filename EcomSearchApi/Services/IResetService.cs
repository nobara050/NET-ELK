namespace EcomSearchApi.Services;

// Reset service contract for restoring sample datasets
public interface IResetService
{
    Task<object> ResetAllAsync();
}
