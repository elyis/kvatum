using KVATUM_CHATFLOW_SERVICE.Core.Entities.Response;

namespace KVATUM_CHATFLOW_SERVICE.Core.Entities.Models
{
    public class Workspace
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public List<Chat> Chats { get; set; } = new();
        public Guid HubId { get; set; }
        public Hub Hub { get; set; }

        public WorkspaceBody ToWorkspaceBody()
        {
            return new WorkspaceBody
            {
                Name = Name
            };
        }
    }
}