using Microsoft.Extensions.Logging;
using Moq;

namespace KVATUM_AUTH_SERVICE.Tests.Mocks.Service
{
    public class LoggerFactory
    {
        public static ILogger<T> CreateLogger<T>()
        {
            var logger = new Mock<ILogger<T>>();
            return logger.Object;
        }
    }
}