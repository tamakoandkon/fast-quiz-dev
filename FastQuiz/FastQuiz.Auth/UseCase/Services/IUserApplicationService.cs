using FastQuiz.Auth.UseCase.Commands;
using FastQuiz.Auth.UseCase.Dtos;
using FastQuiz.Auth.UseCase.Queries;

namespace FastQuiz.Auth.UseCase.Services;

public interface IUserApplicationService
{
    Task<AuthDto> RegisterAsync(UserRegisterCommand command);
    Task<AuthResult> LoginAsync(UserLoginCommand command);
    Task<UserDto> GetProfileAsync(string userId);
    Task<IReadOnlyList<UserDto>> GetUsersByTenantIdAsync(
        string userId,
        string tenantId,
        GetUsersByTenantIdQuery query
    );
    Task<IReadOnlyList<UserDto>> GetUsersByOrganizationIdAsync(
        string userId,
        string oragnizationId,
        GetUsersByOrganizationIdQuery query
    );
    Task<IReadOnlyList<UserDto>> GetNotInItUsersByOrganizationIdAsync(
        string userId,
        string oragnizationId,
        GetUsersByOrganizationIdQuery query
    );
    Task<UserDto> CreateUserAsync(string userId, string tenantId, UserRegisterCommand command);
    Task<UserDto> GetUserByIdAsync(string myId, string tenantId, string userId);
    Task UpdateUserByIdAsync(
        string myId,
        string tenantId,
        string userId,
        UpdateUserCommand command
    );
    Task UpdateUserByIdAsync(string myId, UpdateUserCommand command);
    Task<AuthDto> ChangePasswordAsync(string myId, ChangePasswordCommand command);
    Task DeleteUserByIdAsync(string myId, string tenantId, string userId);
}
