using System.Net;
using KVATUM_CHATFLOW_SERVICE.Core.Entities.Request;
using KVATUM_CHATFLOW_SERVICE.Core.Entities.Response;
using KVATUM_CHATFLOW_SERVICE.Core.IRepository;
using KVATUM_CHATFLOW_SERVICE.Core.IService;

namespace KVATUM_CHATFLOW_SERVICE.App.Service
{
    public class WorkspaceService : IWorkspaceService
    {
        private readonly IWorkspaceRepository _workspaceRepository;
        private readonly IHubRepository _hubRepository;

        public WorkspaceService(
            IWorkspaceRepository workspaceRepository,
            IHubRepository hubRepository)
        {
            _workspaceRepository = workspaceRepository;
            _hubRepository = hubRepository;
        }

        public async Task<ServiceResponse<WorkspaceBody>> AddWorkspaceAsync(CreateWorkspaceBody body, Guid hubId, Guid accountId)
        {
            var hub = await _hubRepository.GetHubByIdAsync(body.HubId);
            if (hub == null)
                return new ServiceResponse<WorkspaceBody>
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    Errors = new[] { "HubId is not exists" },
                };

            if (accountId != hub.CreatorId)
                return new ServiceResponse<WorkspaceBody>
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.Forbidden,
                    Errors = new[] { "Not creator of hub" },
                };

            var workspace = await _workspaceRepository.AddWorkspaceAsync(body.Name, hub);
            if (workspace == null)
                return new ServiceResponse<WorkspaceBody>
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    Errors = new[] { "Workspace is not created" },
                };

            return new ServiceResponse<WorkspaceBody>
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Body = workspace.ToWorkspaceBody(),
            };
        }

        public async Task<ServiceResponse<bool>> DeleteWorkspaceAsync(Guid workspaceId, Guid accountId)
        {
            var workspace = await _workspaceRepository.GetWorkspaceAsync(workspaceId);
            if (workspace == null)
                return new ServiceResponse<bool>
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    Errors = new[] { "WorkspaceId is not exists" },
                };

            var hub = await _hubRepository.GetHubByIdAsync(workspace.HubId);
            if (hub == null)
                return new ServiceResponse<bool>
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    Errors = new[] { "HubId is not exists" },
                };

            if (accountId != hub.CreatorId)
                return new ServiceResponse<bool>
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.Forbidden,
                    Errors = new[] { "Not creator of hub" },
                };

            await _workspaceRepository.DeleteWorkspaceAsync(workspaceId);
            return new ServiceResponse<bool>
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.NoContent,
            };
        }

        public async Task<ServiceResponse<IEnumerable<WorkspaceBody>>> GetWorkspacesByHubIdAsync(Guid hubId, int limit, int offset)
        {
            var workspaces = await _workspaceRepository.GetWorkspacesAsync(hubId, limit, offset);
            return new ServiceResponse<IEnumerable<WorkspaceBody>>
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Body = workspaces.Select(e => e.ToWorkspaceBody()),
            };
        }
    }
}