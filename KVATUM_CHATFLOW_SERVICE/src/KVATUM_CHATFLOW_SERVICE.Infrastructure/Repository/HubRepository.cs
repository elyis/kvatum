using Microsoft.EntityFrameworkCore;
using KVATUM_CHATFLOW_SERVICE.Infrastructure.Data;
using KVATUM_CHATFLOW_SERVICE.Core.IRepository;
using KVATUM_CHATFLOW_SERVICE.Core.Entities.Models;
using KVATUM_CHATFLOW_SERVICE.Core.Entities.Request;
using KVATUM_CHATFLOW_SERVICE.Core.Enums;

namespace KVATUM_CHATFLOW_SERVICE.Infrastructure.Repository
{
    public class HubRepository : IHubRepository
    {
        private readonly ServerFlowDbContext _context;

        public HubRepository(ServerFlowDbContext context)
        {
            _context = context;
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

        public async Task<Hub?> AddHubAsync(CreateHubBody body, Guid creatorId, string hashInvitation)
        {
            var hub = new Hub
            {
                Name = body.Name,
                CreatorId = creatorId,
                Members = new List<HubMember> { new HubMember { MemberId = creatorId } },
                HubJoiningInvitation = new HubJoiningInvitation { HashInvitation = hashInvitation }
            };

            var workspace = new Workspace
            {
                Name = body.Name,
                Hub = hub,
            };

            var chats = new List<Chat>
            {
                new() {
                    Name = "General",
                    Type = ChatType.Conference.ToString()
                },

                new() {
                    Name = "Live chat",
                    Type = ChatType.Channel.ToString()
                }
            };

            workspace.Chats = chats;

            await _context.Hubs.AddAsync(hub);
            await _context.Workspaces.AddAsync(workspace);
            await _context.Chats.AddRangeAsync(chats);

            await _context.SaveChangesAsync();
            return hub;
        }

        public async Task<HubMember?> GetHubMemberAsync(Guid hubId, Guid memberId)
        {
            return await _context.HubMembers.FirstOrDefaultAsync(e => e.HubId == hubId
                                                                      && e.MemberId == memberId);
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
            return result.Entity;
        }

        public async Task<bool> DeleteHubAsync(Guid id)
        {
            var hub = await GetHubByIdAsync(id);
            if (hub == null)
                return true;

            _context.Hubs.Remove(hub);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Hub?> GetHubByIdAsync(Guid id)
            => await _context.Hubs.FindAsync(id);

        public async Task<bool> RemoveMemberFromHubAsync(Guid hubId, Guid memberId)
        {
            var hubMember = await GetHubMemberAsync(hubId, memberId);
            if (hubMember == null)
                return true;

            _context.HubMembers.Remove(hubMember);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Hub?> UpdateHubAsync(Guid id, string name)
        {
            var hub = await GetHubByIdAsync(id);
            if (hub == null)
                return null;

            hub.Name = name;
            await _context.SaveChangesAsync();
            return hub;
        }

        public async Task<HubJoiningInvitation?> GetHubJoiningInvitationByHubIdAsync(Guid hubId)
        {
            return await _context.HubJoiningInvitations.FirstOrDefaultAsync(e => e.HubId == hubId);
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
            return result.Entity;
        }

        public async Task<HubJoiningInvitation?> GetHubJoiningInvitationByHashAsync(string hash)
        {
            return await _context.HubJoiningInvitations.FirstOrDefaultAsync(e => e.HashInvitation == hash);
        }

        public async Task<Hub?> UpdateHubIconAsync(Guid id, string fileName)
        {
            var hub = await GetHubByIdAsync(id);
            if (hub == null)
                return null;

            hub.Icon = fileName;
            await _context.SaveChangesAsync();
            return hub;
        }
    }
}