using KVATUM_STREAMING_SERVICE.Core.Entities.Request;
using KVATUM_STREAMING_SERVICE.Core.IService;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

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
        [SwaggerOperation(
            Summary = "Connect to main websocket connection",
            Description = "WebSocket API для управления комнатами. Подключение к URL: wss://example.com/ws/rooms"
        )]
        [SwaggerResponse(400, "Bad Request if connection is not a WebSocket request")]
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