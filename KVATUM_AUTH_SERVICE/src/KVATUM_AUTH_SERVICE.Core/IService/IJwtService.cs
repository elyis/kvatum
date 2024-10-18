using KVATUM_AUTH_SERVICE.Core.Entities.Response;

namespace KVATUM_AUTH_SERVICE.Core.IService
{
    public interface IJwtService
    {
        OutputAccountCredentialsBody GenerateDefaultTokenPair(TokenPayload tokenPayload);
        TokenPayload GetTokenPayload(string token);
    }
}