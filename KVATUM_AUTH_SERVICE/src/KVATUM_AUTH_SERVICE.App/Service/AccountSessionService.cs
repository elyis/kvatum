using System.Net;
using KVATUM_AUTH_SERVICE.Core.Entities.Response;
using KVATUM_AUTH_SERVICE.Core.IRepository;
using KVATUM_AUTH_SERVICE.Core.IService;

namespace KVATUM_AUTH_SERVICE.App.Service
{
    public class AccountSessionService : IAccountSessionService
    {
        private readonly ISessionRepository _sessionRepository;

        public AccountSessionService(ISessionRepository sessionRepository)
        {
            _sessionRepository = sessionRepository;
        }

        public async Task<ServiceResponse<List<AccountSessionBody>>> GetAllAccountSessionsAsync(Guid accountId, int limit, int offset)
        {
            var sessions = await _sessionRepository.GetSessionsAsync(accountId, limit, offset);
            return new ServiceResponse<List<AccountSessionBody>>
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Body = sessions.Select(e => e.ToAccountSessionBody()).ToList()
            };
        }

        public async Task<HttpStatusCode> RemoveSessionAsync(Guid sessionId, Guid accountId)
        {
            var account = await _sessionRepository.GetSessionAsync(sessionId);
            if (account == null || account.AccountId != accountId)
                return HttpStatusCode.Forbidden;

            return await _sessionRepository.RemoveSessionAsync(sessionId) ? HttpStatusCode.NoContent : HttpStatusCode.InternalServerError;
        }
    }
}