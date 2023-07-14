using CryptoBank.WebApi.Features.Auth.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using System.Security.Claims;

namespace CryptoBank.WebApi.Integrations.Tests.Helpers;

public static class GetUserIdFromAccessToken
{
    public static ClaimsPrincipal GetId(string accessToken, AsyncServiceScope scope)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtOptions = scope.ServiceProvider.GetRequiredService<IOptions<AuthOptions>>().Value;

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtOptions.Jwt.Issuer,
                ValidAudience = jwtOptions.Jwt.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(jwtOptions.Jwt.SigningKey)),
            };

            var principal = tokenHandler.ValidateToken(accessToken, tokenValidationParameters, out var validatedToken);

            if (!CheckJwtAlgorithm(validatedToken))
            {
                throw new SecurityTokenException("Invalid token passed");
            }

            return principal;
        }
        catch (Exception)
        {
            throw new AuthenticationException("One or more validation failures have occurred");
        }
    }
    public static bool CheckJwtAlgorithm(SecurityToken securityToken)
    {
        return (securityToken is JwtSecurityToken jwtSecurityToken) &&
            jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256Signature,
                   StringComparison.InvariantCultureIgnoreCase);
    }
}
