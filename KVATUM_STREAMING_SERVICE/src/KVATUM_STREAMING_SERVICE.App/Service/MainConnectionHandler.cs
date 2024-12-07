using System.Net.WebSockets;
using KVATUM_CHATFLOW_SERVICE.Core.Entities.Response;
using KVATUM_STREAMING_SERVICE.Core.Entities.Request;
using KVATUM_STREAMING_SERVICE.Core.Entities.Response;
using KVATUM_STREAMING_SERVICE.Core.IHandler;
using KVATUM_STREAMING_SERVICE.Core.IService;
using Microsoft.Extensions.Logging;

namespace KVATUM_STREAMING_SERVICE.App.Service
{
    public class MainConnectionHandler : IMainConnectionHandler
    {
        private readonly IJwtService _jwtService;
        private readonly ISerializationService _serializationService;
        private readonly IMainConnectionService _mainConnectionService;
        private readonly IRoomConnectionService _roomConnectionService;
        private readonly IAccountService _accountService;
        private readonly ILogger<MainConnectionHandler> _logger;
        private readonly IMessageHandler _messageHandler;


        public MainConnectionHandler(
            IJwtService jwtService,
            ILogger<MainConnectionHandler> logger,
            ISerializationService serializationService,
            IMessageHandler messageHandler,
            IMainConnectionService mainConnectionService,
            IRoomConnectionService roomConnectionService,
            IAccountService accountService)
        {
            _serializationService = serializationService;
            _jwtService = jwtService;
            _logger = logger;
            _messageHandler = messageHandler;
            _mainConnectionService = mainConnectionService;
            _roomConnectionService = roomConnectionService;
            _accountService = accountService;
        }

        public async Task Invoke(WebSocket socket)
        {
            Guid? accountId = null;
            try
            {
                var tokenPayload = await AuthenticateAccount(socket);
                if (tokenPayload == null)
                    return;

                accountId = tokenPayload.AccountId;
                var accountProfile = await _accountService.GetAccountProfileAsync(accountId.Value);
                if (accountProfile == null)
                {
                    _logger.LogError($"Account {accountId} not found");
                    throw new Exception("Account profile not found");
                }

                InitMainConnection(socket, tokenPayload, accountProfile);
                await ProcessIncomingMessages(socket, tokenPayload);
            }
            catch (Exception ex)
            {
                _logger.LogError($"WebSocket error: {ex.Message}");
            }
            finally
            {
                if (accountId != null)
                {
                    var currentRoomId = _mainConnectionService.GetCurrentRoomId(accountId.Value);
                    if (currentRoomId != null)
                        _roomConnectionService.LeaveFromRoom(currentRoomId.Value, accountId.Value);

                    _mainConnectionService.Disconnect(accountId.Value);
                }

                if (socket.State != WebSocketState.Closed && socket.State != WebSocketState.Aborted)
                    await socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Connection closed", CancellationToken.None);

                _logger.LogInformation("Main connection closed: " + socket.State);
            }
        }

        private async Task ProcessIncomingMessages(WebSocket socket, TokenPayload tokenPayload)
        {
            while (socket.State == WebSocketState.Open)
            {
                var messageBytes = await ReceiveMessage(socket, CancellationToken.None);
                if (messageBytes == null)
                    break;

                var receivedMessage = _serializationService.DeserializeMessage<ReceivedMessageEvent>(messageBytes);
                if (receivedMessage == null)
                    break;

                await _messageHandler.InvokeAsync(receivedMessage.EventType, receivedMessage.EventBody, tokenPayload.AccountId);
            }
        }

        private async Task<byte[]?> ReceiveMessage(WebSocket webSocket, CancellationToken token)
        {
            byte[] bytes = new byte[4096];
            MemoryStream stream = new();

            WebSocketReceiveResult? receiveResult;
            do
            {
                receiveResult = await webSocket.ReceiveAsync(bytes, token);
                if (receiveResult.MessageType == WebSocketMessageType.Close && webSocket.State != WebSocketState.Closed)
                {
                    await webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, null, token);
                    return null;
                }
                else if (receiveResult.Count > 0)
                    stream.Write(bytes, 0, receiveResult.Count);
            } while (!receiveResult.EndOfMessage && webSocket.State == WebSocketState.Open);

            return stream.ToArray();
        }

        private async Task<TokenPayload?> AuthenticateAccount(WebSocket socket)
        {
            var receivedTokenBytes = await ReceiveMessage(socket, CancellationToken.None);
            if (receivedTokenBytes == null)
                return null;

            var receivedTokenMessage = _serializationService.DeserializeMessage<ReceivedMessageEvent>(receivedTokenBytes);
            if (receivedTokenMessage == null)
                return null;

            var tokenPayload = _jwtService.GetTokenPayload(receivedTokenMessage.EventBody.ToString());
            if (tokenPayload == null)
            {
                await socket.CloseOutputAsync(WebSocketCloseStatus.InvalidMessageType, "Invalid token", CancellationToken.None);
                return null;
            }

            return tokenPayload;
        }

        private void InitMainConnection(WebSocket socket, TokenPayload tokenPayload, AccountProfileBody accountProfile)
        {
            _mainConnectionService.ConnectOrUpdate(tokenPayload.AccountId, new MainConnectionBody
            {
                IsMicroEnabled = false,
                IsVideoEnabled = false,
                Socket = socket,
                CurrentRoomId = null,
                AccountProfile = accountProfile
            });
        }
    }
}