using CryptoBank.WebApi.Features.Accounts.Domain;
using CryptoBank.WebApi.Features.Auth.Domain;
using CryptoBank.WebApi.Features.Deposits.Domain;
using CryptoBank.WebApi.Features.Users.Domain;
using Microsoft.EntityFrameworkCore;

namespace CryptoBank.WebApi.Database;

public class ApplicationDbContext:DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
  
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        MapUsers(modelBuilder);
        MapRoles(modelBuilder);
        MapAccounts(modelBuilder);
        MapRefreshTokens(modelBuilder);
        MapTpubs(modelBuilder);
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Tpub> Tpubs { get; set; }
    public DbSet<DepositAddress> DepositAddresses { get; set; }

    private void MapUsers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(user =>
        {
            user.HasKey(x => x.Id);

            user.Property(u => u.Email)
                .IsRequired();

            user.Property(x => x.PasswordHashAndSalt)
                .IsRequired();

            user.Property(x => x.MemorySize)
                .IsRequired();

            user.Property(x => x.Parallelism)
                .IsRequired();

            user.Property(x => x.Iterations)
                .IsRequired();

            user.Property(x => x.BirthDate)
                .IsRequired();

            user.Property(x => x.CreatedAt)
                .IsRequired();

            user.Property(x => x.UpdatedAt);
        });
    }

    private void MapRoles(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>(role =>
        {
            role.HasKey(x => x.Id);

            role.Property(x => x.UserId)
                .IsRequired();

            role.Property(x => x.Name)
                .IsRequired();

            role.Property(x => x.CreatedAt)
                .IsRequired();

            role.HasOne(r => r.User)
                .WithMany(u => u.Roles)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void MapAccounts(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(account =>
        {
            account.HasKey(a => a.Id);

            account.Property(a => a.Number)
                 .IsRequired();

            account.Property(a => a.Currency)
                 .IsRequired();

            account.Property(a => a.Amount)
                .IsRequired();

            account.Property(a => a.CreatedAt)
                .IsRequired();

            account.Property(a => a.UserId)
                .IsRequired();

            account.HasOne(a => a.User)
                .WithMany(a => a.Accounts)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    public void MapRefreshTokens(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RefreshToken>(refreshToken =>
        {
            refreshToken.HasKey(r => r.Id);

            refreshToken.Property(e => e.Token)
                 .IsRequired()
                 .HasMaxLength(1000);

            refreshToken.HasIndex(r => r.Token)
                 .IsUnique();

            refreshToken.Property(r => r.UserId)
                 .IsRequired();

            refreshToken.Property(r => r.CreatedAt)
                .IsRequired();

            refreshToken.Property(r => r.TokenValidityPeriod)
                 .IsRequired();

            refreshToken.Property(r => r.TokenStoragePeriod)
                 .IsRequired();

            refreshToken.Property(r => r.Revoked)
                 .IsRequired();

            refreshToken.Property(r => r.ReplacedByNextToken)
                 .IsRequired(false);

            refreshToken.HasOne(d => d.User)
                 .WithMany(p => p.RefreshTokens)
                 .HasForeignKey(d => d.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
        });
    }
    public void MapTpubs(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tpub>(tbup =>
        {
            tbup.HasKey(t => t.Id);

            tbup.Property(t => t.CurrencyCode)
                .IsRequired();

            tbup.Property(t => t.Value)
                .IsRequired();
        });
    }

    public void MapDepositAddresses(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DepositAddress>(depositAddress =>
        {
            depositAddress.HasKey(d => d.Id);

            depositAddress.Property(d => d.CurrencyCode)
               .IsRequired();

            depositAddress.Property(d => d.UserId)
               .IsRequired();

            depositAddress.Property(d => d.TpubId)
               .IsRequired();

            depositAddress.HasOne(u => u.User)
               .WithMany(p => p.DepositAddresses)
               .HasForeignKey(d => d.UserId)
               .OnDelete(DeleteBehavior.Cascade);

            depositAddress.HasOne(d => d.Tpub)
               .WithMany()
               .HasForeignKey(d => d.TpubId);

            depositAddress.Property(d => d.CryptoAddress)
               .IsRequired();

            depositAddress.Property(d => d.DerivationIndex)
               .IsRequired();
        });
    }
}
