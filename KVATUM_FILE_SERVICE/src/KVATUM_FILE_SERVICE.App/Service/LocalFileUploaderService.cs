using System.Net;
using MimeDetective;
using KVATUM_FILE_SERVICE.Core.Entities.Response;
using KVATUM_FILE_SERVICE.Core.IService;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace KVATUM_FILE_SERVICE.App.Service
{
    public class LocalFileUploaderService : IFileUploaderService
    {
        private readonly ContentInspector _contentInspector;
        private readonly string[] _supportedImageExtensionsForConvertingToWebp = new string[]
        {
            "jpg",
            "jpeg",
            "jfif",
            "png",
            "gif",
        };

        public LocalFileUploaderService(ContentInspector contentInspector)
        {
            _contentInspector = contentInspector;
        }

        public async Task<ServiceResponse<string>> UploadFileAsync(
            string directoryPath,
            Stream stream,
            string[] supportedExtensions)
        {
            try
            {
                if (stream == null || stream.Length == 0)
                    return new ServiceResponse<string>()
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        IsSuccess = false,
                        Errors = new string[] { "Stream is empty" }
                    };

                var result = _contentInspector.Inspect(stream);
                var fileExtension = result.MaxBy(e => e.Points)?.Definition.File.Extensions.First();
                if (fileExtension == null || !supportedExtensions.Contains(fileExtension))
                    return new ServiceResponse<string>()
                    {
                        StatusCode = HttpStatusCode.UnsupportedMediaType,
                        IsSuccess = false,
                        Errors = new string[] { "Unsupported file extension" }
                    };


                if (!Directory.Exists(directoryPath))
                    Directory.CreateDirectory(directoryPath);

                string originalFilename = SaveOriginalFile(directoryPath, stream, fileExtension);

                if (_supportedImageExtensionsForConvertingToWebp.Contains(fileExtension))
                    await SaveImageAs(directoryPath, stream, originalFilename, "webp", new WebpEncoder());
                else if (fileExtension == "webp")
                    await SaveImageAs(directoryPath, stream, originalFilename, "jpeg", new JpegEncoder());

                stream.Seek(0, SeekOrigin.Begin);

                return new ServiceResponse<string>()
                {
                    StatusCode = HttpStatusCode.OK,
                    IsSuccess = true,
                    Body = originalFilename
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<string>()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    Errors = new string[] { "File upload failed" }
                };
            }
        }

        private string SaveOriginalFile(string directoryPath, Stream stream, string fileExtension)
        {
            stream.Seek(0, SeekOrigin.Begin);
            string uuid = Guid.NewGuid().ToString();
            string filename = $"{uuid}.{fileExtension}";
            string fullPathToFile = Path.Combine(directoryPath, filename);

            using (var file = File.Create(fullPathToFile))
            {
                stream.Seek(0, SeekOrigin.Begin);
                stream.CopyTo(file);
            }

            return filename;
        }

        private async Task SaveImageAs(string directoryPath, Stream stream, string originalFilename, string fileExtension, ImageEncoder encoder)
        {
            stream.Seek(0, SeekOrigin.Begin);
            string tempFilename = Path.ChangeExtension(originalFilename, fileExtension);
            string tempFullPath = Path.Combine(directoryPath, tempFilename);


            using var image = Image.Load(stream);
            using var memoryStream = new MemoryStream();
            image.Save(memoryStream, encoder);
            memoryStream.Seek(0, SeekOrigin.Begin);

            using var tempFile = File.Create(tempFullPath);
            await memoryStream.CopyToAsync(tempFile);
        }

        public async Task<ServiceResponse<UploadedResult>> GetStreamAsync(string directoryPath, string filename)
        {
            string fullPathToFile = Path.Combine(directoryPath, filename);
            if (!File.Exists(fullPathToFile))
                return new ServiceResponse<UploadedResult>()
                {
                    StatusCode = HttpStatusCode.NotFound,
                    IsSuccess = false,
                    Errors = new string[] { "File not found" }
                };

            string fileExtension = Path.GetExtension(filename);
            string filenameWithoutExtension = Path.GetFileNameWithoutExtension(filename);
            bool isImage = _supportedImageExtensionsForConvertingToWebp.Contains(fileExtension);
            if (isImage)
            {
                string webpFilename = $"{filenameWithoutExtension}.webp";

                var tempPath = Path.Combine(directoryPath, webpFilename);
                if (File.Exists(tempPath))
                {
                    fullPathToFile = tempPath;
                    fileExtension = "webp";
                }
            }

            Stream fileStream = File.OpenRead(fullPathToFile);
            return new ServiceResponse<UploadedResult>()
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Body = new UploadedResult
                {
                    Stream = fileStream,
                    IsImage = isImage,
                    FileExtension = fileExtension
                }
            };
        }

        public async Task<ServiceResponse<UploadedResult>> GetImageAsync(string directoryPath, string filename, bool isWebp = false)
        {
            if (isWebp)
            {
                var filenameWithoutExtension = Path.GetFileNameWithoutExtension(filename);
                if (filenameWithoutExtension == null)
                {
                    return new ServiceResponse<UploadedResult>()
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        IsSuccess = false,
                        Errors = new string[] { "File not found" }
                    };
                }

                var tempFilename = $"{filenameWithoutExtension}.webp";
                var tempPath = Path.Combine(directoryPath, tempFilename);
                if (File.Exists(tempPath))
                    filename = tempFilename;
            }

            return await GetStreamAsync(directoryPath, filename);
        }
    }
}