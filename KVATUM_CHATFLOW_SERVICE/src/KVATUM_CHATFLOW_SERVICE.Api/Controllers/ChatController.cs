using KVATUM_CHATFLOW_SERVICE.Core.Entities.Request;
using KVATUM_CHATFLOW_SERVICE.Core.Entities.Response;
using KVATUM_CHATFLOW_SERVICE.Core.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace KVATUM_CHATFLOW_SERVICE.Api.Controllers
{
    [ApiController]
    [Route("api")]
    public class ChatController : ControllerBase
    {
        private readonly IChatRepository _chatRepository;
        private readonly IWorkspaceRepository _workspaceRepository;
        public ChatController(
            IChatRepository chatRepository,
            IWorkspaceRepository workspaceRepository)
        {
            _chatRepository = chatRepository;
            _workspaceRepository = workspaceRepository;
        }

        [HttpGet("chats"), Authorize]
        [SwaggerOperation(Summary = "Получить чаты в workspace", Description = "Получить чаты в workspace")]
        [SwaggerResponse(200, Description = "Успешное получение чатов", Type = typeof(List<WorkspaceChatsBody>))]
        public async Task<IActionResult> GetWorkspaceChatsAsync(List<Guid> workspaceIds)
        {
            var chats = await _chatRepository.GetWorkspaceChatsAsync(workspaceIds);
            return Ok(chats);
        }

        [HttpPost("chat"), Authorize]
        [SwaggerOperation(Summary = "Создать чат в workspace", Description = "Создать чат в workspace")]
        [SwaggerResponse(200, Description = "Успешное создание чата", Type = typeof(ChatBody))]
        [SwaggerResponse(400, Description = "WorkspaceId is not exist")]
        public async Task<IActionResult> CreateChatAsync(CreateChatBody body)
        {
            var workspace = await _workspaceRepository.GetWorkspaceAsync(body.WorkspaceId);
            if (workspace == null)
                return BadRequest("WorkspaceId is not exist");

            var chat = await _chatRepository.AddChatAsync(body.Name, body.Type, workspace);
            return Ok(chat?.ToChatBody());
        }

        [HttpDelete("chat"), Authorize]
        [SwaggerOperation(Summary = "Удалить чат", Description = "Удалить чат")]
        [SwaggerResponse(204, Description = "Успешное удаление чата")]
        [SwaggerResponse(400, Description = "ChatId is not exist")]
        public async Task<IActionResult> DeleteChatAsync(Guid chatId)
        {
            await _chatRepository.DeleteChatAsync(chatId);
            return NoContent();
        }

        [HttpPost("chat/attach"), Authorize]
        [SwaggerOperation(Summary = "Прикрепить чат к workspace", Description = "Прикрепить чат к workspace")]
        [SwaggerResponse(200, Description = "Успешное прикрепление чата", Type = typeof(ChatBody))]
        [SwaggerResponse(400, Description = "ChatId or WorkspaceId is not exist")]
        public async Task<IActionResult> AttachChatToWorkspaceAsync(Guid chatId, Guid workspaceId)
        {
            var workspace = await _workspaceRepository.GetWorkspaceAsync(workspaceId);
            if (workspace == null)
                return BadRequest("WorkspaceId is not exist");

            var chat = await _chatRepository.AttachChatToWorkspaceAsync(chatId, workspace);
            return Ok(chat?.ToChatBody());
        }
    }
}