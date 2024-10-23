namespace KVATUM_FILE_SERVICE.Core.Entities.Events
{
    public class ProfileImageUploadEvent
    {
        public Guid AccountId { get; set; }
        public string FileName { get; set; }
    }
}