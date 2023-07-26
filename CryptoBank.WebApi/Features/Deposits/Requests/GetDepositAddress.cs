using CryptoBank.WebApi.Database;
using CryptoBank.WebApi.Errors.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CryptoBank.WebApi.Features.Deposits.Domain;
using NBitcoin;

using static CryptoBank.WebApi.Features.Deposits.Errors.DepositValidationErrors;
using static CryptoBank.WebApi.Features.Deposits.Errors.DepositsLogicConflictErrors;

namespace CryptoBank.WebApi.Features.Deposits.Requests;

public static class GetDepositAddress
{
    public record Request(long UserId, string CurrencyCode):IRequest<Response>;

    public record Response(string DepositAddress);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator(ApplicationDbContext applicationDbContext)
        {
            RuleFor(x => x.CurrencyCode)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithErrorCode(CurrencyCodeRequired)
                .MinimumLength(3)
                .WithErrorCode(CurrencyCodeLenght);
        }
    }

    public class RequestHandler : IRequestHandler<Request, Response>
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public RequestHandler(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var tpub = await _applicationDbContext.Tpubs.SingleOrDefaultAsync(x => x.CurrencyCode == request.CurrencyCode, cancellationToken);

            if(tpub is null)
            {
                throw new LogicConflictException("Tbup not exist", tbupNotExists);
            }

            var depositAddress = await _applicationDbContext.DepositAddresses
                .SingleOrDefaultAsync(deposit => deposit.UserId == request.UserId && deposit.TpubId == tpub.Id, cancellationToken);

            if (depositAddress != null)
            {
                return new Response(depositAddress.CryptoAddress);
            }

            var lastDerivationIndex = await GetLastDerivationIndex(cancellationToken, tpub);
            var bitcoinAddress = GetBitcoinAddress(lastDerivationIndex, tpub);

            await _applicationDbContext.DepositAddresses.AddAsync(new DepositAddress()
            {
                CurrencyCode = request.CurrencyCode,
                UserId = request.UserId,
                TpubId = tpub.Id,
                DerivationIndex = lastDerivationIndex,
                CryptoAddress = bitcoinAddress.ToString(),
            }, cancellationToken);
            await _applicationDbContext.SaveChangesAsync(cancellationToken);

            return new Response(bitcoinAddress.ToString());
        }

        private BitcoinAddress GetBitcoinAddress(int lastDerivationIndex, Tpub tpub)
        {
            var pubkey = ExtPubKey.Parse(tpub.Value, Network.TestNet);
            var bitcoinAddress = pubkey.Derive(0).Derive(Convert.ToUInt32(lastDerivationIndex)).PubKey.GetAddress(ScriptPubKeyType.Segwit, Network.TestNet);

            return bitcoinAddress;
        }

        private async Task<int> GetLastDerivationIndex(CancellationToken cancellationToken, Tpub tpub)
        {
            var lastDerivationIndex = (await _applicationDbContext.DepositAddresses.CountAsync(x => x.TpubId == tpub.Id, cancellationToken: cancellationToken)) + 1;
            return lastDerivationIndex;
        }
    }
}
