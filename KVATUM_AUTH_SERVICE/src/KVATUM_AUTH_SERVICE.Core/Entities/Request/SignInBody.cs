using System.ComponentModel.DataAnnotations;

namespace KVATUM_AUTH_SERVICE.Core.Entities.Request
{
    public class SignInBody
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}