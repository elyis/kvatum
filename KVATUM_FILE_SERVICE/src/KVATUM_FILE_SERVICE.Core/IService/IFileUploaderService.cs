using KVATUM_FILE_SERVICE.Core.Entities.Response;

namespace KVATUM_FILE_SERVICE.Core.IService
{
    public interface IFileUploaderService
    {
        Task<ServiceResponse<string>> UploadFileAsync(string directoryPath, Stream stream, string[] supportedExtensions);
        Task<ServiceResponse<UploadedResult>> GetStreamAsync(string directoryPath, string filename);
        Task<ServiceResponse<UploadedResult>> GetImageAsync(string directoryPath, string filename, bool isWebp = false);
    }
}