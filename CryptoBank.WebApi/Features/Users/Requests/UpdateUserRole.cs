using CryptoBank.WebApi.Database;
using CryptoBank.WebApi.Features.Users.Domain;
using CryptoBank.WebApi.Features.Users.Models;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CryptoBank.WebApi.Features.Users.Requests;

public class UpdateUserRole
{
    public record Request(string Email, string UpdateRole) : IRequest<Response>;

    public record Response(UserModel UserModel);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator(ApplicationDbContext applicationDbContext)
        {
            RuleFor(x => x.Email)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Email empty")
                .EmailAddress()
                .WithMessage("Email format not correct");

            RuleFor(x => x.Email)
              .Cascade(CascadeMode.Stop)
              .MustAsync(async (x, token) =>
              {
                  var userExists = await applicationDbContext.Users.AnyAsync(user => user.Email == x, token);

                  return userExists;
              }).WithMessage("User not exist");

            RuleFor(x => x.UpdateRole)
                .Cascade(CascadeMode.Stop)
                .MustAsync(async (role, cancellationToken) => await IsValidRole(role, cancellationToken))
                .WithMessage("Invalid role name, role not found");
        }

        private async Task<bool> IsValidRole(string role, CancellationToken cancellationToken)
        {
            foreach (var item in Enum.GetValues(typeof(UserRole)))
            {
                if (Enum.GetName(typeof(UserRole), item) == role)
                {
                    return true;
                }
            }
            return false;
        }
    }

    public class RequestHandler : IRequestHandler<Request, Response>
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public RequestHandler(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            UserRole updateuserRole = (UserRole)Enum.Parse(typeof(UserRole), request.UpdateRole);
            var user = await _applicationDbContext.Users
                .Include(u => u.Roles)
                .SingleOrDefaultAsync(u => u.Email == request.Email.ToLower(), cancellationToken);

            var role = user.Roles.SingleOrDefault(r => r.Name == updateuserRole);

            if (role != null)
            {
                throw new Exception();
            }

            var newRole = new Role
            {
                UserId = user.Id,
                Name = updateuserRole,
                CreatedAt = DateTime.Now.ToUniversalTime()
            };

            user.Roles.Add(newRole);

            await _applicationDbContext.SaveChangesAsync(cancellationToken);
            return new Response(new UserModel()
            {
                Id = user.Id,
                Email = user.Email,
                DateOfBirth = user.BirthDate,
                CreatedAt = user.CreatedAt,
                Roles = user.Roles.Select(role => new RoleModel
                {
                    RoleName = role.Name.ToString(),
                    CreatedAt = role.CreatedAt
                }).ToList()
            });
        }
    }
}

