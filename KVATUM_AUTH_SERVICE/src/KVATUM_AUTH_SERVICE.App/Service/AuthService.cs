using System.Net;
using KVATUM_AUTH_SERVICE.Core.Entities.Events;
using KVATUM_AUTH_SERVICE.Core.Entities.Request;
using KVATUM_AUTH_SERVICE.Core.Entities.Response;
using KVATUM_AUTH_SERVICE.Core.IRepository;
using KVATUM_AUTH_SERVICE.Core.IService;
using Microsoft.Extensions.Logging;

namespace KVATUM_AUTH_SERVICE.App.Service
{
    public class AuthService : IAuthService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IUnverifiedAccountRepository _unverifiedAccountRepository;
        private readonly ISessionRepository _sessionRepository;
        private readonly IJwtService _jwtService;
        private readonly IHashPasswordService _hashPasswordService;
        private readonly INotifyService _notificationService;
        private readonly ILogger<AuthService> _logger;

        public AuthService
        (
            IAccountRepository accountRepository,
            IUnverifiedAccountRepository unverifiedAccountRepository,
            ISessionRepository sessionRepository,
            IJwtService jwtService,
            IHashPasswordService hashPasswordService,
            INotifyService notificationService,
            ILogger<AuthService> logger
        )
        {
            _accountRepository = accountRepository;
            _unverifiedAccountRepository = unverifiedAccountRepository;
            _sessionRepository = sessionRepository;
            _jwtService = jwtService;
            _hashPasswordService = hashPasswordService;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<ServiceResponse<OutputAccountCredentialsBody>> RestoreAccessToken(string refreshToken)
        {
            var session = await _sessionRepository.GetSessionByToken(refreshToken);
            if (session == null)
            {
                return new ServiceResponse<OutputAccountCredentialsBody>
                {
                    Errors = new string[] { "Session not found" },
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.NotFound
                };
            }

            var account = await _accountRepository.GetAsync(session.AccountId);
            var accountCredentials = await UpdateToken(account.Role, account.Id, session.Id);
            return new ServiceResponse<OutputAccountCredentialsBody>
            {
                Body = accountCredentials,
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK
            };
        }

        public async Task<ServiceResponse<OutputAccountCredentialsBody>> SignIn(
            SignInBody body,
            string userAgent,
            string ipAddress)
        {
            var account = await _accountRepository.GetAsync(body.Email);
            if (account == null)
                return new ServiceResponse<OutputAccountCredentialsBody>
                {
                    Errors = new string[] { "Account not found" },
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.NotFound
                };

            var inputPasswordHash = _hashPasswordService.Compute(body.Password);
            account = await _accountRepository.AccountAuthAsync(body.Email, inputPasswordHash);
            if (account == null)
            {
                return new ServiceResponse<OutputAccountCredentialsBody>
                {
                    Errors = new string[] { "Password is not correct" },
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            var sessionId = await CreateOrGetSession(userAgent, ipAddress, account.Id);
            var outputAccountCredentials = await UpdateToken(account.Role, account.Id, sessionId.Value);
            return new ServiceResponse<OutputAccountCredentialsBody>
            {
                Body = outputAccountCredentials,
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK
            };
        }

        public async Task<ServiceResponse<OutputAccountCredentialsBody>> RegisterNewAccount(
            string email,
            string verificationCode,
            string userAgent,
            string ipAddress,
            string rolename)
        {
            var unverifiedAccount = await _unverifiedAccountRepository.GetAsync(email);
            if (unverifiedAccount == null)
            {
                return new ServiceResponse<OutputAccountCredentialsBody>
                {
                    Errors = new string[] { "No application for account registration" },
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            if (unverifiedAccount.VerificationCode != verificationCode)
            {
                return new ServiceResponse<OutputAccountCredentialsBody>
                {
                    Errors = new string[] { "Invalid verification code" },
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            var account = await _accountRepository.AddAsync(
                email,
                unverifiedAccount.Nickname,
                unverifiedAccount.Password,
                rolename);
            if (account == null)
                return new ServiceResponse<OutputAccountCredentialsBody>
                {
                    Errors = new string[] { "Account exists" },
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.Conflict
                };

            await _unverifiedAccountRepository.DeleteAsync(email);

            var sessionId = await CreateOrGetSession(userAgent, ipAddress, account.Id);
            var tokenPair = await UpdateToken(rolename, account.Id, sessionId.Value);
            return new ServiceResponse<OutputAccountCredentialsBody>
            {
                Body = tokenPair,
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK
            };
        }

        private async Task<OutputAccountCredentialsBody> UpdateToken(string rolename, Guid accountId, Guid sessionId)
        {
            var tokenPayload = new TokenPayload
            {
                Role = rolename,
                AccountId = accountId,
                SessionId = sessionId
            };

            var accountCredentials = _jwtService.GenerateOutputAccountCredentials(tokenPayload);
            accountCredentials.RefreshToken = await _sessionRepository.UpdateTokenAsync(accountCredentials.RefreshToken, tokenPayload.SessionId);
            return accountCredentials;
        }

        private async Task<Guid?> CreateOrGetSession(string userAgent, string ipAddress, Guid accountId)
        {
            var account = await _accountRepository.GetAsync(accountId);
            if (account == null)
                return null;

            var ip = string.IsNullOrEmpty(ipAddress) ? "Unknown" : ipAddress;
            var session = await _sessionRepository.GetOrAddSessionAsync(userAgent, ip, accountId);
            return session?.Id;
        }

        public async Task<HttpStatusCode> CreateUnverifiedAccount(SignUpBody body)
        {
            var verificationCode = CodeGeneratorService.GenerateCode();
            var existingAccount = await _accountRepository.GetAsync(body.Email);
            if (existingAccount != null)
                return HttpStatusCode.Conflict;

            await _unverifiedAccountRepository.DeleteAsync(body.Email);

            var hashPassword = _hashPasswordService.Compute(body.Password);
            body.Password = hashPassword;

            await _unverifiedAccountRepository.AddAsync(body, verificationCode);

            var emailConfirmationEvent = new EmailConfirmationEvent
            {
                Email = body.Email,
                Code = verificationCode
            };
            _notificationService.Publish(emailConfirmationEvent, PublishEvent.SendConfirmationCode);
            return HttpStatusCode.OK;
        }
    }
}