using System.Text.Json;
using KVATUM_STREAMING_SERVICE.Core.Enums;
using KVATUM_STREAMING_SERVICE.Core.IHandler;
using Microsoft.Extensions.Logging;

namespace KVATUM_STREAMING_SERVICE.App.Service
{
    public class MessageHandler : IMessageHandler
    {
        private readonly Dictionary<MessageEvent, IEventHandler> _eventHandlers;
        private readonly ILogger<MessageHandler> _logger;

        public MessageHandler(IEnumerable<IEventHandler> eventHandlers, ILogger<MessageHandler> logger)
        {
            _eventHandlers = eventHandlers.ToDictionary(eh => eh.Event);
            _logger = logger;
        }

        public async Task InvokeAsync(MessageEvent messageEvent, JsonElement message, Guid accountId)
        {
            var eventHandler = _eventHandlers.GetValueOrDefault(messageEvent);
            if (eventHandler == null)
            {
                _logger.LogWarning($"No event handler found for message event: {messageEvent}");
                return;
            }

            await eventHandler.HandleAsync(message, accountId);
        }
    }
}