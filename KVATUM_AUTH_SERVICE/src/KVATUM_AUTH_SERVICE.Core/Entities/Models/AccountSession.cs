using KVATUM_AUTH_SERVICE.Core.Entities.Response;

namespace KVATUM_AUTH_SERVICE.Core.Entities.Models
{
    public class AccountSession
    {
        public Guid Id { get; set; }
        public string Ip { get; set; }
        public string UserAgent { get; set; }
        public Account Account { get; set; }
        public Guid AccountId { get; set; }

        public string? Token { get; set; }
        public DateTime? TokenValidBefore { get; set; }

        public AccountSessionBody ToAccountSessionBody()
        {
            return new AccountSessionBody
            {
                Id = Id,
                Ip = Ip,
                UserAgent = UserAgent,
            };
        }
    }
}