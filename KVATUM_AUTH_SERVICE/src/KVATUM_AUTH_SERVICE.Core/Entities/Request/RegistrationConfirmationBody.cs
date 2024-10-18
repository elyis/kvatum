using System.ComponentModel.DataAnnotations;

namespace KVATUM_AUTH_SERVICE.Core.Entities.Request
{
    public class RegistrationConfirmationBody
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string VerificationCode { get; set; }
    }
}