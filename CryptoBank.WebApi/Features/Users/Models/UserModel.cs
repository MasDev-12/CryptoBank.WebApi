namespace CryptoBank.WebApi.Features.Users.Models;

public class UserModel
{
    public UserModel()
    {
        Roles = new HashSet<RoleModel>();
    }
    public long Id { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime DateOfBirth { get; set; }
    public ICollection<RoleModel> Roles { get; set; }
}
