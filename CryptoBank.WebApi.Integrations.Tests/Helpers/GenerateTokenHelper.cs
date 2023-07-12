using CryptoBank.WebApi.Features.Auth.Services;
using CryptoBank.WebApi.Features.Users.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace CryptoBank.WebApi.Integrations.Tests.Helpers;

public static class GenerateTokenHelper
{
    public static async Task GetAccessToken(HttpClient httpClient, AsyncServiceScope scope, User user, CancellationToken cancellationToken)
    {
        var tokenService = scope.ServiceProvider.GetRequiredService<TokenGenerateService>();
        var tokens = await tokenService.GenerateTokens(user, cancellationToken);
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {tokens.accessToken}");
    }
}
