namespace OmniCart.Application.Common.Interfaces;

public interface ICacheService
{
    // Cache Read
    Task<T?>  GetAsync<T>(string key, CancellationToken cancellationToken = default);

    // Write 
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

    // Remove
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}