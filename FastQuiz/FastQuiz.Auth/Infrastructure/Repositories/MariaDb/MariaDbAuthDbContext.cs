using FastQuiz.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FastQuiz.Auth.Infrastructure.Repositories.MariaDb;

public class MariaDbAuthDbContext(
    DbContextOptions<MariaDbAuthDbContext> _options,
    IOptions<AuthDbContextSettings> _dbSettings
) : DbContext(_options)
{
    private AuthDbContextSettings Settings => _dbSettings.Value;
    public DbSet<User> Users { get; set; }
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantUserMap> TenantUserMaps { get; set; }
    public DbSet<Organization> Organizations { get; set; }
    public DbSet<OrganizationUserMap> OrganizationUserMaps { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        OnUserCreating(modelBuilder);
        OnTenantCreating(modelBuilder);
        OnTenantUserMapCreating(modelBuilder);
        OnOrganizationCreating(modelBuilder);
        OnRoleCreating(modelBuilder);
        OnUserRoleCreating(modelBuilder);
        OnRolePermissionCreating(modelBuilder);
        OnOrganizationUserMapCreating(modelBuilder);
    }

    protected void OnUserCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable(Settings.UserTableName);

            entity.HasKey(u => u.Id);
            entity.Property(u => u.Id).HasMaxLength(128);

            entity.Property(u => u.Username).IsRequired().HasMaxLength(100);

            entity
                .Property(u => u.Email)
                .HasMaxLength(256)
                .IsRequired(false)
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(u => u.PasswordHash).IsRequired().HasMaxLength(500);

            entity.Property(u => u.CreatedAt).IsRequired();

            entity.HasIndex(u => u.Username).IsUnique().HasDatabaseName("IX_Users_Username");
            ;

            entity.HasIndex(u => u.Email).IsUnique().HasFilter("`Email` IS NOT NULL");
        });
    }

    protected void OnTenantCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.ToTable(Settings.TenantTableName);

            entity.HasKey(t => t.Id);
            entity.Property(t => t.Id).HasMaxLength(128);

            entity.Property(t => t.Name).IsRequired().HasMaxLength(200);

            entity.Property(t => t.Description).IsRequired().HasMaxLength(10000);

            entity.Property(t => t.CreatedAt).IsRequired();
            entity.Property(t => t.UpdatedAt).IsRequired();
        });
    }

    protected void OnOrganizationCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Organization>(entity =>
        {
            entity.ToTable(Settings.OrganizationTableName);

            entity.HasKey(o => o.Id);
            entity.Property(o => o.Id).HasMaxLength(128);

            entity.Property(o => o.TenantId).IsRequired().HasMaxLength(128);
            entity.Property(o => o.ParentId).HasMaxLength(128).IsRequired(false);

            entity.Property(o => o.Name).IsRequired().HasMaxLength(200);

            entity.Property(o => o.CreatedAt).IsRequired();
            entity.Property(o => o.UpdatedAt).IsRequired();

            entity.HasIndex(o => o.TenantId).HasDatabaseName("IX_Organization_TenantId");

            entity.HasIndex(o => o.ParentId).HasDatabaseName("IX_Organization_ParentId");

            entity
                .HasOne<Tenant>()
                .WithMany()
                .HasForeignKey(o => o.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity
                .HasOne<Organization>()
                .WithMany()
                .HasForeignKey(o => o.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    protected void OnRoleCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable(Settings.RoleTableName);

            entity.HasKey(r => r.Id);
            entity.Property(r => r.Id).HasMaxLength(128);

            entity.Property(r => r.TenantId).IsRequired().HasMaxLength(128);

            entity.Property(r => r.Name).IsRequired().HasMaxLength(100);
            entity.Property(r => r.Description).HasMaxLength(500).IsRequired(false);

            entity.Property(r => r.IsSystem).IsRequired();
            entity.Property(r => r.IsEnabled).IsRequired();

            entity.Property(r => r.CreatedAt).IsRequired();
            entity.Property(r => r.UpdatedAt).IsRequired();

            entity.HasIndex(r => new { r.TenantId, r.Name }).IsUnique();

            entity.Ignore(x => x.Permissions);

            entity
                .HasOne<Tenant>()
                .WithMany()
                .HasForeignKey(r => r.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    protected void OnUserRoleCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.ToTable(Settings.UserRoleTableName);

            entity.HasKey(ur => new
            {
                ur.UserId,
                ur.RoleId,
                ur.TenantId,
            });

            entity.Property(ur => ur.UserId).IsRequired().HasMaxLength(128);
            entity.Property(ur => ur.RoleId).IsRequired().HasMaxLength(128);
            entity.Property(ur => ur.TenantId).IsRequired().HasMaxLength(128);

            entity.Property(ur => ur.CreatedAt).IsRequired();

            entity.HasIndex(ur => ur.UserId);

            entity.HasIndex(ur => ur.RoleId);

            entity.HasIndex(ur => ur.TenantId);

            entity
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity
                .HasOne<Role>()
                .WithMany()
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    protected void OnRolePermissionCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.ToTable(Settings.RolePermissionTableName);

            entity.HasKey(rp => new { rp.RoleId, rp.Permission });

            entity.Property(rp => rp.RoleId).IsRequired().HasMaxLength(128);
            entity.Property(rp => rp.Permission).IsRequired().HasMaxLength(100);

            entity.Property(rp => rp.CreatedAt).IsRequired();
        });
    }

    protected void OnTenantUserMapCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TenantUserMap>(entity =>
        {
            entity.ToTable(Settings.TenantUserMapTableName);

            entity.HasKey(tum => new { tum.TenantId, tum.UserId });

            entity.Property(tum => tum.TenantId).IsRequired().HasMaxLength(128);
            entity.Property(tum => tum.UserId).IsRequired().HasMaxLength(128);
            entity.Property(tum => tum.CreatedAt).IsRequired();

            entity.HasIndex(tum => tum.TenantId);

            entity.HasIndex(tum => tum.UserId);

            entity
                .HasOne<Tenant>()
                .WithMany()
                .HasForeignKey(tum => tum.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            entity
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(tum => tum.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    protected void OnOrganizationUserMapCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrganizationUserMap>(entity =>
        {
            entity.ToTable(Settings.OrganizationUserMapTableName);

            entity.HasKey(oum => new { oum.OrganizationId, oum.UserId });

            entity.Property(oum => oum.OrganizationId).IsRequired().HasMaxLength(128);
            entity.Property(oum => oum.UserId).IsRequired().HasMaxLength(128);
            entity.Property(oum => oum.CreatedAt).IsRequired();

            entity.HasIndex(oum => oum.OrganizationId);

            entity.HasIndex(oum => oum.UserId);

            entity
                .HasOne<Organization>()
                .WithMany()
                .HasForeignKey(oum => oum.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(oum => oum.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
