namespace CryptoBank.WebApi.Features.Auth.Options;

public class RefreshTokenOptions
{
    public TimeSpan StoragePeriod { get; set; }
    public TimeSpan ValidityPeriod { get; set; }
    public int LengthBytes { get; set; }
}
