using CryptoBank.WebApi.Database;
using CryptoBank.WebApi.Features.Deposits.Domain;
using Microsoft.EntityFrameworkCore;
using NBitcoin;

namespace CryptoBank.WebApi.Features.Deposits.Jobs;

public class TbupGenerator : IHostedService
{
    private readonly ILogger<TbupGenerator> _logger;
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
    private readonly string _currencyCode = "BTC";

    public TbupGenerator(ILogger<TbupGenerator> logger, IDbContextFactory<ApplicationDbContext> dbContextFactory)
    {
        _logger=logger;
        _dbContextFactory=dbContextFactory;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var tbupExists = await dbContext.Tpubs.AnyAsync(x => x.CurrencyCode == _currencyCode, cancellationToken: cancellationToken);

        if (tbupExists) 
        {
            return;
        }

        var publicKey = GeneratePublicKey();

        var tpub = new Tpub()
        {
            CurrencyCode = _currencyCode,
            Value = publicKey
        };

        await dbContext.Tpubs.AddAsync(tpub, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private string GeneratePublicKey()
    {
        ExtKey masterKey = new ExtKey();
        _logger.LogInformation($"Master key: " + masterKey.ToString(Network.TestNet));

        ExtPubKey masterPubKey = masterKey.Neuter();
        var masterPubKeyAsString = masterPubKey.ToString(Network.TestNet);
        _logger.LogInformation($"PubKey: " + masterPubKeyAsString);
        return masterPubKeyAsString;
    }
}
