namespace KVATUM_AUTH_SERVICE.Core.Entities.Events
{
    public class EmailConfirmationEvent
    {
        public string Email { get; set; }
        public string Code { get; set; }
    }
}