using FastQuiz.Auth.Domain.Entities;
using FastQuiz.Auth.Domain.Repositories;
using FastQuiz.Auth.Test.Infrastructure.Repositories.Utils;
using FastQuiz.Common.Exceptions;
using Shouldly;

namespace FastQuiz.Auth.Test.Infrastructure.Repositories;

[TestFixture]
internal abstract class TenantRepositoryTestBase
{
    protected abstract ITenantRepository Sut { get; }
    protected abstract IUserRepository UserRepository { get; }

    [Test]
    public async Task Create_WhenTenantDoesNotExist_ShouldInsertOne()
    {
        // Arrange
        var tenant = TenantBuilder.New().Build();

        // Act
        await Sut.CreateAsync(tenant);

        // Assert
        var result = await Sut.FindByIdAsync(tenant.Id);
        result.ShouldNotBeNull();
        result.Id.ShouldBe(tenant.Id);
    }

    [Test]
    public async Task Create_WhenTenantDoesAlreadyExist_ShouldInsertFail()
    {
        // Arrange
        var tenant = TenantBuilder.New().Build();
        await Sut.CreateAsync(tenant);

        // Act & Assert
        Should.Throw<DataAccessException>(() => Sut.CreateAsync(tenant));
    }

    [Test]
    public async Task FindById_WhenTenantDoesExist_ShouldReturnOne()
    {
        // Arrange
        var tenant = TenantBuilder.New().Build();
        await Sut.CreateAsync(tenant);

        // Act
        var result = await Sut.FindByIdAsync(tenant.Id);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(tenant.Id);
    }

    [Test]
    public async Task FindById_WhenTenantDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var tenant = TenantBuilder.New().Build();

        // Act
        var result = await Sut.FindByIdAsync(tenant.Id);

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public async Task GetAll_WhenInitialLoad_ShouldReturnCorrectCountAndOrder()
    {
        // Arrange:
        var builder = TenantBuilder.New();
        List<Tenant> tenants = [builder.Build(), builder.Build(), builder.Build()];

        foreach (var t in tenants)
            await Sut.CreateAsync(t);

        // Act:
        var result = await Sut.GetAllAsync(cursor: null, count: 2, isPrevious: false);

        // Assert:
        result.Count.ShouldBe(2);
        result.All(r => tenants.Select(g => g.Id).Contains(r.Id)).ShouldBeTrue();
    }

    [Test]
    public async Task GetAll_WhenForwardPagination_ShouldReturnNextPage()
    {
        // Arrange
        var builder = TenantBuilder.New();
        var tenants = Enumerable
            .Range(1, 5)
            .Select(i => builder.WithId(i.ToString().PadLeft(3, '0')).Build())
            .ToList();

        foreach (var t in tenants)
            await Sut.CreateAsync(t);

        // Act:
        var result = await Sut.GetAllAsync(cursor: "004", count: 2, isPrevious: false);

        // Assert
        result.Count.ShouldBe(2);
        var inputIds = tenants.Select(g => g.Id).ToHashSet();
        result.All(r => inputIds.Contains(r.Id)).ShouldBeTrue();

        result.All(r => string.Compare(r.Id, "004", StringComparison.Ordinal) < 0).ShouldBeTrue();

        var ids = result.Select(r => r.Id).ToList();
        ids[0].ShouldBe("003");
        ids[1].ShouldBe("002");
    }

    [Test]
    public async Task GetAll_WhenPreviousPagination_ShouldReturnPreviousPageInCorrectOrder()
    {
        // Arrange
        var builder = TenantBuilder.New();
        var tenants = Enumerable
            .Range(1, 5)
            .Select(i => builder.WithId(i.ToString().PadLeft(3, '0')).Build())
            .ToList();

        foreach (var t in tenants)
            await Sut.CreateAsync(t);

        // Act:
        var cursor = "002";
        var count = 2;
        var result = await Sut.GetAllAsync(cursor: cursor, count: count, isPrevious: true);

        // Assert:
        result.Count.ShouldBe(count);

        var expectedIds = tenants
            .Where(t => string.Compare(t.Id, cursor, StringComparison.Ordinal) > 0)
            .OrderBy(t => t.Id)
            .Take(count)
            .Select(t => t.Id)
            .ToList();

        expectedIds.Reverse();

        result.Select(r => r.Id).ShouldBe(expectedIds);
    }

    [Test]
    public async Task GetMyTenants_WhenInitialLoad_ShouldReturnCorrectCountAndOrder()
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
        var builder = TenantBuilder.New();
        List<Tenant> tenants = [builder.Build(), builder.Build(), builder.Build()];

        foreach (var t in tenants)
        {
            await Sut.CreateAsync(t);
            await Sut.AddUserToTenantAsync(t.Id, userId);
        }

        // Act:
        var result = await Sut.GetAllMyTenantAsync(userId, cursor: null, count: 2);

