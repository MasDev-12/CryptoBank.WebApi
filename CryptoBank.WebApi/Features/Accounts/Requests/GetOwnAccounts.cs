using CryptoBank.WebApi.Database;
using CryptoBank.WebApi.Features.Accounts.Models;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

using static CryptoBank.WebApi.Features.Users.Errors.UserValidationErrors;

namespace CryptoBank.WebApi.Features.Accounts.Requests;

public static class GetOwnAccounts
{
    public record Request(long UserId) : IRequest<Response>;

    public record Response(List<AccountModel> AccountModels);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator(ApplicationDbContext applicationDbContext)
        {
            RuleFor(x => x.UserId)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithErrorCode(UserIdRequired);

            RuleFor(x => x.UserId)
                .Cascade(CascadeMode.Stop)
                .MustAsync(async (x, token) =>
                {
                    var ExistUser = await applicationDbContext.Users.AnyAsync(user => user.Id == x);
                    return ExistUser;
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
            var accounts = await _applicationDbContext.Accounts
                .Select(account => new AccountModel()
                {
                    Id = account.Id,
                    Number = account.Number,
                    UserId = account.UserId,
                    Amount = account.Amount,
                    Currency = account.Currency,
                    CreatedAt = account.CreatedAt
                }).ToListAsync();

            return new Response(accounts);
        }
    }
}
