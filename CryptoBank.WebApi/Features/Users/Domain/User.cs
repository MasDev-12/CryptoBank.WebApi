namespace CryptoBank.WebApi.Features.Users.Domain;

public class User
{
    public User()
    {
        Roles = new HashSet<Role>();
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

    public virtual ICollection<Role> Roles { get; set; }
}
