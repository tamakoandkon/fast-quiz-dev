using FastQuiz.Auth.Domain.Entities;
using FastQuiz.Auth.Domain.Repositories;
using FastQuiz.Auth.Test.Infrastructure.Repositories.Utils;
using FastQuiz.Common.Exceptions;
using Shouldly;

namespace FastQuiz.Auth.Test.Infrastructure.Repositories;

[TestFixture]
internal abstract class RoleRepositoryTestBase
{
    protected abstract IRoleRepository Sut { get; }
    protected abstract ITenantRepository TenantRepository { get; }
    protected abstract IUserRepository UserRepository { get; }

    #region Role Tests

    [Test]
    public async Task Create_WhenRoleDoesNotExist_ShouldInsertOne()
    {
        // Arrange
        var tenant = await CreateTenantAsync();
        var role = RoleBuilder.New().WithTenantId(tenant.Id).Build();

        // Act
        await Sut.CreateAsync(role);

        // Assert
        var result = await Sut.FindByIdAsync(role.Id);
        result.ShouldNotBeNull();
        result.Id.ShouldBe(role.Id);
        result.TenantId.ShouldBe(tenant.Id);
    }

    [Test]
    public async Task Create_WhenRoleDoesAlreadyExist_ShouldInsertFail()
    {
        // Arrange
        var tenant = await CreateTenantAsync();
        var role = RoleBuilder.New().WithTenantId(tenant.Id).Build();
        await Sut.CreateAsync(role);

        // Act & Assert
        Should.Throw<DataAccessException>(() => Sut.CreateAsync(role));
    }

    [Test]
    public async Task FindById_WhenRoleDoesExist_ShouldReturnOne()
    {
        // Arrange
        var tenant = await CreateTenantAsync();
        var role = RoleBuilder.New().WithTenantId(tenant.Id).Build();
        await Sut.CreateAsync(role);

        // Act
        var result = await Sut.FindByIdAsync(role.Id);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(role.Id);
        result.TenantId.ShouldBe(tenant.Id);
    }

    [Test]
    public async Task FindById_WhenRoleDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var role = RoleBuilder.New().Build();

        // Act
        var result = await Sut.FindByIdAsync(role.Id);

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public async Task FindById_WhenRoleHasPermissions_ShouldLoadPermissions()
    {
        // Arrange
        var tenant = await CreateTenantAsync();
        var role = RoleBuilder.New().WithTenantId(tenant.Id).Build();
        await Sut.CreateAsync(role);
        await Sut.AddPermissionAsync(role.Id, "permission1");
        await Sut.AddPermissionAsync(role.Id, "permission2");

        // Act
        var result = await Sut.FindByIdAsync(role.Id);

        // Assert
        result.ShouldNotBeNull();
        result.Permissions.ShouldNotBeNull();
        result.Permissions.Count.ShouldBe(2);
        result.Permissions.ShouldContain("permission1");
        result.Permissions.ShouldContain("permission2");
    }

    [Test]
    public async Task FindByTenantId_WhenInitialLoad_ShouldReturnCorrectCountAndOrder()
    {
        // Arrange
        var tenant = await CreateTenantAsync();
        var builder = RoleBuilder.New().WithTenantId(tenant.Id);
        var roles = new List<Role>
        {
            builder.WithId("003").Build(),
            builder.WithId("002").Build(),
            builder.WithId("001").Build(),
        };

        foreach (var role in roles)
            await Sut.CreateAsync(role);

        // Act
        var result = await Sut.FindByTenantIdAsync(
            tenant.Id,
            cursor: null,
            count: 2,
            isPrevious: false
        );

        // Assert
        result.Count.ShouldBe(2);
        result.Select(r => r.Id).ShouldBe(["003", "002"]);
        result.All(r => r.TenantId == tenant.Id).ShouldBeTrue();
    }

