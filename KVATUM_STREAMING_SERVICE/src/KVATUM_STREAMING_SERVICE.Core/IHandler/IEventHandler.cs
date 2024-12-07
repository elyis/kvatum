using System.Text.Json;
using KVATUM_STREAMING_SERVICE.Core.Enums;

namespace KVATUM_STREAMING_SERVICE.Core.IHandler
{
    public interface IEventHandler
    {
        MessageEvent Event { get; }
        Task HandleAsync(JsonElement message, Guid accountId);
    }
}