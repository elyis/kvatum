using KVATUM_STREAMING_SERVICE.Core.Enums;

namespace KVATUM_STREAMING_SERVICE.Core.Entities.Response
{
    public class AnswerBody
    {
        public MessageEvent EventType { get; set; }
        public object Answer { get; set; }
        public AccountConnectionBody From { get; set; }
        public Guid To { get; set; }
    }
}