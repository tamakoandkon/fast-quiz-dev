using FastQuiz.Auth.Domain.Entities;
using FastQuiz.Auth.Domain.Exceptions;
using FastQuiz.Auth.Domain.Repositories;
using FastQuiz.Auth.Domain.Services;
using FastQuiz.Auth.UseCase.Commands;
using FastQuiz.Auth.UseCase.Dtos;
using FastQuiz.Auth.UseCase.Queries;
using FastQuiz.Common.Exceptions;
using FastQuiz.Common.Utils;
using FastQuiz.Common.Validation;
using FluentValidation;

namespace FastQuiz.Auth.UseCase.Services;

public class UserApplicationService(
    IIdGenerator _idGenerator,
    IPasswordHasher _passwordHasher,
    ITokenGenerator _tokenGenerator,
    IUserRepository _userRepository,
    IPermissionService _permissionService,
    IOrganizationRepository _organizationRepository,
    ITenantRepository _tenantRepository,
    IValidator<UserLoginCommand> _userLoginCommandValidator,
    IValidator<UserRegisterCommand> _userRegisterCommandValidator
) : IUserApplicationService
{
    public async Task<UserDto> GetProfileAsync(string userId)
    {
        var user = await _userRepository.FindByIdAsync(userId) ?? throw new UserNotFoundException();
        return new UserDto(user.Id, user.Username);
    }

    public async Task<IReadOnlyList<UserDto>> GetUsersByOrganizationIdAsync(
        string userId,
        string organizationId,
        GetUsersByOrganizationIdQuery query
    )
    {
        var org = await EnsureAndGetOrganizationAsync(organizationId).ConfigureAwait(false);
        if (!await _permissionService.HasPermissionAsync(org.TenantId, userId, Permissions.User))
        {
            throw new PermissionException();
        }

        var maps = await _organizationRepository
            .GetUserIdsByOrganizationIdAsync(organizationId)
            .ConfigureAwait(false);

        var result = await _userRepository.GetUsersByIdsAsync(maps).ConfigureAwait(false);

        return [.. result.Select(x => new UserDto(x.Id, x.Username))];
    }

    public async Task<IReadOnlyList<UserDto>> GetNotInItUsersByOrganizationIdAsync(
        string userId,
        string organizationId,
        GetUsersByOrganizationIdQuery query
    )
    {
        var org = await EnsureAndGetOrganizationAsync(organizationId).ConfigureAwait(false);
        if (!await _permissionService.HasPermissionAsync(org.TenantId, userId, Permissions.User))
        {
            throw new PermissionException();
        }

        var maps = await _organizationRepository.GetUserIdsNotInOrganizationAsync(
            org.TenantId,
            organizationId
        );

        var result = await _userRepository.GetUsersByIdsAsync(maps);

        return [.. result.Select(x => new UserDto(x.Id, x.Username))];
    }

    public async Task<IReadOnlyList<UserDto>> GetUsersByTenantIdAsync(
        string userId,
        string tenantId,
        GetUsersByTenantIdQuery query
    )
    {
        if (!await _permissionService.HasPermissionAsync(tenantId, userId, Permissions.User))
        {
            throw new PermissionException();
        }

        var maps = await _tenantRepository.GetUserIdsByTenantIdAsync(tenantId);

        var result = await _userRepository.GetUsersByIdsAsync(maps);

        return [.. result.Select(x => new UserDto(x.Id, x.Username))];
    }

    public async Task<AuthResult> LoginAsync(UserLoginCommand command)
    {
        _userLoginCommandValidator.ValidateAndThrowArgumentException(command);

        var user = await _userRepository
            .FindByUsernameAsync(command.Username)
            .ConfigureAwait(false);

        if (user == null || !_passwordHasher.Verify(command.Password, user.PasswordHash))
        {
            return new AuthResult() { IsAuthentication = false };
        }

        var token = _tokenGenerator.GenerateToken(user);

        return new AuthResult()
        {
            IsAuthentication = true,
            Dto = new AuthDto(token, -1, new UserDto(user.Id, user.Username)),
        };
    }

    public async Task<AuthDto> RegisterAsync(UserRegisterCommand command)
    {
        _userRegisterCommandValidator.ValidateAndThrowArgumentException(command);

        if (
            await _userRepository.FindByUsernameAsync(command.Username).ConfigureAwait(false)
            != null
        )
        {
            throw new UserAlreadyExistsException("用户名已经存在");
        }

        var newUser = new User(
            _idGenerator.GenerateUlId(),
            command.Username,
            string.IsNullOrWhiteSpace(command.Email) ? null : command.Email,
            _passwordHasher.Hash(command.Password)
        );

        await _userRepository.CreateAsync(newUser);

        var token = _tokenGenerator.GenerateToken(newUser);

        return new AuthDto(token, -1, new UserDto(newUser.Id, newUser.Username));
    }

    public async Task<UserDto> CreateUserAsync(
        string userId,
        string tenantId,
        UserRegisterCommand command
    )
    {
        if (!await _permissionService.HasPermissionAsync(tenantId, userId, Permissions.User))
        {
            throw new PermissionException();
        }

        var result = await RegisterAsync(command).ConfigureAwait(false);

        await _tenantRepository
            .AddUserToTenantAsync(tenantId, result.User.Id)
            .ConfigureAwait(false);

        return result.User;
    }

    public async Task<UserDto> GetUserByIdAsync(string myId, string tenantId, string userId)
    {
        if (!await _permissionService.HasPermissionAsync(tenantId, myId, Permissions.User))
        {
            throw new PermissionException();
        }

        return await GetProfileAsync(userId);
    }

    public async Task UpdateUserByIdAsync(
        string myId,
        string tenantId,
        string userId,
        UpdateUserCommand command
    )
    {
        if (!await _permissionService.HasPermissionAsync(tenantId, myId, Permissions.User))
        {
            throw new PermissionException();
        }

        var user = await EnsureAndGetUserAsync(userId).ConfigureAwait(false);

        user.SetEmail(command.Email);
        user.SetPassword(command.Password, _passwordHasher);
        user.SetUsername(command.Username);

        await _userRepository.UpdateAsync(user);
    }

    public async Task UpdateUserByIdAsync(string myId, UpdateUserCommand command)
    {
        var user = await EnsureAndGetUserAsync(myId).ConfigureAwait(false);

        user.SetEmail(command.Email);
        user.SetPassword(command.Password, _passwordHasher);
        user.SetUsername(command.Username);

        await _userRepository.UpdateAsync(user);
    }

    public async Task DeleteUserByIdAsync(string myId, string tenantId, string userId)
    {
        if (!await _permissionService.HasPermissionAsync(tenantId, myId, Permissions.User))
        {
            throw new PermissionException();
        }

        await _tenantRepository.RemoveUserFromTenantAsync(tenantId, userId);

        await _userRepository.DeleteByIdAsync(userId);
    }

    private async Task<User> EnsureAndGetUserAsync(string userId)
    {
        return await _userRepository.FindByIdAsync(userId).ConfigureAwait(false)
            ?? throw new UserNotFoundException("用户不存在");
    }

    private async Task<Organization> EnsureAndGetOrganizationAsync(string orgId)
    {
        return await _organizationRepository.FindByIdAsync(orgId).ConfigureAwait(false)
            ?? throw new OrganizationNotFoundException("组织不存在");
    }
}
