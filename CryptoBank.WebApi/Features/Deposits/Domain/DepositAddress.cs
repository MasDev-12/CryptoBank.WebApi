using CryptoBank.WebApi.Features.Users.Domain;

namespace CryptoBank.WebApi.Features.Deposits.Domain;

public class DepositAddress
{
    public long Id { get; set; }
    public string CurrencyCode { get; set; }
    public long UserId { get; set; }
    public User User { get; set; }
    public long TpubId { get; set; }
    public Tpub Tpub { get; set; }
    public int DerivationIndex { get; set; }
    public string CryptoAddress { get; set; }
}
