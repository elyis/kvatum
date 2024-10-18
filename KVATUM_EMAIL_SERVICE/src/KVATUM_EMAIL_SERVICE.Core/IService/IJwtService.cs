using KVATUM_EMAIL_SERVICE.Core.Entities.Response;

namespace KVATUM_EMAIL_SERVICE.Core.IService
{
    public interface IJwtService
    {
        TokenPayload GetTokenPayload(string token);
    }
}