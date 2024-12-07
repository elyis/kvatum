using KVATUM_STREAMING_SERVICE.Core.Entities.Response;

namespace KVATUM_STREAMING_SERVICE.Core.IService
{
    public interface IAccountService
    {
        Task<AccountProfileBody?> GetAccountProfileAsync(Guid accountId);
    }
}