using KVATUM_FILE_SERVICE.Core;
using KVATUM_FILE_SERVICE.Core.Entities.Events;
using KVATUM_FILE_SERVICE.Core.Enums;
using KVATUM_FILE_SERVICE.Core.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace KVATUM_FILE_SERVICE.Api.Controllers
{
    [ApiController]
    [Route("")]
    public class HubController : ControllerBase
    {
        private readonly IFileUploaderService _fileUploaderService;
        private readonly INotifyService _notifyService;
        private readonly string[] _supportedImageExtensions = new string[]
        {
            "gif",
            "jpg",
            "jpeg",
            "jfif",
            "png",
            "svg"
        };

        public HubController(
            IFileUploaderService fileUploaderService,
            INotifyService notifyService)
        {
            _fileUploaderService = fileUploaderService;
            _notifyService = notifyService;
        }

        [HttpPost("api/hub/upload"), Authorize]
        [SwaggerOperation(Summary = "Загрузка иконки сервера", Description = "Загрузка иконки сервера")]
        [SwaggerResponse(StatusCodes.Status200OK, "Иконка сервера загружена")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Пользователь не авторизован")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Некорректные данные")]
        [SwaggerResponse(StatusCodes.Status415UnsupportedMediaType, "Некорректный формат файла")]
        public async Task<IActionResult> UploadFile(
            [SwaggerParameter(Description = "Иконка сервера", Required = true)] IFormFile file,
            [FromQuery] Guid hubId)
        {
            var uploadResult = await _fileUploaderService.UploadFileAsync(
                Constants.LocalPathToHubIcons,
                file.OpenReadStream(),
                _supportedImageExtensions);
            if (!uploadResult.IsSuccess)
                return StatusCode((int)uploadResult.StatusCode, uploadResult.Errors);

            var body = new HubIconUploadEvent
            {
                HubId = hubId,
                FileName = uploadResult.Body
            };
            _notifyService.Publish(body, ContentUploaded.HubIcon);
            return Ok();
        }

        [HttpGet("/api/images/hub/{filename}")]
        [SwaggerOperation(Summary = "Скачивание иконки сервера", Description = "Скачивание иконки сервера")]
        [SwaggerResponse(StatusCodes.Status200OK, "Иконка сервера скачана", Type = typeof(FileStreamResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Иконка сервера не найдена")]
        public async Task<IActionResult> Download(
            string filename,
            [FromQuery] int width,
            [FromQuery] int height)
        {
            bool isSupportedWebp = Request.Headers["Accept"].ToString().Contains("image/webp");

            var response = await _fileUploaderService.GetImageAsync(Constants.LocalPathToHubIcons, filename, isSupportedWebp);
            if (!response.IsSuccess)
                return StatusCode((int)response.StatusCode, response.Errors);

            var contentType = response.Body.IsImage ? $"image/{response.Body.FileExtension}" : "application/octet-stream";
            return File(response.Body.Stream, contentType, filename);
        }
    }
}