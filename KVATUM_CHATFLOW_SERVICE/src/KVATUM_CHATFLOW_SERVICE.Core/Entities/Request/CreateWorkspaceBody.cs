namespace KVATUM_CHATFLOW_SERVICE.Core.Entities.Request
{
    public class CreateWorkspaceBody
    {
        public string Name { get; set; }
        public Guid HubId { get; set; }
    }
}