    [Test]
    public async Task FindByTenantId_WhenForwardPagination_ShouldReturnNextPage()
    {
        // Arrange
        var tenant = await CreateTenantAsync();
        var builder = RoleBuilder.New().WithTenantId(tenant.Id);
        var roles = Enumerable
            .Range(1, 5)
            .Select(i => builder.WithId(i.ToString().PadLeft(3, '0')).Build())
            .ToList();

        foreach (var role in roles)
            await Sut.CreateAsync(role);

        // Act
        var result = await Sut.FindByTenantIdAsync(
            tenant.Id,
            cursor: "004",
            count: 2,
            isPrevious: false
        );

        // Assert
        result.Count.ShouldBe(2);
        result.Select(r => r.Id).ShouldBe(["003", "002"]);
        result.All(r => r.TenantId == tenant.Id).ShouldBeTrue();
    }

    [Test]
    public async Task FindByTenantId_WhenPreviousPagination_ShouldReturnPreviousPage()
    {
        // Arrange
        var tenant = await CreateTenantAsync();
        var builder = RoleBuilder.New().WithTenantId(tenant.Id);
        var roles = Enumerable
            .Range(1, 5)
            .Select(i => builder.WithId(i.ToString().PadLeft(3, '0')).Build())
            .ToList();

        foreach (var role in roles)
            await Sut.CreateAsync(role);

        // Act
        var result = await Sut.FindByTenantIdAsync(
            tenant.Id,
            cursor: "002",
            count: 2,
            isPrevious: true
        );

        // Assert
        result.Count.ShouldBe(2);
        result.Select(r => r.Id).ShouldBe(["004", "003"]);
        result.All(r => r.TenantId == tenant.Id).ShouldBeTrue();
    }

    [Test]
    public async Task FindEnabledByTenantId_WhenRolesExist_ShouldReturnOnlyEnabled()
    {
        // Arrange
        var tenant = await CreateTenantAsync();
        var builder = RoleBuilder.New().WithTenantId(tenant.Id);
        var enabledRole = builder.WithIsEnabled(true).Build();
        var disabledRole = builder.WithIsEnabled(false).Build();
        await Sut.CreateAsync(enabledRole);
        await Sut.CreateAsync(disabledRole);

        // Act
        var result = await Sut.FindEnabledByTenantIdAsync(tenant.Id);

        // Assert
        result.Count.ShouldBe(1);
        result.First().Id.ShouldBe(enabledRole.Id);
        result.First().IsEnabled.ShouldBeTrue();
    }

    [Test]
    public async Task Update_WhenRoleDoesAlreadyExist_ShouldUpdateSuccess()
    {
        // Arrange
        var tenant = await CreateTenantAsync();
        var role = RoleBuilder.New().WithTenantId(tenant.Id).Build();
        await Sut.CreateAsync(role);

        role.Name = Guid.NewGuid().ToString();
        role.Description = Guid.NewGuid().ToString();
        role.IsSystem = true;
        role.IsEnabled = false;
        role.UpdatedAt = DateTimeOffset.MaxValue;

        // Act
        await Sut.UpdateAsync(role);

        // Assert
        var result = await Sut.FindByIdAsync(role.Id);
        result.ShouldNotBeNull();
        result.Name.ShouldBe(role.Name);
        result.Description.ShouldBe(role.Description);
        result.IsSystem.ShouldBe(role.IsSystem);
        result.IsEnabled.ShouldBe(role.IsEnabled);
        result.UpdatedAt.ShouldBe(role.UpdatedAt, TimeSpan.FromSeconds(1));
    }

    [Test]
    public async Task Delete_WhenRoleDoesAlreadyExist_ShouldDeleteSuccess()
    {
        // Arrange
        var tenant = await CreateTenantAsync();
        var role = RoleBuilder.New().WithTenantId(tenant.Id).Build();
        await Sut.CreateAsync(role);

        // Act
        await Sut.DeleteAsync(role.Id);

        // Assert
        var result = await Sut.FindByIdAsync(role.Id);
        result.ShouldBeNull();
    }

