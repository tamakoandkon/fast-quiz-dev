using FastQuiz.Auth.Domain.Entities;
using FastQuiz.Auth.Domain.Exceptions;
using FastQuiz.Auth.Domain.Repositories;
using FastQuiz.Auth.UseCase.Commands;
using FastQuiz.Auth.UseCase.Dtos;
using FastQuiz.Auth.UseCase.Queries;
using FastQuiz.Common.Exceptions;
using FastQuiz.Common.Utils;

namespace FastQuiz.Auth.UseCase.Services;

public class OrganizationService(
    IIdGenerator _idGenerator,
    IPermissionService _permissionService,
    IOrganizationRepository _organizationRepository
) : IOrganizationService
{
    public async Task AddUserToOrganzitionAsync(string myId, string userId, string organizationId)
    {
        var org = await EnsureAndGetOrganizationAsync(organizationId).ConfigureAwait(false);
        if (
            !await _permissionService
                .HasPermissionAsync(org.TenantId, myId, Permissions.Organization)
                .ConfigureAwait(false)
        )
        {
            throw new PermissionException();
        }

        var existedMap = await _organizationRepository
            .GetUserInOrganizationAsync(organizationId, userId)
            .ConfigureAwait(false);

        if (existedMap)
        {
            return;
        }
        await _organizationRepository
            .AddUserToOrganizationAsync(organizationId, userId)
            .ConfigureAwait(false);
    }

    public async Task<OrganizationDto> CreateAsync(
        string userId,
        string tenantId,
        CreateOrganizationCommand command
    )
    {
        if (
            !await _permissionService
                .HasPermissionAsync(tenantId, userId, Permissions.Organization)
                .ConfigureAwait(false)
        )
        {
            throw new PermissionException();
        }

        var organization = new Organization(
            _idGenerator.GenerateUlId(),
            tenantId,
            command.ParentId,
            command.Name
        );
        if (command.ParentId is not null)
        {
            var parent = await EnsureAndGetOrganizationAsync(command.ParentId);
            if (parent.TenantId != tenantId)
                throw new PermissionException();
        }
        await _organizationRepository.CreateAsync(organization).ConfigureAwait(false);
        return new OrganizationDto(
            organization.Id,
            organization.Name,
            organization.TenantId,
            organization.ParentId,
            organization.CreatedAt,
            organization.UpdatedAt
        );
    }

    public async Task DeleteByIdAsync(string userId, string organizationId)
    {
        var org = await EnsureAndGetOrganizationAsync(organizationId).ConfigureAwait(false);
        if (
            !await _permissionService
                .HasPermissionAsync(org.TenantId, userId, Permissions.Organization)
                .ConfigureAwait(false)
        )
        {
            throw new PermissionException();
        }

        await _organizationRepository.DeleteAsync(organizationId).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<OrganizationDto>> GetAllAsync(
        string userId,
        string tenantId,
        GetAllOrganizationQuery query
    )
    {
        if (
            !await _permissionService
                .HasPermissionAsync(tenantId, userId, Permissions.Organization)
                .ConfigureAwait(false)
        )
        {
            throw new PermissionException();
        }

        var result = await _organizationRepository
            .GetAllByTenantIdAsync(tenantId, query.Cursor, query.Count, query.IsPrevious)
            .ConfigureAwait(false);

        return
        [
            .. result.Select(organization => new OrganizationDto(
                organization.Id,
                organization.Name,
                organization.TenantId,
                organization.ParentId,
                organization.CreatedAt,
                organization.UpdatedAt
            )),
        ];
    }

    public async Task<IReadOnlyList<OrganizationDto>> GetAllByParentIdAsync(
        string userId,
        string tenantId,
        string parentId,
        GetAllOrganizationQuery query
    )
    {
        if (
            !await _permissionService
                .HasPermissionAsync(tenantId, userId, Permissions.Organization)
                .ConfigureAwait(false)
        )
        {
            throw new PermissionException();
        }

        var parent = await EnsureAndGetOrganizationAsync(parentId);
        if (parent.TenantId != tenantId)
            throw new PermissionException();
        var result = await _organizationRepository
            .GetByParentIdAsync(parentId, query.Cursor, query.Count, query.IsPrevious, tenantId)
            .ConfigureAwait(false);

        return
        [
            .. result.Select(organization => new OrganizationDto(
                organization.Id,
                organization.Name,
                organization.TenantId,
                organization.ParentId,
                organization.CreatedAt,
                organization.UpdatedAt
            )),
        ];
    }

    public async Task<OrganizationDto> GetByIdAsync(string userId, string organizationId)
    {
        var org = await EnsureAndGetOrganizationAsync(organizationId).ConfigureAwait(false);
        if (
            !await _permissionService
                .HasPermissionAsync(org.TenantId, userId, Permissions.Organization)
                .ConfigureAwait(false)
        )
        {
            throw new PermissionException();
        }

        var organization =
            await _organizationRepository.FindByIdAsync(organizationId).ConfigureAwait(false)
            ?? throw new OrganizationNotFoundException();

        return new OrganizationDto(
            organization.Id,
            organization.Name,
            organization.TenantId,
            organization.ParentId,
            organization.CreatedAt,
            organization.UpdatedAt
        );
    }

    public async Task<bool> GetUserInOrganizationAsync(string organizationId, string userId)
    {
        return await _organizationRepository
            .GetUserInOrganizationAsync(organizationId, userId)
            .ConfigureAwait(false);
    }

    public async Task RemoveUserFromOrganizationAsync(
        string myId,
        string userId,
        string organizationId
    )
    {
        var org = await EnsureAndGetOrganizationAsync(organizationId).ConfigureAwait(false);
        if (
            !await _permissionService
                .HasPermissionAsync(org.TenantId, myId, Permissions.Organization)
                .ConfigureAwait(false)
        )
        {
            throw new PermissionException();
        }

        await _organizationRepository
            .RemoveUserFromOrganizationAsync(organizationId, userId)
            .ConfigureAwait(false);
    }

    public async Task UpdateAsync(
        string userId,
        string organizationId,
        UpdateOrganizationCommand command
    )
    {
        var organization = await EnsureAndGetOrganizationAsync(organizationId)
            .ConfigureAwait(false);

        if (
            !await _permissionService
                .HasPermissionAsync(organization.TenantId, userId, Permissions.Organization)
                .ConfigureAwait(false)
        )
        {
            throw new PermissionException();
        }

        organization.Name = command.Name;
        organization.UpdatedAt = DateTimeOffset.UtcNow;

        await _organizationRepository.UpdateAsync(organization).ConfigureAwait(false);
    }

    private async Task<Organization> EnsureAndGetOrganizationAsync(string id)
    {
        return await _organizationRepository.FindByIdAsync(id).ConfigureAwait(false)
            ?? throw new OrganizationNotFoundException();
    }
}
