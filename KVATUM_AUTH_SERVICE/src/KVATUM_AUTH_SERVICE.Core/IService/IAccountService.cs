using System.Net;
using KVATUM_AUTH_SERVICE.Core.Entities.Response;

namespace KVATUM_AUTH_SERVICE.Core.IService
{
    public interface IAccountService
    {
        Task<ServiceResponse<ProfileBody>> GetProfileByEmail(string email);
        Task<ServiceResponse<IEnumerable<ProfileBody>>> GetAccountsByPatternNickname(string patternNickname, int limit, int offset);
        Task<ServiceResponse<ProfileBody>> GetProfileByNickname(string nickname);
        Task<HttpStatusCode> ChangeAccountNickname(Guid accountId, string nickname);
        Task<ServiceResponse<ProfileBody>> GetProfile(Guid accountId);
    }
}