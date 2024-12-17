using System.Net.WebSockets;
using System.Text.Json;
using KVATUM_STREAMING_SERVICE.Core.Entities.Request;
using KVATUM_STREAMING_SERVICE.Core.Entities.Response;
using KVATUM_STREAMING_SERVICE.Core.Enums;
using KVATUM_STREAMING_SERVICE.Core.IHandler;
using KVATUM_STREAMING_SERVICE.Core.IService;
using Microsoft.Extensions.Logging;

namespace KVATUM_STREAMING_SERVICE.App.Handler
{
    public class HandleJoinToRoom : IEventHandler
    {
        private readonly IMainConnectionService _mainConnectionService;
        private readonly IRoomConnectionService _roomConnectionService;
        private readonly ISerializationService _serializationService;
        private readonly ILogger<HandleJoinToRoom> _logger;

        public HandleJoinToRoom(
            IMainConnectionService mainConnectionService,
            IRoomConnectionService roomConnectionService,
            ISerializationService serializationService,
            ILogger<HandleJoinToRoom> logger)
        {
            _mainConnectionService = mainConnectionService;
            _roomConnectionService = roomConnectionService;
            _serializationService = serializationService;
            _logger = logger;
        }

        public MessageEvent Event => MessageEvent.JoinToRoom;

        public async Task HandleAsync(JsonElement message, Guid accountId)
        {
            var roomConnectionBody = _serializationService.DeserializeMessage<RoomConnectionBody>(message);
            if (roomConnectionBody == null)
                return;

            _mainConnectionService.UpdateConnection(
                accountId,
                !roomConnectionBody.IsMicroMuted,
                roomConnectionBody.IsCameraEnabled,
                roomConnectionBody.RoomId);

            var members = _roomConnectionService.JoinToRoomAndGetMembers(roomConnectionBody.RoomId, accountId).Where(m => m != accountId);

            var connections = _mainConnectionService.GetConnections(members);
            var accountList = new AccountList
            {
                EventType = MessageEvent.UserList,
                Users = connections
            };
            var serializedAccountList = _serializationService.SerializeMessage(accountList);

            bool isSent = await _mainConnectionService.SendMessage(accountId, WebSocketMessageType.Text, serializedAccountList);
            if (!isSent)
                throw new Exception($"Error sending message to account {accountId}");

            _logger.LogInformation($"Account {accountId} joined to room {roomConnectionBody.RoomId}");
        }
    }
}