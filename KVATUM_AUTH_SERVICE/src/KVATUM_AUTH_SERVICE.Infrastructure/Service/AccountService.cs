using Grpc.Core;
using KVATUM_AUTH_SERVICE.Core;
using KVATUM_AUTH_SERVICE.Core.IRepository;
using Microsoft.Extensions.Logging;

namespace KVATUM_AUTH_SERVICE.Infrastructure.Service
{
    public class AccountGrpcService : AccountService.AccountServiceBase
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ILogger<AccountGrpcService> _logger;

        public AccountGrpcService(
            IAccountRepository accountRepository,
            ILogger<AccountGrpcService> logger)
        {
            _accountRepository = accountRepository;
            _logger = logger;
        }

        public override async Task<AccountResponse> GetAccountById(RequestAccountMessage request, ServerCallContext context)
        {
            var account = await _accountRepository.GetAsync(request.Id);
            if (account == null)
                return new AccountResponse
                {
                    Error = "Account not found"
                };

            var response = new ResponseAccountMessage
            {
                Id = account.Id.ToString(),
                Email = account.Email,
                Nickname = account.Nickname,
                Role = account.Role.ToString(),
            };

            string? urlIcon = string.IsNullOrEmpty(account.Image) ? null : $"{Constants.WebUrlToProfileImage}/{account.Image}";
            var images = urlIcon is null ? new() : new List<ImageWithResolutionMessage>
            {
                new() { Resolution = ImageResolutions.Small, UrlImage = $"{urlIcon}?width=128&height=128" },
                new() { Resolution = ImageResolutions.Medium, UrlImage = $"{urlIcon}?width=256&height=256" },
                new() { Resolution = ImageResolutions.Big, UrlImage = $"{urlIcon}?width=512&height=512" },
            };

            response.Image.AddRange(images);

            _logger.LogInformation("Account {accountId} retrieved successfully", account.Id);

            return new AccountResponse
            {
                Account = response
            };
        }
    }
}