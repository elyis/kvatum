namespace KVATUM_FILE_SERVICE.Core.Entities.Response
{
    public class UploadedResult
    {
        public Stream Stream { get; set; }
        public bool IsImage { get; set; } = true;
        public string FileExtension { get; set; }
    }
}