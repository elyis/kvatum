using System.Net;
using KVATUM_AUTH_SERVICE.Core.Entities.Response;

namespace KVATUM_AUTH_SERVICE.Core.IService
{
    public interface IAccountSessionService
    {
        Task<ServiceResponse<List<AccountSessionBody>>> GetAllAccountSessionsAsync(Guid accountId, int limit, int offset);
        Task<HttpStatusCode> RemoveSessionAsync(Guid sessionId, Guid accountId);
    }
}