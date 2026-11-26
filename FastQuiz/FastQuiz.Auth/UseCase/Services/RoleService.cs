using FastQuiz.Auth.Domain.Entities;
using FastQuiz.Auth.Domain.Exceptions;
using FastQuiz.Auth.Domain.Repositories;
using FastQuiz.Auth.UseCase.Commands;
using FastQuiz.Auth.UseCase.Dtos;
using FastQuiz.Auth.UseCase.Queries;
using FastQuiz.Common.Exceptions;
using FastQuiz.Common.Utils;

namespace FastQuiz.Auth.UseCase.Services;

public class RoleService(
    IIdGenerator _idGenerator,
    IRoleRepository _roleRepository,
    IPermissionService _permissionService,
    ITenantRepository _tenantRepository
) : IRoleService
{
    public async Task AddPermissionToRoleAsync(string userId, string roleId, string permission)
    {
        var role =
            await _roleRepository.FindByIdAsync(roleId).ConfigureAwait(false)
            ?? throw new RoleNotFoundExcpetion();

        if (
            !await _permissionService
                .HasPermissionAsync(role.TenantId, userId, Permissions.Role)
                .ConfigureAwait(false)
        )
        {
            throw new PermissionException();
        }

        ValidatePermissions([permission]);
        if (permission == Permissions.SystemAdmin
            && (role.TenantId != Tenant.DefaultTenantId || !await IsSystemAdminAsync(userId).ConfigureAwait(false)))
        {
            throw new PermissionException("只有系统管理员可以在默认租户内分配系统级权限");
        }

        await _roleRepository.AddPermissionAsync(roleId, permission).ConfigureAwait(false);
    }

    public async Task AddRoleToUserAsync(string myId, string userId, string roleId)
    {
        var role =
            await _roleRepository.FindByIdAsync(roleId).ConfigureAwait(false)
            ?? throw new RoleNotFoundExcpetion();
        if (
            !await _permissionService
                .HasPermissionAsync(role.TenantId, myId, Permissions.Role)
                .ConfigureAwait(false)
        )
        {
            throw new PermissionException();
        }

        if ((role.Id == Role.DefaultAdminRoleId || role.Permissions.Contains(Permissions.SystemAdmin))
            && !await IsSystemAdminAsync(myId).ConfigureAwait(false))
        {
            throw new PermissionException("只有系统管理员可以授予系统级角色");
        }

        if (!role.IsEnabled || !await _tenantRepository.GetUserInTenantAsync(role.TenantId, userId))
            throw new PermissionException("只能为本租户成员授予已启用的角色");
        await _roleRepository.AddRoleToUserAsync(userId, roleId, role.TenantId);
        await _permissionService.InvalidateUserPermissionsAsync(role.TenantId, userId);
    }

    public async Task<RoleDto> CreateAsync(
        string userId,
        string tenantId,
        CreateRoleCommand command
    )
    {
        if (
            !await _permissionService
                .HasPermissionAsync(tenantId, userId, Permissions.Role)
                .ConfigureAwait(false)
        )
        {
            throw new PermissionException();
        }

        ValidatePermissions(command.Permissions);
        var distinctPermissions = command.Permissions.Distinct(StringComparer.Ordinal).ToList();
        var isSystemAdmin = await IsSystemAdminAsync(userId).ConfigureAwait(false);
        if (distinctPermissions.Contains(Permissions.SystemAdmin)
            && (tenantId != Tenant.DefaultTenantId || !isSystemAdmin))
        {
            throw new PermissionException("只有系统管理员可以在默认租户内分配系统级权限");
        }

        var role = new Role(
            _idGenerator.GenerateUlId(),
            tenantId,
            command.Name,
            command.Description,
            isSystemAdmin && command.IsSystem,
            command.IsEnable
        );
        await _roleRepository.CreateAsync(role);

        await _roleRepository.AddPermissionsAsync(role.Id, distinctPermissions);

        return new RoleDto(
            role.Id,
            role.Name,
            role.Description,
            role.TenantId,
            role.IsSystem,
            role.IsEnabled,
            role.CreatedAt,
            role.UpdatedAt,
            distinctPermissions
        );
    }

    public async Task DeleteRoleByIdAsync(string userId, string roleId)
    {
        var role = await _roleRepository.FindByIdAsync(roleId).ConfigureAwait(false);

        if (role == null)
        {
            return;
        }

        if (
            !await _permissionService
                .HasPermissionAsync(role.TenantId, userId, Permissions.Role)
                .ConfigureAwait(false)
        )
        {
            throw new PermissionException();
        }

        if (role.Id == Role.DefaultAdminRoleId)
        {
            throw new PermissionException("不能删除系统内置管理员角色");
        }

        if (role.IsSystem && !await IsSystemAdminAsync(userId).ConfigureAwait(false))
        {
            throw new PermissionException("不能删除系统内置角色");
        }

        await _roleRepository.DeleteAsync(roleId).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<RoleDto>> GetAllRolesAsync(
        string userId,
        string tenantId,
        GetAllRoleQuery query
    )
    {
        if (
            !await _permissionService
                .HasPermissionAsync(tenantId, userId, Permissions.Role)
                .ConfigureAwait(false)
        )
        {
            throw new PermissionException();
        }

        var result = await _roleRepository
            .FindByTenantIdAsync(tenantId, query.Cursor, query.Count, query.IsPrevious)
            .ConfigureAwait(false);

        return result
            .Select(role => new RoleDto(
                role.Id,
                role.Name,
                role.Description,
                role.TenantId,
                role.IsSystem,
                role.IsEnabled,
                role.CreatedAt,
                role.UpdatedAt,
                role.Permissions
            ))
            .ToList();
    }

    public async Task<IReadOnlyList<RoleDto>> GetAllUserRolesAsync(
        string myId,
        string tenantId,
        string userId,
        GetAllUserRolesQuery query
    )
    {
        if (
            !await _permissionService
                .HasPermissionAsync(tenantId, myId, Permissions.Role)
                .ConfigureAwait(false)
        )
        {
            throw new PermissionException();
        }

        var result = await _roleRepository
            .GetAllUserRolesAsync(tenantId, userId, query.Cursor, query.Count)
            .ConfigureAwait(false);

        return result
            .Select(role => new RoleDto(
                role.Id,
                role.Name,
                role.Description,
                role.TenantId,
                role.IsSystem,
                role.IsEnabled,
                role.CreatedAt,
                role.UpdatedAt,
                role.Permissions
            ))
            .ToList();
    }

    public async Task<RoleDto> GetRoleByIdAsync(string userId, string roleId)
    {
        var role = await _roleRepository.FindByIdAsync(roleId) ?? throw new RoleNotFoundExcpetion();

        if (
            !await _permissionService
                .HasPermissionAsync(role.TenantId, userId, Permissions.Role)
                .ConfigureAwait(false)
        )
        {
            throw new PermissionException();
        }

        return new RoleDto(
            role.Id,
            role.Name,
            role.Description,
            role.TenantId,
            role.IsSystem,
            role.IsEnabled,
            role.CreatedAt,
            role.UpdatedAt,
            role.Permissions
        );
    }

    public async Task RemovePermissionFromRoleAsync(string userId, string roleId, string permission)
    {
        var role = await _roleRepository.FindByIdAsync(roleId).ConfigureAwait(false);
        if (role == null)
        {
            throw new RoleNotFoundExcpetion();
        }
        if (
            !await _permissionService
                .HasPermissionAsync(role.TenantId, userId, Permissions.Role)
                .ConfigureAwait(false)
        )
        {
            throw new PermissionException();
        }

        if (role.Id == Role.DefaultAdminRoleId && permission == Permissions.SystemAdmin)
        {
            throw new PermissionException("不能移除系统管理员角色的核心权限");
        }

        if (permission == Permissions.SystemAdmin && !await IsSystemAdminAsync(userId).ConfigureAwait(false))
        {
            throw new PermissionException("只有系统管理员可以修改系统级权限");
        }

        await _roleRepository.RemovePermissionAsync(roleId, permission);
    }

    public async Task RemoveRoleFromUserAsync(string myId, string userId, string roleId)
    {
        var role = await _roleRepository.FindByIdAsync(roleId).ConfigureAwait(false);
        if (role == null)
        {
            throw new RoleNotFoundExcpetion();
        }
        if (
            !await _permissionService
                .HasPermissionAsync(role.TenantId, myId, Permissions.Role)
                .ConfigureAwait(false)
        )
        {
            throw new PermissionException();
        }

        if (role.Id == Role.DefaultAdminRoleId && userId == User.DefaultAdminId)
        {
            throw new PermissionException("不能移除默认系统管理员的核心角色");
        }

        if ((role.Id == Role.DefaultAdminRoleId || role.Permissions.Contains(Permissions.SystemAdmin))
            && !await IsSystemAdminAsync(myId).ConfigureAwait(false))
        {
            throw new PermissionException("只有系统管理员可以撤销系统级角色");
        }

        await _roleRepository.RemoveRoleFromUserAsync(userId, roleId);
        await _permissionService.InvalidateUserPermissionsAsync(role.TenantId, userId);
    }

    public async Task UpdateAsync(string userId, string roleId, UpdateRoleCommand command)
    {
        var role = await _roleRepository.FindByIdAsync(roleId) ?? throw new RoleNotFoundExcpetion();

        if (
            !await _permissionService
                .HasPermissionAsync(role.TenantId, userId, Permissions.Role)
                .ConfigureAwait(false)
        )
        {
            throw new PermissionException();
        }

        ValidatePermissions(command.Permissions);
        var distinctPermissions = command.Permissions.Distinct(StringComparer.Ordinal).ToList();
        var isSystemAdmin = await IsSystemAdminAsync(userId).ConfigureAwait(false);

        if (distinctPermissions.Contains(Permissions.SystemAdmin)
            && (role.TenantId != Tenant.DefaultTenantId || !isSystemAdmin))
        {
            throw new PermissionException("只有系统管理员可以在默认租户内分配系统级权限");
        }

        if (role.Id == Role.DefaultAdminRoleId
            && (!command.IsEnable || !distinctPermissions.Contains(Permissions.SystemAdmin)))
        {
            throw new PermissionException("不能禁用默认系统管理员角色或移除其系统权限");
        }

        if (role.IsSystem && !isSystemAdmin)
        {
            throw new PermissionException("只有系统管理员可以修改系统内置角色");
        }

        role.Name = command.Name;
        role.Description = command.Description;
        role.IsEnabled = command.IsEnable;
        role.IsSystem = role.IsSystem || (isSystemAdmin && command.IsSystem);
        role.UpdatedAt = DateTimeOffset.UtcNow;
        role.Permissions = distinctPermissions;

        await _roleRepository.UpdateAsync(role);
    }

    private Task<bool> IsSystemAdminAsync(string userId) =>
        _permissionService.HasPermissionAsync(Tenant.DefaultTenantId, userId, Permissions.SystemAdmin);

    private static void ValidatePermissions(List<string>? permissions)
    {
        if (permissions is null || permissions.Any(x => !Permissions.AllPermissions().Contains(x)))
            throw new EntityArgumentException("角色权限列表无效");
    }

    public Task<IReadOnlyList<string>> GetAllPermissionsAsync(string userId)
    {
        return Task.FromResult(Permissions.AllPermissions());
    }

    public async Task<IReadOnlyCollection<string>> GetPermissionsAsync(
        string tenantId,
        string userId
    )
    {
        return (
            await _permissionService
                .GetUserPermissionSetAsync(tenantId, userId)
                .ConfigureAwait(false)
        ).Permissions;
    }
}
