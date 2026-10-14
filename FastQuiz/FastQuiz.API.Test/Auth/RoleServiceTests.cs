using FastQuiz.Auth.Domain.Entities;
using FastQuiz.Auth.Domain.Repositories;
using FastQuiz.Auth.UseCase.Services;
using FastQuiz.Common.Utils;
using FastQuiz.Common.Exceptions;
using Moq;
using Shouldly;

namespace FastQuiz.API.Test.Auth;

internal class RoleServiceTests
{
    [Test]
    public async Task RemoveRole_WhenOnlyTargetHasPermission_ShouldDenyOperator()
    {
        var role = new Role("role", "tenant", "role", "", false, true);
        var repository = new Mock<IRoleRepository>();
        repository.Setup(x => x.FindByIdAsync(role.Id)).ReturnsAsync(role);
        var permissions = new Mock<IPermissionService>();
        permissions.Setup(x => x.HasPermissionAsync("tenant", "target-admin", Permissions.Role)).ReturnsAsync(true);
        var service = new RoleService(new IdGenerator(), repository.Object, permissions.Object);

        await Should.ThrowAsync<PermissionException>(() =>
            service.RemoveRoleFromUserAsync("intruder", "target-admin", role.Id));
        repository.Verify(x => x.RemoveRoleFromUserAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task RemoveRole_WhenOperatorHasPermission_ShouldRevokeAndInvalidateCache()
    {
        var role = new Role("role", "tenant", "role", "", false, true);
        var repository = new Mock<IRoleRepository>();
        repository.Setup(x => x.FindByIdAsync(role.Id)).ReturnsAsync(role);
        var permissions = new Mock<IPermissionService>();
        permissions.Setup(x => x.HasPermissionAsync("tenant", "operator", Permissions.Role)).ReturnsAsync(true);
        var service = new RoleService(new IdGenerator(), repository.Object, permissions.Object);
        await service.RemoveRoleFromUserAsync("operator", "target", role.Id);
        repository.Verify(x => x.RemoveRoleFromUserAsync("target", role.Id), Times.Once);
        permissions.Verify(x => x.InvalidateUserPermissionsAsync("tenant", "target"), Times.Once);
    }

    [Test]
    public async Task AddRoleToUserAsync_ChecksTheOperatingAdministratorPermission()
    {
        var operatorId = "operator";
        var targetUserId = "target-user";
        var role = new Role("role", "tenant", "role", "", false, true);
        var roleRepository = new Mock<IRoleRepository>();
        roleRepository.Setup(x => x.FindByIdAsync(role.Id)).ReturnsAsync(role);
        var permissionService = new Mock<IPermissionService>();
        permissionService
            .Setup(x => x.HasPermissionAsync(role.TenantId, operatorId, Permissions.Role))
            .ReturnsAsync(true);
        var service = new RoleService(
            Mock.Of<IIdGenerator>(),
            roleRepository.Object,
            permissionService.Object
        );

        await service.AddRoleToUserAsync(operatorId, targetUserId, role.Id);

        permissionService.Verify(
            x => x.HasPermissionAsync(role.TenantId, operatorId, Permissions.Role),
            Times.Once
        );
        permissionService.Verify(
            x => x.HasPermissionAsync(role.TenantId, targetUserId, Permissions.Role),
            Times.Never
        );
        roleRepository.Verify(
            x => x.AddRoleToUserAsync(targetUserId, role.Id, role.TenantId),
            Times.Once
        );
    }
}
