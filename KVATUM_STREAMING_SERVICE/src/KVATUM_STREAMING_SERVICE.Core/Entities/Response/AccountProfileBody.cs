using KVATUM_STREAMING_SERVICE.Core.Enums;

namespace KVATUM_STREAMING_SERVICE.Core.Entities.Response
{
    public class AccountProfileBody
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string Nickname { get; set; }
        public AccountRole Role { get; set; }
        public List<ImageWithResolutionBody> Images { get; set; } = new();
    }
}