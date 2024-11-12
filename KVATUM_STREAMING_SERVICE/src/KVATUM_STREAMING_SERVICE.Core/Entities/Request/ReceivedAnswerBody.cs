namespace KVATUM_STREAMING_SERVICE.Core.Entities.Request
{
    public class ReceivedAnswerBody
    {
        public object Answer { get; set; }
        public Guid From { get; set; }
        public Guid To { get; set; }
    }
}