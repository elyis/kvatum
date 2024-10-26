using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KVATUM_CHATFLOW_SERVICE.Core.Entities.Request;
using KVATUM_CHATFLOW_SERVICE.Core.Entities.Response;
using KVATUM_CHATFLOW_SERVICE.Core.IService;
using Swashbuckle.AspNetCore.Annotations;

namespace KVATUM_CHATFLOW_SERVICE.Api.Controllers
{
    [ApiController]
    [Route("api")]
    public class HubController : ControllerBase
    {
        private readonly IJwtService _jwtService;
        private readonly IHubService _hubService;
        public HubController(
            IHubService hubService,
            IJwtService jwtService)
        {
            _hubService = hubService;
            _jwtService = jwtService;
        }

        [HttpPost("hub"), Authorize]
        [SwaggerOperation(Summary = "Создать хаб", Description = "Создать хаб")]
        [SwaggerResponse(200, Description = "Успешное создание хаба", Type = typeof(HubBody))]
        [SwaggerResponse(400, Description = "Name is empty")]
        public async Task<IActionResult> CreateHub(
            [FromHeader(Name = "Authorization")] string token,
            [FromBody] CreateHubBody body)
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _hubService.CreateHubAsync(body, tokenPayload.AccountId);
            if (!result.IsSuccess)
                return StatusCode((int)result.StatusCode, result.Errors);

            return StatusCode((int)result.StatusCode, result.Body);
        }

        [HttpGet("hubs/me"), Authorize]
        [SwaggerOperation(Summary = "Получить хабы, в которые входит пользователь", Description = "Получить хабы, в которые входит пользователь")]
        [SwaggerResponse(200, Description = "Успешное получение хабов", Type = typeof(List<HubBody>))]
        [SwaggerResponse(400, Description = "Limit or offset is not valid")]
        public async Task<IActionResult> GetConnectedHubs(
            [FromHeader(Name = "Authorization")] string token,
            [FromQuery] int limit = 10,
            [FromQuery] int offset = 0)
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _hubService.GetConnectedHubsAsync(tokenPayload.AccountId, limit, offset);
            if (!result.IsSuccess)
                return StatusCode((int)result.StatusCode, result.Errors);

            return StatusCode((int)result.StatusCode, result.Body);
        }

        [HttpGet("hub/link/{hubId}"), Authorize]
        [SwaggerOperation(Summary = "Получить ссылку на вступление в хаб", Description = "Получить ссылку на вступление в хаб")]
        [SwaggerResponse(200, Description = "Успешное получение ссылки", Type = typeof(HubInvitationLinkBody))]
        [SwaggerResponse(404, Description = "Ссылка не найдена")]
        public async Task<IActionResult> GetLinkToJoinHub([FromRoute] Guid hubId)
        {
            var result = await _hubService.GetHubJoiningInvitationByHubIdAsync(hubId);
            if (!result.IsSuccess)
                return StatusCode((int)result.StatusCode, result.Errors);

            return StatusCode((int)result.StatusCode, result.Body);
        }

        [HttpPost("hub/invitation"), Authorize]
        [SwaggerOperation(Summary = "Присоединиться к серверу", Description = "Присоединиться к серверу")]
        [SwaggerResponse(200, Description = "Успешное ")]
        [SwaggerResponse(404, Description = "Ссылка не найдена")]
        public async Task<IActionResult> GetHubInvitation(
            [FromHeader(Name = "Authorization")] string token,
            [FromQuery] string hash)
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _hubService.AddMemberToHubAsync(tokenPayload.AccountId, hash);
            if (!result.IsSuccess)
                return StatusCode((int)result.StatusCode, result.Errors);

            return StatusCode((int)result.StatusCode);
        }
    }
}