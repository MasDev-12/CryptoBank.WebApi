using CryptoBank.WebApi.Database;
using CryptoBank.WebApi.Features.Auth.Domain;
using CryptoBank.WebApi.Features.Auth.Options;
using CryptoBank.WebApi.Features.Users.Domain;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace CryptoBank.WebApi.Features.Auth.Services;

public class TokenGenerateService
{
    private readonly ApplicationDbContext _applicationDbContext;
    private readonly AuthOptions _authOptions;
    private readonly RefreshTokenOptions _refreshTokenOptions;

    public TokenGenerateService(
         ApplicationDbContext applicationDbContext
         , IOptions<AuthOptions> authOptions
         , IOptions<RefreshTokenOptions> refreshTokenOptions)
    {
        _authOptions = authOptions.Value;
        _refreshTokenOptions = refreshTokenOptions.Value;
        _applicationDbContext = applicationDbContext;
    }

    public async Task<(string accessToken, string refreshToken)> GenerateTokens(User user, CancellationToken cancellationToken)
    {
        var refreshTokens = user.RefreshTokens
              .OrderByDescending(x => x.CreatedAt)
              .ToArray();

        var accessToken = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken();
        await using var transaction = await _applicationDbContext.Database.BeginTransactionAsync(cancellationToken);
        {
            try
            {
                var newRefreshToken = new RefreshToken
                {
                    UserId = user.Id,
                    Token = refreshToken,
                    TokenValidityPeriod = DateTime.Now.Add(_refreshTokenOptions.ValidityPeriod).ToUniversalTime(),
                    TokenStoragePeriod = DateTime.Now.Add(_refreshTokenOptions.StoragePeriod).ToUniversalTime(),
                    CreatedAt = DateTime.Now.ToUniversalTime(),
                };

                user.RefreshTokens.Add(newRefreshToken);
                _applicationDbContext.Add(newRefreshToken);
                await _applicationDbContext.SaveChangesAsync(cancellationToken);

                var notRevokeRefreshToken = refreshTokens.FirstOrDefault(t => !t.Revoked);
                if (notRevokeRefreshToken != null)
                {
                    notRevokeRefreshToken.Revoked = true;
                    notRevokeRefreshToken.ReplacedByNextToken = newRefreshToken.Id;
                }
                var overdueTokens = refreshTokens.Where(x => x.TokenStoragePeriod <= DateTime.Now.ToUniversalTime()).ToArray();

                _applicationDbContext.RefreshTokens.RemoveRange(overdueTokens);
                await _applicationDbContext.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw new Exception();
            }
            return (accessToken, refreshToken);
        }
    }

    private string GenerateAccessToken(User user)
    {
        var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
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

    private string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(_refreshTokenOptions.LengthBytes));
    }
}
