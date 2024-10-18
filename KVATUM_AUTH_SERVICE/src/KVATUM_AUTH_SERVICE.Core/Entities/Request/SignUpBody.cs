using System.ComponentModel.DataAnnotations;

namespace KVATUM_AUTH_SERVICE.Core.Entities.Request
{
    public class SignUpBody
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Nickname { get; set; }

        [Required]
        public string Password { get; set; }
    }
}