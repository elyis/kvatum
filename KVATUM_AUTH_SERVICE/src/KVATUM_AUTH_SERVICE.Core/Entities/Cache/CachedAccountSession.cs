namespace KVATUM_AUTH_SERVICE.Core.Entities.Cache
{
    public class CachedAccountSession
    {
        public Guid Id { get; set; }
        public string Ip { get; set; }
        public string UserAgent { get; set; }
        public Guid AccountId { get; set; }

        public string? Token { get; set; }
    }
}