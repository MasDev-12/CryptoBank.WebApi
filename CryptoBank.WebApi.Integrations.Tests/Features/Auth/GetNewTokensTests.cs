using CryptoBank.WebApi.Database;
using CryptoBank.WebApi.Features.Auth.Domain;
using CryptoBank.WebApi.Features.Auth.Requests;
using CryptoBank.WebApi.Integrations.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using System.Net;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using FluentValidation.TestHelper;

using static CryptoBank.WebApi.Features.Auth.Errors.AuthValidationErrors;

namespace CryptoBank.WebApi.Integrations.Tests.Features.Auth;

public class GetNewTokensTests : IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;

    private ApplicationDbContext? _applicationDbContext;
    private AsyncServiceScope _scope;
    private CancellationToken _cancellationToken;
    private CookieHelper? _cookieHelper;

    private const string DatabaseConnectionString =
        "Host=localhost;Database=CryptoBankDataBaseDraft.Tests;Username=db_creator;Password=12345678;Maximum Pool Size=10;Connection Idle Lifetime=60;";

    public GetNewTokensTests()
    {
        _factory = WebApplicationFactoryBuilderHelper.ConfigureWebApplicationFactory(DatabaseConnectionString);
    }

    [Fact]
    public async Task Should_get_tokens()
    {
        //Arrange
        var client = _factory.CreateClient();
        var user = CreateUserHelper.CreateUser("test@test", _scope);

        await _applicationDbContext!.Users.AddAsync(user, _cancellationToken);
        await _applicationDbContext.SaveChangesAsync(_cancellationToken);

        var response = await client.PostAsJsonAsync("/auth", new
        {
            Email = user.Email,
            Password = "123456"
        }, cancellationToken: _cancellationToken);

        var tokens = await response.Content.ReadFromJsonAsync<LoginUser.Response>(cancellationToken: _cancellationToken);
        var refreshToken = _cookieHelper!.GetCookie(response);

        client.DefaultRequestHeaders.Add("Cookie", $"refreshToken={refreshToken}");

        //Act
        var getNewTokens = await client.GetAsync($"/auth/get-new-tokens", cancellationToken: _cancellationToken);

        var newTokens = await getNewTokens.Content.ReadFromJsonAsync<GetNewTokens.Response>(cancellationToken: _cancellationToken);

        ClaimsPrincipal userIdClaim = GetUserIdFromAccessToken.GetId(tokens!.AccessToken, _scope);
        var userIdFromClaims = userIdClaim.Claims.SingleOrDefault(i => i.Type == ClaimTypes.NameIdentifier)!.Value;
        var userId = long.Parse(userIdFromClaims);
        var userFromDb = await _applicationDbContext.Users.SingleOrDefaultAsync(x => x.Id == userId);

        //Assert
        userFromDb.Should().NotBeNull();
        userFromDb!.Email.Should().Be(user.Email);

        tokens.Should().NotBeNull();
        tokens.AccessToken.Should().NotBeEmpty();
        newTokens.Should().NotBeNull();
        newTokens!.AccessToken.Should().NotBeEmpty();

        refreshToken.Should().NotBeNull();
        var refreshTokenFromDb = _applicationDbContext.RefreshTokens.SingleOrDefault(x => x.Token == refreshToken);
        refreshTokenFromDb.Should().NotBeNull();
        refreshTokenFromDb!.UserId.Should().Be(user.Id);
    }

    [Fact]
    public async Task Should_validate_old_token()
    {
        //Arrange
        var client = _factory.CreateClient();

        var user = CreateUserHelper.CreateUser("test@test.com", _scope);

        await _applicationDbContext!.Users.AddAsync(user, _cancellationToken);
        await _applicationDbContext.SaveChangesAsync(_cancellationToken);

        var testRefreshTokens = new List<RefreshToken>
        {
            new()
            {
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                Revoked = true,
                TokenValidityPeriod = DateTime.UtcNow.AddDays(2),
                Token = "revokedToken",
            },
            new()
            {
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                Revoked = false,
                TokenValidityPeriod = DateTime.UtcNow.AddDays(2),
                Token = "notRevokedToken",
            }
        };

        await _applicationDbContext.RefreshTokens.AddRangeAsync(testRefreshTokens, _cancellationToken);
        await _applicationDbContext.SaveChangesAsync(_cancellationToken);

        client.DefaultRequestHeaders.Add("Cookie", $"refreshToken={testRefreshTokens[0].Token}");

        //Act
        var getNewTokens = await client.GetAsync($"/auth/get-new-tokens", cancellationToken: _cancellationToken);

        getNewTokens.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        //Assert
        var response = await getNewTokens.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken: _cancellationToken);
        response!.Detail.Should().Be("Invalid token");

        _applicationDbContext.RefreshTokens
            .Where(x => x.UserId == user.Id)
            .Select(x => x.Revoked)
            .Should()
            .AllBeEquivalentTo(true);
    }

    [Fact]
    public async Task Should_delete_old_token()
    {
        var client = _factory.CreateClient();

        var user = CreateUserHelper.CreateUser("test@test.com", _scope);

        await _applicationDbContext!.Users.AddAsync(user, _cancellationToken);
        await _applicationDbContext.SaveChangesAsync(_cancellationToken);

        var testRefreshTokens = new List<RefreshToken>
        {
            new()
            {
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow.ToUniversalTime(),
                Revoked = true,
                TokenValidityPeriod = DateTime.UtcNow.AddDays(2),
                TokenStoragePeriod = DateTime.UtcNow.AddDays(-170),
                Token = "oldToken",
            },
            new()
            {
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow.ToUniversalTime(),
                Revoked = false,
                TokenValidityPeriod = DateTime.UtcNow.AddDays(2),
                TokenStoragePeriod = DateTime.UtcNow.AddDays(30),
                Token = "notRevokedToken",
            }
        };

        await _applicationDbContext.RefreshTokens.AddRangeAsync(testRefreshTokens, _cancellationToken);
        await _applicationDbContext.SaveChangesAsync(_cancellationToken);

        client.DefaultRequestHeaders.Add("Cookie", $"refreshToken={testRefreshTokens[1].Token}");

        //Act
        await client.GetAsync($"/auth/get-new-tokens", cancellationToken: _cancellationToken);

        var oldRefreshToken = await _applicationDbContext.RefreshTokens.SingleOrDefaultAsync(x => x.Token == "oldToken",
            _cancellationToken);
        oldRefreshToken.Should().BeNull();
    }

    public async Task DisposeAsync()
    {
        await FactoryInitHelper.ClearDataAndDisposeAsync(_factory, _cancellationToken);

        await _scope.DisposeAsync();
    }

    public Task InitializeAsync()
    {
        FactoryInitHelper.Init(_factory, out _scope, out _cancellationToken);
        _applicationDbContext = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        _cookieHelper = new CookieHelper();

        return Task.CompletedTask;
    }
}

