using KVATUM_AUTH_SERVICE.Core.Entities.Events;

namespace KVATUM_AUTH_SERVICE.Core.IService
{
    public interface INotifyService
    {
        void Publish<T>(T message, PublishEvent eventType);
    }
}