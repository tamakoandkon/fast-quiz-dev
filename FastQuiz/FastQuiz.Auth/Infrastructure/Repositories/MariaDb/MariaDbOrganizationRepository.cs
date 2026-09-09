using FastQuiz.Auth.Domain.Entities;
using FastQuiz.Auth.Domain.Repositories;
using FastQuiz.Common.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace FastQuiz.Auth.Infrastructure.Repositories.MariaDb;

public class MariaDbOrganizationRepository(MariaDbAuthDbContext _dbContext)
    : IOrganizationRepository
{
    private readonly DbSet<Organization> _organizations = _dbContext.Organizations;
    private readonly DbSet<OrganizationUserMap> _organizationUserMaps =
        _dbContext.OrganizationUserMaps;
    private readonly DbSet<TenantUserMap> _tenantUserMaps = _dbContext.TenantUserMaps;

    public async Task CreateAsync(Organization organization)
    {
        try
        {
            await _organizations.AddAsync(organization).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new DataAccessException("组织创建失败", ex);
        }
    }

    public async Task<Organization?> FindByIdAsync(string id)
    {
        return await _organizations
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Organization>> GetAllByTenantIdAsync(
        string tenantId,
        string? cursor,
        int count,
        bool isPrevious
    )
    {
        var query = _organizations
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.ParentId == null);

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

        var organizations = await query.Take(count).ToListAsync().ConfigureAwait(false);

        if (isPrevious)
        {
            organizations.Reverse();
        }

        return organizations;
    }

    public async Task<IReadOnlyList<Organization>> GetByParentIdAsync(
        string parentId,
        string? cursor,
        int count,
        bool isPrevious
    )
    {
        var query = _organizations.AsNoTracking().Where(x => x.ParentId == parentId);

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

        var organizations = await query.Take(count).ToListAsync().ConfigureAwait(false);

        if (isPrevious)
        {
            organizations.Reverse();
        }

        return organizations;
    }

    public async Task UpdateAsync(Organization organization)
    {
        await _organizations
            .Where(x => x.Id == organization.Id)
            .ExecuteUpdateAsync(entity =>
                entity
                    .SetProperty(x => x.ParentId, organization.ParentId)
                    .SetProperty(x => x.Name, organization.Name)
                    .SetProperty(x => x.UpdatedAt, organization.UpdatedAt)
            )
            .ConfigureAwait(false);
    }

    public async Task DeleteAsync(string id)
    {
        await _organizations.Where(x => x.Id == id).ExecuteDeleteAsync().ConfigureAwait(false);
    }

    public async Task AddUserToOrganizationAsync(string orgId, string userId)
    {
        try
        {
            await _organizationUserMaps
                .AddAsync(new OrganizationUserMap(orgId, userId))
                .ConfigureAwait(false);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new DataAccessException("组织创建失败", ex);
        }
    }

    public async Task RemoveUserFromOrganizationAsync(string orgId, string userId)
    {
        await _organizationUserMaps
            .Where(x => x.UserId == userId && x.OrganizationId == orgId)
            .ExecuteDeleteAsync()
            .ConfigureAwait(false);
    }

    public async Task<bool> GetUserInOrganizationAsync(string orgId, string userId)
    {
        return await _organizationUserMaps
            .Where(x => x.OrganizationId == orgId && x.UserId == userId)
            .AnyAsync()
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<string>> GetUserIdsByOrganizationIdAsync(string organizationId)
    {
        return await _organizationUserMaps
            .AsNoTracking()
            .Where(x => x.OrganizationId == organizationId)
            .Select(x => x.UserId)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<string>> GetUserIdsNotInOrganizationAsync(
        string tenantId,
        string organizationId
    )
    {
        return await _tenantUserMaps
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .Where(x =>
                !_organizationUserMaps
                    .Where(o => o.OrganizationId == organizationId)
                    .Select(o => o.UserId)
                    .Contains(x.UserId)
            )
            .Select(x => x.UserId)
            .ToListAsync()
            .ConfigureAwait(false);
    }
}
