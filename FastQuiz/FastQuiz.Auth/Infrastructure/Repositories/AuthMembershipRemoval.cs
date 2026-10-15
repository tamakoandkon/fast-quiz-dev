using FastQuiz.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FastQuiz.Auth.Infrastructure.Repositories;

internal static class AuthMembershipRemoval
{
    internal static async Task RemoveAsync(DbContext db, string tenantId, string userId)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        await db.Set<TenantUserMap>().Where(x => x.UserId == userId && x.TenantId == tenantId)
            .ExecuteUpdateAsync(x => x.SetProperty(m => m.CreatedAt, m => m.CreatedAt));
        await db.Set<UserRole>().Where(x => x.UserId == userId && x.TenantId == tenantId)
            .ExecuteDeleteAsync();
        var organizations = db.Set<Organization>().Where(x => x.TenantId == tenantId).Select(x => x.Id);
        await db.Set<OrganizationUserMap>()
            .Where(x => x.UserId == userId && organizations.Contains(x.OrganizationId))
            .ExecuteDeleteAsync();
        await db.Set<TenantUserMap>().Where(x => x.UserId == userId && x.TenantId == tenantId)
            .ExecuteDeleteAsync();
        await transaction.CommitAsync();
    }
}
