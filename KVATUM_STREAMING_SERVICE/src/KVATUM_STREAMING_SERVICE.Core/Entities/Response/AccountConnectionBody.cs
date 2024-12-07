using KVATUM_STREAMING_SERVICE.Core.Entities.Request;
using KVATUM_STREAMING_SERVICE.Core.Enums;

namespace KVATUM_STREAMING_SERVICE.Core.Entities.Response
{
    public class AccountConnectionBody
    {
        public AccountConnectionBody(AccountProfileBody accountProfileBody, bool isMicroMuted = false, bool hasVideo = false)
        {
            Id = accountProfileBody.Id;
            Email = accountProfileBody.Email;
            Nickname = accountProfileBody.Nickname;
            Role = accountProfileBody.Role;
            Images = accountProfileBody.Images;
            IsMicroMuted = isMicroMuted;
            HasVideo = hasVideo;
        }

        public Guid Id { get; set; }
        public string Email { get; set; }
        public string Nickname { get; set; }
        public AccountRole Role { get; set; }
        public List<ImageWithResolutionBody> Images { get; set; } = new();

        public bool IsMicroMuted { get; set; }
        public bool HasVideo { get; set; }
    }
}