using System.Text.Json;
using KVATUM_STREAMING_SERVICE.Core.Enums;

namespace KVATUM_STREAMING_SERVICE.Core.Entities.Request
{
    public class ReceivedMessageEvent
    {
        public MessageEvent EventType { get; set; }
        public JsonElement EventBody { get; set; }
    }
}