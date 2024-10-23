using KVATUM_CHATFLOW_SERVICE.Core.Entities.Models;
using KVATUM_CHATFLOW_SERVICE.Core.Entities.Request;

namespace KVATUM_CHATFLOW_SERVICE.Core.IRepository
{
    public interface IHubRepository
    {
        Task<HubJoiningInvitation?> GetHubJoiningInvitationByHubIdAsync(Guid hubId);
        Task<HubJoiningInvitation?> GetHubJoiningInvitationByHashAsync(string hash);
        Task<HubJoiningInvitation?> AddHubJoiningInvitationAsync(Hub hub, string hashInvitation);
        Task<Hub?> GetHubByIdAsync(Guid id);
        Task<List<Hub>> GetHubsByConnectedAccountIdAsync(Guid connectedAccountId, int limit, int offset);
        Task<Hub?> AddHubAsync(CreateHubBody body, Guid creatorId, string hashInvitation);
        Task<HubMember?> AddMemberToHubAsync(Guid hubId, Guid memberId);
        Task<bool> RemoveMemberFromHubAsync(Guid hubId, Guid memberId);
        Task<Hub?> UpdateHubAsync(Guid id, string name);
        Task<Hub?> UpdateHubIconAsync(Guid id, string fileName);
        Task<bool> DeleteHubAsync(Guid id);
    }
}