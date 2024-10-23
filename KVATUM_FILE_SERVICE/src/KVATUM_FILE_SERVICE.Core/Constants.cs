namespace KVATUM_FILE_SERVICE.Core
{
    public class Constants
    {
        public static readonly string LocalPathToStorages = Environment.GetEnvironmentVariable("FILE_SERVER_STORAGE_PATH") ?? throw new Exception("FILE_SERVER_STORAGE_PATH environment variable is not set");
        public static readonly string LocalPathToProfileImages = Path.Combine(LocalPathToStorages, "profile_images");
        public static readonly string LocalPathToHubIcons = Path.Combine(LocalPathToStorages, "hub_icons");
    }
}