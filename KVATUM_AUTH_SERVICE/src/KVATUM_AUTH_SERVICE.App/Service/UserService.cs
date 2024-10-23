using System.Net;
using KVATUM_AUTH_SERVICE.Core.Entities.Response;
using KVATUM_AUTH_SERVICE.Core.IRepository;
using KVATUM_AUTH_SERVICE.Core.IService;

namespace KVATUM_AUTH_SERVICE.App.Service
{
    public class UserService : IUserService
    {
        private readonly IAccountRepository _accountRepository;

        public UserService(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        public async Task<ServiceResponse<ProfileBody>> GetProfile(Guid accountId)
        {
            var account = await _accountRepository.GetAsync(accountId);
            if (account == null)
                return new ServiceResponse<ProfileBody>
                {
                    StatusCode = HttpStatusCode.NotFound,
                    IsSuccess = false,
                    Errors = new[] { "Account not found" }
                };

            return new ServiceResponse<ProfileBody>
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Body = account.ToProfileBody()
            };
        }

        public async Task<HttpStatusCode> ChangeAccountNickname(Guid accountId, string nickname)
        {
            var account = await _accountRepository.GetAsync(accountId);
            if (account == null)
                return HttpStatusCode.NotFound;

            if (account.Nickname == nickname)
                return HttpStatusCode.OK;

            var accountByNickname = await _accountRepository.GetAccountByNicknameAsync(nickname);
            if (accountByNickname != null)
                return HttpStatusCode.Conflict;

            account = await _accountRepository.UpdateAccountNicknameAsync(accountId, nickname);
            return account == null ? HttpStatusCode.BadRequest : HttpStatusCode.OK;
        }

        public async Task<ServiceResponse<ProfileBody>> GetProfileByNickname(string nickname)
        {
            var account = await _accountRepository.GetAccountByNicknameAsync(nickname);
            if (account == null)
                return new ServiceResponse<ProfileBody>
                {
                    StatusCode = HttpStatusCode.NotFound,
                    IsSuccess = false,
                    Errors = new[] { "Account not found" }
                };

            return new ServiceResponse<ProfileBody>
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Body = account.ToProfileBody()
            };
        }

        public async Task<ServiceResponse<IEnumerable<ProfileBody>>> GetAccountsByPatternNickname(string patternNickname, int limit, int offset)
        {
            var accounts = await _accountRepository.GetAccountsByPatternNicknameAsync(patternNickname, limit, offset);
            return new ServiceResponse<IEnumerable<ProfileBody>>
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Body = accounts.Select(a => a.ToProfileBody())
            };
        }

        public async Task<ServiceResponse<ProfileBody>> GetProfileByEmail(string email)
        {
            var account = await _accountRepository.GetAsync(email);
            if (account == null)
                return new ServiceResponse<ProfileBody>
                {
                    StatusCode = HttpStatusCode.NotFound,
                    IsSuccess = false,
                    Errors = new[] { "Account not found" }
                };

            return new ServiceResponse<ProfileBody>
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Body = account.ToProfileBody()
            };
        }
    }
}