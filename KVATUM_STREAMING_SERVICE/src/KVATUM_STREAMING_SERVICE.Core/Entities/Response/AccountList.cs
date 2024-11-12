using KVATUM_STREAMING_SERVICE.Core.Enums;

namespace KVATUM_STREAMING_SERVICE.Core.Entities.Response
{
    public class AccountList
    {
        public MessageEvent EventType { get; set; }
        public IEnumerable<UserConnectionBody> Users { get; set; } = new List<UserConnectionBody>();
    }
}