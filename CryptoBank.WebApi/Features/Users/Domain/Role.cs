using System.ComponentModel;

namespace CryptoBank.WebApi.Features.Users.Domain;

public class Role
{
    public long Id { get; set; }
    public UserRole Name { get; set; }
    public long UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public virtual User User { get; set; }
}

public enum UserRole
{
    [Description("UserRole")]
    UserRole = 0,
    [Description("AnalystRole")]
    AnalystRole = 1,
    [Description("AdministratorRole")]
    AdministratorRole = 2,
}
