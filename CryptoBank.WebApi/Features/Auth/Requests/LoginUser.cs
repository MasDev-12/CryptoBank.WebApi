using CryptoBank.WebApi.Database;
using CryptoBank.WebApi.Features.Auth.Services;
using CryptoBank.WebApi.Features.Users.Options;
using CryptoBank.WebApi.Features.Users.Services;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Json.Serialization;
using static CryptoBank.WebApi.Features.Users.Requests.RegisterUser;

namespace CryptoBank.WebApi.Features.Auth.Requests;

public class LoginUser
{
    public record Request(string Email, string Password) : IRequest<Response>;

    public record Response(string AccessToken, [property: JsonIgnore] string RefreshToken);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator(ApplicationDbContext applicationDbContext)
        {
            RuleFor(x => x.Email)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithErrorCode("Email required")
                .EmailAddress()
                .WithErrorCode("Invalid email format");

            RuleFor(x => x.Password)
                 .Cascade(CascadeMode.Stop)
                 .NotEmpty()
                 .WithErrorCode("Password required")
                 .MinimumLength(3)
                 .WithErrorCode("Invalid password format");

            RuleFor(x => x.Email)
                 .Cascade(CascadeMode.Stop)
                 .MustAsync(async (x, CancellationToken) =>
                 {
                    var userExist = await applicationDbContext.Users.AnyAsync(user => user.Email == x, CancellationToken);

                    return userExist;
                 }).WithErrorCode("User not exists");
        }
    }

    public class RequestHandler : IRequestHandler<Request, Response>
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly TokenGenerateService _tokenGenerateService;
        private readonly PasswordHashingService _passwordHeshingService;
        private readonly PasswordHashingOptions _passwordHasherOptions;

        public RequestHandler(ApplicationDbContext applicationDbContext
            , TokenGenerateService tokenGenerateService
            , PasswordHashingService passwordHeshingService
            , IOptions<PasswordHashingOptions> passwordHasherOptions
            )
        {
            _applicationDbContext = applicationDbContext;
            _tokenGenerateService = tokenGenerateService;
            _passwordHeshingService = passwordHeshingService;
            _passwordHasherOptions = passwordHasherOptions.Value;
        }

        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var user = await _applicationDbContext.Users
                .Include(u => u.Roles)
                .Include(u => u.RefreshTokens)
                .SingleOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

            if (user == null)
            {
                throw new Exception("Invalid credentials");
            }

            string[] parts = user.PasswordHashAndSalt.Split(':');
            var passwordHash = _passwordHeshingService.GetPasswordHash(request.Password, Convert.FromBase64String(parts[1]), user);
            if (passwordHash != parts[0])
            {
                throw new Exception("Invalid credentials");
            }

            if (!(user.MemorySize == _passwordHasherOptions.MemorySize
               && user.Parallelism ==_passwordHasherOptions.Parallelism
               && user.Iterations == _passwordHasherOptions.Iterations))
            {
                var updatePasswordHashAndSalt = _passwordHeshingService.GetPasswordHash(request.Password);
                user.PasswordHashAndSalt = updatePasswordHashAndSalt;
                user.UpdatedAt = DateTime.Now.ToUniversalTime();
                user.Parallelism = _passwordHasherOptions.Parallelism;
                user.Iterations = _passwordHasherOptions.Iterations;
                user.MemorySize = _passwordHasherOptions.MemorySize;
                _applicationDbContext.Users.Update(user);
                await _applicationDbContext.SaveChangesAsync(cancellationToken); ;
            }

            var (AccessToken, RefreshToken) = await _tokenGenerateService.GenerateTokens(user, cancellationToken);
            return new Response(AccessToken, RefreshToken);
        }
    }
}
