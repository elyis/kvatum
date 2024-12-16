using Microsoft.EntityFrameworkCore;
using KVATUM_AUTH_SERVICE.Core.Entities.Models;
using KVATUM_AUTH_SERVICE.Core.IRepository;
using KVATUM_AUTH_SERVICE.Infrastructure.Data;
using KVATUM_AUTH_SERVICE.Core.IService;
using Microsoft.Extensions.Logging;
using KVATUM_AUTH_SERVICE.Core.Entities.Cache;
using System.Text.Json;

namespace KVATUM_AUTH_SERVICE.Infrastructure.Repository
{
    public class AccountRepository : IAccountRepository
    {
        private readonly AuthDbContext _context;

        private readonly ILogger<AccountRepository> _logger;
        private readonly string _cacheAccountKeyPrefix = "account";
        private readonly ICacheService _cacheService;

        public AccountRepository(
            AuthDbContext context,
            ILogger<AccountRepository> logger,
            ICacheService cacheService)
        {
            _context = context;
            _logger = logger;
            _cacheService = cacheService;
        }

        public async Task<CachedAccount?> AddAsync(
            string identifier,
            string nickname,
            string password,
            string role)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(e => e.Email == identifier);
            if (account != null)
                return null;

            account = new Account
            {
                Email = identifier,
                Nickname = nickname,
                PasswordHash = password,
                Role = role,
            };

            account = (await _context.Accounts.AddAsync(account))?.Entity;
            await _context.SaveChangesAsync();

            var cachedAccount = account?.ToCachedAccount();
            if (cachedAccount == null)
                return null;

            await CacheAccount(cachedAccount, TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(4));
            return cachedAccount;
        }

        public async Task<CachedAccount?> GetAsync(Guid id)
        {
            var cachedAccount = await GetFromCacheAsync<CachedAccount>($"{_cacheAccountKeyPrefix}:{id}");
            if (cachedAccount != null)
                return cachedAccount;

            var account = await _context.Accounts.FirstOrDefaultAsync(e => e.Id == id);
            if (account == null)
                return null;

            cachedAccount = account.ToCachedAccount();
            await CacheAccount(cachedAccount, TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(4));
            return cachedAccount;
        }

        public async Task<CachedAccount?> GetAsync(string email)
        {
            var cachedAccount = await GetFromCacheAsync<CachedAccount>($"{_cacheAccountKeyPrefix}:{email}");

            if (cachedAccount != null)
                return cachedAccount;

            var account = await _context.Accounts.FirstOrDefaultAsync(e => e.Email == email);
            if (account == null)
                return null;

            cachedAccount = account.ToCachedAccount();
            await CacheAccount(cachedAccount, TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(4));
            return cachedAccount;
        }

        public async Task<CachedAccount?> UpdateProfileIconAsync(Guid accountId, string filename)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(e => e.Id == accountId);
            if (account == null)
                return null;

            account.Image = filename;
            await _context.SaveChangesAsync();

            var cachedAccount = account.ToCachedAccount();
            await CacheAccount(cachedAccount, TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(4));
            return cachedAccount;
        }

        public async Task<List<Account>> GetAccountsAsync(IEnumerable<Guid> ids)
        {
            return await _context.Accounts.Where(e => ids.Contains(e.Id))
                .ToListAsync();
        }

        public async Task<CachedAccount?> GetAccountByEmailOrNicknameAsync(string identifier)
        {
            var cachedAccount = await GetFromCacheAsync<CachedAccount>($"{_cacheAccountKeyPrefix}:{identifier}");
            if (cachedAccount != null)
                return cachedAccount;

            var account = await _context.Accounts.FirstOrDefaultAsync(e => e.Email == identifier || e.Nickname == identifier);
            if (account == null)
                return null;

            cachedAccount = account.ToCachedAccount();
            await CacheAccount(cachedAccount, TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(4));
            return cachedAccount;
        }

        public async Task<CachedAccount?> GetAccountByNicknameAsync(string nickname)
        {
            var cachedAccount = await GetFromCacheAsync<CachedAccount>($"{_cacheAccountKeyPrefix}:{nickname}");
            if (cachedAccount != null)
                return cachedAccount;

            var account = await _context.Accounts.FirstOrDefaultAsync(e => e.Nickname == nickname);
            if (account == null)
                return null;

            cachedAccount = account.ToCachedAccount();
            await CacheAccount(cachedAccount, TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(4));
            return cachedAccount;
        }

        public async Task<List<CachedAccount>> GetAccountsByPatternNicknameAsync(string pattern, int limit, int offset)
        {
            var accounts = await _context.Accounts.Where(e => EF.Functions.Like(e.Nickname, $"%{pattern}%"))
                .ToListAsync();

            return accounts.Select(e => e.ToCachedAccount()).ToList();
        }

        public async Task<CachedAccount?> UpdateAccountNicknameAsync(Guid accountId, string nickname)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(e => e.Id == accountId);
            if (account == null)
                return null;

            account.Nickname = nickname;
            await _context.SaveChangesAsync();

            var cachedAccount = account.ToCachedAccount();
            await CacheAccount(cachedAccount, TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(4));
            return cachedAccount;
        }


        private async Task CacheAccount(CachedAccount cachedAccount, TimeSpan slidingExpiration, TimeSpan absoluteExpiration)
        {

            var indexes = new string[] { cachedAccount.Email, cachedAccount.Nickname };
            await _cacheService.SetIndexedKeyAsync(_cacheAccountKeyPrefix, cachedAccount.Id.ToString(), indexes, cachedAccount, slidingExpiration, absoluteExpiration);
        }

        private async Task<T?> GetFromCacheAsync<T>(string key)
        {
            var cachedEntity = await _cacheService.GetStringAsync(key);
            if (cachedEntity == null)
                return default;

            var partsKey = cachedEntity.Split(':');
            T? entity;

            if (partsKey.Length == 2)
            {
                entity = await _cacheService.GetAsync<T>(cachedEntity);
                _logger.LogInformation($"Получено из индекса: {cachedEntity} со значением {JsonSerializer.Serialize(entity)}");
            }
            else
                entity = JsonSerializer.Deserialize<T>(cachedEntity);
            return entity;
        }

        public async Task<CachedAccount?> AccountAuthAsync(string email, string passwordHash)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(e => e.Email == email && e.PasswordHash == passwordHash);
            if (account == null)
                return null;

            return account.ToCachedAccount();
        }
    }
}