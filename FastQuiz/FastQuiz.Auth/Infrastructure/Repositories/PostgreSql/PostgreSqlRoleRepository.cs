using FastQuiz.Auth.Domain.Entities;
using FastQuiz.Auth.Domain.Repositories;
using FastQuiz.Common.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace FastQuiz.Auth.Infrastructure.Repositories.PostgreSql;

public class PostgreSqlRoleRepository(PostgreSqlAuthDbContext _dbContext) : IRoleRepository
{
    private readonly DbSet<Role> _roles = _dbContext.Roles;
    private readonly DbSet<UserRole> _userRoles = _dbContext.UserRoles;
    private readonly DbSet<RolePermission> _rolePermissions = _dbContext.RolePermissions;

    #region Role
    public async Task CreateAsync(Role role)
    {
        try
        {
            await _roles.AddAsync(role).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new DataAccessException("创建角色失败", ex);
        }
    }

    public async Task<Role?> FindByIdAsync(string id)
    {
        var role = await _roles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id)
            .ConfigureAwait(false);

        if (role == null)
        {
            return null;
        }

        var permissions = await _rolePermissions
            .AsNoTracking()
            .Where(x => x.RoleId == id)
            .ToListAsync();

        role.Permissions = [.. permissions.Select(x => x.Permission)];

