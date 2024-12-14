namespace KVATUM_CHATFLOW_SERVICE.Core.Entities.Request
{
    public class ChatAttachmentToWorkspaceBody
    {
        public Guid ChatId { get; set; }
        public Guid WorkspaceId { get; set; }
    }
}