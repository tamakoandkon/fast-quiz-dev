using FastQuiz.Auth.Domain.Entities;
using FastQuiz.Auth.Domain.Repositories;
using FastQuiz.Auth.Test.Infrastructure.Repositories.Utils;
using FastQuiz.Common.Exceptions;
using Shouldly;

namespace FastQuiz.Auth.Test.Infrastructure.Repositories;

[TestFixture]
internal abstract class OrganizationRepositoryTestBase
{
    protected abstract IOrganizationRepository Sut { get; }
    protected abstract ITenantRepository TenantRepository { get; }
    protected abstract IUserRepository UserRepository { get; }

    [Test]
    public async Task Create_WhenOrganizationDoesNotExist_ShouldInsertOne()
    {
        // Arrange
        var tenant = TenantBuilder.New().Build();
        await TenantRepository.CreateAsync(tenant);
        var organization = OrganizationBuilder.New().WithTenantId(tenant.Id).Build();

        // Act
        await Sut.CreateAsync(organization);

        // Assert
        var result = await Sut.FindByIdAsync(organization.Id);
        result.ShouldNotBeNull();
        result.Id.ShouldBe(organization.Id);
        result.TenantId.ShouldBe(tenant.Id);
    }

    [Test]
    public async Task Create_WhenOrganizationDoesAlreadyExist_ShouldInsertFail()
    {
        // Arrange
        var tenant = TenantBuilder.New().Build();
        await TenantRepository.CreateAsync(tenant);
        var organization = OrganizationBuilder.New().WithTenantId(tenant.Id).Build();
        await Sut.CreateAsync(organization);

        // Act & Assert
        Should.Throw<DataAccessException>(() => Sut.CreateAsync(organization));
    }

    [Test]
    public async Task FindById_WhenOrganizationDoesExist_ShouldReturnOne()
    {
        // Arrange
        var tenant = TenantBuilder.New().Build();
        await TenantRepository.CreateAsync(tenant);
        var organization = OrganizationBuilder.New().WithTenantId(tenant.Id).Build();
        await Sut.CreateAsync(organization);

        // Act
        var result = await Sut.FindByIdAsync(organization.Id);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(organization.Id);
    }

    [Test]
    public async Task FindById_WhenOrganizationDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var organization = OrganizationBuilder.New().Build();

        // Act
        var result = await Sut.FindByIdAsync(organization.Id);

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public async Task GetAllByTenantId_WhenInitialLoad_ShouldReturnCorrectCountAndOrder()
    {
        // Arrange
        var tenant = TenantBuilder.New().Build();
        await TenantRepository.CreateAsync(tenant);
        var builder = OrganizationBuilder.New().WithTenantId(tenant.Id);
        List<Organization> organizations =
        [
            builder.WithId("003").Build(),
            builder.WithId("002").Build(),
            builder.WithId("001").Build(),
        ];

        foreach (var o in organizations)
            await Sut.CreateAsync(o);

        // Act
        var result = await Sut.GetAllByTenantIdAsync(
            tenant.Id,
            cursor: null,
            count: 2,
            isPrevious: false
        );

        // Assert
        result.Count.ShouldBe(2);
        result.All(r => organizations.Select(g => g.Id).Contains(r.Id)).ShouldBeTrue();
        result[0].Id.ShouldBe("003");
        result[1].Id.ShouldBe("002");
    }

