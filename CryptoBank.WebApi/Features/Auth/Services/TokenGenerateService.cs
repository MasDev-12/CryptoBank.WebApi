using CryptoBank.WebApi.Database;
using CryptoBank.WebApi.Features.Auth.Options;
using CryptoBank.WebApi.Features.Users.Domain;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CryptoBank.WebApi.Features.Auth.Services;

public class TokenGenerateService
{
    private readonly ApplicationDbContext _applicationDbContext;
    private readonly AuthOptions _authOptions;

    public TokenGenerateService(ApplicationDbContext applicationDbContext, IOptions<AuthOptions> authOptions)
    {
        _applicationDbContext = applicationDbContext;
        _authOptions = authOptions.Value;
    }

    public async Task<string> GenerateAccessToken(User user,CancellationToken cancellationToken)
    {
        var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
                new Claim(ClaimTypes.Email,user.Email),
            };

        foreach (var role in user.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role.Name.ToString()));
        }

        var keyBytes = Convert.FromBase64String(_authOptions.Jwt.SigningKey);
        var key = new SymmetricSecurityKey(keyBytes);
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);
        var expires = DateTime.Now + _authOptions.Jwt.Expiration;
        var token = new JwtSecurityToken(
            _authOptions.Jwt.Issuer,
            _authOptions.Jwt.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
