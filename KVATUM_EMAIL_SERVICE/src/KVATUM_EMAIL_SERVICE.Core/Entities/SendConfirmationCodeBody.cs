using System.ComponentModel.DataAnnotations;

namespace KVATUM_EMAIL_SERVICE.Core.Entities
{
    public class SendConfirmationCodeBody
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Code { get; set; }
    }
}