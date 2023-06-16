namespace CryptoBank.WebApi.Features.Auth.Options;

public class AuthOptions
{
    public JwtOptions Jwt { get; set; } = new JwtOptions();

    public class JwtOptions
    {
        public string SigningKey { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public TimeSpan Expiration { get; set; }
    }
}
