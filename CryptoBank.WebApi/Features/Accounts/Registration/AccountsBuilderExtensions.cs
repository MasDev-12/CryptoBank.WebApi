using CryptoBank.WebApi.Features.Accounts.Options;

namespace CryptoBank.WebApi.Features.Accounts.Registration;

public static class AccountsBuilderExtensions
{
    public static WebApplicationBuilder AddAccounts(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<AccountOptions>(builder.Configuration.GetSection("Features:Accounts"));
        return builder;
    }
}
