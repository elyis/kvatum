namespace KVATUM_FILE_SERVICE.Core.Entities.Events
{
    public class WorkspaceUploadEvent
    {
        public Guid WorkspaceId { get; set; }
        public string FileName { get; set; }
    }
}