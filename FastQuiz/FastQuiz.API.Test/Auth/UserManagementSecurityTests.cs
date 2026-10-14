using FastQuiz.Auth.Domain.Entities;
using FastQuiz.Auth.Domain.Repositories;
using FastQuiz.Auth.Domain.Services;
using FastQuiz.Auth.UseCase.Commands;
using FastQuiz.Auth.UseCase.Services;
using FastQuiz.Auth.UseCase.Validators;
using FastQuiz.Common.Exceptions;
using FastQuiz.Common.Utils;
using Moq;
using Shouldly;

namespace FastQuiz.API.Test.Auth;

internal class UserManagementSecurityTests
{
    private Mock<IUserRepository> _users = null!;
    private Mock<ITenantRepository> _tenants = null!;
    private Mock<IPermissionService> _permissions = null!;
    private UserApplicationService _service = null!;
    private User _target = null!;

    [SetUp]
    public void SetUp()
    {
        _target = new User("target", "targetuser", null, "old-hash");
        _users = new();
        _users.Setup(x => x.FindByIdAsync(_target.Id)).ReturnsAsync(_target);
        _tenants = new();
        _tenants.Setup(x => x.GetUserInTenantAsync("tenant", _target.Id)).ReturnsAsync(true);
        _tenants.Setup(x => x.GetAllMyTenantAsync(_target.Id, null, 2))
            .ReturnsAsync([new Tenant("tenant", "tenant", "")]);
        _permissions = new();
        _permissions.Setup(x => x.HasPermissionAsync("tenant", "operator", Permissions.User)).ReturnsAsync(true);
        var hasher = new Mock<IPasswordHasher>();
        hasher.Setup(x => x.Hash(It.IsAny<string>())).Returns("changed-hash");
        _service = new UserApplicationService(new IdGenerator(), hasher.Object, Mock.Of<ITokenGenerator>(),
            _users.Object, _permissions.Object, Mock.Of<IOrganizationRepository>(), _tenants.Object,
            new UserLoginCommandValidator(), new UserRegisterCommandValidator(), new UpdateUserCommandValidator());
    }

    [TestCase("read")]
    [TestCase("update")]
    [TestCase("remove")]
    public async Task ManagingUserOutsideTenant_ShouldBeDenied(string operation)
    {
        _tenants.Setup(x => x.GetUserInTenantAsync("tenant", _target.Id)).ReturnsAsync(false);
        await Should.ThrowAsync<PermissionException>(() => Invoke(operation));
        _users.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
        _users.Verify(x => x.DeleteByIdAsync(It.IsAny<string>()), Times.Never);
        _tenants.Verify(x => x.RemoveUserFromTenantAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [TestCase("read")]
    [TestCase("update")]
    [TestCase("remove")]
    public async Task TenantAdministratorCannotManageGlobalAdministrator(string operation)
    {
        _permissions.Setup(x => x.HasPermissionAsync(Tenant.DefaultTenantId, _target.Id, Permissions.SystemAdmin))
            .ReturnsAsync(true);
        await Should.ThrowAsync<PermissionException>(() => Invoke(operation));
    }

    [Test]
    public async Task SharedAccountCannotBeModifiedBySingleTenantAdministrator()
    {
        _tenants.Setup(x => x.GetAllMyTenantAsync(_target.Id, null, 2))
            .ReturnsAsync([new Tenant("tenant", "one", ""), new Tenant("other", "two", "")]);
        await Should.ThrowAsync<PermissionException>(() => Invoke("update"));
        _target.PasswordHash.ShouldBe("old-hash");
    }

    [Test]
    public async Task LocalOrdinaryAccountCanBeModified()
    {
        await Invoke("update");
        _target.PasswordHash.ShouldBe("changed-hash");
        _users.Verify(x => x.UpdateAsync(_target), Times.Once);
    }

    [Test]
    public async Task RemovingTenantMemberMustNotDeleteGlobalAccount()
    {
        await Invoke("remove");
        _tenants.Verify(x => x.RemoveUserFromTenantAsync("tenant", _target.Id), Times.Once);
        _users.Verify(x => x.DeleteByIdAsync(It.IsAny<string>()), Times.Never);
        _permissions.Verify(x => x.InvalidateUserPermissionsAsync("tenant", _target.Id), Times.Once);
    }

    private Task Invoke(string operation) => operation switch
    {
        "read" => _service.GetUserByIdAsync("operator", "tenant", _target.Id),
        "update" => _service.UpdateUserByIdAsync("operator", "tenant", _target.Id,
            new UpdateUserCommand(_target.Username, "new-password", null)),
        _ => _service.DeleteUserByIdAsync("operator", "tenant", _target.Id),
    };
}
