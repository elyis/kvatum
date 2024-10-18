using System.Net;
using KVATUM_AUTH_SERVICE.Core.Entities.Request;
using KVATUM_AUTH_SERVICE.Core.Entities.Response;

namespace KVATUM_AUTH_SERVICE.Core.IService
{
    public interface IAuthService
    {
        Task<ServiceResponse<OutputAccountCredentialsBody>> RegisterNewAccount(string email, string verificationCode, string userAgent, string ipAddress, string rolename);
        Task<HttpStatusCode> CreateUnverifiedAccount(SignUpBody body);
        Task<ServiceResponse<OutputAccountCredentialsBody>> SignIn(SignInBody body, string userAgent, string ipAddress);
        Task<ServiceResponse<OutputAccountCredentialsBody>> RestoreAccessToken(string refreshToken);
    }
}