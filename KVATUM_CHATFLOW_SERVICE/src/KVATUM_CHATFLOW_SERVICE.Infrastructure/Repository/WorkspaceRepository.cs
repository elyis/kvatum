using KVATUM_CHATFLOW_SERVICE.Core.Entities.Cache;
using KVATUM_CHATFLOW_SERVICE.Core.Entities.Models;
using KVATUM_CHATFLOW_SERVICE.Core.Enums;
using KVATUM_CHATFLOW_SERVICE.Core.IRepository;
using KVATUM_CHATFLOW_SERVICE.Core.IService;
using KVATUM_CHATFLOW_SERVICE.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KVATUM_CHATFLOW_SERVICE.Infrastructure.Repository
{
    public class WorkspaceRepository : IWorkspaceRepository
    {
        private readonly ICacheService _cacheService;
        private readonly ServerFlowDbContext _context;
        private readonly string _cacheWorkspaceKeyPrefix = "workspace";

        public WorkspaceRepository(
            ServerFlowDbContext context,
            ICacheService cacheService)
        {
            _context = context;
            _cacheService = cacheService;
        }

        public async Task<CachedWorkspace?> AddWorkspaceAsync(string name, Guid hubId, string hexColor)
        {
            var workspace = new Workspace
            {
                Name = name,
                HubId = hubId,
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

            await _context.Workspaces.AddAsync(workspace);
            await _context.Chats.AddRangeAsync(chats);
            await _context.SaveChangesAsync();

            await CacheWorkspace(workspace);
            return workspace.ToCachedWorkspace();
        }

        public async Task<bool> DeleteWorkspaceAsync(Guid id)
        {
            var workspace = await GetWorkspaceAsync(id);
            if (workspace == null)
                return true;

            var oldEntity = await _context.Workspaces.FirstOrDefaultAsync(e => e.Id == id);
            if (oldEntity == null)
                return true;

            _context.Workspaces.Remove(oldEntity);
            await _context.SaveChangesAsync();

            await RemoveCachedWorkspace(id);
            return true;
        }

        public async Task<CachedWorkspace?> GetWorkspaceAsync(Guid id)
        {
            var cachedWorkspace = await GetCachedWorkspace(id);
            if (cachedWorkspace != null)
                return cachedWorkspace;

            var workspace = await _context.Workspaces.FirstOrDefaultAsync(e => e.Id == id);
            if (workspace == null)
                return null;

            await CacheWorkspace(workspace);
            return workspace.ToCachedWorkspace();
        }

        public async Task<List<Workspace>> GetWorkspacesAsync(Guid hubId, int limit, int offset)
        {
            return await _context.Workspaces.Where(e => e.HubId == hubId)
                                            .OrderBy(e => e.Id)
                                            .Skip(offset)
                                            .Take(limit)
                                            .ToListAsync();
        }

        public async Task<Workspace?> UpdateWorkspaceIconAsync(Guid id, string fileName)
        {
            var workspace = await _context.Workspaces.FirstOrDefaultAsync(e => e.Id == id);
            if (workspace == null)
                return null;

            workspace.Icon = fileName;
            await _context.SaveChangesAsync();
            await CacheWorkspace(workspace);
            return workspace;
        }

        private async Task CacheWorkspace(Workspace workspace)
        {
            var key = $"{_cacheWorkspaceKeyPrefix}:{workspace.Id}";
            var cachedWorkspace = workspace.ToCachedWorkspace();
            await _cacheService.SetAsync(key, cachedWorkspace, TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(6));
        }

        private async Task<CachedWorkspace?> GetCachedWorkspace(Guid id)
        {
            var key = $"{_cacheWorkspaceKeyPrefix}:{id}";
            return await _cacheService.GetAsync<CachedWorkspace>(key);
        }

        private async Task RemoveCachedWorkspace(Guid id)
        {
            var key = $"{_cacheWorkspaceKeyPrefix}:{id}";
            await _cacheService.RemoveAsync(key);
        }
    }
}