using CryptoBank.WebApi.Database;
using CryptoBank.WebApi.Features.Auth.Requests;
using CryptoBank.WebApi.Integrations.Tests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using System.Net;
using FluentAssertions;
using FluentValidation.TestHelper;

using static CryptoBank.WebApi.Features.Users.Errors.UserValidationErrors;
using Microsoft.EntityFrameworkCore;

namespace CryptoBank.WebApi.Integrations.Tests.Features.Auth;

public class LoginUserTests : IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;

    private ApplicationDbContext _applicationDbContext;
    private CookieHelper _cookieHelper;
    private AsyncServiceScope _scope;
    private CancellationToken _cancellationToken;

    private string databaseConnectionString =
        "Host=localhost;Database=CryptoBankDataBaseDraft.Tests;Username=postgres;Password=Masud1992;Maximum Pool Size=10;Connection Idle Lifetime=60;";

    public LoginUserTests()
    {
        _factory = WebApplicationFactoryBuilderHelper.ConfigureWebApplicationFactory(databaseConnectionString);
    }

    [Fact]
    public async Task Should_authenticate_user()
    {
        //Arrange
        var client = _factory.CreateClient();

        var user = CreateUserHelper.CreateUser("test@test.com", _scope);

        await _applicationDbContext.Users.AddAsync(user);
        await _applicationDbContext.SaveChangesAsync();

        //Act
        var response = await client.PostAsJsonAsync("/auth", new
        {
            Email = user.Email!,
            Password = "123456"
        });

        var tokens = await response.Content.ReadFromJsonAsync<LoginUser.Response>(cancellationToken: _cancellationToken);
        var accessToken = tokens.AccessToken;
        var refreshToken = _cookieHelper.GetCookie(response);

        //Assert
        refreshToken.Should().NotBeNull();
        var refreshTokenFromDb = await _applicationDbContext.RefreshTokens.FirstOrDefaultAsync(x => x.Token == refreshToken);
        refreshTokenFromDb.Token.Should().Be(refreshToken);

        accessToken.Should().NotBeNull();
        accessToken.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Should_require_correct_email()
    {
        //Arrange
        var client = _factory.CreateClient();

        var user = CreateUserHelper.CreateUser("test@test.com", _scope);

        await _applicationDbContext.Users.AddAsync(user);
        await _applicationDbContext.SaveChangesAsync();

        //Act
        var response = (await client.PostAsJsonAsync("/auth", new
        {
            Email = "Invalid@example.com"!,
            Password = "123456",
        }));

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Should_require_correct_password()
    {
        //Arrange
        var client = _factory.CreateClient();

        var user = CreateUserHelper.CreateUser("test@test.com", _scope);

        await _applicationDbContext.Users.AddAsync(user);
        await _applicationDbContext.SaveChangesAsync();

        //Act
        var response = (await client.PostAsJsonAsync("/auth", new
        {
            Email = user.Email!,
            Password = "InvalidPassword",
        }));

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
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
        _cookieHelper = new CookieHelper();

        return Task.CompletedTask;
    }
}

public class LoginUserValidatorTests : IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;

    private LoginUser.RequestValidator _validator;

    private ApplicationDbContext _applicationDbContext;
    private AsyncServiceScope _scope;
    private CancellationToken _cancellationToken;

    private string databaseConnectionString =
        "Host=localhost;Database=CryptoBankDataBaseDraft.Tests;Username=postgres;Password=Masud1992;Maximum Pool Size=10;Connection Idle Lifetime=60;";

    public LoginUserValidatorTests()
    {
        _factory = WebApplicationFactoryBuilderHelper.ConfigureWebApplicationFactory(databaseConnectionString);
    }

    [Fact]
    public async Task Should_validate_correct_request()
    {
        //Arrange
        var client = _factory.CreateClient();
        var user = CreateUserHelper.CreateUser("test@test.com", _scope);
        await _applicationDbContext.Users.AddAsync(user);
        await _applicationDbContext.SaveChangesAsync();

        //Act
        var result = await _validator.TestValidateAsync(new LoginUser.Request(user.Email, "123456"));

        //Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Should_validate_email_required(string email)
    {
        //Arrange
        var client = _factory.CreateClient();

        //Act
        var result = await _validator.TestValidateAsync(new LoginUser.Request(email, "123456"));

        //Assert
        result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorCode(EmailRequired);
    }

    [Fact]
    public async Task Should_validate_correct_email_format()
    {
        //Arrange
        var client = _factory.CreateClient();

        //Act
        var result = await _validator.TestValidateAsync(new LoginUser.Request("test", "123456"));

        //Assert
        result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorCode(EmailInvalidFormat);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Should_validate_password_required(string password)
    {
        //Arrange
        var client = _factory.CreateClient();

        //Act
        var result = await _validator.TestValidateAsync(new LoginUser.Request("test@test", password));

        //Assert
        result.ShouldHaveValidationErrorFor(x => x.Password).WithErrorCode(PasswordRequired);
    }

    [Theory]
    [InlineData("1")]
    [InlineData("12")]
    public async Task Should_validate_password_length(string password)
    {
        //Arrange
        var client = _factory.CreateClient();

        //Act
        var result = await _validator.TestValidateAsync(new LoginUser.Request("test@test", password));

        //Assert
        result.ShouldHaveValidationErrorFor(x => x.Password).WithErrorCode(PasswordLenght);
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
        _validator = new LoginUser.RequestValidator(_applicationDbContext);

        return Task.CompletedTask;
    }
}