    [Test]
    public async Task Delete_WhenRoleHasPermissions_ShouldDeleteRoleAndPermissions()
    {
        // Arrange
        var tenant = await CreateTenantAsync();
        var role = RoleBuilder.New().WithTenantId(tenant.Id).Build();
        await Sut.CreateAsync(role);
        await Sut.AddPermissionAsync(role.Id, "permission1");
        await Sut.AddPermissionAsync(role.Id, "permission2");

        // Act
        await Sut.DeleteAsync(role.Id);

        // Assert
        var result = await Sut.FindByIdAsync(role.Id);
        result.ShouldBeNull();

        var permissions = await Sut.GetPermissionsByRoleIdAsync(role.Id);
        permissions.ShouldBeEmpty();
    }

    #endregion

    #region RolePermission Tests

    [Test]
    public async Task AddPermission_WhenPermissionDoesNotExist_ShouldInsertOne()
    {
        // Arrange
        var tenant = await CreateTenantAsync();
        var role = RoleBuilder.New().WithTenantId(tenant.Id).Build();
        await Sut.CreateAsync(role);

        // Act
        await Sut.AddPermissionAsync(role.Id, "permission1");

        // Assert
        var result = await Sut.HasPermissionAsync(role.Id, "permission1");
        result.ShouldBeTrue();
    }

    [Test]
    public async Task AddPermission_WhenPermissionDoesAlreadyExist_ShouldNotInsert()
    {
        // Arrange
        var tenant = await CreateTenantAsync();
        var role = RoleBuilder.New().WithTenantId(tenant.Id).Build();
        await Sut.CreateAsync(role);
        await Sut.AddPermissionAsync(role.Id, "permission1");

        // Act
        await Sut.AddPermissionAsync(role.Id, "permission1");

        // Assert
        var permissions = await Sut.GetPermissionsByRoleIdAsync(role.Id);
        permissions.Count.ShouldBe(1);
    }

    [Test]
    public async Task AddPermissions_WhenPermissionsDoNotExist_ShouldInsertAll()
    {
        // Arrange
        var tenant = await CreateTenantAsync();
        var role = RoleBuilder.New().WithTenantId(tenant.Id).Build();
        await Sut.CreateAsync(role);
        var permissions = new[] { "permission1", "permission2", "permission3" };

        // Act
        await Sut.AddPermissionsAsync(role.Id, permissions);

        // Assert
        var result = await Sut.GetPermissionsByRoleIdAsync(role.Id);
        result.Count.ShouldBe(3);
        result.ShouldContain("permission1");
        result.ShouldContain("permission2");
        result.ShouldContain("permission3");
    }

    [Test]
    public async Task AddPermissions_WhenSomePermissionsAlreadyExist_ShouldInsertOnlyNew()
    {
        // Arrange
        var tenant = await CreateTenantAsync();
        var role = RoleBuilder.New().WithTenantId(tenant.Id).Build();
        await Sut.CreateAsync(role);
        await Sut.AddPermissionAsync(role.Id, "permission1");
        var permissions = new[] { "permission1", "permission2", "permission3" };

        // Act
        await Sut.AddPermissionsAsync(role.Id, permissions);

        // Assert
        var result = await Sut.GetPermissionsByRoleIdAsync(role.Id);
        result.Count.ShouldBe(3);
        result.ShouldContain("permission1");
        result.ShouldContain("permission2");
        result.ShouldContain("permission3");
    }

    [Test]
    public async Task RemovePermission_WhenPermissionExists_ShouldDeleteSuccess()
    {
        // Arrange
        var tenant = await CreateTenantAsync();
        var role = RoleBuilder.New().WithTenantId(tenant.Id).Build();
        await Sut.CreateAsync(role);
        await Sut.AddPermissionAsync(role.Id, "permission1");

        // Act
        await Sut.RemovePermissionAsync(role.Id, "permission1");

        // Assert
        var result = await Sut.HasPermissionAsync(role.Id, "permission1");
        result.ShouldBeFalse();
    }

