using System.Net;
using KVATUM_AUTH_SERVICE.Core.Entities.Response;

namespace KVATUM_AUTH_SERVICE.Core.IService
{
    public interface IUserService
    {
        Task<ServiceResponse<ProfileBody>> GetProfile(string identifier);
        // Task<ServiceResponse<IEnumerable<ProfileBody>>> GetAllProfilesByPatternIdentifier(string patternIdentifier);
        // Task<ServiceResponse<IEnumerable<ProfileBody>>> GetAllProfilesByPatternTag(string patternTag);
        // Task<ServiceResponse<ProfileBody>> GetProfileByTag(string tag);
        // Task<HttpStatusCode> ChangeAccountTag(Guid accountId, string tag);
        Task<ServiceResponse<ProfileBody>> GetProfile(Guid accountId);
    }
}