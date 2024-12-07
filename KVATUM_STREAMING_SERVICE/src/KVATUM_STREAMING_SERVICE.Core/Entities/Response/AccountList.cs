using KVATUM_STREAMING_SERVICE.Core.Enums;

namespace KVATUM_STREAMING_SERVICE.Core.Entities.Response
{
    public class AccountList
    {
        public MessageEvent EventType { get; set; }
        public IEnumerable<AccountConnectionBody> Users { get; set; } = new List<AccountConnectionBody>();
    }
}