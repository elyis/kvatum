using Microsoft.EntityFrameworkCore;
using KVATUM_AUTH_SERVICE.Core.Entities.Models;
using KVATUM_AUTH_SERVICE.Core.IRepository;
using KVATUM_AUTH_SERVICE.Infrastructure.Data;

namespace KVATUM_AUTH_SERVICE.Infrastructure.Repository
{
    public class AccountRepository : IAccountRepository
    {
        private readonly AuthDbContext _context;

        public AccountRepository(AuthDbContext context)
        {
            _context = context;
        }

        public async Task<Account?> AddAsync(
            string identifier,
            string nickname,
            string password,
            string role)
        {
            var account = await GetAsync(identifier);
            if (account != null)
                return null;

            account = new Account
            {
                Email = identifier,
                Nickname = nickname,
                PasswordHash = password,
                Role = role,
            };

            var result = await _context.Accounts.AddAsync(account);
            await _context.SaveChangesAsync();
            return result?.Entity;
        }

        public async Task<AccountSession?> GetSessionAsync(Guid accountId, string userAgent, string ipAddress)
        {
            return await _context.AccountSessions
                .FirstOrDefaultAsync(e => e.AccountId == accountId
                                          && e.UserAgent == userAgent
                                          && e.Ip == ipAddress);
        }

        public async Task<List<AccountSession>> GetSessionsAsync(Guid accountId)
        {
            return await _context.AccountSessions
                    .Where(e => e.AccountId == accountId)
                .ToListAsync();
        }

        public async Task<AccountSession> GetOrAddSessionAsync(string userAgent, string ipAddress, Account account)
        {
            var session = await GetSessionAsync(account.Id, userAgent, ipAddress);
            if (session != null)
                return session;

            session = new AccountSession
            {
                Account = account,
                UserAgent = userAgent,
                Ip = ipAddress
            };

            var result = await _context.AccountSessions.AddAsync(session);
            await _context.SaveChangesAsync();

            return result?.Entity;
        }


        public async Task<List<Account>> GetAccountsAsync(List<string> identifiers)
        {
            var result = await _context.Accounts.Where(e => identifiers.Contains(e.Email))
                .ToListAsync();
            return result;
        }

        public async Task<Account?> GetAsync(Guid id)
            => await _context.Accounts
                .FirstOrDefaultAsync(e => e.Id == id);

        public async Task<Account?> GetAsync(string identifier)
        {
            return await _context.Accounts
                .FirstOrDefaultAsync(e => e.Email == identifier);
        }

        public async Task<AccountSession?> GetSessionByTokenAndAccount(string refreshTokenHash)
            => await _context.AccountSessions
            .Include(e => e.Account)
            .FirstOrDefaultAsync(e => e.Token == refreshTokenHash);

        public async Task<List<Account>> GetAccountsByPatternIdentifier(string identifier)
        {
            return await _context.Accounts
                .Where(e => EF.Functions.Like(e.Email, $"%{identifier}%"))
                .ToListAsync();
        }


        public async Task<Account?> UpdateProfileIconAsync(Guid accountId, string filename)
        {
            var account = await GetAsync(accountId);
            if (account == null)
                return null;

            account.Image = filename;
            await _context.SaveChangesAsync();
            return account;
        }

        public async Task<string?> UpdateTokenAsync(string refreshToken, Guid sessionId, TimeSpan? duration = null)
        {
            var session = await GetSessionAsync(sessionId);
            if (session == null)
                return null;

            if (duration == null)
                duration = TimeSpan.FromDays(1);

            if (session.TokenValidBefore <= DateTime.UtcNow || session.TokenValidBefore == null)
            {
                session.TokenValidBefore = DateTime.UtcNow.Add((TimeSpan)duration);
                session.Token = refreshToken;
                await _context.SaveChangesAsync();
            }

            return session.Token;
        }

        public async Task<AccountSession?> GetSessionAsync(Guid sessionId)
        {
            return await _context.AccountSessions.FirstOrDefaultAsync(e => e.Id == sessionId);
        }

        public async Task<List<Account>> GetAccountsAsync(IEnumerable<Guid> ids)
        {
            return await _context.Accounts.Where(e => ids.Contains(e.Id))
                .ToListAsync();
        }

        public async Task<Account?> GetAccountByEmailOrNicknameAsync(string identifier)
        {
            return await _context.Accounts.FirstOrDefaultAsync(e => e.Email == identifier || e.Nickname == identifier);
        }

        public async Task<Account?> GetAccountByNicknameAsync(string nickname)
        {
            return await _context.Accounts.FirstOrDefaultAsync(e => e.Nickname == nickname);
        }

        public async Task<List<Account>> GetAccountsByPatternNicknameAsync(string pattern, int limit, int offset)
        {
            return await _context.Accounts.Where(e => EF.Functions.Like(e.Nickname, $"%{pattern}%"))
                .ToListAsync();
        }

        public async Task<Account?> UpdateAccountNicknameAsync(Guid accountId, string nickname)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(e => e.Id == accountId);
            if (account == null)
                return null;

            account.Nickname = nickname;
            await _context.SaveChangesAsync();
            return account;
        }
    }
}