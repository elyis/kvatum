using System.ComponentModel.DataAnnotations;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KVATUM_AUTH_SERVICE.Core.Entities.Response;
using KVATUM_AUTH_SERVICE.Core.IService;
using Swashbuckle.AspNetCore.Annotations;
using KVATUM_AUTH_SERVICE.Core.Entities.Request;

namespace KVATUM_AUTH_SERVICE.Api.Controllers.Api
{
    [ApiController]
    [Route("api")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly IJwtService _jwtService;

        public AccountController(
            IAccountService accountService,
            IJwtService jwtService)
        {
            _accountService = accountService;
            _jwtService = jwtService;
        }

        [HttpGet("profile"), Authorize]
        [SwaggerOperation("Получить профиль")]
        [SwaggerResponse(200, Description = "Успешно", Type = typeof(ProfileBody))]
        public async Task<IActionResult> GetProfileAsync(
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token
        )
        {
            var tokenInfo = _jwtService.GetTokenPayload(token);
            var response = await _accountService.GetProfile(tokenInfo.AccountId);

            if (response.IsSuccess)
                return StatusCode((int)response.StatusCode, response.Body);

            return StatusCode((int)response.StatusCode);
        }

        [HttpPatch("profile/nickname"), Authorize]
        [SwaggerOperation("Изменить никнейм")]
        [SwaggerResponse(200)]
        [SwaggerResponse(400)]
        [SwaggerResponse(409, Description = "Nickname already taken")]

        public async Task<IActionResult> ChangeNickname(
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
            [Required] ChangeNicknameBody body
        )
        {
            var tokenInfo = _jwtService.GetTokenPayload(token);
            var response = await _accountService.ChangeAccountNickname(tokenInfo.AccountId, body.Nickname);
            return StatusCode((int)response);
        }

        [HttpGet("profile/nickname")]
        [SwaggerOperation("Получить профиль пользователя по никнейму")]
        [SwaggerResponse(200, Type = typeof(ProfileBody))]
        [SwaggerResponse(404)]

        public async Task<IActionResult> GetAccountByNickname([FromQuery, Required] string name)
        {
            var response = await _accountService.GetProfileByNickname(name);
            if (response.IsSuccess)
                return StatusCode((int)response.StatusCode, response.Body);

            return StatusCode((int)response.StatusCode, response.Errors);
        }

        [HttpGet("users/nickname")]
        [SwaggerOperation("Получить список пользователей по паттерну никнейма")]
        [SwaggerResponse(200, Type = typeof(List<ProfileBody>))]

        public async Task<IActionResult> GetUsersByPatternUserTag(
            [FromQuery, Required] string pattern,
            [FromQuery, Range(1, 100)] int limit,
            [FromQuery, Range(0, int.MaxValue)] int offset)
        {
            var response = await _accountService.GetAccountsByPatternNickname(pattern, limit, offset);
            if (response.IsSuccess)
                return StatusCode((int)response.StatusCode, response.Body);

            return StatusCode((int)response.StatusCode, response.Errors);
        }


        [HttpGet("user")]
        [SwaggerOperation("Получить профиль пользователя")]
        [SwaggerResponse(200, Type = typeof(ProfileBody))]
        [SwaggerResponse(404)]

        public async Task<IActionResult> GetProfileByEmail(
            [FromQuery, EmailAddress] string email
        )
        {
            var response = await _accountService.GetProfileByEmail(email);
            if (response.IsSuccess)
                return StatusCode((int)response.StatusCode, response.Body);

            return StatusCode((int)response.StatusCode, response.Errors);
        }


        [HttpGet("user/{id}")]
        [SwaggerOperation("Получить профиль пользователя по id")]
        [SwaggerResponse(200, Type = typeof(ProfileBody))]
        [SwaggerResponse(404)]

        public async Task<IActionResult> GetUserInfo(
            [FromRoute, Required] Guid id
        )
        {
            var response = await _accountService.GetProfile(id);
            if (response.IsSuccess)
                return StatusCode((int)response.StatusCode, response.Body);

            return StatusCode((int)response.StatusCode, response.Errors);
        }
    }
}