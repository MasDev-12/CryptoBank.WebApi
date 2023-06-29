using CryptoBank.WebApi.Features.Auth.Options;
using CryptoBank.WebApi.Features.Auth.Services;

namespace CryptoBank.WebApi.Features.Auth.Registration;

public static class AuthBuilderExtensions
{
    public static WebApplicationBuilder AddAuth(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<AuthOptions>(builder.Configuration.GetSection("Features:Auth"));
        builder.Services.Configure<RefreshTokenOptions>(builder.Configuration.GetSection("Features:Auth:RefreshToken"));
        builder.Services.AddTransient<TokenGenerateService>();
        return builder;
    }
}
