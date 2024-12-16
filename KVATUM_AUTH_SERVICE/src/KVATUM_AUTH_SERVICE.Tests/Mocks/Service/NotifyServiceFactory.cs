using KVATUM_AUTH_SERVICE.Core.IService;
using Moq;

namespace KVATUM_AUTH_SERVICE.Tests.Mocks.Service
{
    public class NotifyServiceFactory
    {
        public static INotifyService Create()
        {
            var notifyService = new Mock<INotifyService>();
            return notifyService.Object;
        }
    }
}