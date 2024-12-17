using Microsoft.EntityFrameworkCore;
using KVATUM_CHATFLOW_SERVICE.Infrastructure.Data;
using KVATUM_CHATFLOW_SERVICE.Core.IRepository;
using KVATUM_CHATFLOW_SERVICE.Core.Entities.Models;
using KVATUM_CHATFLOW_SERVICE.Core.Entities.Request;
using KVATUM_CHATFLOW_SERVICE.Core.Enums;
using KVATUM_CHATFLOW_SERVICE.Core.IService;
using KVATUM_CHATFLOW_SERVICE.Core.Entities.Cache;

namespace KVATUM_CHATFLOW_SERVICE.Infrastructure.Repository
{
    public class HubRepository : IHubRepository
    {
        private readonly ServerFlowDbContext _context;
        private readonly ICacheService _cacheService;
        private readonly string _cacheHubKeyPrefix = "hub";
        private readonly string _cacheJoiningInvitationKeyPrefix = "hub-invitation";
        private readonly string _cacheHubMemberKeyPrefix = "hub-member";

        public HubRepository(
            ServerFlowDbContext context,
            ICacheService cacheService)
        {
            _context = context;
            _cacheService = cacheService;
        }

        public async Task<List<Hub>> GetHubsByConnectedAccountIdAsync(Guid connectedAccountId, int limit, int offset)
        {
            return await _context.HubMembers
                .Where(e => e.MemberId == connectedAccountId)
                .OrderBy(e => e.HubId)
                .Skip(offset)
                .Take(limit)
                .Include(e => e.Hub)
                .Select(e => e.Hub)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Hub?> AddHubAsync(CreateHubBody body, Guid creatorId, string hashInvitation, string hexColor)
        {
            var hub = new Hub
            {
                Name = body.Name,
                CreatorId = creatorId,
                Members = new List<HubMember> { new HubMember { MemberId = creatorId } },
                HubJoiningInvitation = new HubJoiningInvitation { HashInvitation = hashInvitation },
                HexColor = hexColor
            };

            var workspace = new Workspace
            {
                Name = body.Name,
                Hub = hub,
                HexColor = hexColor
            };

            var chats = new List<Chat>
            {
                new() {
                    Name = "Conference",
                    Type = ChatType.Conference.ToString()
                },

                new() {
                    Name = "Channel",
                    Type = ChatType.Channel.ToString()
                },

                new() {
                    Name = "Chat",
                    Type = ChatType.Chat.ToString()
                }
            };

            workspace.Chats = chats;

            await _context.Hubs.AddAsync(hub);
            await _context.Workspaces.AddAsync(workspace);
            await _context.Chats.AddRangeAsync(chats);
            await _context.SaveChangesAsync();

            await CacheHub(hub);
            return hub;
        }

        private async Task<CachedHubMember?> GetHubMemberAsync(Guid hubId, Guid memberId)
        {
            var cachedHubMember = await GetCachedHubMember(hubId, memberId);
            if (cachedHubMember != null)
                return cachedHubMember;

            var hubMember = await _context.HubMembers.FirstOrDefaultAsync(e => e.HubId == hubId
                                                                      && e.MemberId == memberId);
            if (hubMember == null)
                return null;

            await CacheHubMember(hubMember);
            return hubMember.ToCachedHubMember();
        }

        public async Task<HubMember?> AddMemberToHubAsync(Guid hubId, Guid memberId)
        {
            var hubMember = await GetHubMemberAsync(hubId, memberId);
            if (hubMember != null)
                return null;

            var result = await _context.HubMembers.AddAsync(new HubMember
            {
                HubId = hubId,
                MemberId = memberId
            });
            await _context.SaveChangesAsync();
            await CacheHubMember(result.Entity);
            return result.Entity;
        }

        public async Task<bool> DeleteHubAsync(Guid id)
        {
            var hub = await GetHubByIdAsync(id);
            if (hub == null)
                return true;

            var entityHub = new Hub { Id = id };
            _context.Hubs.Remove(entityHub);
            await _context.SaveChangesAsync();

            await RemoveCachedHub(id);
            return true;
        }

        public async Task<CachedHub?> GetHubByIdAsync(Guid id)
        {
            var cachedHub = await _cacheService.GetAsync<CachedHub>($"{_cacheHubKeyPrefix}:{id}");
            if (cachedHub != null)
                return cachedHub;

            var hub = await _context.Hubs.FindAsync(id);
            if (hub == null)
                return null;

            await CacheHub(hub);
            return hub.ToCachedHub();
        }

        public async Task<bool> RemoveMemberFromHubAsync(Guid hubId, Guid memberId)
        {
            var hubMember = await GetHubMemberAsync(hubId, memberId);
            if (hubMember == null)
                return true;

            _context.HubMembers.Remove(new HubMember { HubId = hubId, MemberId = memberId });
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<CachedHub?> UpdateHubNameAsync(Guid id, string name)
        {
            var hub = await _context.Hubs.FirstOrDefaultAsync(e => e.Id == id);
            if (hub == null)
                return null;

            hub.Name = name;
            await _context.SaveChangesAsync();

            await CacheHub(hub);
            return hub.ToCachedHub();
        }

        public async Task<CachedJoiningInvitation?> GetHubJoiningInvitationByHubIdAsync(Guid hubId)
        {
            var cachedInvitation = await _cacheService.GetAsync<CachedJoiningInvitation>($"{_cacheJoiningInvitationKeyPrefix}:{hubId}");
            if (cachedInvitation != null)
            {
                var trackedInvitation = _context.HubJoiningInvitations.Local.FirstOrDefault(e => e.HubId == hubId);
                if (trackedInvitation != null)
                    return trackedInvitation.ToCachedInvitation();

                return cachedInvitation;
            }

            var invitation = await _context.HubJoiningInvitations.FirstOrDefaultAsync(e => e.HubId == hubId);
            if (invitation == null)
                return null;

            await CacheJoiningInvitation(invitation);
            return invitation.ToCachedInvitation();
        }

        public async Task<HubJoiningInvitation?> AddHubJoiningInvitationAsync(Hub hub, string hashInvitation)
        {
            var hubJoiningInvitation = await GetHubJoiningInvitationByHubIdAsync(hub.Id);
            if (hubJoiningInvitation != null)
                return null;

            var result = await _context.HubJoiningInvitations.AddAsync(new HubJoiningInvitation
            {
                Hub = hub,
                HashInvitation = hashInvitation
            });

            await _context.SaveChangesAsync();
            await CacheJoiningInvitation(result.Entity);
            return result.Entity;
        }

        public async Task<CachedJoiningInvitation?> GetHubJoiningInvitationByHashAsync(string hash)
        {
            var cachedInvitation = await _cacheService.GetAsync<CachedJoiningInvitation>($"{_cacheJoiningInvitationKeyPrefix}:{hash}");
            if (cachedInvitation != null)
            {
                var trackedInvitation = _context.HubJoiningInvitations.Local.FirstOrDefault(e => e.HashInvitation == hash);
                if (trackedInvitation != null)
                    return trackedInvitation.ToCachedInvitation();

                return cachedInvitation;
            }

            var invitation = await _context.HubJoiningInvitations.FirstOrDefaultAsync(e => e.HashInvitation == hash);
            if (invitation == null)
                return null;

            await CacheJoiningInvitation(invitation);
            return invitation.ToCachedInvitation();
        }

        public async Task<CachedHub?> UpdateHubIconAsync(Guid id, string fileName)
        {
            var hub = await _context.Hubs.FirstOrDefaultAsync(e => e.Id == id);
            if (hub == null)
                return null;

            hub.Icon = fileName;
            await _context.SaveChangesAsync();

            await CacheHub(hub);
            return hub.ToCachedHub();
        }

        private async Task CacheJoiningInvitation(HubJoiningInvitation invitation)
        {
            var cachedInvitation = invitation.ToCachedInvitation();
            var key = $"{_cacheJoiningInvitationKeyPrefix}:{invitation.HubId}";
            await _cacheService.SetAsync(key, cachedInvitation, TimeSpan.FromHours(2), TimeSpan.FromHours(12));
            await _cacheService.SetAsync($"{_cacheJoiningInvitationKeyPrefix}:{invitation.HashInvitation}", cachedInvitation, TimeSpan.FromHours(2), TimeSpan.FromHours(12));
        }

        private async Task CacheHubMember(HubMember hubMember)
        {
            var cachedHubMember = hubMember.ToCachedHubMember();
            var key = $"{_cacheHubMemberKeyPrefix}:{hubMember.HubId}:{hubMember.MemberId}";
            await _cacheService.SetAsync(key, cachedHubMember, TimeSpan.FromHours(1), TimeSpan.FromHours(6));
        }

        private async Task<CachedHubMember?> GetCachedHubMember(Guid hubId, Guid memberId)
        {
            var key = $"{_cacheHubMemberKeyPrefix}:{hubId}:{memberId}";
            return await _cacheService.GetAsync<CachedHubMember>(key);
        }

        private async Task CacheHub(Hub hub)
        {
            var cachedHub = hub.ToCachedHub();
            var key = $"{_cacheHubKeyPrefix}:{hub.Id}";
            await _cacheService.SetAsync(key, cachedHub, TimeSpan.FromHours(1), TimeSpan.FromHours(6));
        }

        private async Task RemoveCachedHub(Guid id)
        {
            var key = $"{_cacheHubKeyPrefix}:{id}";
            await _cacheService.RemoveAsync(key);
        }
    }
}