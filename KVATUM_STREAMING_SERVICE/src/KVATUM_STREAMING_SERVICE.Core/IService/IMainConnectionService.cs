using System.Net.WebSockets;
using KVATUM_STREAMING_SERVICE.Core.Entities.Request;
using KVATUM_STREAMING_SERVICE.Core.Entities.Response;

namespace KVATUM_STREAMING_SERVICE.Core.IService
{
    public interface IMainConnectionService
    {
        void ConnectOrUpdate(Guid accountId, MainConnectionBody body);
        void UpdateAccountProfile(Guid accountId, AccountProfileBody accountProfile);
        AccountConnectionBody? GetAccountConnectionBody(Guid accountId);
        Guid? GetCurrentRoomId(Guid accountId);
        void UpdateMicroState(Guid accountId, bool isMicroEnabled);
        void UpdateVideoState(Guid accountId, bool isVideoEnabled);
        void UpdateConnection(Guid accountId, bool isMicroEnabled, bool isVideoEnabled, Guid? roomId);
        IEnumerable<AccountConnectionBody> GetConnections(IEnumerable<Guid> accountIds);
        void Disconnect(Guid accountId);
        Task<bool> SendMessage(Guid recipientId, WebSocketMessageType messageType, byte[] message);
        Task SendMessageToAllExceptSender(Guid senderId, WebSocketMessageType messageType, byte[] message);
    }
}