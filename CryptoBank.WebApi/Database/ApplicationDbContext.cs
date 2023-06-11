using Microsoft.EntityFrameworkCore;

namespace CryptoBank.WebApi.Database;

public class ApplicationDbContext:DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
}
