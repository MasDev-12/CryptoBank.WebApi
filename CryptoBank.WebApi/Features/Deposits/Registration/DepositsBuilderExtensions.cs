using CryptoBank.WebApi.Features.Deposits.Jobs;

namespace CryptoBank.WebApi.Features.Deposits.Registration;

public static class DepositsBuilderExtensions
{
    public static WebApplicationBuilder AddDeposits(this WebApplicationBuilder builder)
    {
        builder.Services.AddHostedService<TbupGenerator>();
        return builder;
    }
}
