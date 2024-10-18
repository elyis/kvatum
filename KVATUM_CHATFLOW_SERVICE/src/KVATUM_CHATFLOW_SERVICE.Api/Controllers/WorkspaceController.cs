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
        private readonly IWorkspaceRepository _workspaceRepository;
        private readonly IHubRepository _hubRepository;
        private readonly IJwtService _jwtService;
        public WorkspaceController(
            IWorkspaceRepository workspaceRepository,
            IHubRepository hubRepository,
            IJwtService jwtService)
        {
            _workspaceRepository = workspaceRepository;
            _hubRepository = hubRepository;
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
            var workspaces = await _workspaceRepository.GetWorkspacesAsync(hubId, limit, offset);
            return Ok(workspaces.Select(e => e.ToWorkspaceBody()));
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
            var hub = await _hubRepository.GetHubByIdAsync(body.HubId);
            if (hub == null)
                return BadRequest("HubId is not exists");

            if (tokenPayload.AccountId != hub.CreatorId)
                return Forbid();

            var workspace = await _workspaceRepository.AddWorkspaceAsync(body.Name, hub);
            return Ok(workspace?.ToWorkspaceBody());
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
            var workspace = await _workspaceRepository.GetWorkspaceAsync(workspaceId);
            if (workspace == null)
                return BadRequest("WorkspaceId is not exists");

            var hub = await _hubRepository.GetHubByIdAsync(workspace.HubId);
            if (hub == null)
                return BadRequest("HubId is not exists");

            if (tokenPayload.AccountId != hub.CreatorId)
                return Forbid();

            await _workspaceRepository.DeleteWorkspaceAsync(workspaceId);
            return Ok();
        }
    }
}