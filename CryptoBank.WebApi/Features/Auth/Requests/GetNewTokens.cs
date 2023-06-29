using CryptoBank.WebApi.Database;
using CryptoBank.WebApi.Features.Auth.Services;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace CryptoBank.WebApi.Features.Auth.Requests;

public static class GetNewTokens
{
    public record Request(string? RefreshToken) : IRequest<Response>;
    public record Response(string AccessToken, [property: JsonIgnore] string RefreshToken);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator(ApplicationDbContext applicationDbContext)
        {
            RuleFor(x => x.RefreshToken)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithErrorCode("Token required");

            RuleFor(x => x.RefreshToken)
                .Cascade(CascadeMode.Stop)
                .MustAsync(async (x, cancellationToken) =>
                {
                    var refreshTokenExist = await applicationDbContext.RefreshTokens.AnyAsync(t => t.Token == x, cancellationToken);

                    return refreshTokenExist;
                }).WithErrorCode("Token not exist");
        }
    }

    public class RequestHandler : IRequestHandler<Request, Response>
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly TokenGenerateService _tokenGenerateService;

        public RequestHandler(ApplicationDbContext applicationDbContext, TokenGenerateService tokenGenerateService)
        {
            _applicationDbContext = applicationDbContext;
            _tokenGenerateService = tokenGenerateService;
        }

        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var refreshToken = await _applicationDbContext.RefreshTokens
                .Include(t => t.User)
                .SingleOrDefaultAsync(t => t.Token == request.RefreshToken, cancellationToken);

            if (refreshToken.Revoke || refreshToken.ExpiryDate <= DateTime.Now.ToUniversalTime())
            {
                var actualRefreshToken = await _applicationDbContext.RefreshTokens
                    .Where(t => t.userId == refreshToken.userId && !t.Revoke)
                    .FirstOrDefaultAsync(cancellationToken);

                if (actualRefreshToken != null)
                {
                    actualRefreshToken.Revoke = true;
                    await _applicationDbContext.SaveChangesAsync(cancellationToken);
                }
                throw new Exception();
            }
            var (accessToken, generatedRefreshToken) =  await _tokenGenerateService.GenerateTokens(refreshToken.User, cancellationToken);
            return new Response(accessToken, generatedRefreshToken);
        }
    }
}
