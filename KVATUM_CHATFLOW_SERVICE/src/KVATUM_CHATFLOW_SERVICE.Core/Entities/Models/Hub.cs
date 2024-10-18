using KVATUM_CHATFLOW_SERVICE.Core.Entities.Response;
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
            return new HubBody
            {
                Id = Id,
                Name = Name,
                IconUrl = string.IsNullOrEmpty(Icon) ? null : $"{Constants.ServerUrl}{Icon}"
            };
        }
    }
}