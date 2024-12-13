namespace KVATUM_AUTH_SERVICE.Core.IService
{
    public interface ICacheService
    {
        Task SetAsync<T>(string key, T data, TimeSpan slidingExpiration, TimeSpan absoluteExpiration);
        Task SetIndexedKeyAsync<T>(string prefix, string key, string[] indexes, T data, TimeSpan slidingExpiration, TimeSpan absoluteExpiration);
        Task<T?> GetAsync<T>(string key);
        Task<string?> GetStringAsync(string key);
        Task RemoveAsync(string key);
    }
}