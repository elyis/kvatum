using System.Net.WebSockets;
using System.Text.Json;
using KVATUM_STREAMING_SERVICE.Core.Entities.Response;
using KVATUM_STREAMING_SERVICE.Core.Enums;
using KVATUM_STREAMING_SERVICE.Core.IHandler;
using KVATUM_STREAMING_SERVICE.Core.IService;

namespace KVATUM_STREAMING_SERVICE.App.Handler
{
    public class HandleDisconnect : IEventHandler
    {
        private readonly IMainConnectionService _mainConnectionService;
        private readonly IRoomConnectionService _roomConnectionService;
        private readonly ISerializationService _serializationService;

        public HandleDisconnect(
            IMainConnectionService mainConnectionService,
            IRoomConnectionService roomConnectionService,
            ISerializationService serializationService)
        {
            _mainConnectionService = mainConnectionService;
            _roomConnectionService = roomConnectionService;
            _serializationService = serializationService;
        }

        public MessageEvent Event => MessageEvent.Disconnect;

        public async Task HandleAsync(JsonElement message, Guid accountId)
        {
            var roomId = _mainConnectionService.GetCurrentRoomId(accountId);
            if (roomId != null)
                _roomConnectionService.LeaveFromRoom(roomId.Value, accountId);

            var userDisconnectionBody = new UserDisconnectionBody
            {
                EventType = MessageEvent.Disconnect,
                From = accountId
            };

            var serializedMessage = _serializationService.SerializeMessage(userDisconnectionBody);
            await _mainConnectionService.SendMessageToAllExceptSender(accountId, WebSocketMessageType.Text, serializedMessage);
        }
    }
}