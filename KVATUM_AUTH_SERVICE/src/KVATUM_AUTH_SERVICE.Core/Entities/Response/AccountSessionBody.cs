namespace KVATUM_AUTH_SERVICE.Core.Entities.Response
{
    public class AccountSessionBody
    {
        public Guid Id { get; set; }
        public string Ip { get; set; }
        public string UserAgent { get; set; }
    }
}