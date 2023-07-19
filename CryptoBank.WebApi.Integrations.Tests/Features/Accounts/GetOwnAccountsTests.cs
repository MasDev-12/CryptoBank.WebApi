using CryptoBank.WebApi.Database;
using CryptoBank.WebApi.Features.Accounts.Domain;
using CryptoBank.WebApi.Features.Accounts.Options;
using CryptoBank.WebApi.Features.Accounts.Requests;
using CryptoBank.WebApi.Integrations.Tests.Helpers;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

using static CryptoBank.WebApi.Features.Users.Errors.UserValidationErrors;

namespace CryptoBank.WebApi.Integrations.Tests.Features.Accounts;

public class GetOwnAccountsTests : IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private ApplicationDbContext? _applicationDbContext;
    private AsyncServiceScope _scope;
    private CancellationToken _cancellationToken;
    private AccountOptions? _accountOptions;

    private string DatabaseConnectionString =
       "Host=localhost;Database=CryptoBankDataBaseDraft;Username=postgres;Password=Masud1992;Maximum Pool Size=10;Connection Idle Lifetime=60;";

    public GetOwnAccountsTests()
    {
        _factory = WebApplicationFactoryBuilderHelper.ConfigureWebApplicationFactory(DatabaseConnectionString);
    }

    [Fact]
    public async Task Should_get_own_accounts()
    {
        //Arrange
        var client = _factory.CreateClient();
        var user = CreateUserHelper.CreateUser("test@test.com", _scope);
        await _applicationDbContext!.Users.AddAsync(user, _cancellationToken);
        await _applicationDbContext.SaveChangesAsync(_cancellationToken);

        var accounts = new List<Account>();
        for (int i = 0; i < _accountOptions!.MaxAccountsPerUser; i++)
        {
            accounts.Add(new Account()
            {
                UserId = user.Id,
                Number = $"1100-1200-1300-140{i}",
                Amount = 0,
                Currency = "BTC",
                CreatedAt = DateTime.UtcNow.ToUniversalTime(),
            });
        }

        _applicationDbContext.Accounts.AddRange(accounts);
        await _applicationDbContext.SaveChangesAsync(_cancellationToken);

        await GenerateTokenHelper.GetAccessToken(client, _scope, user, _cancellationToken);

        var response = await client.GetFromJsonAsync<GetOwnAccounts.Response>("/accounts/own", cancellationToken: _cancellationToken);

        response.Should().NotBeNull();
        foreach (var item in response!.AccountModels)
        {
            item.UserId.Should().Be(user.Id);
        }
    }

    [Fact]
    public async Task Should_not_authorize_user()
    {   //Arrange
        var client = _factory.CreateClient();
        var user = CreateUserHelper.CreateUser("test@test.com", _scope);

        await _applicationDbContext!.Users.AddAsync(user, _cancellationToken);
        await _applicationDbContext.SaveChangesAsync(_cancellationToken);

        client.DefaultRequestHeaders.Add("Authorization", $"Bearer" +
            $"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c");

        //Act
        var response = await client.GetAsync("/accounts/own", cancellationToken: _cancellationToken);

        //Assert
        response!.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
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
        _accountOptions = _scope.ServiceProvider.GetRequiredService<IOptions<AccountOptions>>().Value;

        return Task.CompletedTask;
    }
}

public class GetOwnAccountsValidationTests : IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private ApplicationDbContext? _applicationDbContext;
    private AsyncServiceScope _scope;
    private CancellationToken _cancellationToken;
    private GetOwnAccounts.RequestValidator? _validator;

    private string DatabaseConnectionString =
       "Host=localhost;Database=CryptoBankDataBaseDraft;Username=postgres;Password=Masud1992;Maximum Pool Size=10;Connection Idle Lifetime=60;";

    public GetOwnAccountsValidationTests()
    {
        _factory = WebApplicationFactoryBuilderHelper.ConfigureWebApplicationFactory(DatabaseConnectionString);
    }

    [Fact]
    public async Task Should_validate_correct_request()
    {
        //Arrange
        var client = _factory.CreateClient();
        var user = CreateUserHelper.CreateUser("test@test.com", _scope);
        await _applicationDbContext!.Users.AddAsync(user, _cancellationToken);
        await _applicationDbContext.SaveChangesAsync(_cancellationToken);

        //Act
        var result = await _validator.TestValidateAsync(new GetOwnAccounts.Request(user.Id), cancellationToken: _cancellationToken);

        //Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [MemberData(nameof(GenerateRandomLong))]
    public async Task Should_validate_empty_user_request(long userId)
    {
        var result = await _validator.TestValidateAsync(
            new GetOwnAccounts.Request(userId), cancellationToken: _cancellationToken);
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorCode(NotExists);
    }

    public static IEnumerable<object[]> GenerateRandomLong()
    {
        Random random = new Random();
        for (int i = 0; i < 10; i++)
        {
            yield return new object[] { random.NextInt64() };
        }
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
        _validator = new GetOwnAccounts.RequestValidator(_applicationDbContext);

        return Task.CompletedTask;
    }
}