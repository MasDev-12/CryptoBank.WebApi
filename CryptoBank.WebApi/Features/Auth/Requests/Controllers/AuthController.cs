using CryptoBank.WebApi.Features.Auth.Options;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CryptoBank.WebApi.Features.Auth.Requests.Controllers;

[ApiController]
[Route("/auth")]
public class AuthController: Controller
{
    private readonly IMediator _mediator;
    private readonly RefreshTokenOptions _options;
    private const string RefreshTokenPath = "/auth/get-new-tokens";
    private const string RefreshTokenForCookies = "refreshToken";

    public AuthController(IMediator mediator, IOptions<RefreshTokenOptions> options)
    {
        _mediator = mediator;
        _options = options.Value;
    }

    [HttpPost]
    public async Task<LoginUser.Response> LoginUser(LoginUser.Request request, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(request, cancellationToken);
        AddToCookieRefreshToken(response.AccessToken);
        return response;
    }


    [HttpGet("get-new-tokens")]
    public async Task<GetNewTokens.Response> GetNewTokens(CancellationToken cancellationToken)
    {
        var refreshToken = Request.Cookies[RefreshTokenForCookies];

        var request = new GetNewTokens.Request(refreshToken);
        var response = await _mediator.Send(request, cancellationToken);
        AddToCookieRefreshToken(response.AccessToken);

        return response;
    }

    private void AddToCookieRefreshToken(string refreshToken) 
    {
        HttpContext.Response.Cookies.Append(RefreshTokenForCookies, refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTime.UtcNow.Add(_options.ValidityPeriod),
            Path = RefreshTokenPath
        });
    }
}
