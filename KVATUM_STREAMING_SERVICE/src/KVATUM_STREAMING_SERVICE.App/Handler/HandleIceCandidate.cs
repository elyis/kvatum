using System.Net.WebSockets;
using System.Text.Json;
using KVATUM_STREAMING_SERVICE.Core.Entities.Request;
using KVATUM_STREAMING_SERVICE.Core.Entities.Response;
using KVATUM_STREAMING_SERVICE.Core.Enums;
using KVATUM_STREAMING_SERVICE.Core.IHandler;
using KVATUM_STREAMING_SERVICE.Core.IService;
using Microsoft.Extensions.Logging;

namespace KVATUM_STREAMING_SERVICE.App.Handler
{
    public class HandleIceCandidate : IEventHandler
    {
        private readonly IMainConnectionService _mainConnectionService;
        private readonly ISerializationService _serializationService;

        private readonly ILogger<HandleIceCandidate> _logger;

        public HandleIceCandidate(
            IMainConnectionService mainConnectionService,
            ISerializationService serializationService,
            ILogger<HandleIceCandidate> logger)
        {
            _mainConnectionService = mainConnectionService;
            _serializationService = serializationService;
            _logger = logger;
        }

        public MessageEvent Event => MessageEvent.IceCandidate;

        public async Task HandleAsync(JsonElement message, Guid accountId)
        {
            var receivedIceCandidateBody = _serializationService.DeserializeMessage<ReceivedIceCandidateBody>(message);
            if (receivedIceCandidateBody == null)
                return;

            _logger.LogInformation($"HandleIceCandidate: {receivedIceCandidateBody.From} -> {receivedIceCandidateBody.To}");

            var accountConnection = _mainConnectionService.GetAccountConnectionBody(receivedIceCandidateBody.From);
            if (accountConnection == null)
                return;

            var accountConnectionString = JsonSerializer.Serialize(accountConnection);
            _logger.LogInformation($"AccountConnection: {accountConnectionString}");

            var iceCandidateBody = new IceCandidateBody
            {
                EventType = MessageEvent.IceCandidate,
                IceCandidate = receivedIceCandidateBody.IceCandidate,
                From = accountConnection,
                To = receivedIceCandidateBody.To
            };

            var serializedIceCandidateBody = _serializationService.SerializeMessage(iceCandidateBody);
            await _mainConnectionService.SendMessage(receivedIceCandidateBody.To, WebSocketMessageType.Text, serializedIceCandidateBody);
        }
    }
}