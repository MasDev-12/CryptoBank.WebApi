using CryptoBank.WebApi.Features.Users.Domain;

namespace CryptoBank.WebApi.Features.Auth.Domain;

public class RefreshToken
{
    public long Id { get; set; }
    public string Token { get; set; }
    public long userId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiryDate { get; set; }
    public bool Revoke { get; set; }
    public long ReplacedByNextToken { get; set; }

    public User User { get; set; }
}
