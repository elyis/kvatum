using KVATUM_AUTH_SERVICE.Core.Entities.Cache;
using KVATUM_AUTH_SERVICE.Core.Entities.Models;

namespace KVATUM_AUTH_SERVICE.Core.IRepository
{
    public interface IAccountRepository
    {
        Task<CachedAccount?> AddAsync(string email, string nickname, string password, string role);
        Task<CachedAccount?> AccountAuthAsync(string email, string passwordHash);
        Task<CachedAccount?> GetAccountByEmailOrNicknameAsync(string identifier);
        Task<CachedAccount?> UpdateAccountNicknameAsync(Guid accountId, string nickname);
        Task<CachedAccount?> GetAccountByNicknameAsync(string nickname);
        Task<List<CachedAccount>> GetAccountsByPatternNicknameAsync(string pattern, int limit, int offset);
        Task<CachedAccount?> GetAsync(Guid id);
        Task<List<Account>> GetAccountsAsync(List<string> emails);
        Task<List<Account>> GetAccountsAsync(IEnumerable<Guid> ids);
        Task<CachedAccount?> GetAsync(string email);
        Task<CachedAccount?> UpdateProfileIconAsync(Guid accountId, string filename);
    }
}