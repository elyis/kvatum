using KVATUM_CHATFLOW_SERVICE.Core.Entities.Cache;
using KVATUM_CHATFLOW_SERVICE.Core.Entities.Models;
using KVATUM_CHATFLOW_SERVICE.Core.Entities.Response;
using KVATUM_CHATFLOW_SERVICE.Core.Enums;

namespace KVATUM_CHATFLOW_SERVICE.Core.IRepository
{
    public interface IChatRepository
    {
        Task<CachedChat?> GetChatAsync(Guid chatId);
        Task<CachedChat?> AddChatAsync(string name, ChatType type, Guid workspaceId);
        Task<bool> DeleteChatAsync(Guid chatId);
        Task<List<Chat>> GetChatsByWorkspaceIdAsync(Guid workspaceId);
        Task<List<WorkspaceChatsBody>> GetWorkspaceChatsAsync(IEnumerable<Guid> workspaceIds);
        Task<Chat?> AttachChatToWorkspaceAsync(Guid chatId, Guid workspaceId);
        Task<bool> DetachChatFromWorkspaceAsync(Guid chatId, Guid workspaceId);
    }
}