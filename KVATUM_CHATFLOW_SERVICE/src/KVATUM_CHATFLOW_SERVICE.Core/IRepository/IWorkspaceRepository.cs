using KVATUM_CHATFLOW_SERVICE.Core.Entities.Models;

namespace KVATUM_CHATFLOW_SERVICE.Core.IRepository
{
    public interface IWorkspaceRepository
    {
        Task<Workspace?> GetWorkspaceAsync(Guid id);
        Task<Workspace?> AddWorkspaceAsync(string name, Hub hub, string hexColor);
        Task<bool> DeleteWorkspaceAsync(Guid id);
        Task<List<Workspace>> GetWorkspacesAsync(Guid hubId, int limit, int offset);
        Task<Workspace?> UpdateWorkspaceIconAsync(Guid id, string fileName);
    }
}