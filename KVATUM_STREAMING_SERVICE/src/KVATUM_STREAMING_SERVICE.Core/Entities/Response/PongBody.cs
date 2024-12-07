using KVATUM_STREAMING_SERVICE.Core.Enums;

namespace KVATUM_STREAMING_SERVICE.Core.Entities.Response
{
    public class PongBody
    {
        public MessageEvent EventType => MessageEvent.Pong;
        public string Message { get; set; }
    }
}