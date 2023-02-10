using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SecureTransferBackend.Data.Common;
using SecureTransferBackend.Services.Auth.Models;
using SecureTransferBackend.Services.Transfer.Models;

namespace SecureTransferBackend.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseNpgsql();
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.UseIdentityColumns();
        base.OnModelCreating(builder);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var addedEntities = ChangeTracker.Entries().Where(E => E.State == EntityState.Added).ToList();

        addedEntities.ForEach(e =>
        {
            if (e.Entity.GetType().IsSubclassOf(typeof(BaseModel)))
            {
                e.Property("CreatedAt").CurrentValue = DateTime.UtcNow;
                e.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
            }
        });

        var editedEntities = ChangeTracker.Entries().Where(E => E.State == EntityState.Modified).ToList();

        editedEntities.ForEach(e =>
        {
            if (e.Entity.GetType().IsSubclassOf(typeof(BaseModel)))
            {
                e.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
            }
        });

        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    public DbSet<PublicKeyPair> PublicKeyPairs { get; set; }
    public DbSet<Bundle> Bundles { get; set; }
    public DbSet<Attachment> Attachments { get; set; }
    public DbSet<BundleRecipient> BundleRecipients { get; set; }
    public DbSet<DecryptorKey> DecryptorKeys { get; set; }
}