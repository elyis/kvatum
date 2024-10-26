using KVATUM_CHATFLOW_SERVICE.Core.Entities.Request;
using KVATUM_CHATFLOW_SERVICE.Core.Entities.Response;

namespace KVATUM_CHATFLOW_SERVICE.Core.IService
{
    public interface IHubService
    {
        Task<ServiceResponse<HubBody>> CreateHubAsync(CreateHubBody body, Guid creatorId);
        Task<ServiceResponse<List<HubBody>>> GetConnectedHubsAsync(Guid connectedAccountId, int limit, int offset);
        Task<ServiceResponse<HubInvitationLinkBody>> GetHubJoiningInvitationByHubIdAsync(Guid hubId);
        Task<ServiceResponse<bool>> AddMemberToHubAsync(Guid memberId, string hash);
    }
}