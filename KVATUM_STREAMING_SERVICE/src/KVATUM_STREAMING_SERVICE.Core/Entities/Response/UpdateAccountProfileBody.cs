using KVATUM_STREAMING_SERVICE.Core.Enums;

namespace KVATUM_STREAMING_SERVICE.Core.Entities.Response
{
    public class UpdateAccountProfileBody
    {
        public AccountProfileBody AccountProfile { get; set; }
        public MessageEvent EventType { get; set; }
    }
}