using System.Text.Json;
using KVATUM_CHATFLOW_SERVICE.Core.Entities.Cache;
using KVATUM_CHATFLOW_SERVICE.Core.IService;
using Microsoft.Extensions.Caching.Distributed;

namespace KVATUM_CHATFLOW_SERVICE.App.Service
{
    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _cache;

        public CacheService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task SetAsync<T>(string key, T data, TimeSpan slidingExpiration, TimeSpan absoluteExpiration)
        {
            var options = new DistributedCacheEntryOptions
            {
                SlidingExpiration = slidingExpiration,
                AbsoluteExpiration = DateTime.Now.Add(absoluteExpiration)
            };
            var serializedData = JsonSerializer.Serialize(data);
            await _cache.SetStringAsync(key, serializedData, options);
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var cachedData = await _cache.GetStringAsync(key);
            return cachedData != null ? JsonSerializer.Deserialize<T>(cachedData) : default;
        }

        public async Task RemoveAsync(string key)
        {
            await _cache.RemoveAsync(key);
        }

        public async Task<ResponseCache?> GetResponseCacheAsync(string key)
        {
            var cachedData = await _cache.GetStringAsync(key);
            if (cachedData == null)
                return null;

            bool isIndexKey = Guid.TryParse(cachedData, out var indexKey);
            return new ResponseCache
            {
                Value = cachedData,
                IsIndexKey = isIndexKey
            };
        }
    }
}