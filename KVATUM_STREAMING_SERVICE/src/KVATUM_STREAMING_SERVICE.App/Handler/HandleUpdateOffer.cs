using System.Net.WebSockets;
using System.Text.Json;
using KVATUM_STREAMING_SERVICE.Core.Entities.Request;
using KVATUM_STREAMING_SERVICE.Core.Entities.Response;
using KVATUM_STREAMING_SERVICE.Core.Enums;
using KVATUM_STREAMING_SERVICE.Core.IHandler;
using KVATUM_STREAMING_SERVICE.Core.IService;

namespace KVATUM_STREAMING_SERVICE.App.Handler
{
    public class HandleUpdateOffer : IEventHandler
    {
        private readonly IMainConnectionService _mainConnectionService;
        private readonly ISerializationService _serializationService;

        public HandleUpdateOffer(
            IMainConnectionService mainConnectionService,
            ISerializationService serializationService)
        {
            _mainConnectionService = mainConnectionService;
            _serializationService = serializationService;
        }

        public MessageEvent Event => MessageEvent.UpdateOffer;

        public async Task HandleAsync(JsonElement message, Guid accountId)
        {
            var receivedOfferBody = _serializationService.DeserializeMessage<ReceivedOfferBody>(message);
            if (receivedOfferBody == null)
                return;

            var accountConnection = _mainConnectionService.GetAccountConnectionBody(receivedOfferBody.From);
            if (accountConnection == null)
                return;

            var offerBody = new OfferBody
            {
                EventType = MessageEvent.UpdateOffer,
                Offer = receivedOfferBody.Offer,
                From = accountConnection,
                To = receivedOfferBody.To
            };

            var serializedOfferBody = _serializationService.SerializeMessage(offerBody);
            await _mainConnectionService.SendMessage(receivedOfferBody.To, WebSocketMessageType.Text, serializedOfferBody);
        }

    }
}