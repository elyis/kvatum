namespace KVATUM_AUTH_SERVICE.Core
{
    public class Constants
    {
        public static readonly string FileServerUrl = Environment.GetEnvironmentVariable("FILE_SERVER_URL") ?? throw new Exception("FILE_SERVER_URL is not found in enviroment variables");
        public static readonly string WebUrlToProfileImage = $"{FileServerUrl}/api/images/profile";
    }
}