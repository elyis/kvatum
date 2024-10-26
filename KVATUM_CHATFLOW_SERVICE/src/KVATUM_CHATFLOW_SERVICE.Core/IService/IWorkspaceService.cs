using KVATUM_CHATFLOW_SERVICE.Core.Entities.Request;
using KVATUM_CHATFLOW_SERVICE.Core.Entities.Response;

namespace KVATUM_CHATFLOW_SERVICE.Core.IService
{
    public interface IWorkspaceService
    {
        Task<ServiceResponse<IEnumerable<WorkspaceBody>>> GetWorkspacesByHubIdAsync(Guid hubId, int limit, int offset);
        Task<ServiceResponse<WorkspaceBody>> AddWorkspaceAsync(CreateWorkspaceBody body, Guid hubId, Guid accountId);
        Task<ServiceResponse<bool>> DeleteWorkspaceAsync(Guid workspaceId, Guid accountId);
    }
}