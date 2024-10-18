using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KVATUM_CHATFLOW_SERVICE.Core.Entities.Request;
using KVATUM_CHATFLOW_SERVICE.Core.Entities.Response;
using KVATUM_CHATFLOW_SERVICE.Core.IRepository;
using KVATUM_CHATFLOW_SERVICE.Core.IService;
using Swashbuckle.AspNetCore.Annotations;

namespace KVATUM_CHATFLOW_SERVICE.Api.Controllers
{
    [ApiController]
    [Route("api")]
    public class HubController : ControllerBase
    {
        private readonly IHubRepository _hubRepository;
        private readonly IJwtService _jwtService;
        private readonly IHashGenerator _hashGenerator;
        public HubController(
            IHubRepository hubRepository,
            IJwtService jwtService,
            IHashGenerator hashGenerator)
        {
            _hubRepository = hubRepository;
            _jwtService = jwtService;
            _hashGenerator = hashGenerator;
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
            var hashInvitation = _hashGenerator.Compute();
            var hub = await _hubRepository.AddHubAsync(body, tokenPayload.AccountId, hashInvitation);
            return Ok(hub?.ToHubBody());
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
            var hubs = await _hubRepository.GetHubsByConnectedAccountIdAsync(tokenPayload.AccountId, limit, offset);
            return Ok(hubs.Select(e => e.ToHubBody()));
        }

        [HttpGet("hub/link/{hubId}"), Authorize]
        [SwaggerOperation(Summary = "Получить ссылку на вступление в хаб", Description = "Получить ссылку на вступление в хаб")]
        [SwaggerResponse(200, Description = "Успешное получение ссылки", Type = typeof(HubInvitationLinkBody))]
        [SwaggerResponse(404, Description = "Ссылка не найдена")]
        public async Task<IActionResult> GetLinkToJoinHub([FromRoute] Guid hubId)
        {
            var joiningInvitation = await _hubRepository.GetHubJoiningInvitationByHubIdAsync(hubId);
            if (joiningInvitation == null)
                return NotFound();

            var link = Url.Action("GetHubInvitation", "Hub", new { hash = joiningInvitation.HashInvitation }, Request.Scheme);
            return Ok(new HubInvitationLinkBody { Link = link });
        }

        [HttpGet("hub/invitation"), Authorize]
        [SwaggerOperation(Summary = "Получить ссылку на вступление в хаб", Description = "Получить ссылку на вступление в хаб")]
        [SwaggerResponse(200, Description = "Успешное получение ссылки", Type = typeof(HubInvitationLinkBody))]
        [SwaggerResponse(404, Description = "Ссылка не найдена")]
        public async Task<IActionResult> GetHubInvitation(
            [FromHeader(Name = "Authorization")] string? token,
            [FromQuery] string hash)
        {
            var hubInvitation = await _hubRepository.GetHubJoiningInvitationByHashAsync(hash);
            if (hubInvitation == null)
                return NotFound();

            if (token == null)
                return Unauthorized();

            var tokenPayload = _jwtService.GetTokenPayload(token);
            await _hubRepository.AddMemberToHubAsync(hubInvitation.HubId, tokenPayload.AccountId);

            return Ok();
        }
    }
}