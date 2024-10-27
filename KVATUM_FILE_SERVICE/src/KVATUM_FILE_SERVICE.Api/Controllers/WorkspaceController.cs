using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    public class WorkspaceController : ControllerBase
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
            "svg",
            "webp"
        };

        public WorkspaceController(
            IFileUploaderService fileUploaderService,
            INotifyService notifyService)
        {
            _fileUploaderService = fileUploaderService;
            _notifyService = notifyService;
        }

        [HttpPost("api/workspace/upload"), Authorize]
        [SwaggerOperation(Summary = "Загрузка изображения профиля", Description = "Загрузка изображения профиля")]
        [SwaggerResponse(StatusCodes.Status200OK, "Изображение профиля загружено")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Пользователь не авторизован")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Некорректные данные")]
        [SwaggerResponse(StatusCodes.Status415UnsupportedMediaType, "Некорректный формат файла")]
        public async Task<IActionResult> UploadFile(
            [SwaggerParameter(Description = "Файл изображения", Required = true)] IFormFile file,
            [FromQuery] Guid workspaceId)
        {
            var uploadResult = await _fileUploaderService.UploadFileAsync(
                Constants.LocalPathToWorkspaceIcons,
                file.OpenReadStream(),
                _supportedImageExtensions);

            if (!uploadResult.IsSuccess)
                return StatusCode((int)uploadResult.StatusCode, uploadResult.Errors);

            var body = new WorkspaceUploadEvent
            {
                WorkspaceId = workspaceId,
                FileName = uploadResult.Body
            };
            _notifyService.Publish(body, ContentUploaded.WorkspaceIcon);
            return Ok();
        }

        [HttpGet("/api/images/workspace/{filename}")]
        [SwaggerOperation(Summary = "Скачивание изображения профиля", Description = "Скачивание изображения профиля")]
        [SwaggerResponse(StatusCodes.Status200OK, "Изображение профиля скачано", Type = typeof(FileStreamResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Изображение профиля не найдено")]
        public async Task<IActionResult> Download(
            string filename,
            [FromQuery] int width,
            [FromQuery] int height)
        {
            bool isSupportedWebp = Request.Headers["Accept"].ToString().Contains("image/webp");

            var response = await _fileUploaderService.GetImageAsync(Constants.LocalPathToWorkspaceIcons, filename, isSupportedWebp);
            if (!response.IsSuccess)
                return StatusCode((int)response.StatusCode, response.Errors);

            var contentType = response.Body.IsImage ? $"image/{response.Body.FileExtension}" : "application/octet-stream";
            return File(response.Body.Stream, contentType, filename);
        }
    }
}