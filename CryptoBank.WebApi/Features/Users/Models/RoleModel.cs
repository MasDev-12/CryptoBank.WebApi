using CryptoBank.WebApi.Features.Users.Domain;

namespace CryptoBank.WebApi.Features.Users.Models;

public class RoleModel
{
    public long Id { get; set; }
    public UserRole Name { get; set; }
}
