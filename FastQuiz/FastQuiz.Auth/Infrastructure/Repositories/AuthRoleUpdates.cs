using System.Data;
using FastQuiz.Auth.Domain.Entities;
using FastQuiz.Common.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace FastQuiz.Auth.Infrastructure.Repositories;

internal static class AuthRoleUpdates
{
    internal static async Task UpdateAsync(DbContext db, Role role)
    {
        await using var transaction = await db.Database.BeginTransactionAsync();
        await db.Set<Role>().Where(x => x.Id == role.Id).ExecuteUpdateAsync(x => x
            .SetProperty(r => r.Name, role.Name)
            .SetProperty(r => r.Description, role.Description)
            .SetProperty(r => r.IsEnabled, role.IsEnabled)
            .SetProperty(r => r.IsSystem, role.IsSystem)
            .SetProperty(r => r.UpdatedAt, role.UpdatedAt));
        var existing = await db.Set<RolePermission>().Where(x => x.RoleId == role.Id).ToListAsync();
        var wanted = role.Permissions.ToHashSet(StringComparer.Ordinal);
        db.RemoveRange(existing.Where(x => !wanted.Contains(x.Permission)));
        var current = existing.Select(x => x.Permission).ToHashSet(StringComparer.Ordinal);
        db.AddRange(wanted.Except(current).Select(p => new RolePermission(p, role.Id, DateTimeOffset.UtcNow)));
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    internal static async Task AddToMemberAsync(DbContext db, string tenantId, string userId, string roleId)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        // Lock the same membership row as removal. The no-op UPDATE is portable to all providers.
        await db.Set<TenantUserMap>().Where(x => x.TenantId == tenantId && x.UserId == userId)
            .ExecuteUpdateAsync(x => x.SetProperty(m => m.CreatedAt, m => m.CreatedAt));
        if (!await db.Set<TenantUserMap>().AnyAsync(x => x.TenantId == tenantId && x.UserId == userId)
            || !await db.Set<Role>().AnyAsync(x => x.Id == roleId && x.TenantId == tenantId && x.IsEnabled))
            throw new PermissionException("只能为本租户成员授予已启用的角色");
        if (!await db.Set<UserRole>().AnyAsync(x => x.TenantId == tenantId && x.UserId == userId && x.RoleId == roleId))
        {
            db.Add(new UserRole(userId, roleId, tenantId));
            await db.SaveChangesAsync();
        }
        await transaction.CommitAsync();
    }
}
