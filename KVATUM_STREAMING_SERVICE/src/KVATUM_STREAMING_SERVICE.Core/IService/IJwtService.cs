using KVATUM_CHATFLOW_SERVICE.Core.Entities.Response;

namespace KVATUM_STREAMING_SERVICE.Core.IService
{
    public interface IJwtService
    {
        TokenPayload GetTokenPayload(string token);
    }
}