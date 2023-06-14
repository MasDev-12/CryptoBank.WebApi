using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CryptoBank.WebApi.Features.Users.Requests.Controllers;

[ApiController]
[Route("/users")]
public class UserController
{
    private readonly IMediator _mediator;

    public UserController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<RegisterUser.Response> Register(RegisterUser.Request request, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(request, cancellationToken);
        return response;
    }
}
