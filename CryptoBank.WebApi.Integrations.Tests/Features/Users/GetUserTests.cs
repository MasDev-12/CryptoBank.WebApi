using CryptoBank.WebApi.Database;
using CryptoBank.WebApi.Features.Users.Options;
using CryptoBank.WebApi.Features.Users.Requests;
using CryptoBank.WebApi.Features.Users.Services;
using CryptoBank.WebApi.Integrations.Tests.Helpers;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

using static CryptoBank.WebApi.Features.Users.Errors.UserValidationErrors;

namespace CryptoBank.WebApi.Integrations.Tests.Features.Users;

public class GetUserTests : IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;

    private ApplicationDbContext _applicationDbContext;
    private AsyncServiceScope _scope;
    private CancellationToken _cancellationToken;

    private string databaseConnectionString =
       "Host=localhost;Database=CryptoBankDataBaseDraft;Username=postgres;Password=Masud1992;Maximum Pool Size=10;Connection Idle Lifetime=60;";

    public GetUserTests()
    {
        _factory = WebApplicationFactoryBuilderHelper.ConfigureWebApplicationFactory(databaseConnectionString);
    }

    [Fact]
    public async Task Should_get_user()
    {
        //Arrange
        var client = _factory.CreateClient();
        var user = CreateUserHelper.CreateUser("test@test.com", _scope);

        await _applicationDbContext.Users.AddAsync(user);
        await _applicationDbContext.SaveChangesAsync();

        await GenerateTokenHelper.GetAccessToken(client, _scope, user, _cancellationToken);

        //Act
        var response = await client.GetFromJsonAsync<GetUserInfo.Response>("users/get-info", cancellationToken: _cancellationToken);

        //Assert
        response.Should().NotBeNull();
        response.UserModel.Email.Should().Be(user.Email);
        response.UserModel.DateOfBirth.Should().Be(user.BirthDate);
    }

    [Fact]
    public async Task Should_not_authorize_user()
    {
        //Arrange
        var client = _factory.CreateClient();
        var user = CreateUserHelper.CreateUser("test@test.com", _scope);

        await _applicationDbContext.Users.AddAsync(user);
        await _applicationDbContext.SaveChangesAsync();

        client.DefaultRequestHeaders.Add("Authorization", $"Bearer" +
            $"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c");

        //Act
        var response = await client.GetAsync("users/get-info", cancellationToken: _cancellationToken);

        //Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    public async Task DisposeAsync()
    {
        await FactoryInitHelper.ClearDataAndDisposeAsync(_factory, _cancellationToken);
        await _applicationDbContext.SaveChangesAsync();
        await _applicationDbContext.DisposeAsync();

        await _scope.DisposeAsync();
    }

    public Task InitializeAsync()
    {
        FactoryInitHelper.Init(_factory, out _scope, out _cancellationToken);
        _applicationDbContext = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return Task.CompletedTask;
    }
}

public class GetUserValidationTest : IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private PasswordHashingService _passwordHashingService;
    private PasswordHashingOptions _passwordHashingOptions;
    private GetUserInfo.RequestValidator _validator;

    private ApplicationDbContext _applicationDbContext;
    private AsyncServiceScope _scope;
    private CancellationToken _cancellationToken;

    private string databaseConnectionString =
      "Host=localhost;Database=CryptoBankDataBaseDraft;Username=postgres;Password=Masud1992;Maximum Pool Size=10;Connection Idle Lifetime=60;";

    public GetUserValidationTest()
    {
        _factory = WebApplicationFactoryBuilderHelper.ConfigureWebApplicationFactory(databaseConnectionString);
    }

    [Fact]
    public async Task Should_validate_correct_request()
    {
        //Arrange
        var client = _factory.CreateClient();
        var user = CreateUserHelper.CreateUser("test@test.com", _scope);
        await _applicationDbContext.Users.AddAsync(user, cancellationToken: _cancellationToken);
        await _applicationDbContext.SaveChangesAsync(cancellationToken: _cancellationToken);

        //Act
        var result = await _validator.TestValidateAsync(new GetUserInfo.Request(user.Id), cancellationToken: _cancellationToken);

        //Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [MemberData(nameof(GenerateRandomLong))]
    public async Task Should_validate_empty_user_request(long userId)
    {
        var result = await _validator.TestValidateAsync(
            new GetUserInfo.Request(userId), cancellationToken: _cancellationToken);
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
        await _applicationDbContext.SaveChangesAsync(_cancellationToken);
        await _applicationDbContext.DisposeAsync();

        await _scope.DisposeAsync();
    }

    public Task InitializeAsync()
    {
        FactoryInitHelper.Init(_factory, out _scope, out _cancellationToken);
        _applicationDbContext = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        _passwordHashingOptions = _scope.ServiceProvider.GetRequiredService<IOptions<PasswordHashingOptions>>().Value;
        _passwordHashingService = _scope.ServiceProvider.GetRequiredService<PasswordHashingService>();
        _validator = new GetUserInfo.RequestValidator(_applicationDbContext);

        return Task.CompletedTask;
    }
}
