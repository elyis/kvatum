using System.Collections.Concurrent;
using KVATUM_STREAMING_SERVICE.Core.IService;
using Microsoft.Extensions.Logging;

namespace KVATUM_STREAMING_SERVICE.App.Service
{
    public class RoomConnectionService : IRoomConnectionService
    {
        private readonly ConcurrentDictionary<Guid, HashSet<Guid>> _rooms = new();
        private readonly ILogger<RoomConnectionService> _logger;

        public RoomConnectionService(ILogger<RoomConnectionService> logger)
        {
            _logger = logger;
        }

        public IEnumerable<Guid> JoinToRoomAndGetMembers(Guid roomId, Guid accountId)
        {
            if (!_rooms.ContainsKey(roomId))
                _rooms[roomId] = new HashSet<Guid>();

            _rooms[roomId].Add(accountId);

            _logger.LogInformation($"Account {accountId} joined to room {roomId}");
            return _rooms[roomId];
        }

        public HashSet<Guid> GetRoomMembers(Guid roomId)
        {
            return _rooms.ContainsKey(roomId) ? _rooms[roomId] : new HashSet<Guid>();
        }

        public void LeaveFromRoom(Guid roomId, Guid accountId)
        {
            if (_rooms.ContainsKey(roomId))
            {
                _rooms[roomId].Remove(accountId);

                if (_rooms[roomId].Count == 0)
                    _rooms.TryRemove(roomId, out var _);
            }

            _logger.LogInformation($"Account {accountId} left from room {roomId}");
        }
    }
}