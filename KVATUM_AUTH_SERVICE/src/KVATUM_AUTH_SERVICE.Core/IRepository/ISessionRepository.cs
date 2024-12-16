using KVATUM_AUTH_SERVICE.Core.Entities.Cache;
using KVATUM_AUTH_SERVICE.Core.Entities.Models;

namespace KVATUM_AUTH_SERVICE.Core.IRepository
{
    public interface ISessionRepository
    {
        Task<CachedAccountSession?> GetSessionByToken(string refreshTokenHash);
        Task<AccountSession?> GetSessionAsync(Guid accountId, string userAgent, string ipAddress);
        Task<List<AccountSession>> GetSessionsAsync(Guid accountId, int limit, int offset);
        Task<CachedAccountSession?> GetOrAddSessionAsync(string userAgent, string ipAddress, Guid accountId);
        Task<bool> RemoveSessionAsync(Guid sessionId);
        Task<string?> UpdateTokenAsync(string refreshToken, Guid sessionId, TimeSpan? duration = null);
        Task<CachedAccountSession?> GetSessionAsync(Guid sessionId);
    }
}