using CryptoBank.WebApi.Database;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace CryptoBank.WebApi.Integrations.Tests.Helpers;

public static class FactoryInitHelper
{
    public static void Init(WebApplicationFactory<Program> factory
        , out AsyncServiceScope scope
        , out CancellationToken cancellationToken)
    {
        var _ = factory.Server;
        scope = factory.Services.CreateAsyncScope();
        cancellationToken = CancellationTokenHelper.GetCancellationToken();
    }

    public static async Task ClearDataAndDisposeAsync(WebApplicationFactory<Program> factory, CancellationToken cancellationToken)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Accounts.RemoveRange(dbContext.Accounts);
        dbContext.RefreshTokens.RemoveRange(dbContext.RefreshTokens);
        dbContext.Roles.RemoveRange(dbContext.Roles);
        dbContext.Users.RemoveRange(dbContext.Users);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
