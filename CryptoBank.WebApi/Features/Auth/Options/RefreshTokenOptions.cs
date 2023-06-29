namespace CryptoBank.WebApi.Features.Auth.Options;

public class RefreshTokenOptions
{
    public TimeSpan RefreshTokenExpiration { get; set; }
    public int HashLengthInBytes { get; set; }
}
