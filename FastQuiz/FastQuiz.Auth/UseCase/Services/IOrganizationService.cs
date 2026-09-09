using FastQuiz.Auth.UseCase.Commands;
using FastQuiz.Auth.UseCase.Dtos;
using FastQuiz.Auth.UseCase.Queries;

namespace FastQuiz.Auth.UseCase.Services;

public interface IOrganizationService
{
    Task<OrganizationDto> CreateAsync(
        string userId,
        string tenantId,
        CreateOrganizationCommand command
    );
    Task<IReadOnlyList<OrganizationDto>> GetAllAsync(
        string userId,
        string tenantId,
        GetAllOrganizationQuery query
    );
    Task<IReadOnlyList<OrganizationDto>> GetAllByParentIdAsync(
        string userId,
        string tenantId,
        string parentId,
        GetAllOrganizationQuery query
    );
    Task<OrganizationDto> GetByIdAsync(string userId, string organizationId);
    Task<bool> GetUserInOrganizationAsync(string organizationId, string userId);
    Task UpdateAsync(string userId, string organizationId, UpdateOrganizationCommand command);
    Task DeleteByIdAsync(string userId, string organizationId);
    Task AddUserToOrganzitionAsync(string myId, string userId, string organizationId);
    Task RemoveUserFromOrganizationAsync(string myId, string userId, string organizationId);
}
