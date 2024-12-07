using KVATUM_AUTH_SERVICE.Core.Entities.Response;
using KVATUM_AUTH_SERVICE.Core.Enums;

namespace KVATUM_AUTH_SERVICE.Core.Entities.Cache
{
    public class CachedAccount
    {
        public Guid Id { get; set; }

        public string Email { get; set; }
        public string Nickname { get; set; }
        public string Role { get; set; }
        public string? Image { get; set; }

        public ProfileBody ToProfileBody()
        {
            string? urlIcon = string.IsNullOrEmpty(Image) ? null : $"{Constants.WebUrlToProfileImage}/{Image}";
            return new ProfileBody
            {
                Id = Id,
                Email = Email,
                Nickname = Nickname,
                Role = Enum.Parse<AccountRole>(Role),
                Images = Image is null ? new() : new List<ImageWithResolutionBody>
                {
                    new() { Resolution = ImageResolutions.Small, UrlImage = $"{urlIcon}?width=128&height=128" },
                    new() { Resolution = ImageResolutions.Medium, UrlImage = $"{urlIcon}?width=256&height=256" },
                    new() { Resolution = ImageResolutions.Big, UrlImage = $"{urlIcon}?width=512&height=512" },
                },
            };
        }
    }
}