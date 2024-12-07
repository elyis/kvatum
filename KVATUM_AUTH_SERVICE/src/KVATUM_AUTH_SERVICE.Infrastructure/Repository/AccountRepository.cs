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
        private readonly string _cacheAccountKeyPrefix = "account:";
        private readonly string _cacheAccountSessionKeyPrefix = "session:";
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

        public async Task<AccountSession?> GetSessionAsync(Guid accountId, string userAgent, string ipAddress)
        {
            return await _context.AccountSessions
                .FirstOrDefaultAsync(e => e.AccountId == accountId
                                          && e.UserAgent == userAgent
                                          && e.Ip == ipAddress);
        }

        public async Task<List<AccountSession>> GetSessionsAsync(Guid accountId, int limit, int offset)
        {
            return await _context.AccountSessions
                    .Where(e => e.AccountId == accountId)
                    .Skip(offset)
                    .Take(limit)
                .ToListAsync();
        }

        public async Task<CachedAccountSession?> GetOrAddSessionAsync(string userAgent, string ipAddress, Guid accountId)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(e => e.Id == accountId);
            if (account == null)
                return null;

            var session = await GetSessionAsync(accountId, userAgent, ipAddress);
            if (session != null)
                return session.ToCachedAccountSession();

            session = new AccountSession
            {
                Account = account,
                UserAgent = userAgent,
                Ip = ipAddress
            };

            var result = (await _context.AccountSessions.AddAsync(session))?.Entity;
            await _context.SaveChangesAsync();

            var cachedSession = result?.ToCachedAccountSession();
            if (cachedSession == null)
                return null;

            await CacheAccountSession(cachedSession, TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(4));
            return cachedSession;
        }


        public async Task<List<Account>> GetAccountsAsync(List<string> identifiers)
        {
            var result = await _context.Accounts.Where(e => identifiers.Contains(e.Email))
                .ToListAsync();
            return result;
        }

        public async Task<CachedAccount?> GetAsync(Guid id)
        {
            var cachedAccount = await GetFromCacheAsync<CachedAccount>($"{_cacheAccountKeyPrefix}{id}");
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
            var cachedAccount = await GetFromCacheAsync<CachedAccount>($"{_cacheAccountKeyPrefix}{email}");
            if (cachedAccount != null)
                return cachedAccount;

            var account = await _context.Accounts.FirstOrDefaultAsync(e => e.Email == email);
            if (account == null)
                return null;

            cachedAccount = account.ToCachedAccount();
            await CacheAccount(cachedAccount, TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(4));
            return cachedAccount;
        }

        public async Task<AccountSession?> GetSessionByTokenAndAccount(string refreshTokenHash)
        {
            return await _context.AccountSessions
                    .Include(e => e.Account)
                    .FirstOrDefaultAsync(e => e.Token == refreshTokenHash);
        }

        public async Task<List<Account>> GetAccountsByPatternIdentifier(string identifier)
        {
            return await _context.Accounts
                .Where(e => EF.Functions.Like(e.Email, $"%{identifier}%"))
                .ToListAsync();
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

        public async Task<string?> UpdateTokenAsync(string refreshToken, Guid sessionId, TimeSpan? duration = null)
        {
            var session = await _context.AccountSessions.FirstOrDefaultAsync(e => e.Id == sessionId);
            if (session == null)
                return null;

            if (duration == null)
                duration = TimeSpan.FromDays(1);

            if (session.TokenValidBefore == null || session.TokenValidBefore <= DateTime.UtcNow)
            {
                session.TokenValidBefore = DateTime.UtcNow.Add((TimeSpan)duration);
                session.Token = refreshToken;
                await _context.SaveChangesAsync();

                await CacheAccountSession(session.ToCachedAccountSession(), TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(4));
            }

            return session.Token;
        }

        public async Task<CachedAccountSession?> GetSessionAsync(Guid sessionId)
        {
            var cachedAccountSession = await GetFromCacheAsync<CachedAccountSession>($"{_cacheAccountSessionKeyPrefix}{sessionId}");
            if (cachedAccountSession != null)
                return cachedAccountSession;

            var session = await _context.AccountSessions.FirstOrDefaultAsync(e => e.Id == sessionId);
            if (session == null)
                return null;

            cachedAccountSession = session.ToCachedAccountSession();
            await CacheAccountSession(cachedAccountSession, TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(4));
            return cachedAccountSession;
        }

        public async Task<List<Account>> GetAccountsAsync(IEnumerable<Guid> ids)
        {
            return await _context.Accounts.Where(e => ids.Contains(e.Id))
                .ToListAsync();
        }

        public async Task<CachedAccount?> GetAccountByEmailOrNicknameAsync(string identifier)
        {
            var cachedAccount = await GetFromCacheAsync<CachedAccount>($"{_cacheAccountKeyPrefix}{identifier}");
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
            var cachedAccount = await GetFromCacheAsync<CachedAccount>($"{_cacheAccountKeyPrefix}{nickname}");
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

        public async Task<bool> RemoveSessionAsync(Guid sessionId)
        {
            var session = await _context.AccountSessions.FirstOrDefaultAsync(e => e.Id == sessionId);
            if (session == null)
                return true;

            _context.AccountSessions.Remove(session);
            await _context.SaveChangesAsync();

            await RemoveFromCacheAsync(sessionId, session.Token);
            return true;
        }

        private async Task CacheAccount(CachedAccount cachedAccount, TimeSpan slidingExpiration, TimeSpan absoluteExpiration)
        {
            var mainKey = $"{_cacheAccountKeyPrefix}{cachedAccount.Id}";
            await _cacheService.CacheSetAsync(mainKey, cachedAccount, slidingExpiration, absoluteExpiration);
            await _cacheService.CacheSetAsync($"{_cacheAccountKeyPrefix}{cachedAccount.Email}", mainKey, slidingExpiration, absoluteExpiration);
            await _cacheService.CacheSetAsync($"{_cacheAccountKeyPrefix}{cachedAccount.Nickname}", mainKey, slidingExpiration, absoluteExpiration);
        }

        private async Task CacheAccountSession(CachedAccountSession cachedAccountSession, TimeSpan slidingExpiration, TimeSpan absoluteExpiration)
        {
            var mainKey = $"{_cacheAccountSessionKeyPrefix}{cachedAccountSession.Id}";
            await _cacheService.CacheSetAsync(mainKey, cachedAccountSession, slidingExpiration, absoluteExpiration);

            if (cachedAccountSession.Token != null)
                await _cacheService.CacheSetAsync($"{_cacheAccountSessionKeyPrefix}{cachedAccountSession.Token}", mainKey, slidingExpiration, absoluteExpiration);
        }

        private async Task<T?> GetFromCacheAsync<T>(string key)
        {
            var cachedEntity = await _cacheService.GetResponseCacheAsync(key);
            if (cachedEntity == null)
                return default;

            _logger.LogInformation($"Cache value is ${cachedEntity.Value}");

            if (cachedEntity.IsIndexKey)
            {
                var entity = await _cacheService.GetFromCacheAsync<T>(cachedEntity.Value);
                return entity;
            }

            return JsonSerializer.Deserialize<T>(cachedEntity.Value);
        }

        private async Task RemoveFromCacheAsync(Guid sessionId, string? token)
        {
            await _cacheService.RemoveCacheAsync($"{_cacheAccountSessionKeyPrefix}{sessionId}");
            if (token != null)
                await _cacheService.RemoveCacheAsync($"{_cacheAccountSessionKeyPrefix}{token}");
        }

        private void AttachAccountEntityIfNotAttached(Account account)
        {
            if (!_context.Accounts.Local.Any(e => e.Id == account.Id))
                _context.Accounts.Attach(account);
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