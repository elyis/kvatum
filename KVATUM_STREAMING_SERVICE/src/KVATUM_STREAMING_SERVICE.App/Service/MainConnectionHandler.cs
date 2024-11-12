using System.Net.WebSockets;
using System.Text.Json;
using KVATUM_CHATFLOW_SERVICE.Core.Entities.Response;
using KVATUM_STREAMING_SERVICE.Core.Entities.Request;
using KVATUM_STREAMING_SERVICE.Core.Entities.Response;
using KVATUM_STREAMING_SERVICE.Core.Enums;
using KVATUM_STREAMING_SERVICE.Core.IService;
using Microsoft.Extensions.Logging;

namespace KVATUM_STREAMING_SERVICE.App.Service
{
    public class MainConnectionHandler : IMainConnectionHandler
    {
        private readonly IMainConnectionService _mainConnectionService;
        private readonly IRoomConnectionService _roomConnectionService;
        private readonly IJwtService _jwtService;
        private readonly ILogger<MainConnectionHandler> _logger;

        private readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        public MainConnectionHandler(
            IMainConnectionService mainConnectionService,
            IRoomConnectionService roomConnectionService,
            IJwtService jwtService,
            ILogger<MainConnectionHandler> logger)
        {
            _mainConnectionService = mainConnectionService;
            _roomConnectionService = roomConnectionService;
            _jwtService = jwtService;
            _logger = logger;
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
                InitMainConnection(socket, tokenPayload);
                await ProcessIncomingMessages(socket, tokenPayload);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MainConnectionHandler");
            }
            finally
            {
                if (accountId != null)
                {
                    _mainConnectionService.Disconnect(accountId.Value);

                    var currentRoomId = _mainConnectionService.GetCurrentRoomId(accountId.Value);
                    if (currentRoomId != null)
                        _roomConnectionService.LeaveFromRoom(currentRoomId.Value, accountId.Value);
                }

                await socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Connection closed", CancellationToken.None);
            }
        }

        private async Task HandleJoinToRoom(JsonElement message, Guid accountId)
        {
            var roomConnectionBody = message.Deserialize<RoomConnectionBody>(_jsonSerializerOptions);
            if (roomConnectionBody == null)
                return;

            _mainConnectionService.UpdateConnection(
                accountId,
                !roomConnectionBody.IsMicroMuted,
                roomConnectionBody.IsCameraEnabled,
                roomConnectionBody.RoomId);

            _roomConnectionService.JoinToRoom(roomConnectionBody.RoomId, accountId);

            var members = _roomConnectionService.GetRoomMembers(roomConnectionBody.RoomId).Where(m => m != accountId);
            var connections = _mainConnectionService.GetConnections(members);
            var accountList = new AccountList
            {
                EventType = MessageEvent.UserList,
                Users = connections
            };
            var serializedAccountList = SerializeMessage(accountList);

            bool isSent = await _mainConnectionService.SendMessage(accountId, WebSocketMessageType.Text, serializedAccountList);
            if (!isSent)
                throw new Exception($"Error sending message to account {accountId}");
        }

        private async Task HandleChangeMicroState(JsonElement message, Guid accountId)
        {
            bool isMicroEnabled = JsonSerializer.Deserialize<bool>(message, _jsonSerializerOptions);
            _mainConnectionService.UpdateMicroState(accountId, isMicroEnabled);

            var userChangeMicroStateBody = new UserChangeMicroStateBody
            {
                EventType = MessageEvent.ChangeMicroState,
                From = accountId,
                IsMicroMuted = !isMicroEnabled
            };

            await NotifyRoomMembers(accountId, userChangeMicroStateBody, MessageEvent.ChangeMicroState);
        }

        private async Task NotifyRoomMembers(Guid accountId, object message, MessageEvent eventType)
        {
            var serializedMessage = SerializeMessage(message);

            var currentRoomId = _mainConnectionService.GetCurrentRoomId(accountId);
            if (currentRoomId != null)
            {
                var roomMembers = _roomConnectionService.GetRoomMembers(currentRoomId.Value).Where(m => m != accountId);
                foreach (var member in roomMembers)
                {
                    bool isMessageSent = await _mainConnectionService.SendMessage(member, WebSocketMessageType.Text, serializedMessage);
                    if (!isMessageSent)
                        throw new Exception($"Error sending message of type {eventType} to account {member}");
                }
            }
        }

        private async Task HandleChangeVideoState(JsonElement message, Guid accountId)
        {
            bool isVideoEnabled = JsonSerializer.Deserialize<bool>(message, _jsonSerializerOptions);
            _mainConnectionService.UpdateVideoState(accountId, isVideoEnabled);

            var userChangeVideoStateBody = new UserChangeVideoStateBody
            {
                EventType = MessageEvent.ChangeVideoState,
                From = accountId,
                HasVideo = isVideoEnabled
            };

            await NotifyRoomMembers(accountId, userChangeVideoStateBody, MessageEvent.ChangeVideoState);
        }

        private async Task HandleOffer(JsonElement message, bool isUpdateOffer = false)
        {
            var receivedOfferBody = message.Deserialize<ReceivedOfferBody>(_jsonSerializerOptions);
            if (receivedOfferBody == null)
                return;

            var offerBody = new OfferBody
            {
                EventType = isUpdateOffer ? MessageEvent.UpdateOffer : MessageEvent.Offer,
                Offer = receivedOfferBody.Offer,
                From = FillAccountProfile(receivedOfferBody.From),
                To = receivedOfferBody.To
            };

            var serializedOfferBody = SerializeMessage(offerBody);
            bool isOfferSent = await _mainConnectionService.SendMessage(receivedOfferBody.To, WebSocketMessageType.Text, serializedOfferBody);
            if (!isOfferSent)
                throw new Exception($"Error sending offer to account {receivedOfferBody.To}");
        }

