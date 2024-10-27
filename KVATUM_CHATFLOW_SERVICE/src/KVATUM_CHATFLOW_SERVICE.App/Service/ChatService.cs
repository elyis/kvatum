using System.Net;
using KVATUM_CHATFLOW_SERVICE.Core.Entities.Request;
using KVATUM_CHATFLOW_SERVICE.Core.Entities.Response;
using KVATUM_CHATFLOW_SERVICE.Core.IRepository;
using KVATUM_CHATFLOW_SERVICE.Core.IService;

namespace KVATUM_CHATFLOW_SERVICE.App.Service
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository _chatRepository;
        private readonly IWorkspaceRepository _workspaceRepository;
        public ChatService(
            IChatRepository chatRepository,
            IWorkspaceRepository workspaceRepository)
        {
            _chatRepository = chatRepository;
            _workspaceRepository = workspaceRepository;
        }

        public async Task<ServiceResponse<ChatBody>> AttachChatToWorkspaceAsync(Guid chatId, Guid workspaceId)
        {
            var workspace = await _workspaceRepository.GetWorkspaceAsync(workspaceId);
            if (workspace == null)
                return new ServiceResponse<ChatBody>
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    Errors = new[] { "WorkspaceId is not exist" },
                };

            var chat = await _chatRepository.AttachChatToWorkspaceAsync(chatId, workspace);
            return new ServiceResponse<ChatBody>
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Body = chat?.ToChatBody(),
            };
        }

        public async Task<ServiceResponse<ChatBody>> CreateChatAsync(CreateChatBody body)
        {
            var workspace = await _workspaceRepository.GetWorkspaceAsync(body.WorkspaceId);
            if (workspace == null)
                return new ServiceResponse<ChatBody>
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    Errors = new[] { "WorkspaceId is not exist" },
                };

            var chat = await _chatRepository.AddChatAsync(body.Name, body.Type, workspace);
            if (chat == null)
                return new ServiceResponse<ChatBody>
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    Errors = new[] { "Failed to create chat" },
                };

            return new ServiceResponse<ChatBody>
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Body = chat?.ToChatBody(),
            };
        }

        public async Task<ServiceResponse<bool>> DeleteChatAsync(Guid chatId)
        {
            var result = await _chatRepository.DeleteChatAsync(chatId);
            return new ServiceResponse<bool>
            {
                IsSuccess = result,
                StatusCode = HttpStatusCode.NoContent,
            };
        }

        public async Task<ServiceResponse<bool>> DetachChatFromWorkspaceAsync(Guid chatId, Guid workspaceId)
        {
            var workspace = await _workspaceRepository.GetWorkspaceAsync(workspaceId);
            if (workspace == null)
                return new ServiceResponse<bool>
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    Errors = new[] { "WorkspaceId is not exist" },
                };

            var result = await _chatRepository.DetachChatFromWorkspaceAsync(chatId, workspaceId);
            return new ServiceResponse<bool>
            {
                IsSuccess = result,
                StatusCode = HttpStatusCode.NoContent,
            };
        }

        public async Task<ServiceResponse<WorkspaceChatsBody>> GetChatsByWorkspaceAsync(Guid workspaceId)
        {
            var chats = await _chatRepository.GetWorkspaceChatsAsync(new[] { workspaceId });
            return new ServiceResponse<WorkspaceChatsBody>
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Body = chats.FirstOrDefault(),
            };
        }

        public async Task<ServiceResponse<List<WorkspaceChatsBody>>> GetChatsByWorkspacesAsync(List<Guid> workspaceIds)
        {
            var chats = await _chatRepository.GetWorkspaceChatsAsync(workspaceIds);
            return new ServiceResponse<List<WorkspaceChatsBody>>
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Body = chats,
            };
        }
    }
}