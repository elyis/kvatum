using KVATUM_CHATFLOW_SERVICE.Core.Entities.Response;
using KVATUM_CHATFLOW_SERVICE.Core.Enums;

namespace KVATUM_CHATFLOW_SERVICE.Core.Entities.Cache
{
    public class CachedHub
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Image { get; set; }
        public string HexColor { get; set; }
        public Guid CreatorId { get; set; }

        public HubBody ToHubBody()
        {
            string? urlIcon = string.IsNullOrEmpty(Image) ? null : $"{Constants.WebUrlToHubIcon}/{Image}";

            return new HubBody
            {
                Id = Id,
                Name = Name,
                HexColor = $"#{HexColor}",
                Images = urlIcon is null ? new() : new List<ImageWithResolutionBody>
                {
                    new() { Resolution = ImageResolutions.Small, UrlImage = $"{urlIcon}?width=128&height=128" },
                    new() { Resolution = ImageResolutions.Medium, UrlImage = $"{urlIcon}?width=256&height=256" },
                    new() { Resolution = ImageResolutions.Big, UrlImage = $"{urlIcon}?width=512&height=512" },
                },
            };
        }
    }
}