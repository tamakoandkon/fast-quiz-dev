using FastQuiz.Auth.Domain.Entities;
using FastQuiz.Auth.Domain.Repositories;
using FastQuiz.Common.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace FastQuiz.Auth.Infrastructure.Repositories.MariaDb;

public class MariaDbTenantRepository(MariaDbAuthDbContext _dbContext) : ITenantRepository
{
    private readonly DbSet<Tenant> _tenants = _dbContext.Tenants;
    private readonly DbSet<TenantUserMap> _tenantUserMaps = _dbContext.TenantUserMaps;

    public async Task CreateAsync(Tenant tenant)
    {
        try
        {
            await _tenants.AddAsync(tenant).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new DataAccessException("创建租户时数据访问失败", ex);
        }
    }

    public async Task<Tenant?> FindByIdAsync(string id)
    {
        return await _tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Tenant>> GetAllAsync(string? cursor, int count, bool isPrevious)
    {
        var query = _tenants.AsNoTracking();

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

        var tenants = await query.Take(count).ToListAsync().ConfigureAwait(false);

        if (isPrevious)
        {
            tenants.Reverse();
        }

        return tenants;
    }

    public async Task<IReadOnlyList<Tenant>> GetAllMyTenantAsync(
        string userId,
        string? cursor,
        int count
    )
    {
        var query = _tenantUserMaps.AsNoTracking().Where(x => x.UserId == userId);

        if (!string.IsNullOrWhiteSpace(cursor))
        {
            query = query
                .Where(x => x.TenantId.CompareTo(cursor) < 0)
                .OrderByDescending(x => x.TenantId);
        }
        else
        {
            query = query.OrderByDescending(x => x.TenantId);
        }

        var maps = await query.Take(count).ToListAsync().ConfigureAwait(false);
        var tenantIds = maps.Select(x => x.TenantId);
        var tenants = await _tenants
            .AsNoTracking()
            .Where(x => tenantIds.Contains(x.Id))
            .OrderByDescending(x => x.Id)
            .ToListAsync()
            .ConfigureAwait(false);

        return tenants;
    }

    public async Task UpdateAsync(Tenant tenant)
    {
        await _tenants
            .Where(x => x.Id == tenant.Id)
            .ExecuteUpdateAsync(entity =>
                entity
                    .SetProperty(x => x.Name, tenant.Name)
                    .SetProperty(x => x.Description, tenant.Description)
                    .SetProperty(x => x.UpdatedAt, tenant.UpdatedAt)
            )
            .ConfigureAwait(false);
    }

    public async Task DeleteAsync(string id)
    {
        await _tenants.Where(x => x.Id == id).ExecuteDeleteAsync().ConfigureAwait(false);
    }

    public async Task AddUserToTenantAsync(string tenantId, string userId)
    {
        try
        {
            await _tenantUserMaps
                .AddAsync(new TenantUserMap(tenantId, userId))
                .ConfigureAwait(false);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
        catch (DbUpdateException ex)
        {
            throw new DataAccessException("用户或租户不存在，或已存在该映射关系", ex);
        }
    }

    public async Task<bool> GetUserInTenantAsync(string tenantId, string userId)
    {
        return await _tenantUserMaps
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.TenantId == tenantId)
            .AnyAsync()
            .ConfigureAwait(false);
    }

    public async Task RemoveUserFromTenantAsync(string tenantId, string userId)
    {
        await _tenantUserMaps
            .Where(x => x.UserId == userId && x.TenantId == tenantId)
            .ExecuteDeleteAsync()
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<string>> GetUserIdsByTenantIdAsync(string tenantId)
    {
        return await _tenantUserMaps
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .Select(x => x.UserId)
            .ToListAsync()
            .ConfigureAwait(false);
    }
}
