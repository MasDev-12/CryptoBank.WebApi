using CryptoBank.WebApi.Authorization.Requirements;
using CryptoBank.WebApi.Features.Users.Requests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CryptoBank.WebApi.Features.Accounts.Requests.Contollers;

[ApiController]
[Route("/accounts")]
public class AccountController : Controller
{
    private readonly IMediator _mediator;

    public AccountController(IMediator mediator) => _mediator = mediator;

    [Authorize]
    [HttpPost("create")]
    public async Task<CreateAccount.Response> CreateAccount(CreateAccount.Request request, CancellationToken cancellationToken)
    {
        var user = HttpContext?.User;
        if (long.TryParse(user.Claims.SingleOrDefault(i => i.Type == ClaimTypes.NameIdentifier)?.Value, out var userId))
        {
            var response = await _mediator.Send(new CreateAccount.Request(request.Number, request.Currency, userId), cancellationToken);
            return response;
        }
        throw new Exception();
    }

    [Authorize]
    [HttpGet("get-info")]
    public async Task<GetOwnAccounts.Response> GetOwnAccounts(CancellationToken cancellationToken)
    {
        var user = HttpContext?.User;
        if (long.TryParse(user.Claims.SingleOrDefault(i => i.Type == ClaimTypes.NameIdentifier)?.Value, out var userId))
        {
            var response = await _mediator.Send(new GetOwnAccounts.Request(userId), cancellationToken);
            return response;
        }
        throw new Exception();
    }

    [Authorize(Policy = PolicyNames.AnalystRole)]
    [HttpGet("get-info-by-period")]
    public async Task<GetAccountsByPeriod.Response> GetAccountsByPeriod([FromBody] GetAccountsByPeriod.Request request, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(request, cancellationToken);
        return response;
    }
}

