// Data/AppIdentityDbContext.cs
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApiToko.ModelsEF.Toko;

namespace WebApiToko.Data;

public class AppIdentityDbContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<RevokeToken> RevokedTokens { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    public AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Additional configuration for UserProfile if needed
    }
}