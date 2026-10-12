using FastQuiz.Auth.UseCase.Commands;
using FastQuiz.Auth.UseCase.Dtos;
using FastQuiz.Auth.UseCase.Queries;

namespace FastQuiz.Auth.UseCase.Services;

public interface ITenantService
{
    Task<TenantDto> CreateTenantAsync(string userId, CreateTenantCommand command);
    Task<IReadOnlyList<TenantDto>> GetMyTenantsAsync(string userId, GetAllMyTenantQuery query);
    Task<IReadOnlyList<TenantDto>> GetAllTenantsAsync(string userId, GetAllTenantQuery query);
    Task<TenantDto> GetTenantByIdAsync(string userId, string tenantId);
    Task<bool> GetUserInTenantAsync(string tenantId, string userId);
    Task UpdateTenantAsync(string userId, string tenantId, UpdateTenantCommand command);
    Task DeleteTenantAsync(string userId, string tenantId);
    Task RemoveUserFromTenantAsync(string myId, string userId, string tenantId);
}
