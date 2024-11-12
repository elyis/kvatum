using System.Text.Json.Serialization;

namespace KVATUM_STREAMING_SERVICE.Core.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum MessageEvent
    {
        UserList,
        Token,
        JoinToRoom,
        ChangeMicroState,
        ChangeVideoState,
        Offer,
        UpdateOffer,
        IceCandidate,
        Answer,
        UpdateAnswer,
        Disconnect
    }
}