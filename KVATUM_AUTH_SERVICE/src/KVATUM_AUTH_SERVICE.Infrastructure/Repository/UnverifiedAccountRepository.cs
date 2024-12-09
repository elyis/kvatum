using Microsoft.EntityFrameworkCore;
using KVATUM_AUTH_SERVICE.Core.Entities.Models;
using KVATUM_AUTH_SERVICE.Core.Entities.Request;
using KVATUM_AUTH_SERVICE.Core.IRepository;
using KVATUM_AUTH_SERVICE.Infrastructure.Data;
using KVATUM_AUTH_SERVICE.Core.IService;

namespace KVATUM_AUTH_SERVICE.Infrastructure.Repository
{
    public class UnverifiedAccountRepository : IUnverifiedAccountRepository
    {
        private readonly AuthDbContext _context;
        private readonly ICacheService _cacheService;

        private readonly string _cacheUnverifiedAccountKeyPrefix = "unverified-account:";

        public UnverifiedAccountRepository(
            AuthDbContext context,
            ICacheService cacheService)
        {
            _context = context;
            _cacheService = cacheService;
        }

        public async Task<UnverifiedAccount?> AddAsync(SignUpBody body, string verificationCode)
        {
            var unverifiedAccount = await GetAsync(body.Email);
            if (unverifiedAccount != null)
                return null;

            unverifiedAccount = new UnverifiedAccount
            {
                Email = body.Email,
                Nickname = body.Nickname,
                Password = body.Password,
                VerificationCode = verificationCode
            };

            await _context.UnverifiedAccounts.AddAsync(unverifiedAccount);
            await _context.SaveChangesAsync();
            await CacheUnverifiedAccount(unverifiedAccount);
            return unverifiedAccount;
        }

        public async Task<bool> DeleteAsync(string email)
        {
            var account = await GetAsync(email);
            if (account == null)
                return true;

            _context.UnverifiedAccounts.Remove(account);
            await _context.SaveChangesAsync();
            await RemoveCachedUnverifiedAccount(email);
            return true;
        }

        public async Task<UnverifiedAccount?> GetAsync(string email)
        {
            var cachedAccount = await GetCachedUnverifiedAccount(email);
            if (cachedAccount != null)
            {
                AttachUnverifiedAccount(cachedAccount);
                return cachedAccount;
            }

            var account = await _context.UnverifiedAccounts.FirstOrDefaultAsync(u => u.Email == email);
            if (account != null)
                await CacheUnverifiedAccount(account);

            return account;
        }

        private async Task CacheUnverifiedAccount(UnverifiedAccount account)
        {
            var key = $"{_cacheUnverifiedAccountKeyPrefix}{account.Email}";
            await _cacheService.SetAsync(key, account, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(10));
        }

        private async Task<UnverifiedAccount?> GetCachedUnverifiedAccount(string email)
        {
            var key = $"{_cacheUnverifiedAccountKeyPrefix}{email}";
            return await _cacheService.GetAsync<UnverifiedAccount>(key);
        }

        private void AttachUnverifiedAccount(UnverifiedAccount account)
        {
            if (!_context.UnverifiedAccounts.Local.Any(e => e.Id == account.Id))
                _context.UnverifiedAccounts.Attach(account);
        }

        private async Task RemoveCachedUnverifiedAccount(string email)
        {
            var key = $"{_cacheUnverifiedAccountKeyPrefix}{email}";
            await _cacheService.RemoveAsync(key);
        }
    }
}