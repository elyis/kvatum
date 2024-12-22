using System.Text.Json;
using KVATUM_AUTH_SERVICE.Core.IService;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace KVATUM_AUTH_SERVICE.App.Service
{
    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<CacheService> _logger;

        public CacheService(
            IDistributedCache cache,
            ILogger<CacheService> logger)
        {
            _cache = cache;
            _logger = logger;
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

        public async Task SetIndexedKeyAsync<T>(
            string prefix,
            string key,
            string[] indexes,
            T data,
            TimeSpan slidingExpiration,
            TimeSpan absoluteExpiration)
        {
            var options = new DistributedCacheEntryOptions
            {
                SlidingExpiration = slidingExpiration,
                AbsoluteExpiration = DateTime.Now.Add(absoluteExpiration)
            };
            var serializedData = JsonSerializer.Serialize(data);

            var mainKey = $"{prefix}:{key}";
            await _cache.SetStringAsync(mainKey, serializedData, options);

            foreach (var index in indexes)
            {
                await _cache.SetStringAsync($"{prefix}:{index}", mainKey, options);
            }
        }


        public async Task<T?> GetAsync<T>(string key)
        {
            var cachedData = await _cache.GetStringAsync(key);
            if (cachedData == null)
            {
                return default;
            }

            _logger.LogInformation($"Получено: {key} со значением {cachedData}");
            return JsonSerializer.Deserialize<T>(cachedData);
        }

        public async Task RemoveAsync(string key)
        {
            await _cache.RemoveAsync(key);
            _logger.LogInformation("Удалено: {Key}", key);
        }

        public async Task<string?> GetStringAsync(string key)
        {
            return await _cache.GetStringAsync(key);
        }
    }
}