using KVATUM_AUTH_SERVICE.Core.Entities.Cache;
using KVATUM_AUTH_SERVICE.Core.Entities.Models;
using KVATUM_AUTH_SERVICE.Core.IRepository;
using KVATUM_AUTH_SERVICE.Core.IService;
using KVATUM_AUTH_SERVICE.Infrastructure.Data;
using KVATUM_AUTH_SERVICE.Infrastructure.Repository;
using KVATUM_AUTH_SERVICE.Tests.Mocks;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace KVATUM_AUTH_SERVICE.Tests.Repository
{
    public class SessionRepositoryTests
    {
        private readonly ISessionRepository _sessionRepository;
        private readonly AuthDbContext _context;
        private readonly Mock<ICacheService> _cacheServiceMock;

        private const string DefaultUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36";
        private const string DefaultIp = "localhost";

        public SessionRepositoryTests()
        {
            _context = AuthDbFactory.Create("sessions");
            _cacheServiceMock = CreateCacheServiceMock();
            _sessionRepository = new SessionRepository(_context, _cacheServiceMock.Object);

            MockSetupForExistingSession(_context.AccountSessions.First());
        }

        private Mock<ICacheService> CreateCacheServiceMock()
        {
            var mock = new Mock<ICacheService>();
            mock.Setup(e => e.SetAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>()))
                .Returns(Task.CompletedTask);
            mock.Setup(e => e.RemoveAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            mock.Setup(e => e.GetAsync<CachedAccountSession>(It.IsAny<string>()))
                .ReturnsAsync(default(CachedAccountSession));
            return mock;
        }

        private void MockSetupForExistingSession(AccountSession session)
        {
            var cachedSession = session.ToCachedAccountSession();
            _cacheServiceMock.Setup(e => e.SetAsync($"session:{session.Id}", cachedSession, TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(4)))
                .Returns(Task.CompletedTask);
            _cacheServiceMock.Setup(e => e.RemoveAsync($"session:{session.Id}"))
                .Returns(Task.CompletedTask);
            _cacheServiceMock.Setup(e => e.GetAsync<CachedAccountSession>($"session:{session.Id}"))
                .ReturnsAsync(cachedSession);
        }

        private async Task<AccountSession?> CreateSessionForTestAsync()
        {
            var account = await _context.Accounts.FirstAsync();
            Assert.NotNull(account);

            return await _sessionRepository.GetSessionAsync(account.Id, DefaultUserAgent, DefaultIp);
        }

        [Fact]
        public async Task GetSession_WithValidAccount_ReturnsSession()
        {
            var session = await CreateSessionForTestAsync();
            Assert.NotNull(session);

            var existedSession = await _sessionRepository.GetSessionAsync(session.Id);
            Assert.NotNull(existedSession);
        }

        [Fact]
        public async Task GetSession_WithInvalidAccount_ReturnsNull()
        {
            var nonExistingAccountId = Guid.NewGuid();

            var session = await _sessionRepository.GetSessionAsync(nonExistingAccountId, DefaultUserAgent, DefaultIp);
            Assert.Null(session);
        }

        [Fact]
        public async Task UpdateExistingSessionToken_ReturnsNewToken()
        {
            var session = await CreateSessionForTestAsync();
            Assert.NotNull(session);

            var newToken = Guid.NewGuid().ToString();
            var token = await _sessionRepository.UpdateTokenAsync(newToken, session.Id, TimeSpan.FromHours(1));
            Assert.NotNull(token);
            Assert.Equal(session.Token, token);
        }
    }
}
