using System.Diagnostics;
using System.Net.Http.Json;
using Grpc.Net.Client;
using KVATUM_STREAMING_SERVICE.Core.Entities.Cache;
using KVATUM_STREAMING_SERVICE.Core.Entities.Response;
using KVATUM_STREAMING_SERVICE.Core.Enums;
using KVATUM_STREAMING_SERVICE.Core.IService;
using Microsoft.Extensions.Logging;

namespace KVATUM_STREAMING_SERVICE.Infrastructure.Service
{
    public class AccountManager : IAccountManager
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ICacheService _cacheService;
        private readonly ILogger<AccountManager> _logger;
        private readonly string _cacheAccountKeyPrefix = "account";
        private readonly GrpcChannel _grpcChannel;

        public AccountManager(
            IHttpClientFactory httpClientFactory,
            ICacheService cacheService,
            ILogger<AccountManager> logger)
        {
            _httpClientFactory = httpClientFactory;
            _cacheService = cacheService;
            _logger = logger;

            var baseUrl = Environment.GetEnvironmentVariable("AUTH_SERVICE_BASE_URL");
            var httpClientHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            _grpcChannel = GrpcChannel.ForAddress(baseUrl, new GrpcChannelOptions { HttpHandler = httpClientHandler });
        }

        public async Task<AccountProfileBody?> GetAccountProfileAsync(Guid accountId)
        {
            var stopwatch = Stopwatch.StartNew();
            var accountProfileBody = await GetFromCacheAsync(accountId);
            stopwatch.Stop();
            if (accountProfileBody != null)
            {
                _logger.LogInformation("Account profile get from cache, time: {time}", stopwatch.ElapsedMilliseconds);
                return accountProfileBody;
            }

            stopwatch.Restart();
            accountProfileBody = await GetAccountProfileByGrpcAsync(accountId);
            stopwatch.Stop();
            if (accountProfileBody != null)
            {
                _logger.LogInformation("Account profile get from grpc, time: {time}", stopwatch.ElapsedMilliseconds);
                return accountProfileBody;
            }

            stopwatch.Restart();
            accountProfileBody = await GetAccountProfileByApiAsync(accountId);
            stopwatch.Stop();
            if (accountProfileBody != null)
            {
                _logger.LogInformation("Account profile get from api, time: {time}", stopwatch.ElapsedMilliseconds);
                return accountProfileBody;
            }

            return null;
        }

        private async Task<AccountProfileBody?> GetFromCacheAsync(Guid accountId)
        {
            var cacheKey = $"{_cacheAccountKeyPrefix}:{accountId}";

            var cachedData = await _cacheService.GetFromCacheAsync<CachedAccount>(cacheKey);
            return cachedData?.ToAccountProfileBody();
        }

        private async Task<AccountProfileBody?> GetAccountProfileByGrpcAsync(Guid accountId)
        {
            try
            {
                var grpcClient = new AccountService.AccountServiceClient(_grpcChannel);
                var request = new RequestAccountMessage { Id = accountId.ToString() };

                var grpcResponse = await grpcClient.GetAccountByIdAsync(request);
                if (grpcResponse.ResultCase == AccountResponse.ResultOneofCase.Account)
                {
                    var account = grpcResponse.Account;
                    var accountProfileBody = new AccountProfileBody
                    {
                        Id = Guid.Parse(account.Id),
                        Email = account.Email,
                        Role = Enum.Parse<AccountRole>(account.Role),
                        Nickname = account.Nickname,
                        Images = account.Image.Select(e => new ImageWithResolutionBody
                        {
                            Resolution = Enum.Parse<Core.Enums.ImageResolutions>(e.Resolution.ToString()),
                            UrlImage = e.UrlImage
                        }).ToList()
                    };
                    return accountProfileBody;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting account profile by grpc");
            }
            return null;
        }

        private async Task<AccountProfileBody?> GetAccountProfileByApiAsync(Guid accountId)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("AccountServiceClient");

                var uri = $"/api/user/{accountId}";
                var response = await client.GetAsync(uri);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<AccountProfileBody>();
                    return result;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting account profile by api");
            }

            return null;
        }
    }
}