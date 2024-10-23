namespace KVATUM_CHATFLOW_SERVICE.Core.Entities.Events
{
    public class HubIconUploadEvent
    {
        public Guid HubId { get; set; }
        public string FileName { get; set; }
    }
}