using CryptoBank.WebApi.Features.Accounts.Requests;
using CryptoBank.WebApi.Helpers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;

namespace CryptoBank.WebApi.Features.Deposits.Requests.Controllers;

[ApiController]
[Route("/deposits")]
public class DepositController: Controller
{
    private readonly IMediator _mediator;

    public DepositController(IMediator mediator) => _mediator = mediator;

    [Authorize]
    [HttpGet]
    public async Task<GetDepositAddress.Response> GetDepositAddress([FromQuery] string currecycode, CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromClaims.GetUserId(HttpContext!.User);
        var response = await _mediator.Send(new GetDepositAddress.Request(userId, currecycode), cancellationToken);
        return response;
    }
}