        // Assert:
        result.Count.ShouldBe(2);
        result.All(r => tenants.Select(g => g.Id).Contains(r.Id)).ShouldBeTrue();
    }

    [Test]
    public async Task GetMyTenants_WhenTenantDoesNotExist_ShouldReturnEmpty()
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

        // Act:
        var result = await Sut.GetAllMyTenantAsync(userId, cursor: null, count: 2);

        // Assert:
        result.Count.ShouldBe(0);
    }

    [Test]
    public async Task UpdateTenant_WhenTenantDoesAlreadyExist_ShouldUpdateSuccess()
    {
        // Arrange
        var tenant = TenantBuilder.New().Build();
        await Sut.CreateAsync(tenant);
        tenant.Name = Guid.NewGuid().ToString();
        tenant.Description = Guid.NewGuid().ToString();
        tenant.UpdatedAt = DateTimeOffset.MaxValue;

        // Act
        await Sut.UpdateAsync(tenant);

        // Assert
        var result = await Sut.FindByIdAsync(tenant.Id);
        result.ShouldNotBeNull();
        result.Id.ShouldBe(tenant.Id);
        result.Name.ShouldBe(tenant.Name);
        result.Description.ShouldBe(tenant.Description);
        result.UpdatedAt.ShouldBe(tenant.UpdatedAt, TimeSpan.FromSeconds(1));
    }

    [Test]
    public async Task DeleteTenant_WhenTenantDoesAlreadyExist_ShouldDeleteSuccess()
    {
        // Arrange
        var tenant = TenantBuilder.New().Build();
        await Sut.CreateAsync(tenant);

        // Act
        await Sut.DeleteAsync(tenant.Id);

        // Assert
        var result = await Sut.FindByIdAsync(tenant.Id);
        result.ShouldBeNull();
    }

    [Test]
    public async Task AddUserToTenant_WhenUserDoesAlreadyExist_ShouldInsertOne()
    {
        // Arrange
        var user = new Domain.Entities.User(
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString()
        );
        await UserRepository.CreateAsync(user);
        var tenant = TenantBuilder.New().Build();
        await Sut.CreateAsync(tenant);

        // Act & Assert
        await Sut.AddUserToTenantAsync(tenant.Id, user.Id);

        // Assert
        var result = await Sut.GetUserInTenantAsync(tenant.Id, user.Id);
        result.ShouldBe(true);
    }

    [Test]
    public async Task AddUserToTenant_WhenUserDoesNotExist_ShouldThrowException()
    {
        // Arrange
        string userId = Guid.NewGuid().ToString();
        var tenant = TenantBuilder.New().Build();
        await Sut.CreateAsync(tenant);

        // Act & Assert
        Assert.ThrowsAsync<DataAccessException>(() => Sut.AddUserToTenantAsync(tenant.Id, userId));
    }

    [Test]
    public async Task AddUserToTenant_WhenTenantDoesNotExist_ShouldThrowException()
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

        // Act & Assert
        Assert.ThrowsAsync<DataAccessException>(() => Sut.AddUserToTenantAsync(tenant.Id, userId));
    }

    [Test]
    public async Task GetUserInTenant_WhenUserInTenant_ShouldReturnTrue()
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
        await Sut.CreateAsync(tenant);
        await Sut.AddUserToTenantAsync(tenant.Id, userId);

        // Act
        var result = await Sut.GetUserInTenantAsync(tenant.Id, userId);

        // Assert
        result.ShouldBe(true);
    }

    [Test]
    public async Task GetUserInTenant_WhenUserNotInTenant_ShouldReturnTrue()
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
        await Sut.CreateAsync(tenant);

        // Act
        var result = await Sut.GetUserInTenantAsync(tenant.Id, userId);

        // Assert
        result.ShouldBe(false);
    }

    [Test]
    public async Task RemoveUserFromTenant_WhenUserInTenant_ShouldDeleteSuccess()
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
        await Sut.CreateAsync(tenant);
        await Sut.AddUserToTenantAsync(tenant.Id, userId);

        // Act
        await Sut.RemoveUserFromTenantAsync(tenant.Id, userId);

        // Assert
        var result = await Sut.GetUserInTenantAsync(tenant.Id, userId);
        result.ShouldBe(false);
    }

    [Test]
    public async Task GetUserIdsByTenantId_WhenUserDoesAlreadyExist_ShouldReturnIdList()
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
        await Sut.CreateAsync(tenant);
        await Sut.AddUserToTenantAsync(tenant.Id, userId1);
        await Sut.AddUserToTenantAsync(tenant.Id, userId2);

        // Act
        var result = await Sut.GetUserIdsByTenantIdAsync(tenant.Id);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);
        result.ShouldBe([userId1, userId2], true);
    }

    [Test]
    public async Task GetUserIdsByTenantId_WhenUserDoesNotExist_ShouldReturnEmpty()
    {
        // Arrange
        var tenant = TenantBuilder.New().Build();
        await Sut.CreateAsync(tenant);

        // Act
        var result = await Sut.GetUserIdsByTenantIdAsync(tenant.Id);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(0);
    }
}
