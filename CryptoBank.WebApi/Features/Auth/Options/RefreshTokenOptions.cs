namespace CryptoBank.WebApi.Features.Auth.Options;

public class RefreshTokenOptions
{
    public TimeSpan RefreshTokenExpiration { get; set; }
    public TimeSpan RefreshTokenExpirationPeriod { get; set; }
    public int LengthBytes { get; set; }
}
