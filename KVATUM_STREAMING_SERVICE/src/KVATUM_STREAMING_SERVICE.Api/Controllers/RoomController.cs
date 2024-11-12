using KVATUM_STREAMING_SERVICE.Core.IService;
using Microsoft.AspNetCore.Mvc;

namespace KVATUM_STREAMING_SERVICE.Api.Controllers
{
    [ApiController]
    [Route("ws")]
    public class RoomController : ControllerBase
    {
        private readonly IMainConnectionHandler _mainConnectionHandler;

        public RoomController(IMainConnectionHandler mainConnectionHandler)
        {
            _mainConnectionHandler = mainConnectionHandler;
        }

        [HttpGet("rooms")]
        public async Task ConnectToMainConnection()
        {
            if (!HttpContext.WebSockets.IsWebSocketRequest)
            {
                HttpContext.Response.StatusCode = 400;
                return;
            }

            var socket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            await _mainConnectionHandler.Invoke(socket);
        }
    }
}