    [Test]
    public async Task GetAllByTenantId_WhenForwardPagination_ShouldReturnNextPage()
    {
        // Arrange
        var tenant = TenantBuilder.New().Build();
        await TenantRepository.CreateAsync(tenant);
        var builder = OrganizationBuilder.New().WithTenantId(tenant.Id);
        var organizations = Enumerable
            .Range(1, 5)
            .Select(i => builder.WithId(i.ToString().PadLeft(3, '0')).Build())
            .ToList();

        foreach (var o in organizations)
            await Sut.CreateAsync(o);

        // Act
        var result = await Sut.GetAllByTenantIdAsync(
            tenant.Id,
            cursor: "004",
            count: 2,
            isPrevious: false
        );

        // Assert
        result.Count.ShouldBe(2);
        var inputIds = organizations.Select(g => g.Id).ToHashSet();
        result.All(r => inputIds.Contains(r.Id)).ShouldBeTrue();
        result.All(r => string.Compare(r.Id, "004", StringComparison.Ordinal) < 0).ShouldBeTrue();
        result[0].Id.ShouldBe("003");
        result[1].Id.ShouldBe("002");
    }

    [Test]
    public async Task GetAllByTenantId_WhenPreviousPagination_ShouldReturnPreviousPageInCorrectOrder()
    {
        // Arrange
        var tenant = TenantBuilder.New().Build();
        await TenantRepository.CreateAsync(tenant);
        var builder = OrganizationBuilder.New().WithTenantId(tenant.Id);
        var organizations = Enumerable
            .Range(1, 5)
            .Select(i => builder.WithId(i.ToString().PadLeft(3, '0')).Build())
            .ToList();

        foreach (var o in organizations)
            await Sut.CreateAsync(o);

        // Act
        var cursor = "002";
        var count = 2;
        var result = await Sut.GetAllByTenantIdAsync(
            tenant.Id,
            cursor: cursor,
            count: count,
            isPrevious: true
        );

        // Assert
        result.Count.ShouldBe(count);
        var expectedIds = organizations
            .Where(o => string.Compare(o.Id, cursor, StringComparison.Ordinal) > 0)
            .OrderBy(o => o.Id)
            .Take(count)
            .Select(o => o.Id)
            .ToList();
        expectedIds.Reverse();
        result.Select(r => r.Id).ShouldBe(expectedIds);
    }

    [Test]
    public async Task GetAllByTenantId_ShouldOnlyReturnRootOrganizations()
    {
        // Arrange
        var tenant = TenantBuilder.New().Build();
        await TenantRepository.CreateAsync(tenant);
        var builder = OrganizationBuilder.New().WithTenantId(tenant.Id);
        var rootOrg = builder.WithId("001").Build();
        var childOrg = builder.WithId("002").WithParentId("001").Build();
        await Sut.CreateAsync(rootOrg);
        await Sut.CreateAsync(childOrg);

        // Act
        var result = await Sut.GetAllByTenantIdAsync(
            tenant.Id,
            cursor: null,
            count: 10,
            isPrevious: false
        );

        // Assert
        result.Count.ShouldBe(1);
        result[0].Id.ShouldBe("001");
    }

    [Test]
    public async Task GetByParentId_WhenInitialLoad_ShouldReturnCorrectCountAndOrder()
    {
        // Arrange
        var tenant = TenantBuilder.New().Build();
        await TenantRepository.CreateAsync(tenant);
        var builder = OrganizationBuilder.New().WithTenantId(tenant.Id);
        var parentOrg = builder.WithId("001").Build();
        await Sut.CreateAsync(parentOrg);
        List<Organization> childOrgs =
        [
            builder.WithId("0003").WithParentId("001").Build(),
            builder.WithId("0002").WithParentId("001").Build(),
            builder.WithId("0001").WithParentId("001").Build(),
        ];

        foreach (var o in childOrgs)
            await Sut.CreateAsync(o);

        // Act
        var result = await Sut.GetByParentIdAsync("001", cursor: null, count: 2, isPrevious: false);

        // Assert
        result.Count.ShouldBe(2);
        result.All(r => childOrgs.Select(g => g.Id).Contains(r.Id)).ShouldBeTrue();
        result[0].Id.ShouldBe("0003");
        result[1].Id.ShouldBe("0002");
    }

