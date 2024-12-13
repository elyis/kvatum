using System.ComponentModel.DataAnnotations;
using KVATUM_AUTH_SERVICE.Core.Entities.Response;
using KVATUM_AUTH_SERVICE.Core.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace KVATUM_AUTH_SERVICE.Api.Controllers.Api
{
    [ApiController]
    [Route("api")]
    public class AccountSessionController : ControllerBase
    {
        private readonly IAccountSessionService _accountSessionService;
        private readonly IJwtService _jwtService;

        public AccountSessionController(
            IAccountSessionService accountSessionService,
            IJwtService jwtService)
        {
            _accountSessionService = accountSessionService;
            _jwtService = jwtService;
        }


        [HttpGet("sessions"), Authorize]
        [SwaggerOperation("Получить список сессий входа")]
        [SwaggerResponse(200, Description = "Успешно", Type = typeof(List<AccountSessionBody>))]
        public async Task<IActionResult> GetAllAccountSessionsAsync(
            [FromHeader(Name = "Authorization")] string token,
            [FromQuery, Range(1, 100)] int limit = 5,
            [FromQuery, Range(0, 1000)] int offset = 0)
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var response = await _accountSessionService.GetAllAccountSessionsAsync(tokenPayload.AccountId, limit, offset);
            if (!response.IsSuccess)
                return StatusCode((int)response.StatusCode, response.Body);

            return Ok(response.Body);
        }


        [HttpDelete("session/{sessionId}"), Authorize]
        [SwaggerOperation("Удалить сессию")]
        [SwaggerResponse(204, Description = "Успешно")]
        [SwaggerResponse(403, Description = "Удаление не своей сессии")]
        public async Task<IActionResult> RemoveAccountSession(
            [FromHeader(Name = "Authorization")] string token,
            [FromRoute] Guid sessionId)
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var responseCode = await _accountSessionService.RemoveSessionAsync(sessionId, tokenPayload.AccountId);
            return StatusCode((int)responseCode);
        }
    }
}