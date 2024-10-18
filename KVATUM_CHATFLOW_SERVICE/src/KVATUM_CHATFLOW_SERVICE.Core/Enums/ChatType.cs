using System.Text.Json.Serialization;

namespace KVATUM_CHATFLOW_SERVICE.Core.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ChatType
    {
        Conference,
        Chat,
        Channel
    }
}