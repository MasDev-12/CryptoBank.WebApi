using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CryptoBank.WebApi.Features.Users.Requests.Controllers;

[ApiController]
[Route("/users")]
public class UserController: Controller
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

    [Authorize]
    [HttpGet("get-user-info")]
    public async Task<GetUserInfo.Response> GetUserInfo(CancellationToken cancellationToken)
    {
        var user = HttpContext?.User;
        long userId = Convert.ToInt64(user.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value);

        var request = new GetUserInfo.Request(userId);
        var response = await _mediator.Send(request, cancellationToken);
        return response;
    }
}
