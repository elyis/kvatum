namespace KVATUM_CHATFLOW_SERVICE.Core.Entities.Response
{
    public class WorkspaceBody
    {
        public string Name { get; set; }
        public string HexColor { get; set; }
        public List<ImageWithResolutionBody> ImageUrls { get; set; } = new();
    }
}