using KVATUM_CHATFLOW_SERVICE.Core.Entities.Response;
using KVATUM_CHATFLOW_SERVICE.Core.Enums;
using Microservice_module.Core;

namespace KVATUM_CHATFLOW_SERVICE.Core.Entities.Models
{
    public class Hub
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Icon { get; set; }
        public Guid CreatorId { get; set; }
        public List<Workspace> Workspaces { get; set; } = new();
        public List<HubMember> Members { get; set; } = new();
        public HubJoiningInvitation HubJoiningInvitation { get; set; }


        public HubBody ToHubBody()
        {
            string? urlIcon = string.IsNullOrEmpty(Icon) ? null : $"{Constants.WebUrlToHubIcon}/{Icon}";
            return new HubBody
            {
                Id = Id,
                Name = Name,
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