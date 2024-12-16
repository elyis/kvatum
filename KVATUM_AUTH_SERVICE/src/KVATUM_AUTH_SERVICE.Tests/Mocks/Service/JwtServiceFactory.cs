using KVATUM_AUTH_SERVICE.Core.Entities.Response;
using KVATUM_AUTH_SERVICE.Core.Enums;
using KVATUM_AUTH_SERVICE.Core.IService;
using Moq;

namespace KVATUM_AUTH_SERVICE.Tests.Mocks
{
    public class JwtServiceFactory
    {
        public static IJwtService Create()
        {
            var jwtService = new Mock<IJwtService>();

            var tokenPayload = new TokenPayload
            {
                Role = AccountRole.User.ToString(),
                AccountId = Guid.Parse("d6f9c87a-569a-40a8-9a6e-8f9af0d45ecb"),
                SessionId = Guid.Parse("d6f9c87a-569a-40a8-9a6e-8f9af0d45ecb")
            };

            jwtService.Setup(e => e.GenerateOutputAccountCredentials(It.IsAny<TokenPayload>())).Returns(new OutputAccountCredentialsBody("access_token", "refresh_token"));
            jwtService.Setup(e => e.GetTokenPayload(It.IsAny<string>())).Returns(tokenPayload);

            return jwtService.Object;
        }
    }
}