    [Test]
    public async Task RemovePermissions_WhenPermissionsExist_ShouldDeleteAll()
    {
        // Arrange
        var tenant = await CreateTenantAsync();
        var role = RoleBuilder.New().WithTenantId(tenant.Id).Build();
        await Sut.CreateAsync(role);
        var permissions = new[] { "permission1", "permission2", "permission3" };
        await Sut.AddPermissionsAsync(role.Id, permissions);

        // Act
        await Sut.RemovePermissionsAsync(role.Id, permissions);

        // Assert
        var result = await Sut.GetPermissionsByRoleIdAsync(role.Id);
        result.ShouldBeEmpty();
    }

    [Test]
    public async Task GetPermissionsByRoleId_WhenPermissionsExist_ShouldReturnAll()
    {
        // Arrange
        var tenant = await CreateTenantAsync();
        var role = RoleBuilder.New().WithTenantId(tenant.Id).Build();
        await Sut.CreateAsync(role);
        await Sut.AddPermissionAsync(role.Id, "permission1");
        await Sut.AddPermissionAsync(role.Id, "permission2");

        // Act
        var result = await Sut.GetPermissionsByRoleIdAsync(role.Id);

        // Assert
        result.Count.ShouldBe(2);
        result.ShouldContain("permission1");
        result.ShouldContain("permission2");
    }

    [Test]
    public async Task GetPermissionsByRoleId_WhenNoPermissions_ShouldReturnEmpty()
    {
        // Arrange
        var tenant = await CreateTenantAsync();
        var role = RoleBuilder.New().WithTenantId(tenant.Id).Build();
        await Sut.CreateAsync(role);

        // Act
        var result = await Sut.GetPermissionsByRoleIdAsync(role.Id);

        // Assert
        result.ShouldBeEmpty();
    }

    [Test]
    public async Task GetPermissionsByRoleId_WithMultipleRoleIds_ShouldReturnAllPermissions()
    {
        // Arrange
        var tenant = await CreateTenantAsync();
        var role1 = RoleBuilder.New().WithTenantId(tenant.Id).Build();
        var role2 = RoleBuilder.New().WithTenantId(tenant.Id).Build();
        await Sut.CreateAsync(role1);
        await Sut.CreateAsync(role2);
        await Sut.AddPermissionAsync(role1.Id, "permission1");
        await Sut.AddPermissionAsync(role1.Id, "permission2");
        await Sut.AddPermissionAsync(role2.Id, "permission3");

        // Act
        var result = await Sut.GetPermissionsByRoleIdAsync(new[] { role1.Id, role2.Id });

        // Assert
        result.Count.ShouldBe(3);
        result.ShouldContain("permission1");
        result.ShouldContain("permission2");
        result.ShouldContain("permission3");
    }

    [Test]
    public async Task HasPermission_WhenPermissionExists_ShouldReturnTrue()
    {
        // Arrange
        var tenant = await CreateTenantAsync();
        var role = RoleBuilder.New().WithTenantId(tenant.Id).Build();
        await Sut.CreateAsync(role);
        await Sut.AddPermissionAsync(role.Id, "permission1");

        // Act
        var result = await Sut.HasPermissionAsync(role.Id, "permission1");

        // Assert
        result.ShouldBeTrue();
    }

    [Test]
    public async Task HasPermission_WhenPermissionDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        var tenant = await CreateTenantAsync();
        var role = RoleBuilder.New().WithTenantId(tenant.Id).Build();
        await Sut.CreateAsync(role);

        // Act
        var result = await Sut.HasPermissionAsync(role.Id, "permission1");

