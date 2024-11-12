using System.Net.WebSockets;
using KVATUM_STREAMING_SERVICE.Core.Entities.Request;
using KVATUM_STREAMING_SERVICE.Core.Entities.Response;

namespace KVATUM_STREAMING_SERVICE.Core.IService
{
    public interface IMainConnectionService
    {
        void Connect(Guid accountId, MainConnectionBody body);
        Guid? GetCurrentRoomId(Guid accountId);
        void UpdateMicroState(Guid accountId, bool isMicroEnabled);
        void UpdateVideoState(Guid accountId, bool isVideoEnabled);
        void UpdateConnection(Guid accountId, bool isMicroEnabled, bool isVideoEnabled, Guid? roomId);
        IEnumerable<UserConnectionBody> GetConnections(IEnumerable<Guid> accountIds);
        void Disconnect(Guid accountId);
        Task<bool> SendMessage(Guid accountId, WebSocketMessageType messageType, byte[] message);
    }
}