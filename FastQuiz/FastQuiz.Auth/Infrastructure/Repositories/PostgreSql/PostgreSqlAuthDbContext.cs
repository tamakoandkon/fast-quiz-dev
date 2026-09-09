using FastQuiz.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FastQuiz.Auth.Infrastructure.Repositories.PostgreSql;

public class PostgreSqlAuthDbContext(
    DbContextOptions<PostgreSqlAuthDbContext> _options,
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
        OnOrganizationUserMapCreating(modelBuilder);
        OnRoleCreating(modelBuilder);
        OnUserRoleCreating(modelBuilder);
        OnRolePermissionCreating(modelBuilder);
    }

    protected void OnUserCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable(Settings.UserTableName.ToLower());

            entity.HasKey(u => u.Id);
            entity.Property(u => u.Id).HasMaxLength(128).HasColumnName(nameof(User.Id).ToLower());

            entity
                .Property(u => u.Username)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName(nameof(User.Username).ToLower());

            entity
                .Property(u => u.Email)
                .HasColumnType("citext")
                .HasMaxLength(256)
                .IsRequired(false)
                .HasColumnName(nameof(User.Email).ToLower());

            entity
                .Property(u => u.PasswordHash)
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnName(nameof(User.PasswordHash).ToLower());

            entity
                .Property(u => u.CreatedAt)
                .IsRequired()
                .HasColumnName(nameof(User.CreatedAt).ToLower());

            entity.HasIndex(u => u.Username).IsUnique().HasDatabaseName("IX_Users_Username");
            ;

            entity
                .HasIndex(u => u.Email)
                .IsUnique()
                .HasFilter($"{nameof(User.Email).ToLower()} IS NOT NULL");
        });
    }

    protected void OnTenantCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.ToTable(Settings.TenantTableName.ToLower());

            entity.HasKey(t => t.Id);
            entity.Property(t => t.Id).HasMaxLength(128).HasColumnName(nameof(Tenant.Id).ToLower());

            entity
                .Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnName(nameof(Tenant.Name).ToLower());

            entity
                .Property(t => t.Description)
                .IsRequired()
                .HasMaxLength(10000)
                .HasColumnName(nameof(Tenant.Description).ToLower());

            entity
                .Property(t => t.CreatedAt)
                .IsRequired()
                .HasColumnName(nameof(Tenant.CreatedAt).ToLower());
            entity
                .Property(t => t.UpdatedAt)
                .IsRequired()
                .HasColumnName(nameof(Tenant.UpdatedAt).ToLower());
        });
    }

    protected void OnOrganizationCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Organization>(entity =>
        {
            entity.ToTable(Settings.OrganizationTableName.ToLower());

            entity.HasKey(o => o.Id);
            entity
                .Property(o => o.Id)
                .HasMaxLength(128)
                .HasColumnName(nameof(Organization.Id).ToLower());

            entity
                .Property(o => o.TenantId)
                .IsRequired()
                .HasMaxLength(128)
                .HasColumnName(nameof(Organization.TenantId).ToLower());
            entity
                .Property(o => o.ParentId)
                .HasMaxLength(128)
                .IsRequired(false)
                .HasColumnName(nameof(Organization.ParentId).ToLower());

            entity
                .Property(o => o.Name)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnName(nameof(Organization.Name).ToLower());

            entity
                .Property(o => o.CreatedAt)
                .IsRequired()
                .HasColumnName(nameof(Organization.CreatedAt).ToLower());
            entity
                .Property(o => o.UpdatedAt)
                .IsRequired()
                .HasColumnName(nameof(Organization.UpdatedAt).ToLower());

            entity.HasIndex(o => o.TenantId).HasDatabaseName("IX_Organization_TenantId");

            entity.HasIndex(o => o.ParentId).HasDatabaseName("IX_Organization_ParentId");

            entity
                .HasOne<Tenant>()
                .WithMany()
                .HasForeignKey(o => o.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            entity
                .HasOne<Organization>()
                .WithMany()
                .HasForeignKey(o => o.ParentId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    protected void OnRoleCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable(Settings.RoleTableName.ToLower());

            entity.HasKey(r => r.Id);
            entity.Property(r => r.Id).HasMaxLength(128).HasColumnName(nameof(Role.Id).ToLower());

            entity
                .Property(r => r.TenantId)
                .IsRequired()
                .HasMaxLength(128)
                .HasColumnName(nameof(Role.TenantId).ToLower());

            entity
                .Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName(nameof(Role.Name).ToLower());
            entity
                .Property(r => r.Description)
                .HasMaxLength(500)
                .IsRequired(false)
                .HasColumnName(nameof(Role.Description).ToLower());

            entity
                .Property(r => r.IsSystem)
                .IsRequired()
                .HasColumnName(nameof(Role.IsSystem).ToLower());
            entity
                .Property(r => r.IsEnabled)
                .IsRequired()
                .HasColumnName(nameof(Role.IsEnabled).ToLower());

            entity
                .Property(r => r.CreatedAt)
                .IsRequired()
                .HasColumnName(nameof(Role.CreatedAt).ToLower());
            entity
                .Property(r => r.UpdatedAt)
                .IsRequired()
                .HasColumnName(nameof(Role.UpdatedAt).ToLower());

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
            entity.ToTable(Settings.UserRoleTableName.ToLower());

            entity.HasKey(ur => new
            {
                ur.UserId,
                ur.RoleId,
                ur.TenantId,
            });

            entity
                .Property(ur => ur.UserId)
                .IsRequired()
                .HasMaxLength(128)
                .HasColumnName(nameof(UserRole.UserId).ToLower());
            entity
                .Property(ur => ur.RoleId)
                .IsRequired()
                .HasMaxLength(128)
                .HasColumnName(nameof(UserRole.RoleId).ToLower());
            entity
                .Property(ur => ur.TenantId)
                .IsRequired()
                .HasMaxLength(128)
                .HasColumnName(nameof(UserRole.TenantId).ToLower());

            entity
                .Property(ur => ur.CreatedAt)
                .IsRequired()
                .HasColumnName(nameof(UserRole.CreatedAt).ToLower());

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
            entity.ToTable(Settings.RolePermissionTableName.ToLower());

            entity.HasKey(rp => new { rp.RoleId, rp.Permission });

            entity
                .Property(rp => rp.RoleId)
                .IsRequired()
                .HasMaxLength(128)
                .HasColumnName(nameof(RolePermission.RoleId).ToLower());
            entity
                .Property(rp => rp.Permission)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName(nameof(RolePermission.Permission).ToLower());

            entity
                .Property(rp => rp.CreatedAt)
                .IsRequired()
                .HasColumnName(nameof(RolePermission.CreatedAt).ToLower());
        });
    }

    protected void OnTenantUserMapCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TenantUserMap>(entity =>
        {
            entity.ToTable(Settings.TenantUserMapTableName.ToLower());

            entity.HasKey(tum => new { tum.TenantId, tum.UserId });

            entity
                .Property(tum => tum.TenantId)
                .IsRequired()
                .HasMaxLength(128)
                .HasColumnName(nameof(TenantUserMap.TenantId).ToLower());
            entity
                .Property(tum => tum.UserId)
                .IsRequired()
                .HasMaxLength(128)
                .HasColumnName(nameof(TenantUserMap.UserId).ToLower());
            entity
                .Property(tum => tum.CreatedAt)
                .IsRequired()
                .HasColumnName(nameof(TenantUserMap.CreatedAt).ToLower());

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
            entity.ToTable(Settings.OrganizationUserMapTableName.ToLower());

            entity.HasKey(oum => new { oum.OrganizationId, oum.UserId });

            entity
                .Property(oum => oum.OrganizationId)
                .IsRequired()
                .HasMaxLength(128)
                .HasColumnName(nameof(OrganizationUserMap.OrganizationId).ToLower());
            entity
                .Property(oum => oum.UserId)
                .IsRequired()
                .HasMaxLength(128)
                .HasColumnName(nameof(OrganizationUserMap.UserId).ToLower());
            entity
                .Property(oum => oum.CreatedAt)
                .IsRequired()
                .HasColumnName(nameof(OrganizationUserMap.CreatedAt).ToLower());

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
