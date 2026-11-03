using Microsoft.EntityFrameworkCore;
using ModelHub.Domain.Entities;

namespace ModelHub.Infrastructure.Persistence;

public sealed class ModelHubDbContext : DbContext
{
    public ModelHubDbContext(DbContextOptions<ModelHubDbContext> options) : base(options)
    {
    }
    public DbSet<AppUser> Users => Set<AppUser>();

    public DbSet<InvestmentModel> InvestmentModels => Set<InvestmentModel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.ToTable("Users");

            entity.HasKey(user => user.Id);

            entity.Property(user => user.Email)
                .IsRequired()
                .HasMaxLength(320);

            entity.HasIndex(user => user.Email)
                .IsUnique();

            entity.Property(user => user.PasswordHash)
                .IsRequired();

            entity.Property(user => user.Role)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(user => user.IsActive)
                .IsRequired();
        });

        modelBuilder.Entity<InvestmentModel>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(x => x.Description)
                .HasMaxLength(4000);

            entity.Property(x => x.Status)
                .IsRequired();

            entity.Property(x => x.CreatedUtc)
                .IsRequired();

            entity.Property(x => x.UpdatedUtc)
                .IsRequired();
        });


    }

    public override int SaveChanges()
    {
        ApplyAuditFields();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        ApplyAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditFields()
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property(nameof(BaseEntity.CreatedUtc)).CurrentValue = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property(nameof(BaseEntity.UpdatedUtc)).CurrentValue = now;
                entry.Property(nameof(BaseEntity.CreatedUtc)).IsModified = false;
            }
        }
    }
}
