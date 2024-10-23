using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KVATUM_FILE_SERVICE.Core;
using KVATUM_FILE_SERVICE.Core.Enums;
using KVATUM_FILE_SERVICE.Core.IService;
using Swashbuckle.AspNetCore.Annotations;
using KVATUM_FILE_SERVICE.Core.Entities.Events;

namespace KVATUM_FILE_SERVICE.Api.Controllers
{
    [ApiController]
    [Route("")]
    public class ProfileController : ControllerBase
    {
        private readonly IFileUploaderService _fileUploaderService;
        private readonly INotifyService _notifyService;
        private readonly IJwtService _jwtService;
        private readonly string[] _supportedImageExtensions = new string[]
        {
            "gif",
            "jpg",
            "jpeg",
            "jfif",
            "png",
            "svg",
            "webp"
        };

        public ProfileController(
            IFileUploaderService fileUploaderService,
            INotifyService notifyService,
            IJwtService jwtService)
        {
            _fileUploaderService = fileUploaderService;
            _notifyService = notifyService;
            _jwtService = jwtService;
        }

        [HttpPost("api/profile/upload"), Authorize]
        [SwaggerOperation(Summary = "Загрузка изображения профиля", Description = "Загрузка изображения профиля")]
        [SwaggerResponse(StatusCodes.Status200OK, "Изображение профиля загружено")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Пользователь не авторизован")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Некорректные данные")]
        [SwaggerResponse(StatusCodes.Status415UnsupportedMediaType, "Некорректный формат файла")]
        public async Task<IActionResult> UploadFile(
            [SwaggerParameter(Description = "Файл изображения", Required = true)] IFormFile file,
            [FromHeader(Name = "Authorization")] string token)
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var uploadResult = await _fileUploaderService.UploadFileAsync(
                Constants.LocalPathToProfileImages,
                file.OpenReadStream(),
                _supportedImageExtensions);
            if (!uploadResult.IsSuccess)
                return StatusCode((int)uploadResult.StatusCode, uploadResult.Errors);

            var body = new ProfileImageUploadEvent
            {
                AccountId = tokenPayload.AccountId,
                FileName = uploadResult.Body
            };
            _notifyService.Publish(body, ContentUploaded.ProfileImage);
            return Ok();
        }

        [HttpGet("/api/images/profile/{filename}")]
        [SwaggerOperation(Summary = "Скачивание изображения профиля", Description = "Скачивание изображения профиля")]
        [SwaggerResponse(StatusCodes.Status200OK, "Изображение профиля скачано", Type = typeof(FileStreamResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Изображение профиля не найдено")]
        public async Task<IActionResult> Download(
            string filename,
            [FromQuery] int width,
            [FromQuery] int height)
        {
            bool isSupportedWebp = Request.Headers["Accept"].ToString().Contains("image/webp");

            var response = await _fileUploaderService.GetImageAsync(Constants.LocalPathToProfileImages, filename, isSupportedWebp);
            if (!response.IsSuccess)
                return StatusCode((int)response.StatusCode, response.Errors);

            var contentType = response.Body.IsImage ? $"image/{response.Body.FileExtension}" : "application/octet-stream";
            return File(response.Body.Stream, contentType, filename);
        }
    }
}