using System.ComponentModel.DataAnnotations;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KVATUM_AUTH_SERVICE.Core.Entities.Response;
using KVATUM_AUTH_SERVICE.Core.IService;
using Swashbuckle.AspNetCore.Annotations;

namespace KVATUM_AUTH_SERVICE.Api.Controllers.Api
{
    [ApiController]
    [Route("api")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IJwtService _jwtService;

        public UserController(
            IUserService userService,
            IJwtService jwtService)
        {
            _userService = userService;
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
            var response = await _userService.GetProfile(tokenInfo.AccountId);

            if (response.IsSuccess)
                return StatusCode((int)response.StatusCode, response.Body);

            return StatusCode((int)response.StatusCode);
        }

        [HttpPatch("profile/nickname"), Authorize]
        [SwaggerOperation("Изменить никнейм")]
        [SwaggerResponse(200)]
        [SwaggerResponse(400)]

        public async Task<IActionResult> ChangeNickname(
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
            [FromQuery, Required] string nickname
        )
        {
            var tokenInfo = _jwtService.GetTokenPayload(token);
            var response = await _userService.ChangeAccountNickname(tokenInfo.AccountId, nickname);
            return StatusCode((int)response);
        }

        [HttpGet("profile/nickname")]
        [SwaggerOperation("Получить профиль пользователя по никнейму")]
        [SwaggerResponse(200, Type = typeof(ProfileBody))]
        [SwaggerResponse(404)]

        public async Task<IActionResult> GetAccountByNickname([FromQuery, Required] string name)
        {
            var response = await _userService.GetProfileByNickname(name);
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
            var response = await _userService.GetAccountsByPatternNickname(pattern, limit, offset);
            if (response.IsSuccess)
                return StatusCode((int)response.StatusCode, response.Body);

            return StatusCode((int)response.StatusCode, response.Errors);
        }


        [HttpGet("user")]
        [SwaggerOperation("Получить профиль пользователя")]
        [SwaggerResponse(200, Type = typeof(ProfileBody))]
        [SwaggerResponse(404)]

        public async Task<IActionResult> GetProfileByEmail(
            [FromQuery, Required] string email
        )
        {
            var response = await _userService.GetProfileByEmail(email);
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
            var response = await _userService.GetProfile(id);
            if (response.IsSuccess)
                return StatusCode((int)response.StatusCode, response.Body);

            return StatusCode((int)response.StatusCode, response.Errors);
        }
    }
}