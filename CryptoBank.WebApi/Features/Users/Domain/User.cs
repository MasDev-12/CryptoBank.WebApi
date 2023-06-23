using CryptoBank.WebApi.Features.Accounts.Domain;

namespace CryptoBank.WebApi.Features.Users.Domain;

public class User
{
    public User()
    {
        Roles = new HashSet<Role>();
        Accounts = new HashSet<Account>();
    }
    public long Id { get; set; }
    public string Email { get; set; }
    public string PasswordHashAndSalt { get; set; }
    public int MemorySize { get; set; }
    public int Iterations { get; set; }
    public int Parallelism { get; set; }
    public DateTime BirthDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<Role> Roles { get; set; }
    public ICollection<Account> Accounts { get; set; }
}
