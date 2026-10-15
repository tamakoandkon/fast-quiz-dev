using EasyCaching.Core;
using FastQuiz.Auth.Domain.Entities;
using FastQuiz.Auth.Domain.Repositories;
using FastQuiz.Auth.UseCase.Extensions;

namespace FastQuiz.Auth.UseCase.Services;

public class PermissionService(IRoleRepository roleRepository, IEasyCachingProviderFactory cachingProvider)
    : IPermissionService
{
    // Keep invalidation for callers and old cache entries, but never authorize from stale memory.
    public Task InvalidateUserPermissionsAsync(string tenantId, string userId) =>
        cachingProvider.GetPermissionCacheProvider().RemoveAsync($"permissions:{tenantId}@{userId}");

    public async Task<UserPermissionSet> GetUserPermissionSetAsync(string tenantId, string userId)
    {
        // Repository filters disabled roles and requires a current tenant membership.
        var roles = await roleRepository.FindRolesByUserIdAsync(tenantId, userId);
        IReadOnlyList<string> permissions = roles.Count == 0 ? [] : await roleRepository.GetPermissionsByRoleIdAsync(roles.Select(x => x.Id));
        return new UserPermissionSet { UserId = userId, TenantId = tenantId, Permissions = [.. permissions.Distinct()] };
    }

    public async Task<bool> HasPermissionAsync(string tenantId, string userId, string permission)
    {
        var system = await GetUserPermissionSetAsync(Tenant.DefaultTenantId, userId);
        if (system.Permissions.Contains(Permissions.SystemAdmin)) return true;
        var local = tenantId == Tenant.DefaultTenantId ? system : await GetUserPermissionSetAsync(tenantId, userId);
        return local.Permissions.Contains(permission);
    }
}
