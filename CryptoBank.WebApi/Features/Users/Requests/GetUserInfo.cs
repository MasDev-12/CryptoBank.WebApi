using CryptoBank.WebApi.Database;
using CryptoBank.WebApi.Features.Users.Models;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

using static CryptoBank.WebApi.Features.Users.Errors.UserValidationErrors;

namespace CryptoBank.WebApi.Features.Users.Requests;

public class GetUserInfo
{
    public record Request(long UserId) : IRequest<Response>;

    public record Response(UserModel UserModel);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator(ApplicationDbContext applicationDbContext)
        {
            RuleFor(x => x.UserId)
               .Cascade(CascadeMode.Stop)
               .MustAsync(async (x, token) =>
               {
                   var userExists = await applicationDbContext.Users.AnyAsync(user => user.Id == x, token);

                   return userExists;
               }).WithErrorCode(NotExist);
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
            var user = await _applicationDbContext.Users.Include(u => u.Roles).SingleAsync(u => u.Id == request.UserId, cancellationToken);
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
