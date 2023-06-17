using CryptoBank.WebApi.Features.Users.Domain;
using CryptoBank.WebApi.Features.Users.Options;
using Konscious.Security.Cryptography;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace CryptoBank.WebApi.Features.Users.Services;

public class PasswordHashingService
{
    private readonly PasswordHashingOptions _options;

    public PasswordHashingService(IOptions<PasswordHashingOptions> options)
    {
        _options = options.Value;
    }

    private byte[] GenerateSalt()
    {
        byte[] salt = new byte[_options.HashLengthInBytes];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(salt);
        return salt;
    }

    public string GetPasswordHash(string password)
    {
        byte[] passwordSalt = GenerateSalt();
        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            DegreeOfParallelism = _options.Parallelism,
            MemorySize = _options.MemorySize,
            Iterations = _options.Iterations,
            Salt = passwordSalt
        };
        byte[] passwordHash = argon2.GetBytes(_options.HashLengthInBytes);
        var passwordHashAndSalt = $"{Convert.ToBase64String(passwordHash)}:{Convert.ToBase64String(passwordSalt)}";
        return passwordHashAndSalt;
    }

    public string GetPasswordHash(string password, byte[] passwordSalt, User user)
    {
        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            DegreeOfParallelism = user.Parallelism,
            MemorySize = user.MemorySize,
            Iterations = user.Iterations,
            Salt = passwordSalt
        };
        byte[] passwordHash = argon2.GetBytes(_options.HashLengthInBytes);
        return Convert.ToBase64String(passwordHash);
    }
}
