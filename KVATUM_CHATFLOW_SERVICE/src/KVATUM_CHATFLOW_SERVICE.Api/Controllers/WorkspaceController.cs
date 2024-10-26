using KVATUM_CHATFLOW_SERVICE.Core.Entities.Request;
using KVATUM_CHATFLOW_SERVICE.Core.Entities.Response;
using KVATUM_CHATFLOW_SERVICE.Core.IRepository;
using KVATUM_CHATFLOW_SERVICE.Core.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace KVATUM_CHATFLOW_SERVICE.Api.Controllers
{
    [ApiController]
    [Route("api")]
    public class WorkspaceController : ControllerBase
    {
        private readonly IJwtService _jwtService;
        private readonly IWorkspaceService _workspaceService;
        public WorkspaceController(
            IWorkspaceService workspaceService,
            IJwtService jwtService)
        {
            _workspaceService = workspaceService;
            _jwtService = jwtService;
        }

        [HttpGet("workspaces"), Authorize]
        [SwaggerOperation(Summary = "Получить все workspace на сервере", Description = "Получить все workspace на сервере")]
        [SwaggerResponse(200, Description = "Успешное получение workspace", Type = typeof(List<WorkspaceBody>))]
        [SwaggerResponse(400, Description = "Limit or offset is not valid")]
        public async Task<IActionResult> GetWorkspacesAsync(
            [FromQuery] Guid hubId,
            [FromQuery] int limit,
            [FromQuery] int offset)
        {
            var response = await _workspaceService.GetWorkspacesByHubIdAsync(hubId, limit, offset);
            if (!response.IsSuccess)
                return StatusCode((int)response.StatusCode, response.Errors);

            return StatusCode((int)response.StatusCode, response.Body);
        }

        [HttpPost("workspace"), Authorize]
        [SwaggerOperation(Summary = "Создать workspace", Description = "Создать workspace")]
        [SwaggerResponse(200, Description = "Успешное создание workspace", Type = typeof(WorkspaceBody))]
        [SwaggerResponse(400, Description = "HubId is not exists")]
        [SwaggerResponse(403, Description = "Не аккаунт создателя хаба")]
        public async Task<IActionResult> AddWorkspaceAsync(
            CreateWorkspaceBody body,
            [FromHeader(Name = "Authorization")] string token)
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var response = await _workspaceService.AddWorkspaceAsync(body, body.HubId, tokenPayload.AccountId);
            if (!response.IsSuccess)
                return StatusCode((int)response.StatusCode, response.Errors);

            return StatusCode((int)response.StatusCode, response.Body);
        }

        [HttpDelete("workspace"), Authorize]
        [SwaggerOperation(Summary = "Удалить workspace", Description = "Удалить workspace")]
        [SwaggerResponse(200, Description = "Успешное удаление workspace")]
        [SwaggerResponse(400, Description = "WorkspaceId is not exists")]
        [SwaggerResponse(403, Description = "Не аккаунт создателя хаба")]
        public async Task<IActionResult> DeleteWorkspaceAsync(
            [FromQuery(Name = "key")] Guid workspaceId,
            [FromHeader(Name = "Authorization")] string token)
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var response = await _workspaceService.DeleteWorkspaceAsync(workspaceId, tokenPayload.AccountId);
            if (!response.IsSuccess)
                return StatusCode((int)response.StatusCode, response.Errors);

            return StatusCode((int)response.StatusCode);
        }
    }
}
