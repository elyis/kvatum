using KVATUM_CHATFLOW_SERVICE.Core.Entities.Response;

namespace KVATUM_CHATFLOW_SERVICE.Core.IService
{
    public interface IJwtService
    {
        TokenPayload GetTokenPayload(string token);
    }
}