using KVATUM_STREAMING_SERVICE.Core.Enums;

namespace KVATUM_STREAMING_SERVICE.Core.Entities.Response
{
    public class UserChangeMicroStateBody
    {
        public MessageEvent EventType { get; set; }
        public Guid From { get; set; }
        public bool IsMicroMuted { get; set; }
    }
}