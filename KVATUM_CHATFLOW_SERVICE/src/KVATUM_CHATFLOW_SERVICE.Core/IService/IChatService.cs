using KVATUM_CHATFLOW_SERVICE.Core.Entities.Request;
using KVATUM_CHATFLOW_SERVICE.Core.Entities.Response;

namespace KVATUM_CHATFLOW_SERVICE.Core.IService
{
    public interface IChatService
    {
        Task<ServiceResponse<List<WorkspaceChatsBody>>> GetWorkspaceChatsAsync(List<Guid> workspaceIds);
        Task<ServiceResponse<ChatBody>> CreateChatAsync(CreateChatBody body);
        Task<ServiceResponse<bool>> DeleteChatAsync(Guid chatId);
        Task<ServiceResponse<ChatBody>> AttachChatToWorkspaceAsync(Guid chatId, Guid workspaceId);
        Task<ServiceResponse<bool>> DetachChatFromWorkspaceAsync(Guid chatId, Guid workspaceId);
    }
}