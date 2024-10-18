namespace Microservice_module.Core
{
    public static class Constants
    {
        public static readonly string ServerUrl = Environment.GetEnvironmentVariable("ServerUrl") ?? throw new Exception("ServerUrl is not found in enviroment variables");
    }
}