    [Test]
    public async Task GetByParentId_WhenForwardPagination_ShouldReturnNextPage()
    {
        // Arrange
        var tenant = TenantBuilder.New().Build();
        await TenantRepository.CreateAsync(tenant);
        var builder = OrganizationBuilder.New().WithTenantId(tenant.Id);
        var parentOrg = builder.WithId("000").Build();
        await Sut.CreateAsync(parentOrg);
        var childOrgs = Enumerable
            .Range(1, 5)
            .Select(i => builder.WithId(i.ToString().PadLeft(3, '0')).WithParentId("000").Build())
            .ToList();

        foreach (var o in childOrgs)
            await Sut.CreateAsync(o);

        // Act
        var result = await Sut.GetByParentIdAsync(
            "000",
            cursor: "004",
            count: 2,
            isPrevious: false
        );

        // Assert
        result.Count.ShouldBe(2);
        var inputIds = childOrgs.Select(g => g.Id).ToHashSet();
        result.All(r => inputIds.Contains(r.Id)).ShouldBeTrue();
        result.All(r => string.Compare(r.Id, "004", StringComparison.Ordinal) < 0).ShouldBeTrue();
        result[0].Id.ShouldBe("003");
        result[1].Id.ShouldBe("002");
    }

    [Test]
    public async Task GetByParentId_WhenPreviousPagination_ShouldReturnPreviousPageInCorrectOrder()
    {
        // Arrange
        var tenant = TenantBuilder.New().Build();
        await TenantRepository.CreateAsync(tenant);
        var builder = OrganizationBuilder.New().WithTenantId(tenant.Id);
        var parentOrg = builder.WithId("000").Build();
        await Sut.CreateAsync(parentOrg);
        var childOrgs = Enumerable
            .Range(1, 5)
            .Select(i =>
                builder
                    .WithId(i.ToString().PadLeft(3, '0'))
                    .WithTenantId(tenant.Id)
                    .WithParentId("000")
                    .Build()
            )
            .ToList();

        foreach (var o in childOrgs)
            await Sut.CreateAsync(o);

        // Act
        var cursor = "002";
        var count = 2;
        var result = await Sut.GetByParentIdAsync(
            "000",
            cursor: cursor,
            count: count,
            isPrevious: true
        );

        // Assert
        result.Count.ShouldBe(count);
        var expectedIds = childOrgs
            .Where(o => string.Compare(o.Id, cursor, StringComparison.Ordinal) > 0)
            .OrderBy(o => o.Id)
            .Take(count)
            .Select(o => o.Id)
            .ToList();
        expectedIds.Reverse();
        result.Select(r => r.Id).ShouldBe(expectedIds);
    }

    [Test]
    public async Task UpdateOrganization_WhenOrganizationDoesAlreadyExist_ShouldUpdateSuccess()
    {
        // Arrange
        var tenant = TenantBuilder.New().Build();
        await TenantRepository.CreateAsync(tenant);
        var organization = OrganizationBuilder.New().WithTenantId(tenant.Id).Build();
        await Sut.CreateAsync(organization);
        organization.Name = Guid.NewGuid().ToString();
        organization.UpdatedAt = DateTimeOffset.MaxValue;

        // Act
        await Sut.UpdateAsync(organization);

        // Assert
        var result = await Sut.FindByIdAsync(organization.Id);
        result.ShouldNotBeNull();
        result.Id.ShouldBe(organization.Id);
        result.Name.ShouldBe(organization.Name);
        result.UpdatedAt.ShouldBe(organization.UpdatedAt, TimeSpan.FromSeconds(1));
    }

    [Test]
    public async Task DeleteOrganization_WhenOrganizationDoesAlreadyExist_ShouldDeleteSuccess()
    {
        // Arrange
        var tenant = TenantBuilder.New().Build();
        await TenantRepository.CreateAsync(tenant);
        var organization = OrganizationBuilder.New().WithTenantId(tenant.Id).Build();
        await Sut.CreateAsync(organization);

        // Act
        await Sut.DeleteAsync(organization.Id);

        // Assert
        var result = await Sut.FindByIdAsync(organization.Id);
        result.ShouldBeNull();
    }

