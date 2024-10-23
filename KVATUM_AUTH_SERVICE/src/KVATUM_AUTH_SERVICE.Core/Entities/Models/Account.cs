using KVATUM_AUTH_SERVICE.Core.Entities.Response;
using KVATUM_AUTH_SERVICE.Core.Enums;

namespace KVATUM_AUTH_SERVICE.Core.Entities.Models
{
    public class Account
    {
        public Guid Id { get; set; }

        public string Email { get; set; }
        public string Nickname { get; set; }
        public string PasswordHash { get; set; }
        public string? RestoreCode { get; set; }
        public string Role { get; set; }
        public DateTime? RestoreCodeValidBefore { get; set; }
        public bool WasPasswordResetRequest { get; set; }
        public string? Image { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<AccountSession> Sessions { get; set; } = new();

        public ProfileBody ToProfileBody()
        {
            string? urlIcon = string.IsNullOrEmpty(Image) ? null : $"{Constants.WebUrlToProfileImage}/{Image}";

            return new ProfileBody
            {
                Id = Id,
                Nickname = Nickname,
                Role = Enum.Parse<AccountRole>(Role),
                Images = urlIcon is null ? new() : new List<ImageWithResolutionBody>
                {
                    new() { Resolution = ImageResolutions.Small, UrlImage = $"{urlIcon}?width=128&height=128" },
                    new() { Resolution = ImageResolutions.Medium, UrlImage = $"{urlIcon}?width=256&height=256" },
                    new() { Resolution = ImageResolutions.Big, UrlImage = $"{urlIcon}?width=512&height=512" },
                },
                Email = Email,
            };
        }

    }
}