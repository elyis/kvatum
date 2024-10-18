namespace KVATUM_AUTH_SERVICE.Core.Entities.Response
{
    public class TokenPayload
    {
        public Guid AccountId { get; set; }
        public string Role { get; set; }
        public Guid SessionId { get; set; }
    }
}