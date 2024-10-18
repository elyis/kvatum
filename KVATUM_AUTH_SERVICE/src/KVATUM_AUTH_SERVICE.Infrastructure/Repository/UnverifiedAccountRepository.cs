using Microsoft.EntityFrameworkCore;
using KVATUM_AUTH_SERVICE.Core.Entities.Models;
using KVATUM_AUTH_SERVICE.Core.Entities.Request;
using KVATUM_AUTH_SERVICE.Core.IRepository;
using KVATUM_AUTH_SERVICE.Infrastructure.Data;

namespace KVATUM_AUTH_SERVICE.Infrastructure.Repository
{
    public class UnverifiedAccountRepository : IUnverifiedAccountRepository
    {
        private readonly AuthDbContext _context;

        public UnverifiedAccountRepository(AuthDbContext context)
        {
            _context = context;
        }

        public async Task<UnverifiedAccount?> AddAsync(SignUpBody body, string verificationCode)
        {
            var unverifiedAccount = await GetAsync(body.Email);
            if (unverifiedAccount != null)
                return null;

            var account = await _context.Accounts.FirstOrDefaultAsync(e => e.Email == body.Email);
            if (account != null)
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
            return unverifiedAccount;
        }

        public async Task<bool> DeleteAsync(string email)
        {
            var account = await GetAsync(email);
            if (account == null)
                return true;

            _context.UnverifiedAccounts.Remove(account);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<UnverifiedAccount?> GetAsync(string email) => await _context.UnverifiedAccounts.FirstOrDefaultAsync(u => u.Email == email);

        public async Task<bool> VerifyAsync(string email, string verificationCode)
        {
            var account = await GetAsync(email);
            if (account == null)
                return false;

            return account.VerificationCode == verificationCode;
        }
    }
}