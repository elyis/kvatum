using KVATUM_CHATFLOW_SERVICE.Core.Entities.Models;
using KVATUM_CHATFLOW_SERVICE.Core.IRepository;
using KVATUM_CHATFLOW_SERVICE.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KVATUM_CHATFLOW_SERVICE.Infrastructure.Repository
{
    public class WorkspaceRepository : IWorkspaceRepository
    {
        private readonly ServerFlowDbContext _context;

        public WorkspaceRepository(ServerFlowDbContext context)
        {
            _context = context;
        }

        public async Task<Workspace?> AddWorkspaceAsync(string name, Hub hub, string hexColor)
        {
            var workspace = new Workspace
            {
                Name = name,
                Hub = hub,
                HexColor = hexColor
            };

            await _context.Workspaces.AddAsync(workspace);
            await _context.SaveChangesAsync();
            return workspace;
        }

        public async Task<bool> DeleteWorkspaceAsync(Guid id)
        {
            var workspace = await GetWorkspaceAsync(id);
            if (workspace == null)
                return true;

            _context.Workspaces.Remove(workspace);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Workspace?> GetWorkspaceAsync(Guid id)
        {
            return await _context.Workspaces.FirstOrDefaultAsync(e => e.Id == id);
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
            var workspace = await GetWorkspaceAsync(id);
            if (workspace == null)
                return null;

            workspace.Icon = fileName;
            await _context.SaveChangesAsync();
            return workspace;
        }
    }
}