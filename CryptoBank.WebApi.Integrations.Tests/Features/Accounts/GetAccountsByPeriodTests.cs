using CryptoBank.WebApi.Database;
using CryptoBank.WebApi.Features.Accounts.Domain;
using CryptoBank.WebApi.Features.Accounts.Options;
using CryptoBank.WebApi.Features.Accounts.Requests;
using CryptoBank.WebApi.Integrations.Tests.Helpers;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Json;

using static CryptoBank.WebApi.Features.Accounts.Errors.AccountValidationErrors;

namespace CryptoBank.WebApi.Integrations.Tests.Features.Accounts;

public class GetAccountsByPeriodTests : IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private ApplicationDbContext? _applicationDbContext;
    private AsyncServiceScope _scope;
    private CancellationToken _cancellationToken;
    private AccountOptions? _accountOptions;

    private string DatabaseConnectionString =
       "Host=localhost;Database=CryptoBankDataBaseDraft;Username=postgres;Password=Masud1992;Maximum Pool Size=10;Connection Idle Lifetime=60;";

    public GetAccountsByPeriodTests()
    {
        _factory = WebApplicationFactoryBuilderHelper.ConfigureWebApplicationFactory(DatabaseConnectionString);
    }

    [Fact]
    public async Task Should_get_accounts_by_period()
    {
        //Arrange
        var client = _factory.CreateClient();
        var user = CreateUserHelper.CreateUser("analyst@test.com", _scope);
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
                CreatedAt = DateTime.UtcNow.AddDays(i),
            });
        }

        _applicationDbContext.Accounts.AddRange(accounts);
        await _applicationDbContext.SaveChangesAsync(_cancellationToken);

        await GenerateTokenHelper.GetAccessToken(client, _scope, user, _cancellationToken);

        var start = new DateOnly(2023, 7, 01);
        var end = new DateOnly(2023, 7, 31);

        var queryParams = new Dictionary<string, string>
        {
            { "start", start.ToString("yyyy-MM-dd") },
            { "end", end.ToString("yyyy-MM-dd") }
        };

        var queryString = QueryHelpers.AddQueryString("/accounts/get-info-by-period", queryParams!);

        //Act
        var response = await client.GetFromJsonAsync<GetAccountsByPeriod.Response>(queryString, cancellationToken: _cancellationToken);

        response!.Result.Should().NotBeNullOrEmpty();
        var filterDate = response.Result.Keys.Where(key => key >= start && key <= end).ToArray();
        filterDate.Length.Should().Be(_accountOptions.MaxAccountsPerUser);
    }

    [Fact]
    public async Task Should_get_response_forbidden_when_role_wrong()
    {
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
                CreatedAt = DateTime.UtcNow.AddDays(i),
            });
        }

        _applicationDbContext.Accounts.AddRange(accounts);
        await _applicationDbContext.SaveChangesAsync(_cancellationToken);

        await GenerateTokenHelper.GetAccessToken(client, _scope, user, _cancellationToken);

        var start = new DateOnly(2023, 7, 01);
        var end = new DateOnly(2023, 7, 31);

        var queryParams = new Dictionary<string, string>
        {
            { "start", start.ToString("yyyy-MM-dd") },
            { "end", end.ToString("yyyy-MM-dd") }
        };

        var queryString = QueryHelpers.AddQueryString("/accounts/get-info-by-period", queryParams!);

        //Act
        var response = await client.GetAsync(queryString, cancellationToken: _cancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Should_not_authorize_user()
    {
        //Arrange
        var client = _factory.CreateClient();

        client.DefaultRequestHeaders.Add("Authorization",
          "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c");

        var start = new DateOnly(2023, 7, 01);
        var end = new DateOnly(2023, 7, 31);

        var queryParams = new Dictionary<string, string>
        {
            { "start", start.ToString("yyyy-MM-dd") },
            { "end", end.ToString("yyyy-MM-dd") }
        };

        var queryString = QueryHelpers.AddQueryString("/accounts/get-info-by-period", queryParams!);

        //Act
        var response = await client.GetAsync(queryString, cancellationToken: _cancellationToken);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
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

public class GetAccountsByPeriodValidatorTests : IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private ApplicationDbContext? _applicationDbContext;
    private AsyncServiceScope _scope;
    private CancellationToken _cancellationToken;
    private GetAccountsByPeriod.RequestValidator? _validator;
    private AccountOptions? _accountOptions;

    private string DatabaseConnectionString =
       "Host=localhost;Database=CryptoBankDataBaseDraft;Username=postgres;Password=Masud1992;Maximum Pool Size=10;Connection Idle Lifetime=60;";

    public GetAccountsByPeriodValidatorTests()
    {
        _factory = WebApplicationFactoryBuilderHelper.ConfigureWebApplicationFactory(DatabaseConnectionString);
    }

    [Fact]
    public async Task Should_validate_correct_request()
    {
        //Arrange
        var client = _factory.CreateClient();
        var user = CreateUserHelper.CreateUser("analyst@test.com", _scope);
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
                CreatedAt = DateTime.UtcNow.AddDays(i),
            });
        }

        _applicationDbContext.Accounts.AddRange(accounts);
        await _applicationDbContext.SaveChangesAsync(_cancellationToken);

        await GenerateTokenHelper.GetAccessToken(client, _scope, user, _cancellationToken);

        var start = new DateOnly(2023, 7, 01);
        var end = new DateOnly(2023, 7, 31);

        //Act
        var result = await _validator.TestValidateAsync(new GetAccountsByPeriod.Request(start, end), cancellationToken: _cancellationToken);

        //Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Should_validate_valid_range()
    {
        var client = _factory.CreateClient();
        var user = CreateUserHelper.CreateUser("analyst@test.com", _scope);
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
                CreatedAt = DateTime.UtcNow.AddDays(i),
            });
        }

        _applicationDbContext.Accounts.AddRange(accounts);
        await _applicationDbContext.SaveChangesAsync(_cancellationToken);

        await GenerateTokenHelper.GetAccessToken(client, _scope, user, _cancellationToken);

        var start = new DateOnly(2023, 7, 31);
        var end = new DateOnly(2023, 7, 01);

        //Act
        var result = await _validator.TestValidateAsync(new GetAccountsByPeriod.Request(start, end), cancellationToken: _cancellationToken);

        result.ShouldHaveValidationErrorFor(x => new { x.Start, x.End }).WithErrorCode(InvalidRange);
    }

    public async Task DisposeAsync()
    {
        await FactoryInitHelper.ClearDataAndDisposeAsync(_factory, _cancellationToken);

        await _factory.DisposeAsync();
    }

    public Task InitializeAsync()
    {
        FactoryInitHelper.Init(_factory, out _scope, out _cancellationToken);
        _applicationDbContext = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        _accountOptions = _scope.ServiceProvider.GetRequiredService<IOptions<AccountOptions>>().Value;
        _validator = new GetAccountsByPeriod.RequestValidator();

        return Task.CompletedTask;
    }
}