public class GetNewTokensPairValidatorTests : IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;

    private ApplicationDbContext? _applicationDbContext;
    private AsyncServiceScope _scope;
    private CancellationToken _cancellationToken;
    private GetNewTokens.RequestValidator? _validator;

    private const string DatabaseConnectionString =
        "Host=localhost;Database=CryptoBankDataBaseDraft.Tests;Username=postgres;Password=Masud1992;Maximum Pool Size=10;Connection Idle Lifetime=60;";

    public GetNewTokensPairValidatorTests()
    {
        _factory = WebApplicationFactoryBuilderHelper.ConfigureWebApplicationFactory(DatabaseConnectionString);
    }

    [Fact]
    public async Task Should_exist_refreshToken()
    {
        //Arrange
        string refreshToken = "testRefreshToken";

        //Act
        var result = await _validator.TestValidateAsync(new GetNewTokens.Request(refreshToken)
        {
            RefreshToken = refreshToken
        }, cancellationToken: _cancellationToken);

        //Assert
        result.ShouldHaveValidationErrorFor(x => x.RefreshToken)
        .WithErrorCode(TokenNotExists);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Should_require_refresh_token(string refreshToken)
    {
        //Act
        var result = await _validator.TestValidateAsync(new GetNewTokens.Request(refreshToken)
        {
            RefreshToken = refreshToken
        }, cancellationToken: _cancellationToken);

        //Assert
        result.ShouldHaveValidationErrorFor(x => x.RefreshToken)
            .WithErrorCode(TokenRequired);
    }

    public async Task DisposeAsync()
    {
        await FactoryInitHelper.ClearDataAndDisposeAsync(_factory, _cancellationToken);
        await _applicationDbContext!.SaveChangesAsync(_cancellationToken);
        await _applicationDbContext.DisposeAsync();

        await _scope.DisposeAsync();
    }

    public Task InitializeAsync()
    {
        FactoryInitHelper.Init(_factory, out _scope, out _cancellationToken);
        _applicationDbContext = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        _validator = new GetNewTokens.RequestValidator(_applicationDbContext);

        return Task.CompletedTask;
    }
}
