using System.Diagnostics;
using System.Net.Http.Json;
using KVATUM_STREAMING_SERVICE.Core.Entities.Cache;
using KVATUM_STREAMING_SERVICE.Core.Entities.Response;
using KVATUM_STREAMING_SERVICE.Core.IService;
using Microsoft.Extensions.Logging;

namespace KVATUM_STREAMING_SERVICE.App.Service
{
    public class AccountService : IAccountService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ICacheService _cacheService;
        private readonly ILogger<AccountService> _logger;
        private readonly string _cacheAccountKeyPrefix = "account:";

        public AccountService(
            IHttpClientFactory httpClientFactory,
            ICacheService cacheService,
            ILogger<AccountService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<AccountProfileBody?> GetAccountProfileAsync(Guid accountId)
        {
            var cacheKey = $"{_cacheAccountKeyPrefix}{accountId}";

            var stopwatch = Stopwatch.StartNew();
            var cachedData = await _cacheService.GetFromCacheAsync<CachedAccount>(cacheKey);
            stopwatch.Stop();
            if (cachedData != null)
            {
                _logger.LogInformation("Account profile get from cache, time: {time}", stopwatch.ElapsedMilliseconds);
                return cachedData.ToAccountProfileBody();
            }

            stopwatch.Restart();
            var client = _httpClientFactory.CreateClient("AccountServiceClient");
            var uri = $"/api/user/{accountId}";
            var response = await client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<AccountProfileBody>();
                stopwatch.Stop();
                _logger.LogInformation("Account profile get from api, time: {time}", stopwatch.ElapsedMilliseconds);
                return result;
            }

            return null;
        }
    }
}