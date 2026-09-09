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
    IPermissionService _permissionService
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

        await _roleRepository.AddPermissionAsync(roleId, permission).ConfigureAwait(false);
    }

    public async Task AddRoleToUserAsync(string myId, string userId, string roleId)
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

        await _roleRepository.AddRoleToUserAsync(userId, roleId, role.TenantId);
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

        var role = new Role(
            _idGenerator.GenerateUlId(),
            tenantId,
            command.Name,
            command.Description,
            command.IsSystem,
            command.IsEnable
        );
        await _roleRepository.CreateAsync(role);

        await _roleRepository.AddPermissionsAsync(role.Id, command.Permissions);

        return new RoleDto(
            role.Id,
            role.Name,
            role.Description,
            role.TenantId,
            role.IsSystem,
            role.IsEnabled,
            role.CreatedAt,
            role.UpdatedAt,
            command.Permissions
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
                .HasPermissionAsync(role.TenantId, userId, Permissions.Role)
                .ConfigureAwait(false)
        )
        {
            throw new PermissionException();
        }

        await _roleRepository.RemoveRoleFromUserAsync(userId, roleId);
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

        role.Name = command.Name;
        role.Description = command.Description;
        role.IsEnabled = command.IsEnable;
        role.IsSystem = command.IsSystem;
        role.UpdatedAt = DateTimeOffset.UtcNow;

        await _roleRepository.UpdateAsync(role);
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
