using FastQuiz.Auth.Domain.Entities;
using FastQuiz.Auth.Domain.Exceptions;
using FastQuiz.Auth.Domain.Repositories;
using FastQuiz.Auth.UseCase.Commands;
using FastQuiz.Auth.UseCase.Dtos;
using FastQuiz.Auth.UseCase.Queries;
using FastQuiz.Common.Exceptions;
using FastQuiz.Common.Utils;

namespace FastQuiz.Auth.UseCase.Services;

public class TenantService(
    IIdGenerator _idGenerator,
    IPermissionService _permissionService,
    ITenantRepository _tenantRepository
) : ITenantService
{
    public async Task AddUserToTenantAsync(string myId, string userId, string tenantId)
    {
        if (
            !await _permissionService
                .HasPermissionAsync(tenantId, myId, Permissions.SystemAdmin)
                .ConfigureAwait(false)
        )
        {
            throw new PermissionException();
        }

        if (
            !await _permissionService
                .HasPermissionAsync(tenantId, userId, Permissions.SystemAdmin)
                .ConfigureAwait(false)
        )
        {
            throw new PermissionException();
        }

        var mapExisted = await _tenantRepository.GetUserInTenantAsync(tenantId, userId);
        if (mapExisted)
        {
            throw new TenantUserMapExistsException();
        }

        await _tenantRepository.AddUserToTenantAsync(tenantId, userId).ConfigureAwait(false);
    }

    public async Task<TenantDto> CreateTenantAsync(string userId, CreateTenantCommand command)
    {
        if (
            !await _permissionService
                .HasPermissionAsync(Tenant.DefaultTenantId, userId, Permissions.SystemAdmin)
                .ConfigureAwait(false)
        )
        {
            throw new PermissionException();
        }

        var tenant = new Tenant(_idGenerator.GenerateUlId(), command.Name, command.Description);
        await _tenantRepository.CreateAsync(tenant).ConfigureAwait(false);

        await _tenantRepository.AddUserToTenantAsync(tenant.Id, userId).ConfigureAwait(false);

        return new TenantDto(
            tenant.Id,
            tenant.Name,
            tenant.Description,
            tenant.CreatedAt,
            tenant.UpdatedAt
        );
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
        var tenant =
            await _tenantRepository.FindByIdAsync(tenantId).ConfigureAwait(false)
            ?? throw new TenantNotFoundException();

        tenant.Name = command.Name;
        tenant.Description = command.Description;
        tenant.UpdatedAt = DateTimeOffset.UtcNow;

        await _tenantRepository.UpdateAsync(tenant).ConfigureAwait(false);
    }
}
