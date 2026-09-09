using FastQuiz.Auth.Domain.Entities;

namespace FastQuiz.Auth.Domain.Repositories;

public interface IRoleRepository
{
    #region Role
    Task CreateAsync(Role role);
    Task<Role?> FindByIdAsync(string id);
    Task<IReadOnlyList<Role>> FindByTenantIdAsync(
        string tenantId,
        string? cursor,
        int count,
        bool isPrevious
    );
    Task<IReadOnlyList<Role>> GetAllUserRolesAsync(
        string tenantId,
        string userId,
        string? cursor,
        int count
    );
    Task<IReadOnlyList<Role>> FindEnabledByTenantIdAsync(string tenantId);
    Task UpdateAsync(Role role);
    Task DeleteAsync(string id);
    #endregion

    #region RolePermission
    Task AddPermissionAsync(string roleId, string permission);
    Task AddPermissionsAsync(string roleId, IEnumerable<string> permissions);
    Task RemovePermissionAsync(string roleId, string permission);
    Task RemovePermissionsAsync(string roleId, IEnumerable<string> permissions);
    Task<IReadOnlyList<string>> GetPermissionsByRoleIdAsync(string roleId);
    Task<IReadOnlyList<string>> GetPermissionsByRoleIdAsync(IEnumerable<string> roleIds);
    Task<bool> HasPermissionAsync(string roleId, string permission);
    Task ClearPermissionsAsync(string roleId);
    #endregion

    #region User Role
    Task AddRoleToUserAsync(string userId, string roleId, string tenantId);
    Task RemoveRoleFromUserAsync(string userId, string roleId);
    Task<IReadOnlyList<Role>> FindRolesByUserIdAsync(string tenantId, string userId);
    Task<bool> ExistsUserRoleAsync(string userId, string roleId);
    #endregion
}
