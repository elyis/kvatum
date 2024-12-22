using KVATUM_STREAMING_SERVICE.Core.Entities.Response;

namespace KVATUM_STREAMING_SERVICE.Core.IService
{
    public interface IAccountManager
    {
        Task<AccountProfileBody?> GetAccountProfileAsync(Guid accountId);
    }
}