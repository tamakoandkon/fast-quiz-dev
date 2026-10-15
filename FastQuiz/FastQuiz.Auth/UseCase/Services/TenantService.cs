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

public class TenantService(
    IIdGenerator _idGenerator,
    IPasswordHasher _passwordHasher,
    IPermissionService _permissionService,
    ITenantRepository _tenantRepository,
    IUserRepository _userRepository,
    IRoleRepository _roleRepository,
    IValidator<CreateTenantCommand> _createTenantCommandValidator
) : ITenantService
{
    public async Task<UserDto> AddExistingUserAsync(
        string operatorId, string tenantId, AddTenantMemberCommand command)
    {
        // Global account lookup must not be exposed to ordinary tenant administrators.
        if (!await _permissionService.HasPermissionAsync(
            Tenant.DefaultTenantId, operatorId, Permissions.SystemAdmin))
            throw new PermissionException();

        var username = command.Username?.Trim();
        if (string.IsNullOrWhiteSpace(username) || username.Length is < 3 or > 20)
            throw new BusinessException("请输入已有账号的完整用户名（3–20 个字符）");
        _ = await _tenantRepository.FindByIdAsync(tenantId)
            ?? throw new TenantNotFoundException();
        var user = await _userRepository.FindByUsernameAsync(username)
            ?? throw new ResourceNotFoundException("账号不存在，请先注册");

        if (!await _tenantRepository.GetUserInTenantAsync(tenantId, user.Id))
        {
            try
            {
                await _tenantRepository.AddUserToTenantAsync(tenantId, user.Id);
            }
            catch (DataAccessException)
            {
                // The composite membership key also makes concurrent retries safe.
                if (!await _tenantRepository.GetUserInTenantAsync(tenantId, user.Id))
                    throw;
            }
        }
        await _permissionService.InvalidateUserPermissionsAsync(tenantId, user.Id);
        // Membership alone grants no administrative role and never changes credentials.
        return new UserDto(user.Id, user.Username, user.Email, user.CreatedAt);
    }

    public async Task<TenantDto> CreateTenantAsync(string userId, CreateTenantCommand command)
    {
        _createTenantCommandValidator.ValidateAndThrowArgumentException(command);

        if (
            !await _permissionService
                .HasPermissionAsync(Tenant.DefaultTenantId, userId, Permissions.SystemAdmin)
                .ConfigureAwait(false)
        )
        {
            throw new PermissionException();
        }

        if (
            await _userRepository.FindByUsernameAsync(command.AdminUsername).ConfigureAwait(false)
            != null
        )
        {
            throw new UserAlreadyExistsException("管理员用户名已经存在");
        }

        var tenant = new Tenant(_idGenerator.GenerateUlId(), command.Name, command.Description);
        await _tenantRepository.CreateAsync(tenant).ConfigureAwait(false);

        // 创建租户级管理员：用户 + 租户映射 + 专属管理员角色（权限为全量）
        var adminUser = new User(
            _idGenerator.GenerateUlId(),
            command.AdminUsername,
            string.IsNullOrWhiteSpace(command.AdminEmail) ? null : command.AdminEmail,
            _passwordHasher.Hash(command.AdminPassword)
        );
        await _userRepository.CreateAsync(adminUser).ConfigureAwait(false);

        await _tenantRepository
            .AddUserToTenantAsync(tenant.Id, adminUser.Id)
            .ConfigureAwait(false);

        await CreateTenantAdminRoleAsync(tenant.Id, adminUser.Id).ConfigureAwait(false);

        return new TenantDto(
            tenant.Id,
            tenant.Name,
            tenant.Description,
            tenant.CreatedAt,
            tenant.UpdatedAt
        );
    }

    /// <summary>
    /// 为租户创建专属的管理员角色（权限为全量），并授予该租户的初始管理员。
    /// </summary>
    private async Task CreateTenantAdminRoleAsync(string tenantId, string adminUserId)
    {
        var role = new Role(
            _idGenerator.GenerateUlId(),
            tenantId,
            Role.TenantAdminRoleName,
            $"租户 {tenantId} 的租户级管理员",
            true,
            true
        );
        await _roleRepository.CreateAsync(role).ConfigureAwait(false);

        await _roleRepository
            .AddPermissionsAsync(role.Id, Permissions.AllPermissions())
            .ConfigureAwait(false);

        await _roleRepository
            .AddRoleToUserAsync(adminUserId, role.Id, tenantId)
            .ConfigureAwait(false);
    }

    public async Task DeleteTenantAsync(string userId, string tenantId)
    {
        if (
            !await _permissionService
                .HasPermissionAsync(Tenant.DefaultTenantId, userId, Permissions.SystemAdmin)
                .ConfigureAwait(false)
        )
        {
            throw new PermissionException();
        }

        await _tenantRepository.DeleteAsync(tenantId).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<TenantDto>> GetAllTenantsAsync(
        string userId,
        GetAllTenantQuery query
    )
    {
        if (
            !await _permissionService
                .HasPermissionAsync(Tenant.DefaultTenantId, userId, Permissions.SystemAdmin)
                .ConfigureAwait(false)
        )
        {
            throw new PermissionException();
        }

        var tenants = await _tenantRepository.GetAllAsync(
            query.Cursor,
            query.Count,
            query.IsPrevious
        );
        return
        [
            .. tenants.Select(t => new TenantDto(
                t.Id,
                t.Name,
                t.Description,
                t.CreatedAt,
                t.UpdatedAt
            )),
        ];
    }

    public async Task<IReadOnlyList<TenantDto>> GetMyTenantsAsync(
        string userId,
        GetAllMyTenantQuery query
    )
    {
        var tenants = await _tenantRepository.GetAllMyTenantAsync(
            userId,
            query.Cursor,
            query.Count
        );
        return
        [
            .. tenants.Select(t => new TenantDto(
                t.Id,
                t.Name,
                t.Description,
                t.CreatedAt,
                t.UpdatedAt
            )),
        ];
    }

    public async Task<TenantDto> GetTenantByIdAsync(string userId, string tenantId)
    {
        if (
            !await _permissionService
                .HasPermissionAsync(tenantId, userId, Permissions.Tenant)
                .ConfigureAwait(false)
        )
        {
            throw new PermissionException();
        }

        var isInTenant = await _tenantRepository
            .GetUserInTenantAsync(tenantId, userId)
            .ConfigureAwait(false);
        if (!isInTenant)
        {
            throw new PermissionException("不能访问其它组织");
        }

        var tenant =
            await _tenantRepository.FindByIdAsync(tenantId).ConfigureAwait(false)
            ?? throw new TenantNotFoundException();

        return new TenantDto(
            tenant.Id,
            tenant.Name,
            tenant.Description,
            tenant.CreatedAt,
            tenant.UpdatedAt
        );
    }

    public async Task<bool> GetUserInTenantAsync(string tenantId, string userId)
    {
        return await _tenantRepository.GetUserInTenantAsync(tenantId, userId).ConfigureAwait(false);
    }

    public async Task RemoveUserFromTenantAsync(string myId, string userId, string tenantId)
    {
        if (
            !await _permissionService
                .HasPermissionAsync(tenantId, myId, Permissions.SystemAdmin)
                .ConfigureAwait(false)
        )
        {
            throw new PermissionException();
        }

        await _tenantRepository.RemoveUserFromTenantAsync(tenantId, userId).ConfigureAwait(false);
    }

    public async Task UpdateTenantAsync(string userId, string tenantId, UpdateTenantCommand command)
    {
        var systemAdmin = await _permissionService.HasPermissionAsync(
            Tenant.DefaultTenantId, userId, Permissions.SystemAdmin);
        if (!await _permissionService.HasPermissionAsync(tenantId, userId, Permissions.Tenant)
            || (!systemAdmin && !await _tenantRepository.GetUserInTenantAsync(tenantId, userId)))
            throw new PermissionException();
        if (string.IsNullOrWhiteSpace(command.Name) || command.Name.Length > 50
            || command.Description is null || command.Description.Length > 200)
            throw new EntityArgumentException("租户名称不能为空且最多50个字符，描述最多200个字符");

        var tenant =
            await _tenantRepository.FindByIdAsync(tenantId).ConfigureAwait(false)
            ?? throw new TenantNotFoundException();

        tenant.Name = command.Name;
        tenant.Description = command.Description;
        tenant.UpdatedAt = DateTimeOffset.UtcNow;

        await _tenantRepository.UpdateAsync(tenant).ConfigureAwait(false);
    }
}
