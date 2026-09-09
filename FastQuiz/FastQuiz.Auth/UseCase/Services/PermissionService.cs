using System.Collections.Concurrent;
using EasyCaching.Core;
using FastQuiz.Auth.Domain.Entities;
using FastQuiz.Auth.Domain.Repositories;
using FastQuiz.Auth.UseCase.Extensions;

namespace FastQuiz.Auth.UseCase.Services;

public class PermissionService(
    IRoleRepository _roleRepository,
    IEasyCachingProviderFactory _cachingProvider
) : IPermissionService
{
    private readonly IEasyCachingProvider _cach = _cachingProvider.GetPermissionCacheProvider();
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _cacheLocks = new();

    private static TimeSpan GetRandomExpiration()
    {
        var baseExpiration = TimeSpan.FromHours(24);
        var offsetMinutes = Random.Shared.Next(-120, 120);
        return baseExpiration.Add(TimeSpan.FromMinutes(offsetMinutes));
    }

    public async Task<UserPermissionSet> GetUserPermissionSetAsync(string tenantId, string userId)
    {
        string cachingKey = $"permissions:{tenantId}@{userId}";

        var cached = await _cach.GetAsync<UserPermissionSet>(cachingKey).ConfigureAwait(false);

        if (cached.HasValue)
        {
            return cached.Value;
        }

        var lockObj = _cacheLocks.GetOrAdd(cachingKey, new SemaphoreSlim(1, 1));

        await lockObj.WaitAsync().ConfigureAwait(false);

        try
        {
            var reCached = await _cach
                .GetAsync<UserPermissionSet>(cachingKey)
                .ConfigureAwait(false);

            if (reCached.HasValue)
            {
                return reCached.Value;
            }

            var result = await LoadFromDatabaseAsync(tenantId, userId).ConfigureAwait(false);

            var expires =
                result.Permissions.Count != 0 ? GetRandomExpiration() : TimeSpan.FromHours(1);

            await _cach.SetAsync(cachingKey, result, expires).ConfigureAwait(false);

            return result;
        }
        finally
        {
            lockObj.Release();
        }
    }

    private async Task<UserPermissionSet> LoadFromDatabaseAsync(string tenantId, string userId)
    {
        var maps = await _roleRepository
            .FindRolesByUserIdAsync(tenantId, userId)
            .ConfigureAwait(false);

        var permissions = new List<string>();

        if (maps.Count != 0)
        {
            permissions.AddRange(
                await _roleRepository
                    .GetPermissionsByRoleIdAsync(maps.Select(x => x.Id))
                    .ConfigureAwait(false)
            );
        }

        return new UserPermissionSet()
        {
            UserId = userId,
            Permissions = [.. permissions.Distinct()],
            TenantId = tenantId,
        };
    }

    public async Task<bool> HasPermissionAsync(string tenantId, string userId, string permission)
    {
        var systemAdmin = await GetUserPermissionSetAsync(Tenant.DefaultTenantId, userId);
        if (systemAdmin.Permissions.Any(x => x == Permissions.SystemAdmin))
        {
            return true;
        }

        var permissions = await GetUserPermissionSetAsync(tenantId, userId).ConfigureAwait(false);

        return permissions.Permissions.Any(x => x == permission);
    }
}
