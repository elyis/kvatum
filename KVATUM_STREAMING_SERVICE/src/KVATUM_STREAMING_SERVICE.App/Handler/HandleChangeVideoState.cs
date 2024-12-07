using System.Net.WebSockets;
using System.Text.Json;
using KVATUM_STREAMING_SERVICE.Core.Entities.Response;
using KVATUM_STREAMING_SERVICE.Core.Enums;
using KVATUM_STREAMING_SERVICE.Core.IHandler;
using KVATUM_STREAMING_SERVICE.Core.IService;

namespace KVATUM_STREAMING_SERVICE.App.Handler
{
    public class HandleChangeVideoState : IEventHandler
    {
        private readonly IMainConnectionService _mainConnectionService;
        private readonly ISerializationService _serializationService;

        public HandleChangeVideoState(
            IMainConnectionService mainConnectionService,
            ISerializationService serializationService)
        {
            _mainConnectionService = mainConnectionService;
            _serializationService = serializationService;
        }

        public MessageEvent Event => MessageEvent.ChangeVideoState;

        public async Task HandleAsync(JsonElement message, Guid accountId)
        {
            bool isVideoEnabled = _serializationService.DeserializeMessage<bool>(message);
            _mainConnectionService.UpdateVideoState(accountId, isVideoEnabled);

            var userChangeVideoStateBody = new UserChangeVideoStateBody
            {
                EventType = MessageEvent.ChangeVideoState,
                From = accountId,
                HasVideo = isVideoEnabled
            };

            var serializedMessage = _serializationService.SerializeMessage(userChangeVideoStateBody);
            await _mainConnectionService.SendMessageToAllExceptSender(accountId, WebSocketMessageType.Text, serializedMessage);
        }
    }
}