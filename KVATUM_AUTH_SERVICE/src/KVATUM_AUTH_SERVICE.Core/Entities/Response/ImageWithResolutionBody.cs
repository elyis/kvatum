using KVATUM_AUTH_SERVICE.Core.Enums;

namespace KVATUM_AUTH_SERVICE.Core.Entities.Response
{
    public class ImageWithResolutionBody
    {
        public ImageResolutions Resolution { get; set; }
        public string UrlImage { get; set; } = string.Empty;
    }
}