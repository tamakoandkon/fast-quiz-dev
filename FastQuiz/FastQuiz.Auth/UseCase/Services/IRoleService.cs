using FastQuiz.Auth.UseCase.Commands;
using FastQuiz.Auth.UseCase.Dtos;
using FastQuiz.Auth.UseCase.Queries;

namespace FastQuiz.Auth.UseCase.Services;

public interface IRoleService
{
    Task<RoleDto> CreateAsync(string userId, string tenantId, CreateRoleCommand command);
    Task<RoleDto> GetRoleByIdAsync(string userId, string roleId);
    Task<IReadOnlyList<RoleDto>> GetAllRolesAsync(
        string userId,
        string tenantId,
        GetAllRoleQuery query
    );
    Task<IReadOnlyList<RoleDto>> GetAllUserRolesAsync(
        string myId,
        string tenantId,
        string userId,
        GetAllUserRolesQuery query
    );
    Task UpdateAsync(string userId, string roleId, UpdateRoleCommand command);
    Task DeleteRoleByIdAsync(string userId, string roleId);
    Task AddPermissionToRoleAsync(string userId, string roleId, string permission);
    Task RemovePermissionFromRoleAsync(string userId, string roleId, string permission);
    Task AddRoleToUserAsync(string myId, string userId, string roleId);
    Task RemoveRoleFromUserAsync(string myId, string userId, string roleId);
    Task<IReadOnlyList<string>> GetAllPermissionsAsync(string userId);
    Task<IReadOnlyCollection<string>> GetPermissionsAsync(string tenantId, string userId);
}
