using System.ComponentModel.DataAnnotations;

namespace KVATUM_AUTH_SERVICE.Core.Entities.Request
{
    public class TokenBody
    {
        [Required]
        public string Value { get; set; }
    }
}