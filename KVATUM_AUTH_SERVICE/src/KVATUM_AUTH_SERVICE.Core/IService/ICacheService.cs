using KVATUM_AUTH_SERVICE.Core.Entities.Cache;

namespace KVATUM_AUTH_SERVICE.Core.IService
{
    public interface ICacheService
    {
        Task CacheSetAsync<T>(string key, T data, TimeSpan slidingExpiration, TimeSpan absoluteExpiration);
        Task<T?> GetFromCacheAsync<T>(string key);
        Task<ResponseCache?> GetResponseCacheAsync(string key);
        Task RemoveCacheAsync(string key);
    }
}