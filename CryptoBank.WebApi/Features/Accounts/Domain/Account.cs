using CryptoBank.WebApi.Features.Users.Domain;

namespace CryptoBank.WebApi.Features.Accounts.Domain;

public class Account
{
    public long Id { get; set; }
    public string Number { get; set; }
    public string Currency { get; set; }
    public decimal Amount { get; set; } = 0;
    public DateTime CreatedAt { get; set; }
    public long UserId { get; set; }
    public User User { get; set; }
}
