namespace KVATUM_CHATFLOW_SERVICE.Core.Entities.Response
{
    public class WorkspaceChatsBody
    {
        public Guid WorkspaceId { get; set; }
        public List<ChatBody> Chats { get; set; } = new();
    }
}