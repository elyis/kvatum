using System.Net.WebSockets;
using System.Text.Json;
using KVATUM_STREAMING_SERVICE.Core.Entities.Request;
using KVATUM_STREAMING_SERVICE.Core.Entities.Response;
using KVATUM_STREAMING_SERVICE.Core.Enums;
using KVATUM_STREAMING_SERVICE.Core.IHandler;
using KVATUM_STREAMING_SERVICE.Core.IService;

namespace KVATUM_STREAMING_SERVICE.App.Handler
{
    public class HandleAnswer : IEventHandler
    {
        private readonly IMainConnectionService _mainConnectionService;
        private readonly ISerializationService _serializationService;

        public HandleAnswer(
            IMainConnectionService mainConnectionService,
            ISerializationService serializationService)
        {
            _mainConnectionService = mainConnectionService;
            _serializationService = serializationService;
        }

        public MessageEvent Event => MessageEvent.Answer;

        public async Task HandleAsync(JsonElement message, Guid accountId)
        {
            var receivedAnswerBody = _serializationService.DeserializeMessage<ReceivedAnswerBody>(message);
            if (receivedAnswerBody == null)
                return;

            var accountConnection = _mainConnectionService.GetAccountConnectionBody(receivedAnswerBody.From);
            if (accountConnection == null)
                return;

            var answerBody = new AnswerBody
            {
                EventType = MessageEvent.Answer,
                Answer = receivedAnswerBody.Answer,
                From = accountConnection,
                To = receivedAnswerBody.To
            };

            var serializedAnswerBody = _serializationService.SerializeMessage(answerBody);
            await _mainConnectionService.SendMessage(receivedAnswerBody.To, WebSocketMessageType.Text, serializedAnswerBody);
        }
    }
}