using System.Text.Json;
using KVATUM_STREAMING_SERVICE.Core.Enums;

namespace KVATUM_STREAMING_SERVICE.Core.IHandler
{
    public interface IMessageHandler
    {
        Task InvokeAsync(MessageEvent messageEvent, JsonElement message, Guid accountId);
    }
}