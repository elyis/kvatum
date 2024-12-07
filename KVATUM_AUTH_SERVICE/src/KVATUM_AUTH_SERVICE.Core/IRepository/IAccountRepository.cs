using KVATUM_AUTH_SERVICE.Core.Entities.Cache;
using KVATUM_AUTH_SERVICE.Core.Entities.Models;

namespace KVATUM_AUTH_SERVICE.Core.IRepository
{
    public interface IAccountRepository
    {
        Task<CachedAccount?> AddAsync(string email, string nickname, string password, string role);
        Task<CachedAccount?> AccountAuthAsync(string email, string passwordHash);
        Task<AccountSession?> GetSessionAsync(Guid accountId, string userAgent, string ipAddress);
        Task<CachedAccountSession?> GetSessionAsync(Guid sessionId);
        Task<CachedAccountSession?> GetOrAddSessionAsync(string userAgent, string ipAddress, Guid accountId);
        Task<List<AccountSession>> GetSessionsAsync(Guid accountId, int limit, int offset);
        Task<CachedAccount?> GetAccountByEmailOrNicknameAsync(string identifier);
        Task<CachedAccount?> UpdateAccountNicknameAsync(Guid accountId, string nickname);
        Task<CachedAccount?> GetAccountByNicknameAsync(string nickname);
        Task<List<CachedAccount>> GetAccountsByPatternNicknameAsync(string pattern, int limit, int offset);
        Task<CachedAccount?> GetAsync(Guid id);
        Task<bool> RemoveSessionAsync(Guid sessionId);
        Task<List<Account>> GetAccountsAsync(List<string> emails);
        Task<List<Account>> GetAccountsAsync(IEnumerable<Guid> ids);
        Task<CachedAccount?> GetAsync(string email);
        Task<string?> UpdateTokenAsync(string refreshToken, Guid sessionId, TimeSpan? duration = null);
        Task<AccountSession?> GetSessionByTokenAndAccount(string refreshTokenHash);
        Task<CachedAccount?> UpdateProfileIconAsync(Guid accountId, string filename);
    }
}