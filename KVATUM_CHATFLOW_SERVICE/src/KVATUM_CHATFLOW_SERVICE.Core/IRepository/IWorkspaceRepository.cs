using KVATUM_CHATFLOW_SERVICE.Core.Entities.Cache;
using KVATUM_CHATFLOW_SERVICE.Core.Entities.Models;

namespace KVATUM_CHATFLOW_SERVICE.Core.IRepository
{
    public interface IWorkspaceRepository
    {
        Task<CachedWorkspace?> GetWorkspaceAsync(Guid id);
        Task<CachedWorkspace?> AddWorkspaceAsync(string name, Guid hubId, string hexColor);
        Task<bool> DeleteWorkspaceAsync(Guid id);
        Task<List<Workspace>> GetWorkspacesAsync(Guid hubId, int limit, int offset);
        Task<Workspace?> UpdateWorkspaceIconAsync(Guid id, string fileName);
    }
}