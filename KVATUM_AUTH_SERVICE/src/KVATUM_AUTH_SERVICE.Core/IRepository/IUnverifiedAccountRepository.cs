using KVATUM_AUTH_SERVICE.Core.Entities.Models;
using KVATUM_AUTH_SERVICE.Core.Entities.Request;

namespace KVATUM_AUTH_SERVICE.Core.IRepository
{
    public interface IUnverifiedAccountRepository
    {
        Task<UnverifiedAccount?> AddAsync(SignUpBody body, string verificationCode);
        Task<UnverifiedAccount?> GetAsync(string email);
        Task<bool> DeleteAsync(string email);
    }
}