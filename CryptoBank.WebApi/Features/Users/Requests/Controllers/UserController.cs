using CryptoBank.WebApi.Authorization.Requirements;
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
    [HttpGet("get-info")]
    public async Task<GetUserInfo.Response> GetUserInfo(CancellationToken cancellationToken)
    {
        var user = HttpContext?.User;
        if (long.TryParse(user.Claims.SingleOrDefault(i => i.Type == ClaimTypes.NameIdentifier)?.Value, out var userId))
        {
            var request = new GetUserInfo.Request(userId);
            var response = await _mediator.Send(request, cancellationToken);
            return response;
        }
        throw new Exception();
    }

    [Authorize(Policy = PolicyNames.AdministratorRole)]
    [HttpPost("update-role")]
    public async Task<UpdateUserRole.Response> UpdateUserRole(UpdateUserRole.Request request, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(request, cancellationToken);
        return response;
    }
}
