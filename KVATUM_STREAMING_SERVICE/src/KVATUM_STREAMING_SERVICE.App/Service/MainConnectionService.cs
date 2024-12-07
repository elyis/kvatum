using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using KVATUM_STREAMING_SERVICE.Core.Entities.Request;
using KVATUM_STREAMING_SERVICE.Core.Entities.Response;
using KVATUM_STREAMING_SERVICE.Core.IService;
using Microsoft.Extensions.Logging;

namespace KVATUM_STREAMING_SERVICE.App.Service
{
    public class MainConnectionService : IMainConnectionService
    {
        private readonly ConcurrentDictionary<Guid, MainConnectionBody> _connections;
        private readonly IRoomConnectionService _roomConnectionService;
        private readonly ILogger<MainConnectionService> _logger;

        public MainConnectionService(
            ILogger<MainConnectionService> logger,
            IRoomConnectionService roomConnectionService)
        {
            _connections = new ConcurrentDictionary<Guid, MainConnectionBody>();
            _logger = logger;
            _roomConnectionService = roomConnectionService;
        }

        public AccountConnectionBody? GetAccountConnectionBody(Guid accountId)
        {
            if (_connections.TryGetValue(accountId, out var connection))
                return connection.ToAccountConnectionBody();
            return null;
        }

        public void ConnectOrUpdate(Guid accountId, MainConnectionBody body)
        {
            var str = JsonSerializer.Serialize(body);
            _logger.LogInformation($"Account {accountId} connecting with connection: {str}");

            if (_connections.TryGetValue(accountId, out var connection))
            {
                connection.Socket = body.Socket;
                // _logger.LogInformation($"Old socket connection is delete from this account {accountId}");
            }
            else
            {
                _connections.TryAdd(accountId, body);
                // _logger.LogInformation($"New socket connection is added to this account {accountId}");
            }
        }


        public void UpdateMicroState(Guid accountId, bool isMicroEnabled)
        {
            if (_connections.ContainsKey(accountId))
                _connections[accountId].IsMicroEnabled = isMicroEnabled;

            // _logger.LogInformation($"Account {accountId} micro state updated to {isMicroEnabled}");
        }

        public void UpdateConnection(Guid accountId, bool isMicroEnabled, bool isVideoEnabled, Guid? roomId)
        {
            if (_connections.ContainsKey(accountId))
            {
                var connection = _connections[accountId];
                connection.IsMicroEnabled = isMicroEnabled;
                connection.IsVideoEnabled = isVideoEnabled;
                connection.CurrentRoomId = roomId;
            }

            // _logger.LogInformation($"Account {accountId} connection updated to {isMicroEnabled} and {isVideoEnabled}");
        }

        public void UpdateVideoState(Guid accountId, bool isVideoEnabled)
        {
            if (_connections.ContainsKey(accountId))
                _connections[accountId].IsVideoEnabled = isVideoEnabled;

            // _logger.LogInformation($"Account {accountId} video state updated to {isVideoEnabled}");
        }

        public void Disconnect(Guid accountId)
        {
            if (_connections.ContainsKey(accountId))
                _connections.TryRemove(accountId, out var _);

            _logger.LogInformation($"Account {accountId} disconnected from main connection");
        }

        public Guid? GetCurrentRoomId(Guid accountId)
        {
            return _connections.ContainsKey(accountId) ? _connections[accountId].CurrentRoomId : null;
        }

        public IEnumerable<AccountConnectionBody> GetConnections(IEnumerable<Guid> accountIds)
        {
            var connections = _connections.Where(connection => accountIds.Contains(connection.Key))
                .Select(connection => connection.Value.ToAccountConnectionBody());
            return connections;
        }

        public async Task<bool> SendMessage(Guid recipientId, WebSocketMessageType messageType, byte[] message)
        {
            try
            {
                if (!_connections.ContainsKey(recipientId))
                    return false;

                var connection = _connections[recipientId];
                _logger.LogInformation($"Sending message to account {recipientId} message: {Encoding.UTF8.GetString(message)}");
                await connection.Socket.SendAsync(new ArraySegment<byte>(message), messageType, true, CancellationToken.None);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending message to account {recipientId}: {ex.Message}");
                return false;
            }
        }

        public async Task SendMessageToAllExceptSender(Guid senderId, WebSocketMessageType messageType, byte[] message)
        {
            var currentRoomId = GetCurrentRoomId(senderId);
            if (currentRoomId != null)
            {
                _logger.LogInformation($"Sending message {Encoding.UTF8.GetString(message)} to all members in room {currentRoomId.Value} except sender {senderId}");
                var roomMembers = _roomConnectionService.GetRoomMembers(currentRoomId.Value).Where(m => m != senderId);
                var tasks = roomMembers.Select(member => SendMessage(member, messageType, message));
                await Task.WhenAll(tasks);
            }
        }
    }
}