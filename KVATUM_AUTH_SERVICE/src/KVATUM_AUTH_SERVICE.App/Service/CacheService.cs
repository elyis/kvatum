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
        private readonly string _instanceName;

        public CacheService(
            IDistributedCache cache,
            ILogger<CacheService> logger)
        {
            _cache = cache;
            _logger = logger;
            _instanceName = Environment.GetEnvironmentVariable("REDIS_INSTANCE_NAME") ?? throw new Exception("REDIS_INSTANCE_NAME is not set");
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
            _logger.LogInformation($"Закэшировано: {key} со значением {serializedData}");
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
                _logger.LogInformation($"Ключ {key} не найден в кэше");
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
            var cachedData = await _cache.GetStringAsync(key);
            if (cachedData == null)
                _logger.LogInformation($"Ключ {key} не найден в кэше");
            else
                _logger.LogInformation($"Получено из кэша: {key} со значением {cachedData}");

            return cachedData;
        }
    }
}