namespace CryptoBank.WebApi.Features.Accounts.Models;

public class AccountModel
{
    public long Id { get; set; }
    public string Number { get; set; }
    public string Currency { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; }
    public long UserId { get; set; }
}
