using System.Text.Json;
using KVATUM_STREAMING_SERVICE.Core.IService;

namespace KVATUM_STREAMING_SERVICE.App.Service
{
    public class SerializationService : ISerializationService
    {
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public SerializationService()
        {
            _jsonSerializerOptions = new()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };
        }

        public T? DeserializeMessage<T>(byte[] message)
        {
            return JsonSerializer.Deserialize<T>(message, _jsonSerializerOptions);
        }

        public T? DeserializeMessage<T>(JsonElement message)
        {
            var json = message.GetRawText();
            return JsonSerializer.Deserialize<T>(json, _jsonSerializerOptions);
        }

        public byte[] SerializeMessage(object message)
        {
            return JsonSerializer.SerializeToUtf8Bytes(message, _jsonSerializerOptions);
        }
    }
}