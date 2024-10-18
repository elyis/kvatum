using KVATUM_AUTH_SERVICE.Core.Entities.Models;
using KVATUM_AUTH_SERVICE.Core.Entities.Request;

namespace KVATUM_AUTH_SERVICE.Core.IRepository
{
    public interface IAccountRepository
    {
        Task<Account?> AddAsync(string email, string nickname, string password, string role);
        Task<AccountSession?> GetSessionAsync(Guid accountId, UserAgentDetails userAgentDetails, string ipAddress);
        Task<AccountSession?> GetSessionAsync(Guid sessionId);
        Task<AccountSession> GetOrAddSessionAsync(UserAgentDetails userAgentDetails, string ipAddress, Account account);
        Task<List<AccountSession>> GetSessionsAsync(Guid accountId);
        Task<Account?> GetAsync(Guid id);
        Task<List<Account>> GetAccountsAsync(List<string> identifiers);
        Task<List<Account>> GetAccountsAsync(IEnumerable<Guid> ids);
        Task<Account?> GetAsync(string identifier);
        Task<string?> UpdateTokenAsync(string refreshToken, Guid sessionId, TimeSpan? duration = null);
        Task<AccountSession?> GetSessionByTokenAndAccount(string refreshTokenHash);
        Task<Account?> UpdateProfileIconAsync(Guid accountId, string filename);
    }
}