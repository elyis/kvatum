namespace KVATUM_AUTH_SERVICE.Core.Entities.Cache
{
    public class ResponseCache
    {
        public string Value { get; set; }
        public bool IsIndexKey { get; set; } = false;
    }
}