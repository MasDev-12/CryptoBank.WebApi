using CryptoBank.WebApi.Authorization.Requirements;
using CryptoBank.WebApi.Helpers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    [HttpGet("get-info")]
    public async Task<GetUserInfo.Response> GetUserInfo(CancellationToken cancellationToken)
    {
        var user = HttpContext?.User;
        var userId = GetUserIdFromClaims.GetUserId(user);
        var response = await _mediator.Send(new GetUserInfo.Request(userId), cancellationToken);
        return response;
    }

    [Authorize(Policy = PolicyNames.AdministratorRole)]
    [HttpPut("update-role")]
    public async Task<UpdateUserRole.Response> UpdateUserRole([FromBody] UpdateUserRole.Request request, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(request, cancellationToken);
        return response;
    }
}
