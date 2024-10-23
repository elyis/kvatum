using KVATUM_CHATFLOW_SERVICE.Core.Entities.Models;
using KVATUM_CHATFLOW_SERVICE.Core.Entities.Response;
using KVATUM_CHATFLOW_SERVICE.Core.Enums;

namespace KVATUM_CHATFLOW_SERVICE.Core.IRepository
{
    public interface IChatRepository
    {
        Task<Chat?> GetChatAsync(Guid chatId);
        Task<Chat?> AddChatAsync(string name, ChatType type, Workspace workspace);
        Task<bool> DeleteChatAsync(Guid chatId);
        Task<List<Chat>> GetChatsByWorkspaceIdAsync(Guid workspaceId);
        Task<List<WorkspaceChatsBody>> GetWorkspaceChatsAsync(IEnumerable<Guid> workspaceIds);
        Task<Chat?> AttachChatToWorkspaceAsync(Guid chatId, Workspace workspace);
        Task<bool> DetachChatFromWorkspaceAsync(Guid chatId, Guid workspaceId);
    }
}