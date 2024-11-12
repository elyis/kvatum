using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using KVATUM_CHATFLOW_SERVICE.Core.Entities.Response;
using KVATUM_STREAMING_SERVICE.Core.IService;

namespace KVATUM_STREAMING_SERVICE.App.Service
{
    public class JwtService : IJwtService
    {
        private List<Claim> GetClaims(string token)
        {
            return new JwtSecurityTokenHandler()
                .ReadJwtToken(token.Replace("Bearer ", ""))
                .Claims
                .ToList();
        }

        public TokenPayload GetTokenPayload(string token)
        {
            var claims = GetClaims(token);
            return new TokenPayload
            {
                Role = claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Role)?.Value,
                AccountId = Guid.Parse(claims.FirstOrDefault(claim => claim.Type == "AccountId")?.Value),
                SessionId = Guid.Parse(claims.FirstOrDefault(claim => claim.Type == "SessionId")?.Value),
            };
        }
    }
}