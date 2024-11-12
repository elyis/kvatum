using System.Text.Json.Serialization;

namespace KVATUM_STREAMING_SERVICE.Core.Entities.Request
{
    public class RoomConnectionBody
    {
        public Guid RoomId { get; set; }

        [JsonPropertyName("muted")]
        public bool IsMicroMuted { get; set; }

        [JsonPropertyName("camera")]
        public bool IsCameraEnabled { get; set; }
    }
}