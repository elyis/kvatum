using Microsoft.AspNetCore.Mvc;
using KVATUM_AUTH_SERVICE.Core.Entities.Request;
using KVATUM_AUTH_SERVICE.Core.Entities.Response;
using KVATUM_AUTH_SERVICE.Core.Enums;
using KVATUM_AUTH_SERVICE.Core.IService;
using Swashbuckle.AspNetCore.Annotations;

namespace KVATUM_AUTH_SERVICE.Api.Controllers.Api
{
    [ApiController]
    [Route("api")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [SwaggerOperation("Запрос на регистрацию")]
        [SwaggerResponse(200, "Успешно создан", Type = typeof(OutputAccountCredentialsBody))]
        [SwaggerResponse(400, "Токен не валиден или активирован")]
        [SwaggerResponse(409, "Почта уже существует")]

        [HttpPost("signup/request")]
        public async Task<IActionResult> RequestToSignUpAsync(SignUpBody signUpBody)
        {
            var result = await _authService.CreateUnverifiedAccount(signUpBody);
            return StatusCode((int)result);
        }

        [SwaggerOperation("Подтверждение регистрации")]
        [SwaggerResponse(200, "Успешно создан", Type = typeof(OutputAccountCredentialsBody))]
        [SwaggerResponse(400, "Токен не валиден или активирован")]
        [SwaggerResponse(409, "Почта уже существует")]

        [HttpPost("signup/complete")]
        public async Task<IActionResult> SignUpAsync(RegistrationConfirmationBody body)
        {
            string role = Enum.GetName(AccountRole.User)!;
            var context = HttpContext.Request;
            var userAgent = context.Headers.UserAgent;
            var ipAddress = context.Headers["X-Real-IP"];

            var result = await _authService.RegisterNewAccount(body.Email, body.VerificationCode, userAgent, ipAddress, role);
            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, result.Body);

            return StatusCode((int)result.StatusCode, result.Errors);
        }

        [SwaggerOperation("Авторизация")]
        [SwaggerResponse(200, "Успешно", Type = typeof(OutputAccountCredentialsBody))]
        [SwaggerResponse(400, "Пароли не совпадают")]
        [SwaggerResponse(404, "Email не зарегистрирован")]
        [SwaggerResponse(409, "Аккаунт создан другим провайдером")]

        [HttpPost("signin")]
        public async Task<IActionResult> SignInAsync(SignInBody signInBody)
        {
            var context = HttpContext.Request;
            var userAgent = context.Headers.UserAgent;
            var ipAddress = context.Headers["X-Real-IP"];

            var result = await _authService.SignIn(signInBody, userAgent, ipAddress);
            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, result.Body);

            return StatusCode((int)result.StatusCode, result.Errors);
        }

        [SwaggerOperation("Восстановление токена")]
        [SwaggerResponse(200, "Успешно создан", Type = typeof(OutputAccountCredentialsBody))]
        [SwaggerResponse(400, "Идентификатор устройства не валиден")]
        [SwaggerResponse(404, "Токен не используется")]

        [HttpPost("refresh")]
        public async Task<IActionResult> RestoreTokenAsync(TokenBody body)
        {
            var result = await _authService.RestoreAccessToken(body.Value);
            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, result.Body);

            return StatusCode((int)result.StatusCode, result.Errors);
        }
    }
}