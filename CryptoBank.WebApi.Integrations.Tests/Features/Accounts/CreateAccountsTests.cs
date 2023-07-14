using CryptoBank.WebApi.Database;
using CryptoBank.WebApi.Features.Accounts.Domain;
using CryptoBank.WebApi.Features.Accounts.Options;
using CryptoBank.WebApi.Features.Accounts.Requests;
using CryptoBank.WebApi.Integrations.Tests.Helpers;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Json;

using static CryptoBank.WebApi.Features.Accounts.Errors.AccountValidationErrors;

namespace CryptoBank.WebApi.Integrations.Tests.Features.Accounts;

public class CreateAccountsTests: IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;

    private ApplicationDbContext? _applicationDbContext;
    private AsyncServiceScope _scope;
    private CancellationToken _cancellationToken;
    private AccountOptions? _accountOptions;

    private const string DatabaseConnectionString =
       "Host=localhost;Database=CryptoBankDataBaseDraft;Username=postgres;Password=Masud1992;Maximum Pool Size=10;Connection Idle Lifetime=60;";

    public CreateAccountsTests()
    {
        _factory = WebApplicationFactoryBuilderHelper.ConfigureWebApplicationFactory(DatabaseConnectionString);
    }

    [Fact]
    public async Task Should_create_account()
    {
        //Arrange
        var client = _factory.CreateClient();
        var user = CreateUserHelper.CreateUser("test@test.com", _scope);
        await _applicationDbContext!.Users.AddAsync(user, _cancellationToken);
        await _applicationDbContext.SaveChangesAsync(_cancellationToken);

        await GenerateTokenHelper.GetAccessToken(client, _scope, user, _cancellationToken);

        //Act
        (await client.PostAsJsonAsync("/accounts", new
        {
            Number = "1000-1100-1200-1300",
            Currency = "BTC",
        },cancellationToken: _cancellationToken)).EnsureSuccessStatusCode();

        //Assert
        var account = await _applicationDbContext.Accounts.SingleOrDefaultAsync(a => a.Number == "1000-1100-1200-1300", _cancellationToken);

        account.Should().NotBeNull();
        account!.UserId.Should().Be(user.Id);
    }

    [Fact]
    public async Task Should_not_authorize_user()
    {
        //Arrange
        var client = _factory.CreateClient();
        var user = CreateUserHelper.CreateUser("test@test.com", _scope);
        await _applicationDbContext!.Users.AddAsync(user, _cancellationToken);
        await _applicationDbContext.SaveChangesAsync(_cancellationToken);

        client.DefaultRequestHeaders.Add("Authorization", $"Bearer" +
          $"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c");

        //Act
        var response = await client.PostAsJsonAsync("/accounts", new
        {
            Number = "1000-1100-1200-1300",
            Currency = "BTC",
        }, cancellationToken: _cancellationToken);

        //Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Should_return_LogicConflictException()
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

        //Act
        await GenerateTokenHelper.GetAccessToken(client, _scope, user, _cancellationToken);

        var response = await client.PostAsJsonAsync("/accounts", new
        {
            Number = "0000-0000-0000-0000",
            Currency = "BTC",
        }, cancellationToken: _cancellationToken);

        //Assert
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken: _cancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        result!.Detail.Should().Be("Accounts limit exceeded");
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

public class CreateAccountsValidationTests : IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;

    private ApplicationDbContext? _applicationDbContext;
    private AsyncServiceScope _scope;
    private CancellationToken _cancellationToken;
    private CookieHelper? _cookieHelper;
    private CreateAccount.RequestValidator? _validator;

    private const string DatabaseConnectionString =
       "Host=localhost;Database=CryptoBankDataBaseDraft;Username=postgres;Password=Masud1992;Maximum Pool Size=10;Connection Idle Lifetime=60;";
    private const string defaultNumberOfAccount = "1111-1111-1111-1111";

    public CreateAccountsValidationTests()
    {
        _factory = WebApplicationFactoryBuilderHelper.ConfigureWebApplicationFactory(DatabaseConnectionString);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Should_require_currency(string currency)
    {
        var user = CreateUserHelper.CreateUser("test@test.com", _scope);
        await _applicationDbContext!.Users.AddAsync(user, _cancellationToken);
        await _applicationDbContext.SaveChangesAsync(_cancellationToken);

        var result = await _validator.TestValidateAsync(new CreateAccount.Request(defaultNumberOfAccount, currency, user.Id), cancellationToken: _cancellationToken);
        result.ShouldHaveValidationErrorFor(x => x.Currency)
            .WithErrorCode(CurrencyRequired);
    }

    [Fact]
    public async Task Should_exist_account()
    {
        //Arrange
        var user = CreateUserHelper.CreateUser("test@test.com", _scope);
        await _applicationDbContext!.Users.AddAsync(user, _cancellationToken);
        await _applicationDbContext.SaveChangesAsync(_cancellationToken);

        var account = new Account()
        {
            UserId = user.Id,
            Amount = 0,
            Number = defaultNumberOfAccount,
            Currency = "BTC",
            CreatedAt = DateTime.UtcNow.ToUniversalTime(),
        };
        await _applicationDbContext.Accounts.AddAsync(account, _cancellationToken);
        await _applicationDbContext.SaveChangesAsync(_cancellationToken);

        //Act
        var result = await _validator.TestValidateAsync(new CreateAccount.Request(defaultNumberOfAccount, account.Currency, user.Id), cancellationToken: _cancellationToken);

        //Assert
        result.ShouldHaveValidationErrorFor(x => x.Number)
            .WithErrorCode(AccountExists);
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
        _validator = new CreateAccount.RequestValidator(_applicationDbContext);

        return Task.CompletedTask;
    }
}