using CryptoBank.WebApi.Database;
using CryptoBank.WebApi.Features.Users.Domain;
using CryptoBank.WebApi.Features.Users.Options;
using CryptoBank.WebApi.Features.Users.Requests;
using CryptoBank.WebApi.Integrations.Tests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Net;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;

using static CryptoBank.WebApi.Features.Users.Errors.UserValidationErrors;

namespace CryptoBank.WebApi.Integrations.Tests.Features.Users;

public class UpdateUserRoleTests : IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private UsersOptions _usersOptions;

    private ApplicationDbContext _applicationDbContext;
    private AsyncServiceScope _scope;
    private CancellationToken _cancellationToken;

    private string databaseConnectionString =
      "Host=localhost;Database=CryptoBankDataBaseDraft.Tests;Username=postgres;Password=Masud1992;Maximum Pool Size=10;Connection Idle Lifetime=60;";

    public UpdateUserRoleTests()
    {
        _factory = WebApplicationFactoryBuilderHelper.ConfigureWebApplicationFactory(databaseConnectionString);
    }

    [Fact]
    public async Task Should_update_role()
    {
        //Arrange
        var client = _factory.CreateClient();

        var userAdmin = CreateUserHelper.CreateUser(_usersOptions.AdministratorEmail.ToLower(), _scope);
        var user = CreateUserHelper.CreateUser("test@test.com", _scope);

        await _applicationDbContext.Users.AddAsync(user);
        await _applicationDbContext.Users.AddAsync(userAdmin);
        await _applicationDbContext.SaveChangesAsync();

        var updateRole = UserRole.AnalystRole;

        await GenerateTokenHelper.GetAccessToken(client, _scope, userAdmin, _cancellationToken);

        var request = new UpdateUserRole.Request(user.Email, updateRole);

        //Act
        (await client.PutAsJsonAsync("/users/update-role", request, cancellationToken: _cancellationToken))
            .EnsureSuccessStatusCode();

        var roles = await _applicationDbContext.Roles.Where(u => u.UserId == user.Id).ToArrayAsync();

        //Assert
        roles.Should().NotBeEmpty();
        roles.Length.Should().Be(2);

        roles.Select(x => x.Name).Should().Contain(updateRole);
    }

    [Fact]
    public async Task Should_be_unauthorized()
    {
        //Arrange
        var client = _factory.CreateClient();

        var user = CreateUserHelper.CreateUser("test@test.com", _scope);
        await _applicationDbContext.Users.AddAsync(user);
        await _applicationDbContext.SaveChangesAsync();
        
        var updateRole = UserRole.AnalystRole;

        client.DefaultRequestHeaders.Add("Authorization",
          "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c");

        var request = new UpdateUserRole.Request(user.Email, updateRole);

        //Act
        var respone = await client.PutAsJsonAsync("/users/update-role", request, cancellationToken: _cancellationToken);

        //Assert
        respone.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    public async Task DisposeAsync()
    {
        FactoryInitHelper.ClearDataAndDisposeAsync(ref _applicationDbContext);
        await _applicationDbContext.SaveChangesAsync();
        await _applicationDbContext.DisposeAsync();

        await _scope.DisposeAsync();
    }

    public Task InitializeAsync()
    {
        FactoryInitHelper.Init(_factory, ref _scope, ref _applicationDbContext, ref _cancellationToken);
        _usersOptions = _scope.ServiceProvider.GetRequiredService<IOptions<UsersOptions>>().Value;

        return Task.CompletedTask;
    }
}

public class UpdateUserRoleValidatorTests : IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private UpdateUserRole.RequestValidator _validator;
    private UsersOptions _usersOptions;

    private ApplicationDbContext _applicationDbContext;
    private AsyncServiceScope _scope;
    private CancellationToken _cancellationToken;

    private string databaseConnectionString =
    "Host=localhost;Database=CryptoBankDataBaseDraft.Tests;Username=postgres;Password=Masud1992;Maximum Pool Size=10;Connection Idle Lifetime=60;";

    public UpdateUserRoleValidatorTests()
    {
        _factory = WebApplicationFactoryBuilderHelper.ConfigureWebApplicationFactory(databaseConnectionString);
    }

    [Fact]
    public async Task Should_validate_correct_request()
    {
        //Arrange
        var client = _factory.CreateClient();

        var userAdmin = CreateUserHelper.CreateUser(_usersOptions.AdministratorEmail.ToLower(), _scope);
        var user = CreateUserHelper.CreateUser("test@test.com", _scope);

        var updateRole = UserRole.AnalystRole;

        await _applicationDbContext.Users.AddAsync(user);
        await _applicationDbContext.Users.AddAsync(userAdmin);
        await _applicationDbContext.SaveChangesAsync();

        await GenerateTokenHelper.GetAccessToken(client, _scope, userAdmin, _cancellationToken);

        //Act
        var result = await _validator.TestValidateAsync(new UpdateUserRole.Request(user.Email, updateRole), cancellationToken: _cancellationToken);

        //Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("test@test.com")]
    public async Task Should_validate_user_exists(string email)
    {
        //Arrange
        var client = _factory.CreateClient();
        var updateRole = UserRole.AnalystRole;

        var userAdmin = CreateUserHelper.CreateUser(_usersOptions.AdministratorEmail.ToLower(), _scope);

        await _applicationDbContext.Users.AddAsync(userAdmin);
        await _applicationDbContext.SaveChangesAsync();

        //Act
        var result = await _validator.TestValidateAsync(new UpdateUserRole.Request(email, updateRole), cancellationToken: _cancellationToken);

        //Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorCode(NotExists);
    }


    public async Task DisposeAsync()
    {
        FactoryInitHelper.ClearDataAndDisposeAsync(ref _applicationDbContext);
        await _applicationDbContext.SaveChangesAsync();
        await _applicationDbContext.DisposeAsync();

        await _scope.DisposeAsync();
    }

    public Task InitializeAsync()
    {
        FactoryInitHelper.Init(_factory, ref _scope, ref _applicationDbContext, ref _cancellationToken);
        _usersOptions = _scope.ServiceProvider.GetRequiredService<IOptions<UsersOptions>>().Value;
        _validator = new UpdateUserRole.RequestValidator(_applicationDbContext);

        return Task.CompletedTask;
    }
}
