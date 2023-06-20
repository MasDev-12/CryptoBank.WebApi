using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CryptoBank.WebApi.Features.Auth.Requests.Controllers;

[ApiController]
[Route("/auth")]
public class AuthController
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("login")]
    public async Task<LoginUser.Response> LoginUser(LoginUser.Request request, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(request, cancellationToken);
        return response;
    }
}
