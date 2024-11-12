namespace KVATUM_STREAMING_SERVICE.Core.Entities.Request
{
    public class ReceivedOfferBody
    {
        public object Offer { get; set; }
        public Guid From { get; set; }
        public Guid To { get; set; }
    }
}