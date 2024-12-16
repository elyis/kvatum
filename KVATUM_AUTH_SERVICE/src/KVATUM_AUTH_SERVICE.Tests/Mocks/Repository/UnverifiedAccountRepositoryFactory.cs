using KVATUM_AUTH_SERVICE.Core.Entities.Models;
using KVATUM_AUTH_SERVICE.Core.IRepository;
using KVATUM_AUTH_SERVICE.Core.IService;
using KVATUM_AUTH_SERVICE.Infrastructure.Repository;
using Microsoft.Extensions.Logging;
using Moq;

namespace KVATUM_AUTH_SERVICE.Tests.Mocks
{
    public class UnverifiedAccountRepositoryFactory
    {
        public static IUnverifiedAccountRepository Create(string name = "unverified_account_repository_test")
        {
            var loggerMock = new Mock<ILogger<UnverifiedAccountRepository>>();
            var context = AuthDbFactory.Create(name);
            var cacheServiceMock = new Mock<ICacheService>();

            SetupMocks(context.UnverifiedAccounts.First(), cacheServiceMock);
            var unverifiedAccountRepository = new UnverifiedAccountRepository(context, cacheServiceMock.Object, loggerMock.Object);
            return unverifiedAccountRepository;
        }

        private static void SetupMocks(UnverifiedAccount unverifiedAccount, Mock<ICacheService> cacheServiceMock)
        {
            cacheServiceMock.Setup(e => e.SetAsync($"unverified_account:{unverifiedAccount.Id}", unverifiedAccount, TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(4))).Returns(Task.CompletedTask);
            cacheServiceMock.Setup(e => e.RemoveAsync($"unverified_account:{unverifiedAccount.Id}")).Returns(Task.CompletedTask);
            cacheServiceMock.Setup(e => e.GetAsync<UnverifiedAccount>($"unverified_account:{unverifiedAccount.Id}")).ReturnsAsync(unverifiedAccount);
        }
    }
}