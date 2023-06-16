using CryptoBank.WebApi.Database;
using CryptoBank.WebApi.Features.Users.Domain;
using CryptoBank.WebApi.Features.Users.Models;
using CryptoBank.WebApi.Features.Users.Options;
using CryptoBank.WebApi.Features.Users.Services;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CryptoBank.WebApi.Features.Users.Requests;

public static class RegisterUser
{
    public record Request(string Email, string Password, DateTime BirthDate) : IRequest<Response>;
    public record Response(UserModel UserModel);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator(ApplicationDbContext applicationDbContext)
        {
            RuleFor(x => x.Email)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.Password)
                 .Cascade(CascadeMode.Stop)
                 .NotEmpty()
                 .MinimumLength(3);

            RuleFor(x => x.BirthDate)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .Must(date => IsValidBirthDateMoreThenEightTeen(date));

            RuleFor(x => x.Email)
                .Cascade(CascadeMode.Stop)
                .MustAsync(async (x, token) =>
                {
                    var userExists = await applicationDbContext.Users.AnyAsync(user => user.Email == x, token);

                    return !userExists;
                }).WithMessage("The user is already in the system");
        }

        private bool IsValidBirthDateMoreThenEightTeen(DateTime? birthDate)
        {
            var currentDate = DateTime.UtcNow;
            var minDate = currentDate.AddYears(-18);
            return birthDate <= minDate;
        }
    }

    public class RequestHandler : IRequestHandler<Request, Response>
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly PasswordHashingService _passwordHeshingService;
        private readonly UsersOptions _usersOptions;
        private readonly PasswordHashingOptions _passwordHashOptions;

        public RequestHandler(ApplicationDbContext applicationDbContext
            , PasswordHashingService passwordHeshingService
            , IOptions<UsersOptions> usersOptions
            , IOptions<PasswordHashingOptions> passwordHashOptions)
        {
            _applicationDbContext = applicationDbContext;
            _passwordHeshingService = passwordHeshingService;
            _usersOptions = usersOptions.Value;
            _passwordHashOptions = passwordHashOptions.Value;
        }

        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            UserRole userRole = UserRole.UserRole;
            var passwordHashAndSalt = _passwordHeshingService.GetPasswordHash(request.Password);
            if (request.Email.Equals(_usersOptions.AdministratorEmail.ToLower()))
            {
                userRole = UserRole.AdministratorRole;
            }
            var user = new User()
            {
                Email = request.Email.ToLower(),
                PasswordHashAndSalt = passwordHashAndSalt,
                Parallelism = _passwordHashOptions.Parallelism,
                Iterations = _passwordHashOptions.Iterations,
                MemorySize = _passwordHashOptions.MemorySize,
                BirthDate = request.BirthDate.ToUniversalTime(),
                CreatedAt = DateTime.Now.ToUniversalTime(),
                Roles = new List<Role>()
                    {
                        new Role()
                        {
                            Name = userRole,
                            CreatedAt = DateTime.Now.ToUniversalTime()
                        }
                    }
            };
            await _applicationDbContext.Users.AddAsync(user, cancellationToken);
            await _applicationDbContext.SaveChangesAsync(cancellationToken);
            return new Response(ToUserModel(user));
        }

        private static UserModel ToUserModel(User user)
        {
            return new UserModel()
            {
                Id = user.Id,
                Email = user.Email,
                DateOfBirth = user.BirthDate,
                CreatedAt = user.CreatedAt,
            };
        }
    }
}
