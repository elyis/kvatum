using KVATUM_CHATFLOW_SERVICE.Core.Entities.Cache;

namespace KVATUM_CHATFLOW_SERVICE.Core.Entities.Models
{
    public class HubJoiningInvitation
    {
        public Hub Hub { get; set; }
        public Guid HubId { get; set; }
        public string HashInvitation { get; set; }

        public CachedJoiningInvitation ToCachedInvitation()
        {
            return new CachedJoiningInvitation
            {
                HashInvitation = HashInvitation,
                HubId = HubId,
            };
        }
    }
}