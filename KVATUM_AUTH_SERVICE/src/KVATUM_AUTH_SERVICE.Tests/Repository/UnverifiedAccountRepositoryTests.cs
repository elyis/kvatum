using KVATUM_AUTH_SERVICE.Core.Entities.Models;
using KVATUM_AUTH_SERVICE.Core.Entities.Request;
using KVATUM_AUTH_SERVICE.Core.IRepository;
using KVATUM_AUTH_SERVICE.Core.IService;
using KVATUM_AUTH_SERVICE.Infrastructure.Data;
using KVATUM_AUTH_SERVICE.Infrastructure.Repository;
using KVATUM_AUTH_SERVICE.Tests.Mocks;
using KVATUM_AUTH_SERVICE.Tests.Mocks.Service;
using Moq;

namespace KVATUM_AUTH_SERVICE.Tests.Repository
{
    public class UnverifiedAccountRepositoryTests
    {
        private readonly AuthDbContext _context;
        private readonly Mock<ICacheService> _cacheServiceMock;
        private readonly IUnverifiedAccountRepository _accountRepository;

        public UnverifiedAccountRepositoryTests()
        {
            var logger = LoggerFactory.CreateLogger<UnverifiedAccountRepository>();
            _cacheServiceMock = new Mock<ICacheService>();

            _context = AuthDbFactory.Create("unverified");
            SetupMocks(_context.UnverifiedAccounts.First());

            _accountRepository = new UnverifiedAccountRepository(_context, _cacheServiceMock.Object, logger);
        }

        void SetupMocks(UnverifiedAccount account)
        {
            _cacheServiceMock.Setup(e => e.SetAsync($"account:{account.Id}", account, TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(4))).Returns(Task.CompletedTask);
            _cacheServiceMock.Setup(e => e.RemoveAsync($"account:{account.Id}")).Returns(Task.CompletedTask);
            _cacheServiceMock.Setup(e => e.SetIndexedKeyAsync("account", account.Id.ToString(), new string[] { account.Email, account.Nickname }, account, TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(4)));
            _cacheServiceMock.Setup(e => e.GetAsync<UnverifiedAccount>($"account:{account.Id}")).ReturnsAsync(account);
        }

        [Fact]
        public async Task AddNewAccount_ReturnsTrue()
        {
            var body = new SignUpBody
            {
                Email = "newUnverifed@gmail.com",
                Nickname = "newUnverified",
                Password = "newUnver"
            };

            var result = await _accountRepository.AddAsync(body, "123");
            Assert.NotNull(result);
            Assert.Equal(body.Email, result.Email);
        }

        [Fact]
        public async Task AddExistingAccount_ReturnsTrue()
        {
            var body = new SignUpBody
            {
                Email = "test4@test.com",
                Nickname = "newUnverified",
                Password = "newUnver"
            };

            var result = await _accountRepository.AddAsync(body, "123");
            Assert.Null(result);
        }

        [Fact]
        public async Task RemoveExistingAccount_ReturnsTrue()
        {
            var email = "test4@test.com";
            var result = await _accountRepository.DeleteAsync(email);
            Assert.True(result);
        }

        [Fact]
        public async Task RemoveDoesNotExistingAccount_ReturnsTrue()
        {
            var email = "test11@test.com";
            var result = await _accountRepository.DeleteAsync(email);
            Assert.True(result);
        }
    }
}