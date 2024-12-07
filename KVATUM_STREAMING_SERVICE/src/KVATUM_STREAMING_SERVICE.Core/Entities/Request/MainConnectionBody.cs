using System.Net.WebSockets;
using KVATUM_STREAMING_SERVICE.Core.Entities.Response;

namespace KVATUM_STREAMING_SERVICE.Core.Entities.Request
{
    public class MainConnectionBody
    {
        public WebSocket Socket { get; set; }
        public bool IsMicroEnabled { get; set; }
        public bool IsVideoEnabled { get; set; }
        public Guid? CurrentRoomId { get; set; }
        public AccountProfileBody AccountProfile { get; set; }

        public AccountConnectionBody ToAccountConnectionBody()
        {
            return new AccountConnectionBody(AccountProfile, IsMicroEnabled, IsVideoEnabled);
        }
    }
}