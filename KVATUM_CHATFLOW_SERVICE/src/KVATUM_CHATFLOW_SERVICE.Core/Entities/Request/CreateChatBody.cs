using KVATUM_CHATFLOW_SERVICE.Core.Enums;

namespace KVATUM_CHATFLOW_SERVICE.Core.Entities.Request
{
    public class CreateChatBody
    {
        public string Name { get; set; }
        public ChatType Type { get; set; }
        public Guid WorkspaceId { get; set; }
    }
}