        return role;
    }

    public async Task<IReadOnlyList<Role>> FindByTenantIdAsync(
        string tenantId,
        string? cursor,
        int count,
        bool isPrevious
    )
    {
        var query = _roles.AsNoTracking().Where(x => x.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(cursor))
        {
            if (isPrevious)
            {
                query = query.Where(x => x.Id.CompareTo(cursor) > 0).OrderBy(x => x.Id);
            }
            else
            {
                query = query.Where(x => x.Id.CompareTo(cursor) < 0).OrderByDescending(x => x.Id);
            }
        }
        else
        {
            query = query.OrderByDescending(x => x.Id);
        }

        var roles = await query.Take(count).ToListAsync().ConfigureAwait(false);

        if (isPrevious)
        {
            roles.Reverse();
        }

        return roles;
    }

    public async Task<IReadOnlyList<Role>> GetAllUserRolesAsync(
        string tenantId,
        string userId,
        string? cursor,
        int count
    )
    {
        var query = _userRoles
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.UserId == userId);

        if (!string.IsNullOrWhiteSpace(cursor))
        {
            query = query
                .Where(x => x.RoleId.CompareTo(cursor) < 0)
                .OrderByDescending(x => x.RoleId);
        }
        else
        {
            query = query.OrderByDescending(x => x.RoleId);
        }

        var roles = await query.Take(count).ToListAsync().ConfigureAwait(false);

        var roleIdList = roles.Select(x => x.RoleId).ToList();

        var result = await _roles
            .AsNoTracking()
            .Where(x => roleIdList.Contains(x.Id))
            .OrderByDescending(x => x.Id)
            .ToListAsync()
            .ConfigureAwait(false);

        return result;
    }

    public async Task<Role?> FindByNameAsync(string tenantId, string name)
    {
        return await _roles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Name == name)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Role>> FindEnabledByTenantIdAsync(string tenantId)
    {
        return await _roles
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.IsEnabled)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task UpdateAsync(Role role)
    {
        await _roles
            .Where(x => x.Id == role.Id)
            .ExecuteUpdateAsync(entity =>
                entity
                    .SetProperty(x => x.Name, role.Name)
                    .SetProperty(x => x.Description, role.Description)
                    .SetProperty(x => x.IsSystem, role.IsSystem)
                    .SetProperty(x => x.IsEnabled, role.IsEnabled)
                    .SetProperty(x => x.UpdatedAt, role.UpdatedAt)
            )
            .ConfigureAwait(false);
    }

    public async Task DeleteAsync(string id)
    {
        await _rolePermissions
            .Where(x => x.RoleId == id)
            .ExecuteDeleteAsync()
            .ConfigureAwait(false);

        await _roles.Where(x => x.Id == id).ExecuteDeleteAsync().ConfigureAwait(false);
    }
    #endregion

    #region RolePermission
    public async Task AddPermissionAsync(string roleId, string permission)
    {
        var exists = await _rolePermissions
            .AnyAsync(x => x.RoleId == roleId && x.Permission == permission)
            .ConfigureAwait(false);

        if (exists)
        {
            return;
        }

        var rolePermission = new RolePermission(permission, roleId, DateTimeOffset.UtcNow);

        await _rolePermissions.AddAsync(rolePermission).ConfigureAwait(false);
        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task AddPermissionsAsync(string roleId, IEnumerable<string> permissions)
    {
        var permissionList = permissions.ToList();
        if (permissionList.Count == 0)
        {
            return;
        }

        var existingPermissions = await _rolePermissions
            .Where(x => x.RoleId == roleId)
            .Select(x => x.Permission)
            .ToListAsync()
            .ConfigureAwait(false);

        var newPermissions = permissionList
            .Where(p => !existingPermissions.Contains(p))
            .Select(p => new RolePermission(p, roleId, DateTimeOffset.UtcNow))
            .ToList();

        if (newPermissions.Count != 0)
        {
            await _rolePermissions.AddRangeAsync(newPermissions).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
    }

    public async Task RemovePermissionAsync(string roleId, string permission)
    {
        await _rolePermissions
            .Where(x => x.RoleId == roleId && x.Permission == permission)
            .ExecuteDeleteAsync()
            .ConfigureAwait(false);
    }

    public async Task RemovePermissionsAsync(string roleId, IEnumerable<string> permissions)
    {
        var permissionList = permissions.ToList();
        if (permissionList.Count == 0)
        {
            return;
        }

        await _rolePermissions
            .Where(x => x.RoleId == roleId && permissionList.Contains(x.Permission))
            .ExecuteDeleteAsync()
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<string>> GetPermissionsByRoleIdAsync(string roleId)
    {
        return await _rolePermissions
            .AsNoTracking()
            .Where(x => x.RoleId == roleId)
            .Select(x => x.Permission)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<string>> GetPermissionsByRoleIdAsync(
        IEnumerable<string> roleIds
    )
    {
        return await _rolePermissions
            .AsNoTracking()
            .Where(x => roleIds.Contains(x.RoleId))
            .Select(x => x.Permission)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<bool> HasPermissionAsync(string roleId, string permission)
    {
        return await _rolePermissions
            .AsNoTracking()
            .AnyAsync(x => x.RoleId == roleId && x.Permission == permission)
            .ConfigureAwait(false);
    }

    public async Task ClearPermissionsAsync(string roleId)
    {
        await _rolePermissions
            .Where(x => x.RoleId == roleId)
            .ExecuteDeleteAsync()
            .ConfigureAwait(false);
    }

    #endregion

    #region User Role
    public async Task AddRoleToUserAsync(string userId, string roleId, string tenantId)
    {
        try
        {
            await _userRoles.AddAsync(new UserRole(userId, roleId, tenantId)).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new DataAccessException("为用户添加角色出错", ex);
        }
    }

    public async Task RemoveRoleFromUserAsync(string userId, string roleId)
    {
        await _userRoles
            .Where(x => x.UserId == userId && x.RoleId == roleId)
            .ExecuteDeleteAsync()
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Role>> FindRolesByUserIdAsync(string tenantId, string userId)
    {
        return await _roles
            .AsNoTracking()
            .Where(r =>
                _userRoles
                    .AsNoTracking()
                    .Where(x => x.TenantId == tenantId && x.UserId == userId)
                    .Select(x => x.RoleId)
                    .ToList()
                    .Contains(r.Id)
            )
            .ToListAsync();
    }

    public async Task<bool> ExistsUserRoleAsync(string userId, string roleId)
    {
        return await _userRoles
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.RoleId == roleId)
            .AnyAsync()
            .ConfigureAwait(false);
    }
    #endregion
}
