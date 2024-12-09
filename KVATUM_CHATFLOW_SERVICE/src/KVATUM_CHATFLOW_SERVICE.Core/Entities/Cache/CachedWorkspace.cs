using KVATUM_CHATFLOW_SERVICE.Core.Entities.Response;
using KVATUM_CHATFLOW_SERVICE.Core.Enums;

namespace KVATUM_CHATFLOW_SERVICE.Core.Entities.Cache
{
    public class CachedWorkspace
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string HexColor { get; set; }
        public string? Image { get; set; }
        public Guid HubId { get; set; }

        public WorkspaceBody ToWorkspaceBody()
        {
            string? urlIcon = string.IsNullOrEmpty(Image) ? null : $"{Constants.WebUrlToWorkspaceIcon}/{Image}";

            return new WorkspaceBody
            {
                Id = Id,
                Name = Name,
                HexColor = $"#{HexColor}",
                ImageUrls = urlIcon is null ? new() : new List<ImageWithResolutionBody>
                {
                    new() { Resolution = ImageResolutions.Small, UrlImage = $"{urlIcon}?width=128&height=128" },
                    new() { Resolution = ImageResolutions.Medium, UrlImage = $"{urlIcon}?width=256&height=256" },
                    new() { Resolution = ImageResolutions.Big, UrlImage = $"{urlIcon}?width=512&height=512" },
                },
            };
        }
    }
}