using FastQuiz.Auth.Domain.Entities;
using FastQuiz.Auth.Domain.Repositories;
using FastQuiz.Auth.UseCase.Services;
using FastQuiz.Common.Utils;
using Moq;

namespace FastQuiz.API.Test.Auth;

internal class RoleServiceTests
{
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
