using KVATUM_CHATFLOW_SERVICE.Core.Entities.Request;
using KVATUM_CHATFLOW_SERVICE.Core.Entities.Response;
using KVATUM_CHATFLOW_SERVICE.Core.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace KVATUM_CHATFLOW_SERVICE.Api.Controllers
{
    [ApiController]
    [Route("api")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpGet("chats"), Authorize]
        [SwaggerOperation(Summary = "Получить список чатов в workspace", Description = "Получить список чатов в workspace's")]
        [SwaggerResponse(200, Description = "Успешное получение чатов", Type = typeof(WorkspaceChatsBody))]
        public async Task<IActionResult> GetWorkspaceChatsAsync([FromQuery] Guid workspaceId)
        {
            var response = await _chatService.GetChatsByWorkspaceAsync(workspaceId);
            if (!response.IsSuccess)
                return StatusCode((int)response.StatusCode, response.Errors);

            return StatusCode((int)response.StatusCode, response.Body);
        }

        [HttpPost("chat"), Authorize]
        [SwaggerOperation(Summary = "Создать чат в workspace", Description = "Создать чат в workspace")]
        [SwaggerResponse(200, Description = "Успешное создание чата", Type = typeof(ChatBody))]
        [SwaggerResponse(400, Description = "WorkspaceId is not exist")]
        public async Task<IActionResult> CreateChatAsync(CreateChatBody body)
        {
            var response = await _chatService.CreateChatAsync(body);
            if (!response.IsSuccess)
                return StatusCode((int)response.StatusCode, response.Errors);

            return StatusCode((int)response.StatusCode, response.Body);
        }

        [HttpDelete("chat"), Authorize]
        [SwaggerOperation(Summary = "Удалить чат", Description = "Удалить чат")]
        [SwaggerResponse(204, Description = "Успешное удаление чата")]
        [SwaggerResponse(400, Description = "ChatId is not exist")]
        public async Task<IActionResult> DeleteChatAsync(Guid chatId)
        {
            var response = await _chatService.DeleteChatAsync(chatId);
            if (!response.IsSuccess)
                return StatusCode((int)response.StatusCode, response.Errors);

            return StatusCode((int)response.StatusCode);
        }

        [HttpPost("chat/attach"), Authorize]
        [SwaggerOperation(Summary = "Прикрепить чат к workspace", Description = "Прикрепить чат к workspace")]
        [SwaggerResponse(200, Description = "Успешное прикрепление чата", Type = typeof(ChatBody))]
        [SwaggerResponse(400, Description = "ChatId or WorkspaceId is not exist")]
        public async Task<IActionResult> AttachChatToWorkspaceAsync(Guid chatId, Guid workspaceId)
        {
            var response = await _chatService.AttachChatToWorkspaceAsync(chatId, workspaceId);
            if (!response.IsSuccess)
                return StatusCode((int)response.StatusCode, response.Errors);

            return StatusCode((int)response.StatusCode, response.Body);
        }

        [HttpDelete("chat/detach"), Authorize]
        [SwaggerOperation(Summary = "Открепить чат от workspace", Description = "Открепить чат от workspace")]
        [SwaggerResponse(204, Description = "Успешное открепление чата")]
        [SwaggerResponse(400, Description = "ChatId or WorkspaceId is not exist")]
        public async Task<IActionResult> DetachChatFromWorkspaceAsync(Guid chatId, Guid workspaceId)
        {
            var response = await _chatService.DetachChatFromWorkspaceAsync(chatId, workspaceId);
            if (!response.IsSuccess)
                return StatusCode((int)response.StatusCode, response.Errors);

            return StatusCode((int)response.StatusCode);
        }
    }
}