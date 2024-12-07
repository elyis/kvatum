using System.Net.WebSockets;
using System.Text.Json;
using KVATUM_STREAMING_SERVICE.Core.Entities.Response;
using KVATUM_STREAMING_SERVICE.Core.Enums;
using KVATUM_STREAMING_SERVICE.Core.IHandler;
using KVATUM_STREAMING_SERVICE.Core.IService;

namespace KVATUM_STREAMING_SERVICE.App.Handler
{
    public class HandlePing : IEventHandler
    {
        private readonly ISerializationService _serializationService;
        private readonly IMainConnectionService _mainConnectionService;

        public HandlePing(
            ISerializationService serializationService,
            IMainConnectionService mainConnectionService)
        {
            _serializationService = serializationService;
            _mainConnectionService = mainConnectionService;
        }

        public MessageEvent Event => MessageEvent.Ping;

        public async Task HandleAsync(JsonElement message, Guid accountId)
        {
            var pingBody = _serializationService.DeserializeMessage<string>(message);
            if (pingBody != null)
            {
                var pongBody = new PongBody
                {
                    Message = pingBody
                };

                var serializedMessage = _serializationService.SerializeMessage(pongBody);
                await _mainConnectionService.SendMessage(accountId, WebSocketMessageType.Text, serializedMessage);
            }
        }
    }
}