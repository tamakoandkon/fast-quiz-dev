using FastQuiz.Auth.Domain.Entities;

namespace FastQuiz.Auth.UseCase.Services;

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(string tenantId, string userId, string permission);
    Task<UserPermissionSet> GetUserPermissionSetAsync(string tenantId, string userId);
}
