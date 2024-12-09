using KVATUM_CHATFLOW_SERVICE.Core.Entities.Cache;

namespace KVATUM_CHATFLOW_SERVICE.Core.IService
{
    public interface ICacheService
    {
        Task SetAsync<T>(string key, T data, TimeSpan slidingExpiration, TimeSpan absoluteExpiration);
        Task<T?> GetAsync<T>(string key);
        Task<ResponseCache?> GetResponseCacheAsync(string key);
        Task RemoveAsync(string key);
    }
}