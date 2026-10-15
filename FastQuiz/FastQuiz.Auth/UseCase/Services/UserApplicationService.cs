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
    IValidator<UserRegisterCommand> _userRegisterCommandValidator,
    IValidator<UpdateUserCommand> _updateUserCommandValidator
) : IUserApplicationService
{
    public async Task<UserDto> GetProfileAsync(string userId)
    {
        var user = await _userRepository.FindByIdAsync(userId) ?? throw new UserNotFoundException();
        return new UserDto(user.Id, user.Username, user.Email, user.CreatedAt);
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

        return [.. result.Select(x => new UserDto(x.Id, x.Username, x.Email, x.CreatedAt))];
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

        return [.. result.Select(x => new UserDto(x.Id, x.Username, x.Email, x.CreatedAt))];
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
            Dto = new AuthDto(token, -1, new UserDto(user.Id, user.Username, user.Email, user.CreatedAt)),
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

        return new AuthDto(token, -1, new UserDto(newUser.Id, newUser.Username, newUser.Email, newUser.CreatedAt));
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
        await EnsureCanManageUserAsync(myId, tenantId, userId, false).ConfigureAwait(false);
        return await GetProfileAsync(userId);
    }

    public async Task UpdateUserByIdAsync(
        string myId,
        string tenantId,
        string userId,
        UpdateUserCommand command
    )
    {
        await EnsureCanManageUserAsync(myId, tenantId, userId, true).ConfigureAwait(false);
        var user = await EnsureAndGetUserAsync(userId).ConfigureAwait(false);

        ApplyUpdate(user, command);

        if (string.IsNullOrEmpty(command.Password))
            await _userRepository.UpdateProfileAsync(user);
        else
            await _userRepository.UpdateAsync(user);
    }

    public async Task UpdateUserByIdAsync(string myId, UpdateUserCommand command)
    {
        if (!string.IsNullOrEmpty(command.Password))
            throw new EntityArgumentException("修改密码请使用密码修改接口并验证当前密码");
        var user = await EnsureAndGetUserAsync(myId).ConfigureAwait(false);

        ApplyUpdate(user, command);

        // A concurrent profile edit must never restore an earlier password hash.
        await _userRepository.UpdateProfileAsync(user);
    }

    public async Task<AuthDto> ChangePasswordAsync(string myId, ChangePasswordCommand command)
    {
        if (string.IsNullOrEmpty(command.CurrentPassword) || command.CurrentPassword.Length > 20
            || string.IsNullOrEmpty(command.NewPassword) || command.NewPassword.Length is < 6 or > 20)
            throw new EntityArgumentException("请输入当前密码，新密码须为6–20个字符");
        var user = await EnsureAndGetUserAsync(myId);
        if (!_passwordHasher.Verify(command.CurrentPassword, user.PasswordHash))
            throw new PermissionException("当前密码不正确");
        var newHash = _passwordHasher.Hash(command.NewPassword);
        if (!await _userRepository.TryChangePasswordAsync(user.Id, user.PasswordHash, newHash))
            throw new PermissionException("账号凭据已变更，请重新登录后重试");
        user.PasswordHash = newHash;
        return new AuthDto(_tokenGenerator.GenerateToken(user), -1,
            new UserDto(user.Id, user.Username, user.Email, user.CreatedAt));
    }

    /// <summary>
    /// 应用用户资料修改。密码留空表示不修改密码，避免把密码误置为空。
    /// </summary>
    private void ApplyUpdate(User user, UpdateUserCommand command)
    {
        _updateUserCommandValidator.ValidateAndThrowArgumentException(command);

        user.SetEmail(string.IsNullOrWhiteSpace(command.Email) ? null : command.Email);
        user.SetUsername(command.Username);

        if (!string.IsNullOrEmpty(command.Password))
        {
            user.SetPassword(command.Password, _passwordHasher);
        }
    }

    public async Task DeleteUserByIdAsync(string myId, string tenantId, string userId)
    {
        await EnsureCanManageUserAsync(myId, tenantId, userId, false).ConfigureAwait(false);
        if (userId == User.DefaultAdminId || await _permissionService.HasPermissionAsync(
            Tenant.DefaultTenantId, userId, Permissions.SystemAdmin))
        {
            throw new PermissionException("不能从租户中移除系统管理员");
        }

        // This is a tenant membership endpoint. A shared global account must survive removal.
        await _tenantRepository.RemoveUserFromTenantAsync(tenantId, userId);
        await _permissionService.InvalidateUserPermissionsAsync(tenantId, userId);
    }

    private async Task EnsureCanManageUserAsync(
        string operatorId,
        string tenantId,
        string targetUserId,
        bool modifiesGlobalAccount
    )
    {
        if (!await _permissionService.HasPermissionAsync(tenantId, operatorId, Permissions.User)
            || !await _tenantRepository.GetUserInTenantAsync(tenantId, targetUserId))
        {
            throw new PermissionException();
        }

        var isSystemAdmin = await _permissionService.HasPermissionAsync(
            Tenant.DefaultTenantId, operatorId, Permissions.SystemAdmin);
        if (!isSystemAdmin && (targetUserId == User.DefaultAdminId
            || await _permissionService.HasPermissionAsync(
                Tenant.DefaultTenantId, targetUserId, Permissions.SystemAdmin)))
        {
            throw new PermissionException("只有系统管理员可以管理系统管理员账号");
        }

        if (modifiesGlobalAccount && !isSystemAdmin)
        {
            var memberships = await _tenantRepository.GetAllMyTenantAsync(targetUserId, null, 2);
            if (memberships.Any(x => x.Id != tenantId))
            {
                throw new PermissionException("跨租户共享账号只能由本人或系统管理员修改");
            }
        }
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
