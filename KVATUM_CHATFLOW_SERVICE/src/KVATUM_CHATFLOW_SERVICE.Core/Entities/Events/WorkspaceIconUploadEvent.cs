namespace KVATUM_CHATFLOW_SERVICE.Core.Entities.Events
{
    public class WorkspaceIconUploadEvent
    {
        public Guid WorkspaceId { get; set; }
        public string FileName { get; set; }
    }
}