namespace KVATUM_CHATFLOW_SERVICE.Core.Entities.Models
{
    public class WorkspaceChat
    {
        public Guid WorkspaceId { get; set; }
        public Guid ChatId { get; set; }
        public Chat Chat { get; set; }
    }
}