        // Assert
        result.ShouldBeFalse();
    }

    [Test]
    public async Task ClearPermissions_WhenPermissionsExist_ShouldDeleteAll()
    {
        // Arrange
        var tenant = await CreateTenantAsync();
        var role = RoleBuilder.New().WithTenantId(tenant.Id).Build();
        await Sut.CreateAsync(role);
        await Sut.AddPermissionAsync(role.Id, "permission1");
        await Sut.AddPermissionAsync(role.Id, "permission2");

        // Act
        await Sut.ClearPermissionsAsync(role.Id);

        // Assert
        var result = await Sut.GetPermissionsByRoleIdAsync(role.Id);
        result.ShouldBeEmpty();
    }

    #endregion

    #region User Role Tests

    [Test]
    public async Task AddRoleToUser_WhenUserAndRoleExist_ShouldInsertOne()
    {
        // Arrange
        var tenant = await CreateTenantAsync();
        var user = await CreateUserAsync(tenant.Id);
        var role = RoleBuilder.New().WithTenantId(tenant.Id).Build();
        await Sut.CreateAsync(role);

        // Act
        await Sut.AddRoleToUserAsync(user.Id, role.Id, tenant.Id);

        // Assert
        var result = await Sut.ExistsUserRoleAsync(user.Id, role.Id);
        result.ShouldBeTrue();
    }

    [Test]
    public async Task AddRoleToUser_WhenUserDoesNotExist_ShouldThrowException()
    {
        // Arrange
        var tenant = await CreateTenantAsync();
        var role = RoleBuilder.New().WithTenantId(tenant.Id).Build();
        await Sut.CreateAsync(role);

        // Act & Assert
        await Assert.ThrowsAsync<PermissionException>(
            () => Sut.AddRoleToUserAsync(Guid.NewGuid().ToString(), role.Id, tenant.Id)
        );
    }

    [Test]
    public async Task AddRoleToUser_WhenRoleDoesNotExist_ShouldThrowException()
    {
        // Arrange
        var tenant = await CreateTenantAsync();
        var user = await CreateUserAsync(tenant.Id);

        // Act & Assert
        await Assert.ThrowsAsync<PermissionException>(
            () => Sut.AddRoleToUserAsync(user.Id, Guid.NewGuid().ToString(), tenant.Id)
        );
    }

    [Test]
    public async Task RemoveRoleFromUser_WhenUserRoleExists_ShouldDeleteSuccess()
    {
        // Arrange
        var tenant = await CreateTenantAsync();
        var user = await CreateUserAsync(tenant.Id);
        var role = RoleBuilder.New().WithTenantId(tenant.Id).Build();
        await Sut.CreateAsync(role);
        await Sut.AddRoleToUserAsync(user.Id, role.Id, tenant.Id);

        // Act
        await Sut.RemoveRoleFromUserAsync(user.Id, role.Id);

        // Assert
        var result = await Sut.ExistsUserRoleAsync(user.Id, role.Id);
        result.ShouldBeFalse();
    }

    [Test]
    public async Task FindRolesByUserId_WhenUserHasRoles_ShouldReturnAll()
    {
        // Arrange
        var tenant = await CreateTenantAsync();
        var user = await CreateUserAsync(tenant.Id);
        var role1 = RoleBuilder.New().WithTenantId(tenant.Id).WithName("Role1").Build();
        var role2 = RoleBuilder.New().WithTenantId(tenant.Id).WithName("Role2").Build();
        await Sut.CreateAsync(role1);
        await Sut.CreateAsync(role2);
        await Sut.AddRoleToUserAsync(user.Id, role1.Id, tenant.Id);
        await Sut.AddRoleToUserAsync(user.Id, role2.Id, tenant.Id);

        // Act
        var result = await Sut.FindRolesByUserIdAsync(tenant.Id, user.Id);

        // Assert
        result.Count.ShouldBe(2);
        result.Select(r => r.Id).ShouldContain(role1.Id);
        result.Select(r => r.Id).ShouldContain(role2.Id);
    }

    [Test]
    public async Task FindRolesByUserId_WhenUserHasNoRoles_ShouldReturnEmpty()
    {
        // Arrange
        var tenant = await CreateTenantAsync();
        var user = await CreateUserAsync(tenant.Id);

        // Act
        var result = await Sut.FindRolesByUserIdAsync(tenant.Id, user.Id);

        // Assert
        result.ShouldBeEmpty();
    }

    [Test]
    public async Task GetAllUserRoles_WhenInitialLoad_ShouldReturnCorrectCountAndOrder()
    {
        // Arrange
        var tenant = await CreateTenantAsync();
        var user = await CreateUserAsync(tenant.Id);
        var builder = RoleBuilder.New().WithTenantId(tenant.Id);
        var roles = new List<Role>
        {
            builder.WithId("003").Build(),
            builder.WithId("002").Build(),
            builder.WithId("001").Build(),
        };

        foreach (var role in roles)
        {
            await Sut.CreateAsync(role);
            await Sut.AddRoleToUserAsync(user.Id, role.Id, tenant.Id);
        }

        // Act
        var result = await Sut.GetAllUserRolesAsync(tenant.Id, user.Id, cursor: null, count: 2);

        // Assert
        result.Count.ShouldBe(2);
        result.Select(r => r.Id).ShouldBe(["003", "002"]);
    }

    [Test]
    public async Task GetAllUserRoles_WhenForwardPagination_ShouldReturnNextPage()
    {
        // Arrange
        var tenant = await CreateTenantAsync();
        var user = await CreateUserAsync(tenant.Id);
        var builder = RoleBuilder.New().WithTenantId(tenant.Id);
        var roles = Enumerable
            .Range(1, 5)
            .Select(i => builder.WithId(i.ToString().PadLeft(3, '0')).Build())
            .ToList();

        foreach (var role in roles)
        {
            await Sut.CreateAsync(role);
            await Sut.AddRoleToUserAsync(user.Id, role.Id, tenant.Id);
        }

        // Act
        var result = await Sut.GetAllUserRolesAsync(tenant.Id, user.Id, cursor: "004", count: 2);

        // Assert
        result.Count.ShouldBe(2);
        result.Select(r => r.Id).ShouldBe(["003", "002"]);
    }

    [Test]
    public async Task GetAllUserRoles_WhenUserHasNoRoles_ShouldReturnEmpty()
    {
        // Arrange
        var tenant = await CreateTenantAsync();
        var user = await CreateUserAsync(tenant.Id);

        // Act
        var result = await Sut.GetAllUserRolesAsync(tenant.Id, user.Id, cursor: null, count: 2);

        // Assert
        result.ShouldBeEmpty();
    }

    [Test]
    public async Task ExistsUserRole_WhenUserRoleExists_ShouldReturnTrue()
    {
        // Arrange
        var tenant = await CreateTenantAsync();
        var user = await CreateUserAsync(tenant.Id);
        var role = RoleBuilder.New().WithTenantId(tenant.Id).Build();
        await Sut.CreateAsync(role);
        await Sut.AddRoleToUserAsync(user.Id, role.Id, tenant.Id);

        // Act
        var result = await Sut.ExistsUserRoleAsync(user.Id, role.Id);

        // Assert
        result.ShouldBeTrue();
    }

    [Test]
    public async Task ExistsUserRole_WhenUserRoleDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        var tenant = await CreateTenantAsync();
        var user = await CreateUserAsync(tenant.Id);
        var role = RoleBuilder.New().WithTenantId(tenant.Id).Build();
        await Sut.CreateAsync(role);

        // Act
        var result = await Sut.ExistsUserRoleAsync(user.Id, role.Id);

        // Assert
        result.ShouldBeFalse();
    }

    #endregion

    #region Helper Methods

    private async Task<Tenant> CreateTenantAsync()
    {
        var tenant = TenantBuilder.New().Build();
        await TenantRepository.CreateAsync(tenant);
        return tenant;
    }

    private async Task<User> CreateUserAsync(string tenantId)
    {
        var user = new User(
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString()
        );
        await UserRepository.CreateAsync(user);
        await TenantRepository.AddUserToTenantAsync(tenantId, user.Id);
        return user;
    }

    #endregion
}
