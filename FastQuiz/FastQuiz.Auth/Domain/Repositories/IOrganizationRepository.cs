using FastQuiz.Auth.Domain.Entities;

namespace FastQuiz.Auth.Domain.Repositories;

public interface IOrganizationRepository
{
    Task CreateAsync(Organization organization);
    Task<Organization?> FindByIdAsync(string id);
    Task<IReadOnlyList<Organization>> GetAllByTenantIdAsync(
        string tenantId,
        string? cursor,
        int count,
        bool isPrevious
    );
    Task<IReadOnlyList<Organization>> GetByParentIdAsync(
        string parentId,
        string? cursor,
        int count,
        bool isPrevious,
        string? tenantId = null
    );
    Task UpdateAsync(Organization organization);
    Task DeleteAsync(string id);
    Task AddUserToOrganizationAsync(string orgId, string userId);
    Task RemoveUserFromOrganizationAsync(string orgId, string userId);
    Task<bool> GetUserInOrganizationAsync(string orgId, string userId);
    Task<IReadOnlyList<string>> GetUserIdsByOrganizationIdAsync(string organizationId);
    Task<IReadOnlyList<string>> GetUserIdsNotInOrganizationAsync(
        string tenantId,
        string organizationId
    );
}
