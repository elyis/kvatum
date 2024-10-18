namespace KVATUM_EMAIL_SERVICE.Core.IService
{
    public interface INotifyService
    {
        void Publish<T>(T message, PublishEvent eventType);
    }
}