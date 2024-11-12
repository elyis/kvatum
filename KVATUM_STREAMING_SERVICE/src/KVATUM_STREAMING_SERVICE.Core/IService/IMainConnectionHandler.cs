using System.Net.WebSockets;

namespace KVATUM_STREAMING_SERVICE.Core.IService
{
    public interface IMainConnectionHandler
    {
        Task Invoke(WebSocket socket);
    }
}