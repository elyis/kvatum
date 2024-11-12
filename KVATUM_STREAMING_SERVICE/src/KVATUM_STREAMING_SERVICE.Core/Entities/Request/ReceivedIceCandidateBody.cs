namespace KVATUM_STREAMING_SERVICE.Core.Entities.Request
{
    public class ReceivedIceCandidateBody
    {
        public object IceCandidate { get; set; }
        public Guid From { get; set; }
        public Guid To { get; set; }
    }
}