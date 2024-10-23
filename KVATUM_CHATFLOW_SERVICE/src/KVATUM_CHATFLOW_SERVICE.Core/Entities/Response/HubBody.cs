namespace KVATUM_CHATFLOW_SERVICE.Core.Entities.Response
{
    public class HubBody
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<ImageWithResolutionBody> Images { get; set; } = new();
    }
}