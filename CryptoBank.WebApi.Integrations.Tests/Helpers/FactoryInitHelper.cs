using CryptoBank.WebApi.Database;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace CryptoBank.WebApi.Integrations.Tests.Helpers;

public static class FactoryInitHelper
{
    public static void Init(WebApplicationFactory<Program> factory
        , ref AsyncServiceScope scope
        , ref ApplicationDbContext applicationDbContext
        , ref CancellationToken cancellationToken)
    {
        var _ = factory.Server;
        scope = factory.Services.CreateAsyncScope();
        applicationDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        cancellationToken = CancellationTokenHelper.GetCancellationToken();
    }

    public static void ClearDataAndDisposeAsync(ref ApplicationDbContext applicationDbContext)
    {
        applicationDbContext.Accounts.RemoveRange(applicationDbContext.Accounts);
        applicationDbContext.RefreshTokens.RemoveRange(applicationDbContext.RefreshTokens);
        applicationDbContext.Roles.RemoveRange(applicationDbContext.Roles);
        applicationDbContext.Users.RemoveRange(applicationDbContext.Users);
    }
}
