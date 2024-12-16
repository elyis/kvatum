using KVATUM_AUTH_SERVICE.Core.Entities.Cache;
using KVATUM_AUTH_SERVICE.Core.Entities.Models;
using KVATUM_AUTH_SERVICE.Core.Enums;
using KVATUM_AUTH_SERVICE.Core.IRepository;
using KVATUM_AUTH_SERVICE.Core.IService;
using KVATUM_AUTH_SERVICE.Infrastructure.Data;
using KVATUM_AUTH_SERVICE.Infrastructure.Repository;
using KVATUM_AUTH_SERVICE.Tests.Mocks;
using KVATUM_AUTH_SERVICE.Tests.Mocks.Service;
using Moq;

namespace KVATUM_AUTH_SERVICE.Tests.Repository
{
    public class AccountRepositoryTests
    {
        private readonly AuthDbContext _context;
        private readonly Mock<ICacheService> _cacheServiceMock;
        private readonly IAccountRepository _accountRepository;

        public AccountRepositoryTests()
        {
            var logger = LoggerFactory.CreateLogger<AccountRepository>();
            _cacheServiceMock = new Mock<ICacheService>();
            _context = AuthDbFactory.Create("account");

            SetupMocks(_context.Accounts.First());
            _accountRepository = new AccountRepository(_context, logger, _cacheServiceMock.Object);
        }

        void SetupMocks(Account account)
        {
            _cacheServiceMock.Setup(e => e.SetAsync($"account:{account.Id}", account.ToCachedAccount(), TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(4))).Returns(Task.CompletedTask);
            _cacheServiceMock.Setup(e => e.RemoveAsync($"account:{account.Id}")).Returns(Task.CompletedTask);
            _cacheServiceMock.Setup(e => e.SetIndexedKeyAsync("account", account.Id.ToString(), new string[] { account.Email, account.Nickname }, account.ToCachedAccount(), TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(4)));
            _cacheServiceMock.Setup(e => e.GetAsync<CachedAccount>($"account:{account.Id}")).ReturnsAsync(account.ToCachedAccount());
        }

        [Fact]
        public async Task AddAsync_AccountDoesNotExist_ReturnsTrue()
        {
            var email = "test5@gmail.com";

            var result = await _accountRepository.AddAsync(email, "test5", "test5", AccountRole.User.ToString());
            Assert.NotNull(result);
            Assert.Equal(email, result.Email);

            var existedAccount = await _accountRepository.GetAsync(result.Id);
            Assert.NotNull(existedAccount);
            Assert.Equal(email, existedAccount.Email);
        }

        [Fact]
        public async Task AddExistedAccount_ReturnsTrue()
        {
            var email = "test@test.com";
            var result = await _accountRepository.AddAsync(email, "test", "test5", AccountRole.User.ToString());
            Assert.Null(result);
        }

        [Fact]
        public async Task GetExistedAccountByEmailOrNickname_ReturnsTrue()
        {
            var email = "test@test.com";
            var nickname = "test";

            var result = await _accountRepository.GetAccountByEmailOrNicknameAsync(email);
            Assert.NotNull(result);
            Assert.Equal(email, result.Email);

            result = await _accountRepository.GetAccountByEmailOrNicknameAsync(nickname);
            Assert.NotNull(result);
            Assert.Equal(nickname, result.Nickname);
        }

        [Fact]
        public async Task GetDoesNotExistedAccountByEmailOrNickname_ReturnsTrue()
        {
            var email = "test11@test.com";
            var nickname = "test11";

            var result = await _accountRepository.GetAccountByEmailOrNicknameAsync(email);
            Assert.Null(result);

            result = await _accountRepository.GetAccountByEmailOrNicknameAsync(nickname);
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateExistedAccountNickname_ReturnsTrue()
        {
            var oldNickname = "test";
            var newNickname = "newTesty";

            var result = await _accountRepository.GetAccountByNicknameAsync(oldNickname);
            Assert.NotNull(result);

            result = await _accountRepository.UpdateAccountNicknameAsync(result.Id, newNickname);
            Assert.NotNull(result);
            Assert.Equal(newNickname, result.Nickname);
        }
    }
}