namespace KVATUM_STREAMING_SERVICE.Core.IService
{
    public interface ICacheService
    {
        Task CacheSetAsync<T>(string key, T data, TimeSpan slidingExpiration, TimeSpan absoluteExpiration);
        Task<T?> GetFromCacheAsync<T>(string key);
    }
}