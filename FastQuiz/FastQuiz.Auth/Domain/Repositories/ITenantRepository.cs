using FastQuiz.Auth.Domain.Entities;

namespace FastQuiz.Auth.Domain.Repositories;

public interface ITenantRepository
{
    Task CreateAsync(Tenant tenant);
    Task<Tenant?> FindByIdAsync(string id);
    Task<IReadOnlyList<Tenant>> GetAllAsync(string? cursor, int count, bool isPrevious);
    Task<IReadOnlyList<Tenant>> GetAllMyTenantAsync(string userId, string? cursor, int count);
    Task UpdateAsync(Tenant tenant);
    Task DeleteAsync(string id);
    Task AddUserToTenantAsync(string tenantId, string userId);
    Task<bool> GetUserInTenantAsync(string tenantId, string userId);
    Task<IReadOnlyList<string>> GetUserIdsByTenantIdAsync(string tenantId);
    Task RemoveUserFromTenantAsync(string tenantId, string userId);
}
