using System.Collections.Concurrent;
using System.Net.WebSockets;
using KVATUM_STREAMING_SERVICE.Core.Entities.Request;
using KVATUM_STREAMING_SERVICE.Core.Entities.Response;
using KVATUM_STREAMING_SERVICE.Core.Enums;
using KVATUM_STREAMING_SERVICE.Core.IService;
using Microsoft.Extensions.Logging;

namespace KVATUM_STREAMING_SERVICE.App.Service
{
    public class MainConnectionService : IMainConnectionService
    {
        private readonly ConcurrentDictionary<Guid, MainConnectionBody> _connections;
        private readonly ILogger<MainConnectionService> _logger;

        public MainConnectionService(ILogger<MainConnectionService> logger)
        {
            _connections = new ConcurrentDictionary<Guid, MainConnectionBody>();
            _logger = logger;
        }

        public void Connect(Guid accountId, MainConnectionBody body)
        {
            _connections.TryAdd(accountId, body);

            _logger.LogInformation("Account {AccountId} connected to main connection", accountId);
        }


        public void UpdateMicroState(Guid accountId, bool isMicroEnabled)
        {
            if (_connections.ContainsKey(accountId))
                _connections[accountId].IsMicroEnabled = isMicroEnabled;

            _logger.LogInformation($"Account {accountId} micro state updated to {isMicroEnabled}");
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

            _logger.LogInformation($"Account {accountId} connection updated to {isMicroEnabled} and {isVideoEnabled}");
        }

        public void UpdateVideoState(Guid accountId, bool isVideoEnabled)
        {
            if (_connections.ContainsKey(accountId))
                _connections[accountId].IsVideoEnabled = isVideoEnabled;

            _logger.LogInformation($"Account {accountId} video state updated to {isVideoEnabled}");
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

        public IEnumerable<UserConnectionBody> GetConnections(IEnumerable<Guid> accountIds)
        {
            var connections = _connections.Where(connection => accountIds.Contains(connection.Key)).ToList();
            return connections.Select(connection => new UserConnectionBody
            {
                Id = connection.Key,
                Email = "",
                Nickname = "",
                Role = AccountRole.User,
                Images = new List<ImageWithResolutionBody>(),
                IsMicroMuted = !connection.Value.IsMicroEnabled,
                HasVideo = connection.Value.IsVideoEnabled
            }).ToList();
        }

        public async Task<bool> SendMessage(Guid accountId, WebSocketMessageType messageType, byte[] message)
        {
            try
            {
                if (!_connections.ContainsKey(accountId))
                    return false;

                var connection = _connections[accountId];
                await connection.Socket.SendAsync(new ArraySegment<byte>(message), messageType, true, CancellationToken.None);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending message to account {accountId}: {ex.Message}");
                return false;
            }
        }
    }
}