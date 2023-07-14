using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace CryptoBank.WebApi.Integrations.Tests.Helpers;

public static class WebApplicationFactoryBuilderHelper
{
    public static WebApplicationFactory<Program> ConfigureWebApplicationFactory(string databaseConnectionString)
    {
        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, configBuilder) =>
            {
                configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "ConnectionStrings:ApplicationDbContext", databaseConnectionString }
                });
            });
        });
    }
}
