using System.Text.Json;

namespace KVATUM_STREAMING_SERVICE.Core.IService
{
    public interface ISerializationService
    {
        byte[] SerializeMessage(object message);
        T? DeserializeMessage<T>(byte[] message);
        T? DeserializeMessage<T>(JsonElement message);
    }
}