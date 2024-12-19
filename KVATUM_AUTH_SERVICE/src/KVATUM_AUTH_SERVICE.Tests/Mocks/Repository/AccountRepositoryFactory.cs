using KVATUM_AUTH_SERVICE.Core.Entities.Cache;
using KVATUM_AUTH_SERVICE.Core.Entities.Models;
using KVATUM_AUTH_SERVICE.Core.IRepository;
using KVATUM_AUTH_SERVICE.Core.IService;
using KVATUM_AUTH_SERVICE.Infrastructure.Repository;
using Microsoft.Extensions.Logging;
using Moq;

namespace KVATUM_AUTH_SERVICE.Tests.Mocks
{
    public class AccountRepositoryFactory
    {
        public static IAccountRepository Create(string name = "account_repository_test")
        {
            var loggerMock = new Mock<ILogger<AccountRepository>>();
            var context = AuthDbFactory.Create(name);
            var cacheServiceMock = new Mock<ICacheService>();
            var notifyServiceMock = new Mock<INotifyService>();


            SetupMocks(context.Accounts.First(), cacheServiceMock);
            var accountRepository = new AccountRepository(context, loggerMock.Object, cacheServiceMock.Object, notifyServiceMock.Object);
            return accountRepository;
        }

        private static void SetupMocks(Account account, Mock<ICacheService> cacheServiceMock)
        {
            cacheServiceMock.Setup(e => e.SetAsync($"account:{account.Id}", account.ToCachedAccount(), TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(4))).Returns(Task.CompletedTask);
            cacheServiceMock.Setup(e => e.RemoveAsync($"account:{account.Id}")).Returns(Task.CompletedTask);
            cacheServiceMock.Setup(e => e.SetIndexedKeyAsync("account", account.Id.ToString(), new string[] { account.Email, account.Nickname }, account.ToCachedAccount(), TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(4)));
            cacheServiceMock.Setup(e => e.GetAsync<CachedAccount>($"account:{account.Id}")).ReturnsAsync(account.ToCachedAccount());
        }
    }
}