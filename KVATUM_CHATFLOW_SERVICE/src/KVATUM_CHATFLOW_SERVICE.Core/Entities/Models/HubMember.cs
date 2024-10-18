namespace KVATUM_CHATFLOW_SERVICE.Core.Entities.Models
{
    public class HubMember
    {
        public Hub Hub { get; set; }
        public Guid HubId { get; set; }

        public Guid MemberId { get; set; }
    }
}