    [Test]
    public async Task AddUserToOrganization_WhenUserDoesAlreadyExist_ShouldInsertOne()
    {
        // Arrange
        var user = new User(
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString()
        );
        await UserRepository.CreateAsync(user);
        var tenant = TenantBuilder.New().Build();
        await TenantRepository.CreateAsync(tenant);
        var organization = OrganizationBuilder.New().WithTenantId(tenant.Id).Build();
        await Sut.CreateAsync(organization);

        // Act
        await Sut.AddUserToOrganizationAsync(organization.Id, user.Id);

        // Assert
        var result = await Sut.GetUserInOrganizationAsync(organization.Id, user.Id);
        result.ShouldBe(true);
    }

    [Test]
    public async Task AddUserToOrganization_WhenUserDoesNotExist_ShouldThrowException()
    {
        // Arrange
        string userId = Guid.NewGuid().ToString();
        var tenant = TenantBuilder.New().Build();
        await TenantRepository.CreateAsync(tenant);
        var organization = OrganizationBuilder.New().WithTenantId(tenant.Id).Build();
        await Sut.CreateAsync(organization);

        // Act & Assert
        await Assert.ThrowsAsync<DataAccessException>(
            () => Sut.AddUserToOrganizationAsync(organization.Id, userId)
        );
    }

    [Test]
    public async Task AddUserToOrganization_WhenOrganizationDoesNotExist_ShouldThrowException()
    {
        // Arrange
        string userId = Guid.NewGuid().ToString();
        await UserRepository.CreateAsync(
            new User(
                userId,
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString()
            )
        );
        var organization = OrganizationBuilder.New().Build();

        // Act & Assert
        await Assert.ThrowsAsync<DataAccessException>(
            () => Sut.AddUserToOrganizationAsync(organization.Id, userId)
        );
    }

    [Test]
    public async Task GetUserInOrganization_WhenUserInOrganization_ShouldReturnTrue()
    {
        // Arrange
        string userId = Guid.NewGuid().ToString();
        await UserRepository.CreateAsync(
            new User(
                userId,
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString()
            )
        );
        var tenant = TenantBuilder.New().Build();
        await TenantRepository.CreateAsync(tenant);
        var organization = OrganizationBuilder.New().WithTenantId(tenant.Id).Build();
        await Sut.CreateAsync(organization);
        await Sut.AddUserToOrganizationAsync(organization.Id, userId);

        // Act
        var result = await Sut.GetUserInOrganizationAsync(organization.Id, userId);

        // Assert
        result.ShouldBe(true);
    }

    [Test]
    public async Task GetUserInOrganization_WhenUserNotInOrganization_ShouldReturnFalse()
    {
        // Arrange
        string userId = Guid.NewGuid().ToString();
        await UserRepository.CreateAsync(
            new User(
                userId,
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString()
            )
        );
        var tenant = TenantBuilder.New().Build();
        await TenantRepository.CreateAsync(tenant);
        var organization = OrganizationBuilder.New().WithTenantId(tenant.Id).Build();
        await Sut.CreateAsync(organization);

        // Act
        var result = await Sut.GetUserInOrganizationAsync(organization.Id, userId);

        // Assert
        result.ShouldBe(false);
    }

    [Test]
    public async Task RemoveUserFromOrganization_WhenUserInOrganization_ShouldDeleteSuccess()
    {
        // Arrange
        string userId = Guid.NewGuid().ToString();
        await UserRepository.CreateAsync(
            new User(
                userId,
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString()
            )
        );
        var tenant = TenantBuilder.New().Build();
        await TenantRepository.CreateAsync(tenant);
        var organization = OrganizationBuilder.New().WithTenantId(tenant.Id).Build();
        await Sut.CreateAsync(organization);
        await Sut.AddUserToOrganizationAsync(organization.Id, userId);

        // Act
        await Sut.RemoveUserFromOrganizationAsync(organization.Id, userId);

        // Assert
        var result = await Sut.GetUserInOrganizationAsync(organization.Id, userId);
        result.ShouldBe(false);
    }

