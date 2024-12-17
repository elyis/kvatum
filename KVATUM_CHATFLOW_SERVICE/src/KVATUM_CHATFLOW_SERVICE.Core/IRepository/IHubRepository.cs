using KVATUM_CHATFLOW_SERVICE.Core.Entities.Cache;
using KVATUM_CHATFLOW_SERVICE.Core.Entities.Models;
using KVATUM_CHATFLOW_SERVICE.Core.Entities.Request;

namespace KVATUM_CHATFLOW_SERVICE.Core.IRepository
{
    public interface IHubRepository
    {
        Task<CachedJoiningInvitation?> GetHubJoiningInvitationByHubIdAsync(Guid hubId);
        Task<CachedJoiningInvitation?> GetHubJoiningInvitationByHashAsync(string hash);
        Task<HubJoiningInvitation?> AddHubJoiningInvitationAsync(Hub hub, string hashInvitation);
        Task<CachedHub?> GetHubByIdAsync(Guid id);
        Task<List<Hub>> GetHubsByConnectedAccountIdAsync(Guid connectedAccountId, int limit, int offset);
        Task<Hub?> AddHubAsync(CreateHubBody body, Guid creatorId, string hashInvitation, string hexColor);
        Task<HubMember?> AddMemberToHubAsync(Guid hubId, Guid memberId);
        Task<bool> RemoveMemberFromHubAsync(Guid hubId, Guid memberId);
        Task<CachedHub?> UpdateHubNameAsync(Guid id, string name);
        Task<CachedHub?> UpdateHubIconAsync(Guid id, string fileName);
        Task<bool> DeleteHubAsync(Guid id);
    }
}