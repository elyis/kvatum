using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace KVATUM_STREAMING_SERVICE.Core.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ImageResolutions
    {
        [Display(Name = "128x128")]
        Small,

        [Display(Name = "256x256")]
        Medium,

        [Display(Name = "512x512")]
        Big
    }
}