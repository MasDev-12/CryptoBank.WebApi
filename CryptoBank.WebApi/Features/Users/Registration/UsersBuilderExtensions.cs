using CryptoBank.WebApi.Features.Users.Options;
using CryptoBank.WebApi.Features.Users.Services;

namespace CryptoBank.WebApi.Features.Users.Registration;

public static class UsersBuilderExtensions
{
    public static WebApplicationBuilder AddUsers(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<UsersOptions>(builder.Configuration.GetSection("Features:Users"));
        builder.Services.Configure<PasswordHashingOptions>(builder.Configuration.GetSection("Argon2Config"));
        builder.Services.AddTransient<PasswordHashingService>();
        return builder;
    }
}
