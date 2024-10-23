using KVATUM_FILE_SERVICE.Core.Enums;

namespace KVATUM_FILE_SERVICE.Core.IService
{
    public interface INotifyService
    {
        void Publish<T>(T message, ContentUploaded content);
    }
}