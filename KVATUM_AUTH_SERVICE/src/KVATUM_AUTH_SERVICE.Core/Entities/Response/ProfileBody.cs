using KVATUM_AUTH_SERVICE.Core.Enums;

namespace KVATUM_AUTH_SERVICE.Core.Entities.Response
{
    public class ProfileBody
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string Nickname { get; set; }
        public AccountRole Role { get; set; }
        public List<ImageWithResolutionBody> Images { get; set; } = new();
    }
}