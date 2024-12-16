using KVATUM_AUTH_SERVICE.Core.IService;
using Moq;

namespace KVATUM_AUTH_SERVICE.Tests.Mocks
{
    public class HashPasswordServiceFactory
    {
        public static IHashPasswordService Create()
        {
            var hashPasswordService = new Mock<IHashPasswordService>();
            hashPasswordService.Setup(e => e.Compute(It.IsAny<string>())).Returns("hash");
            return hashPasswordService.Object;
        }
    }
}