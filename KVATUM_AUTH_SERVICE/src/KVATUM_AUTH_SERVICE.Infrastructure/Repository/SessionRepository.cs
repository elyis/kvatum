using System.Text.Json;
using KVATUM_AUTH_SERVICE.Core.Entities.Cache;
using KVATUM_AUTH_SERVICE.Core.Entities.Models;
using KVATUM_AUTH_SERVICE.Core.IRepository;
using KVATUM_AUTH_SERVICE.Core.IService;
using KVATUM_AUTH_SERVICE.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KVATUM_AUTH_SERVICE.Infrastructure.Repository
{
    public class SessionRepository : ISessionRepository
    {
        private readonly AuthDbContext _context;
        private readonly string _cacheAccountSessionKeyPrefix = "session";
        private readonly ICacheService _cacheService;

        public SessionRepository(AuthDbContext context, ICacheService cacheService)
        {
            _context = context;
            _cacheService = cacheService;
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

        public async Task<CachedAccountSession?> GetSessionByTokenAndAccount(string refreshTokenHash)
        {
            var cachedSession = await GetFromCacheAsync<CachedAccountSession>($"{_cacheAccountSessionKeyPrefix}:{refreshTokenHash}");
            if (cachedSession != null)
            {
                var trackedEntity = _context.AccountSessions.Local.FirstOrDefault(e => e.Id == cachedSession.Id);
                if (trackedEntity == null)
                {
                    AttachAccountSession(cachedSession);
                }

                return cachedSession;
            }

            var session = await _context.AccountSessions
                    .Include(e => e.Account)
                    .FirstOrDefaultAsync(e => e.Token == refreshTokenHash);
            if (session != null)
            {
                cachedSession = session.ToCachedAccountSession();
                await CacheAccountSession(cachedSession, TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(4));
                return cachedSession;
            }

            return null;
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
            var cachedAccountSession = await GetFromCacheAsync<CachedAccountSession>($"{_cacheAccountSessionKeyPrefix}:{sessionId}");
            if (cachedAccountSession != null)
            {
                var trackedEntity = _context.AccountSessions.Local.FirstOrDefault(e => e.Id == cachedAccountSession.Id);
                if (trackedEntity == null)
                {
                    AttachAccountSession(cachedAccountSession);
                }

                return cachedAccountSession;
            }

            var session = await _context.AccountSessions.FirstOrDefaultAsync(e => e.Id == sessionId);
            if (session == null)
                return null;

            cachedAccountSession = session.ToCachedAccountSession();
            await CacheAccountSession(cachedAccountSession, TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(4));
            return cachedAccountSession;
        }

        private async Task<T?> GetFromCacheAsync<T>(string key)
        {
            var cachedEntity = await _cacheService.GetStringAsync(key);
            if (cachedEntity == null)
                return default;

            var partsKey = cachedEntity.Split(':');
            T? entity;

            if (partsKey.Length == 2)
                entity = await _cacheService.GetAsync<T>(cachedEntity);
            else
                entity = JsonSerializer.Deserialize<T>(cachedEntity);
            return entity;
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

        private async Task CacheAccountSession(CachedAccountSession cachedAccountSession, TimeSpan slidingExpiration, TimeSpan absoluteExpiration)
        {
            var indexes = new List<string> { };
            if (cachedAccountSession.Token != null)
                indexes.Add(cachedAccountSession.Token);

            await _cacheService.SetIndexedKeyAsync(_cacheAccountSessionKeyPrefix, cachedAccountSession.Id.ToString(), indexes.ToArray(), cachedAccountSession, slidingExpiration, absoluteExpiration);
        }

        private void AttachAccountSession(CachedAccountSession cachedAccountSession)
        {
            var trackedEntity = _context.AccountSessions.Local.FirstOrDefault(e => e.Id == cachedAccountSession.Id);

            var accountSession = cachedAccountSession.ToAccountSession();
            if (trackedEntity != null)
                _context.Entry(trackedEntity).CurrentValues.SetValues(accountSession);
            else
                _context.AccountSessions.Attach(accountSession);
        }

        private async Task RemoveFromCacheAsync(Guid sessionId, string? token)
        {
            await _cacheService.RemoveAsync($"{_cacheAccountSessionKeyPrefix}:{sessionId}");
            if (token != null)
                await _cacheService.RemoveAsync($"{_cacheAccountSessionKeyPrefix}:{token}");
        }
    }
}