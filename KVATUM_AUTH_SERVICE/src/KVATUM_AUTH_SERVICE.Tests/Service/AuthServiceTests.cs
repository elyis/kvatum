using System.Net;
using KVATUM_AUTH_SERVICE.App.Service;
using KVATUM_AUTH_SERVICE.Core.Entities.Request;
using KVATUM_AUTH_SERVICE.Core.Enums;
using KVATUM_AUTH_SERVICE.Core.IService;
using KVATUM_AUTH_SERVICE.Tests.Mocks;
using KVATUM_AUTH_SERVICE.Tests.Mocks.Service;

namespace KVATUM_AUTH_SERVICE.Tests.Service
{
    public class AuthServiceTests
    {
        private readonly IAuthService _authService;
        private const string _defaultUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36";
        private const string _defaultIpAddress = "localhost";

        public AuthServiceTests()
        {
            var accountRepository = AccountRepositoryFactory.Create("auth_service_tests");
            var unverifiedAccountRepository = UnverifiedAccountRepositoryFactory.Create("auth_service_tests");
            var sessionRepository = SessionRepositoryFactory.Create("auth_service_tests");
            var jwtService = JwtServiceFactory.Create();
            var hashPasswordService = HashPasswordServiceFactory.Create();
            var notifyService = NotifyServiceFactory.Create();
            var logger = LoggerFactory.CreateLogger<AuthService>();

            _authService = new AuthService(accountRepository, unverifiedAccountRepository, sessionRepository, jwtService, hashPasswordService, notifyService, logger);
        }

        [Theory]
        [InlineData("unverified@gmail.com", "123", AccountRole.User, true, HttpStatusCode.OK)]
        [InlineData("unverified@gmail.com", "uncorrectVerificationCode", AccountRole.User, false, HttpStatusCode.BadRequest)]
        [InlineData("test4@test.com", "123", AccountRole.User, false, HttpStatusCode.Conflict)]
        public async Task RegisterNewAccount_ShouldReturnExpectedResult(string email, string verificationCode, AccountRole role, bool isSuccess, HttpStatusCode statusCode)
        {
            var result = await _authService.RegisterNewAccount(email, verificationCode, _defaultUserAgent, _defaultIpAddress, role.ToString());
            Assert.NotNull(result);
            Assert.Equal(isSuccess, result.IsSuccess);
            Assert.Equal(statusCode, result.StatusCode);
        }

        [Fact]
        public async Task SignIn_ShouldReturnSuccess()
        {
            var body = new SignInBody
            {
                Email = "test4@test.com",
                Password = "test4"
            };
            var result = await _authService.SignIn(body, _defaultUserAgent, _defaultIpAddress);
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public async Task SignInWithIncorrectEmail_ShouldReturnBadRequest()
        {
            var body = new SignInBody
            {
                Email = "test112@test.com",
                Password = "hash"
            };
            var result = await _authService.SignIn(body, _defaultUserAgent, _defaultIpAddress);
            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        }
    }
}