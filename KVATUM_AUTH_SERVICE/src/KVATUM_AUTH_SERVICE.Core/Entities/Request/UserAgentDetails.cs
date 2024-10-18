namespace KVATUM_AUTH_SERVICE.Core.Entities.Request
{
    public class UserAgentDetails
    {
        public string ClientId { get; set; }
        public string OS { get; set; }
        public string DeviceName { get; set; }
        public string Model { get; set; }
    }
}