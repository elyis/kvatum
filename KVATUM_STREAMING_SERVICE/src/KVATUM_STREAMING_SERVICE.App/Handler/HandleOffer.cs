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
    public class HandleOffer : IEventHandler
    {
        private readonly IMainConnectionService _mainConnectionService;
        private readonly ISerializationService _serializationService;
        private readonly ILogger<HandleOffer> _logger;

        public HandleOffer(
            IMainConnectionService mainConnectionService,
            ISerializationService serializationService,
            ILogger<HandleOffer> logger)
        {
            _mainConnectionService = mainConnectionService;
            _serializationService = serializationService;
            _logger = logger;
        }

        public MessageEvent Event => MessageEvent.Offer;

        public async Task HandleAsync(JsonElement message, Guid accountId)
        {
            var receivedOfferBody = _serializationService.DeserializeMessage<ReceivedOfferBody>(message);
            if (receivedOfferBody == null)
                return;

            _logger.LogInformation($"HandleOffer: {receivedOfferBody.From} -> {receivedOfferBody.To}");

            var accountConnection = _mainConnectionService.GetAccountConnectionBody(receivedOfferBody.From);
            if (accountConnection == null)
                return;

            var offerBody = new OfferBody
            {
                EventType = MessageEvent.Offer,
                Offer = receivedOfferBody.Offer,
                From = accountConnection,
                To = receivedOfferBody.To
            };

            var serializedOfferBody = _serializationService.SerializeMessage(offerBody);
            await _mainConnectionService.SendMessage(receivedOfferBody.To, WebSocketMessageType.Text, serializedOfferBody);
        }
    }
}