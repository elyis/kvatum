using System.Net.WebSockets;
using System.Text.Json;
using KVATUM_STREAMING_SERVICE.Core.Entities.Response;
using KVATUM_STREAMING_SERVICE.Core.Enums;
using KVATUM_STREAMING_SERVICE.Core.IHandler;
using KVATUM_STREAMING_SERVICE.Core.IService;

namespace KVATUM_STREAMING_SERVICE.App.Handler
{
    public class HandleChangeMicroState : IEventHandler
    {
        private readonly IMainConnectionService _mainConnectionService;
        private readonly ISerializationService _serializationService;

        public HandleChangeMicroState(
            IMainConnectionService mainConnectionService,
            ISerializationService serializationService)
        {
            _mainConnectionService = mainConnectionService;
            _serializationService = serializationService;
        }

        public MessageEvent Event => MessageEvent.ChangeMicroState;

        public async Task HandleAsync(JsonElement message, Guid accountId)
        {
            bool isMicroEnabled = _serializationService.DeserializeMessage<bool>(message);
            _mainConnectionService.UpdateMicroState(accountId, isMicroEnabled);

            var userChangeMicroStateBody = new UserChangeMicroStateBody
            {
                EventType = MessageEvent.ChangeMicroState,
                From = accountId,
                IsMicroMuted = !isMicroEnabled
            };
            var serializedMessage = _serializationService.SerializeMessage(userChangeMicroStateBody);

            await _mainConnectionService.SendMessageToAllExceptSender(accountId, WebSocketMessageType.Text, serializedMessage);
        }
    }
}