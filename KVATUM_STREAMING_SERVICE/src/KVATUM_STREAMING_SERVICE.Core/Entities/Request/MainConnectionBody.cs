using System.Net.WebSockets;

namespace KVATUM_STREAMING_SERVICE.Core.Entities.Request
{
    public class MainConnectionBody
    {
        public WebSocket Socket { get; set; }
        public bool IsMicroEnabled { get; set; }
        public bool IsVideoEnabled { get; set; }
        public Guid? CurrentRoomId { get; set; }
    }
}