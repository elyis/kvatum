namespace KVATUM_AUTH_SERVICE.Core.Entities.Models
{
    public class UnverifiedAccount
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string Nickname { get; set; }
        public string Password { get; set; }
        public string VerificationCode { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}