    [Test]
    public async Task GetUserIdsByOrganizationId_WhenUserDoesAlreadyExist_ShouldReturnIdList()
    {
        // Arrange
        string userId1 = Guid.NewGuid().ToString();
        await UserRepository.CreateAsync(
            new User(
                userId1,
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString()
            )
        );
        string userId2 = Guid.NewGuid().ToString();
        await UserRepository.CreateAsync(
            new User(
                userId2,
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString()
            )
        );
        var tenant = TenantBuilder.New().Build();
        await TenantRepository.CreateAsync(tenant);
        var organization = OrganizationBuilder.New().WithTenantId(tenant.Id).Build();
        await Sut.CreateAsync(organization);
        await Sut.AddUserToOrganizationAsync(organization.Id, userId1);
        await Sut.AddUserToOrganizationAsync(organization.Id, userId2);

        // Act
        var result = await Sut.GetUserIdsByOrganizationIdAsync(organization.Id);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);
        result.ShouldBe([userId1, userId2], true);
    }

    [Test]
    public async Task GetUserIdsByOrganizationId_WhenUserDoesNotExist_ShouldReturnEmpty()
    {
        // Arrange
        var tenant = TenantBuilder.New().Build();
        await TenantRepository.CreateAsync(tenant);
        var organization = OrganizationBuilder.New().WithTenantId(tenant.Id).Build();
        await Sut.CreateAsync(organization);

        // Act
        var result = await Sut.GetUserIdsByOrganizationIdAsync(organization.Id);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(0);
    }

    [Test]
    public async Task GetUserIdsNotInOrganization_WhenUsersExist_ShouldReturnUsersNotInOrganization()
    {
        // Arrange
        string userId1 = Guid.NewGuid().ToString();
        await UserRepository.CreateAsync(
            new User(
                userId1,
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString()
            )
        );
        string userId2 = Guid.NewGuid().ToString();
        await UserRepository.CreateAsync(
            new User(
                userId2,
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString()
            )
        );
        var tenant = TenantBuilder.New().Build();
        await TenantRepository.CreateAsync(tenant);
        await TenantRepository.AddUserToTenantAsync(tenant.Id, userId1);
        await TenantRepository.AddUserToTenantAsync(tenant.Id, userId2);

        var organization = OrganizationBuilder.New().WithTenantId(tenant.Id).Build();
        await Sut.CreateAsync(organization);
        await Sut.AddUserToOrganizationAsync(organization.Id, userId1);

        // Act
        var result = await Sut.GetUserIdsNotInOrganizationAsync(tenant.Id, organization.Id);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);
        result[0].ShouldBe(userId2);
    }

    [Test]
    public async Task GetUserIdsNotInOrganization_WhenAllUsersInOrganization_ShouldReturnEmpty()
    {
        // Arrange
        string userId1 = Guid.NewGuid().ToString();
        await UserRepository.CreateAsync(
            new User(
                userId1,
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString()
            )
        );
        var tenant = TenantBuilder.New().Build();
        await TenantRepository.CreateAsync(tenant);
        await TenantRepository.AddUserToTenantAsync(tenant.Id, userId1);

        var organization = OrganizationBuilder.New().WithTenantId(tenant.Id).Build();
        await Sut.CreateAsync(organization);
        await Sut.AddUserToOrganizationAsync(organization.Id, userId1);

        // Act
        var result = await Sut.GetUserIdsNotInOrganizationAsync(tenant.Id, organization.Id);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(0);
    }
}
