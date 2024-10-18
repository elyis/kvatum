using KVATUM_CHATFLOW_SERVICE.Core.Enums;

namespace KVATUM_CHATFLOW_SERVICE.Core.Entities.Response
{
    public class ChatBody
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public ChatType Type { get; set; }
    }
}