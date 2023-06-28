using CryptoBank.WebApi.Authorization.Requirements;
using CryptoBank.WebApi.Features.Accounts.Models;
using CryptoBank.WebApi.Helpers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CryptoBank.WebApi.Features.Accounts.Requests.Contollers;

[ApiController]
[Route("/accounts")]
public class AccountController : Controller
{
    private readonly IMediator _mediator;

    public AccountController(IMediator mediator) => _mediator = mediator;

    [Authorize]
    [HttpPost]
    public async Task<CreateAccount.Response> CreateAccount(CreateAccountModel createAccountModel, CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromClaims.GetUserId(HttpContext!.User);
        var response = await _mediator.Send(new CreateAccount.Request(createAccountModel.Number, createAccountModel.Currency, userId), cancellationToken);
        return response;
    }

    [Authorize]
    [HttpGet("own")]
    public async Task<GetOwnAccounts.Response> GetOwnAccounts(CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromClaims.GetUserId(HttpContext!.User);
        var response = await _mediator.Send(new GetOwnAccounts.Request(userId), cancellationToken);
        return response;
    }

    [Authorize(Policy = PolicyNames.AnalystRole)]
    [HttpGet("get-info-by-period")]
    public async Task<GetAccountsByPeriod.Response> GetAccountsByPeriod([FromQuery] GetAccountsByPeriod.Request request, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(request, cancellationToken);
        return response;
    }
}

