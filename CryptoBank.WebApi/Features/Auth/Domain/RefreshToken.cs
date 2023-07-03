using CryptoBank.WebApi.Features.Users.Domain;

namespace CryptoBank.WebApi.Features.Auth.Domain;

public class RefreshToken
{
    public long Id { get; set; }
    public string Token { get; set; }
    public long UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime TokenValidityPeriod { get; set; }
    public DateTime TokenStoragePeriod { get; set; }
    public bool Revoked { get; set; }
    public long? ReplacedByNextToken { get; set; }

    public User User { get; set; }
}
