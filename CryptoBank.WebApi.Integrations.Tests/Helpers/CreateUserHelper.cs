using CryptoBank.WebApi.Features.Users.Domain;
using CryptoBank.WebApi.Features.Users.Options;
using CryptoBank.WebApi.Features.Users.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CryptoBank.WebApi.Integrations.Tests.Helpers;

public static class CreateUserHelper
{
    public static User CreateUser(string email, AsyncServiceScope scope)
    {
        var password = "123456";
        var passwordHasher = scope.ServiceProvider.GetRequiredService<PasswordHashingService>();
        var passwordHasherOptins = scope.ServiceProvider.GetRequiredService<IOptions<PasswordHashingOptions>>().Value;
        var userOptions = scope.ServiceProvider.GetRequiredService<IOptions<UsersOptions>>().Value;
        var passwordHashAndSalt = passwordHasher.GetPasswordHash(password);
        var role = UserRole.UserRole;

        if (email.ToLower().Equals(userOptions.AdministratorEmail.ToLower()))
        {
            role = UserRole.AdministratorRole;
        };

        if (email.ToLower().Equals("analyst@test.com"))
        {
            role = UserRole.AnalystRole;
        }

        var user = new User()
        {
            Email = email.ToLower(),
            PasswordHashAndSalt = passwordHashAndSalt,
            Parallelism = passwordHasherOptins.Parallelism,
            Iterations = passwordHasherOptins.Iterations,
            MemorySize = passwordHasherOptins.MemorySize,
            BirthDate = new DateTime(2000, 01, 31).ToUniversalTime(),
            CreatedAt = DateTime.Now.ToUniversalTime(),
            Roles = new List<Role>()
            {
                new Role()
                {
                    Name = role,
                    CreatedAt = DateTime.Now.ToUniversalTime()
                }
            }
        };
        return user;
    }
}
