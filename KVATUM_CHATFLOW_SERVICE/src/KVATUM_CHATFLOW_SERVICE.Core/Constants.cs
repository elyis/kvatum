namespace Microservice_module.Core
{
    public class Constants
    {
        public static readonly string FileServerUrl = Environment.GetEnvironmentVariable("FILE_SERVER_URL") ?? throw new Exception("FileServerUrl is not found in enviroment variables");
        public static readonly string WebUrlToHubIcon = $"{FileServerUrl}/api/images/hub";
    }
}