        private async Task HandleIceCandidate(JsonElement message)
        {
            var receivedIceCandidateBody = message.Deserialize<ReceivedIceCandidateBody>(_jsonSerializerOptions);
            if (receivedIceCandidateBody == null)
                return;

            var iceCandidateBody = new IceCandidateBody
            {
                EventType = MessageEvent.IceCandidate,
                IceCandidate = receivedIceCandidateBody.IceCandidate,
                From = FillAccountProfile(receivedIceCandidateBody.From),
                To = receivedIceCandidateBody.To
            };

            var serializedIceCandidateBody = SerializeMessage(iceCandidateBody);
            bool isIceCandidateSent = await _mainConnectionService.SendMessage(receivedIceCandidateBody.To, WebSocketMessageType.Text, serializedIceCandidateBody);
            if (!isIceCandidateSent)
                throw new Exception($"Error sending ice candidate to account {receivedIceCandidateBody.To}");
        }

        private async Task HandleAnswer(JsonElement message, bool isUpdateAnswer = false)
        {
            var receivedAnswerBody = message.Deserialize<ReceivedAnswerBody>(_jsonSerializerOptions);
            if (receivedAnswerBody == null)
                return;

            var answerBody = new AnswerBody
            {
                EventType = isUpdateAnswer ? MessageEvent.UpdateAnswer : MessageEvent.Answer,
                Answer = receivedAnswerBody.Answer,
                From = FillAccountProfile(receivedAnswerBody.From),
                To = receivedAnswerBody.To
            };

            var serializedAnswerBody = SerializeMessage(answerBody);
            bool isAnswerSent = await _mainConnectionService.SendMessage(receivedAnswerBody.To, WebSocketMessageType.Text, serializedAnswerBody);
            if (!isAnswerSent)
                throw new Exception($"Error sending answer to account {receivedAnswerBody.To}");
        }

        private async Task HandleDisconnect(Guid accountId)
        {
            var roomId = _mainConnectionService.GetCurrentRoomId(accountId);
            if (roomId != null)
            {
                _roomConnectionService.LeaveFromRoom(roomId.Value, accountId);
                var userDisconnectionBody = new UserDisconnectionBody
                {
                    EventType = MessageEvent.Disconnect,
                    From = accountId
                };

                await NotifyRoomMembers(accountId, userDisconnectionBody, MessageEvent.Disconnect);
            }
        }

        private async Task HandleMessage(MessageEvent eventType, JsonElement message, Guid accountId)
        {
            switch (eventType)
            {
                case MessageEvent.JoinToRoom:
                    await HandleJoinToRoom(message, accountId);
                    break;

                case MessageEvent.ChangeMicroState:
                    await HandleChangeMicroState(message, accountId);
                    break;

                case MessageEvent.ChangeVideoState:
                    await HandleChangeVideoState(message, accountId);
                    break;

                case MessageEvent.UpdateOffer:
                    await HandleOffer(message, true);
                    break;

                case MessageEvent.Offer:
                    await HandleOffer(message);
                    break;

                case MessageEvent.IceCandidate:
                    await HandleIceCandidate(message);
                    break;

                case MessageEvent.UpdateAnswer:
                    await HandleAnswer(message, true);
                    break;

                case MessageEvent.Answer:
                    await HandleAnswer(message);
                    break;

                case MessageEvent.Disconnect:
                    await HandleDisconnect(accountId);
                    break;

                default:
                    _logger.LogWarning($"Unknown event type: {eventType}");
                    return;
            }
        }

        private async Task ProcessIncomingMessages(WebSocket socket, TokenPayload tokenPayload)
        {
            while (socket.State == WebSocketState.Open)
            {
                var messageBytes = await ReceiveMessage(socket, CancellationToken.None);
                if (messageBytes == null)
                    break;

                var receivedMessage = DeserializeMessage<ReceivedMessageEvent>(messageBytes);
                if (receivedMessage == null)
                    break;

                await HandleMessage(receivedMessage.EventType, receivedMessage.EventBody, tokenPayload.AccountId);
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

        private T? DeserializeMessage<T>(byte[] message)
        {
            return JsonSerializer.Deserialize<T>(message, _jsonSerializerOptions);
        }

        private byte[] SerializeMessage(object message)
        {
            return JsonSerializer.SerializeToUtf8Bytes(message, _jsonSerializerOptions);
        }

        private UserConnectionBody FillAccountProfile(Guid accountId)
        {
            return new UserConnectionBody
            {
                Id = accountId,
                Email = "test@test.com",
                Nickname = "test",
                Role = AccountRole.User,
                Images = new List<ImageWithResolutionBody>()
            };
        }

        private async Task<TokenPayload?> AuthenticateAccount(WebSocket socket)
        {
            var receivedTokenBytes = await ReceiveMessage(socket, CancellationToken.None);
            if (receivedTokenBytes == null)
                return null;

            var receivedTokenMessage = DeserializeMessage<ReceivedMessageEvent>(receivedTokenBytes);
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

        private void InitMainConnection(WebSocket socket, TokenPayload tokenPayload)
        {
            _mainConnectionService.Connect(tokenPayload.AccountId, new MainConnectionBody
            {
                IsMicroEnabled = false,
                IsVideoEnabled = false,
                Socket = socket,
                CurrentRoomId = null
            });
        }
    }
}