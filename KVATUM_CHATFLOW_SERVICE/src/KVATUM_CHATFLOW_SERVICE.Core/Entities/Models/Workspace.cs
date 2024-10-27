using KVATUM_CHATFLOW_SERVICE.Core.Entities.Response;
using KVATUM_CHATFLOW_SERVICE.Core.Enums;
using Microservice_module.Core;

namespace KVATUM_CHATFLOW_SERVICE.Core.Entities.Models
{
    public class Workspace
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string HexColor { get; set; }
        public string? Icon { get; set; }

        public List<Chat> Chats { get; set; } = new();
        public Guid HubId { get; set; }
        public Hub Hub { get; set; }

        public WorkspaceBody ToWorkspaceBody()
        {
            string? urlIcon = string.IsNullOrEmpty(Icon) ? null : $"{Constants.WebUrlToWorkspaceIcon}/{Icon}";

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