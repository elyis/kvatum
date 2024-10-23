using KVATUM_FILE_SERVICE.Core.Entities.Request;

namespace KVATUM_FILE_SERVICE.Core.IService
{
    public interface IJwtService
    {
        TokenPayload GetTokenPayload(string token);
    }
}