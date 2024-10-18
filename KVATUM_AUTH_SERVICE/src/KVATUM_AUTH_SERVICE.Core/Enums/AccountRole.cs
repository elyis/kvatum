using System.Text.Json.Serialization;

namespace KVATUM_AUTH_SERVICE.Core.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum AccountRole
    {
        User,
        Admin
    }
}