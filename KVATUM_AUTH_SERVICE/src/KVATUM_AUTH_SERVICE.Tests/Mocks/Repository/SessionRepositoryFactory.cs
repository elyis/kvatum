using KVATUM_AUTH_SERVICE.Core.Entities.Cache;
using KVATUM_AUTH_SERVICE.Core.Entities.Models;
using KVATUM_AUTH_SERVICE.Core.IRepository;
using KVATUM_AUTH_SERVICE.Core.IService;
using KVATUM_AUTH_SERVICE.Infrastructure.Repository;
using Moq;

namespace KVATUM_AUTH_SERVICE.Tests.Mocks
{
    public class SessionRepositoryFactory
    {
        public static ISessionRepository Create(string name = "session_repository_test")
        {
            var context = AuthDbFactory.Create(name);
            var cacheServiceMock = new Mock<ICacheService>();

            SetupMocks(context.AccountSessions.First(), cacheServiceMock);
            var sessionRepository = new SessionRepository(context, cacheServiceMock.Object);
            return sessionRepository;
        }

        private static void SetupMocks(AccountSession session, Mock<ICacheService> cacheServiceMock)
        {
            cacheServiceMock.Setup(e => e.SetAsync($"session:{session.Id}", session.ToCachedAccountSession(), TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(4))).Returns(Task.CompletedTask);
            cacheServiceMock.Setup(e => e.RemoveAsync($"session:{session.Id}")).Returns(Task.CompletedTask);
            cacheServiceMock.Setup(e => e.GetAsync<CachedAccountSession>($"session:{session.Id}")).ReturnsAsync(session.ToCachedAccountSession());
        }
    }
}