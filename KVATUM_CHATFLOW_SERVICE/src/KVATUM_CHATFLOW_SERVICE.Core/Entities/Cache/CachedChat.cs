using KVATUM_CHATFLOW_SERVICE.Core.Entities.Response;
using KVATUM_CHATFLOW_SERVICE.Core.Enums;

namespace KVATUM_CHATFLOW_SERVICE.Core.Entities.Cache
{
    public class CachedChat
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }

        public ChatBody ToChatBody()
        {
            return new ChatBody
            {
                Id = Id,
                Name = Name,
                Type = Enum.Parse<ChatType>(Type)
            };
        }
    }
}