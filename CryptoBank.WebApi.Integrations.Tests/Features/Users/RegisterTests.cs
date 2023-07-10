using CryptoBank.WebApi.Database;
using CryptoBank.WebApi.Features.Users.Requests;
using CryptoBank.WebApi.Features.Users.Services;
using CryptoBank.WebApi.Integrations.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using System.Net;
using Microsoft.EntityFrameworkCore;
using FluentValidation.TestHelper;

using static CryptoBank.WebApi.Features.Users.Errors.UserValidationErrors;

namespace CryptoBank.WebApi.Integrations.Tests.Features.Users;

public class RegisterTests : IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;

    private ApplicationDbContext _applicationDbContext;
    private AsyncServiceScope _scope;
    private CancellationToken _cancellationToken;

    private string databaseConnectionString =
        "Host=localhost;Database=CryptoBankDataBaseDraft.Tests;Username=postgres;Password=Masud1992;Maximum Pool Size=10;Connection Idle Lifetime=60;";

    public RegisterTests()
    {
        _factory = WebApplicationFactoryBuilderHelper.ConfigureWebApplicationFactory(databaseConnectionString);
    }

    [Fact]
    public async Task Should_register_user()
    {
        //Arrange
        var client = _factory.CreateClient();

        //Act
        (await client.PostAsJsonAsync("/users/register", new
        {
            Email = "test@test.com",
            Password = "123456",
            BirthDate = "2000-01-31",
        }, cancellationToken: _cancellationToken)).EnsureSuccessStatusCode();

        //Assert
        var user = await _applicationDbContext.Users.SingleOrDefaultAsync(x => x.Email == "test@test.com", cancellationToken: _cancellationToken);
        user.Should().NotBeNull();
        user!.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(20));
        user.BirthDate.Should().Be(new DateTime(2000, 01, 31).ToUniversalTime());

        var passwordHasher = _scope.ServiceProvider.GetRequiredService<PasswordHashingService>();
        string[] parts = user.PasswordHashAndSalt.Split(':');

        var passwordHashAndSalt = passwordHasher.GetPasswordHash("123456", Convert.FromBase64String(parts[1]), user);

        parts[0].Should().Be(passwordHashAndSalt);
    }

    [Fact]
    public async Task Should_validate_same_user()
    {
        //Arrange
        var client = _factory.CreateClient();

        //Act
        var user = CreateUserHelper.CreateUser("test@test.com", _scope);

        await _applicationDbContext.Users.AddAsync(user, cancellationToken: _cancellationToken);
        await _applicationDbContext.SaveChangesAsync(cancellationToken: _cancellationToken);

        var result = (await client.PostAsJsonAsync("/users/register", new
        {
            Email = user.Email!,
            Password = "123456",
            BirthDate = "2000-01-31",
        }, cancellationToken: _cancellationToken));

        //Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    public async Task DisposeAsync()
    {
        FactoryInitHelper.ClearDataAndDisposeAsync(ref _applicationDbContext);
        await _applicationDbContext.SaveChangesAsync(_cancellationToken);
        await _applicationDbContext.DisposeAsync();

        await _scope.DisposeAsync();
    }

    public Task InitializeAsync()
    {
        FactoryInitHelper.Init(_factory, ref _scope, ref _applicationDbContext, ref _cancellationToken);

        return Task.CompletedTask;
    }

    public class RegisterValidatorTests : IAsyncLifetime
    {
        private readonly WebApplicationFactory<Program> _factory;
        private RegisterUser.RequestValidator _validator;

        private ApplicationDbContext _applicationDbContext;
        private AsyncServiceScope _scope;
        private CancellationToken _cancellationToken;

        private string databaseConnectionString =
            "Host=localhost;Database=CryptoBankDataBaseDraft.Tests;Username=postgres;Password=Masud1992;Maximum Pool Size=10;Connection Idle Lifetime=60;";

        public RegisterValidatorTests()
        {
            _factory = WebApplicationFactoryBuilderHelper.ConfigureWebApplicationFactory(databaseConnectionString);
        }

        [Fact]
        public async Task Should_validate_correct_request()
        {
            var result = await _validator.TestValidateAsync(
                new RegisterUser.Request("test@test.com", "password", new DateTime(2000, 01, 31).ToUniversalTime()), cancellationToken: _cancellationToken);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task Should_required_email(string email)
        {
            var result = await _validator.TestValidateAsync(new RegisterUser.Request(email, "password", new DateTime(2000, 01, 31)), cancellationToken: _cancellationToken);
            result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorCode(EmailRequired);
        }

        [Theory]
        [InlineData("test")]
        [InlineData("@test.com")]
        [InlineData("test@")]
        public async Task Should_required_email_format(string email)
        {
            var result = await _validator.TestValidateAsync(new RegisterUser.Request(email, "password", new DateTime(2000, 01, 31)), cancellationToken: _cancellationToken);
            result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorCode(EmailInvalidFormat);
        }

        [Fact]
        public async Task Should_email_exist()
        {
            var user = CreateUserHelper.CreateUser("test@test.com", _scope);

            await _applicationDbContext.Users.AddAsync(user);
            await _applicationDbContext.SaveChangesAsync();

            var result = await _validator.TestValidateAsync(new RegisterUser.Request(user.Email, "password", new DateTime(2000, 01, 31)), cancellationToken: _cancellationToken);
            result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorCode(NotExists);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(" ")]
        [InlineData("")]
        public async Task Should_required_password(string password)
        {
            var result = await _validator.TestValidateAsync(new RegisterUser.Request("test@test.com", password, new DateTime(2000, 01, 31)), cancellationToken: _cancellationToken);
            result.ShouldHaveValidationErrorFor(x => x.Password).WithErrorCode(PasswordRequired);
        }

        [Theory]
        [InlineData("1")]
        [InlineData("12")]
        public async Task Should_minimun_password_lengt(string password)
        {
            var result = await _validator.TestValidateAsync(new RegisterUser.Request("test@test.com", password, new DateTime(2000, 1, 31)), cancellationToken: _cancellationToken);
            result.ShouldHaveValidationErrorFor(x => x.Password).WithErrorCode(PasswordLenght);
        }

        [Theory]
        [InlineData(null)]
        public async Task Should_birthdate_required(DateTime dateTime)
        {
            var result = await _validator.TestValidateAsync(new RegisterUser.Request("test@test.com", "123456", dateTime), cancellationToken: _cancellationToken);
            result.ShouldHaveValidationErrorFor(x => x.BirthDate).WithErrorCode(DateBirthRequired);
        }

        [Fact]
        public async Task Should_validate_TooYoung()
        {
            var result = await _validator.TestValidateAsync(new RegisterUser.Request("test@test.com", "123456", DateTime.UtcNow.AddYears(-10)), cancellationToken: _cancellationToken);
            result.ShouldHaveValidationErrorFor(x => x.BirthDate).WithErrorCode(AgeMoreTheEightTeen);
        }

        public async Task DisposeAsync()
        {
            FactoryInitHelper.ClearDataAndDisposeAsync(ref _applicationDbContext);
            await _applicationDbContext.SaveChangesAsync(_cancellationToken);
            await _applicationDbContext.DisposeAsync();

            await _scope.DisposeAsync();
        }

        public Task InitializeAsync()
        {
            FactoryInitHelper.Init(_factory, ref _scope, ref _applicationDbContext, ref _cancellationToken);
            _validator = new RegisterUser.RequestValidator(_applicationDbContext);

            return Task.CompletedTask;
        }
    }
}
