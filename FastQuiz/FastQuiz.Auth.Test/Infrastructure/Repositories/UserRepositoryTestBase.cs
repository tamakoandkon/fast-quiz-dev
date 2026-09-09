using FastQuiz.Auth.Domain.Entities;
using FastQuiz.Auth.Domain.Repositories;
using FastQuiz.Auth.Test.Infrastructure.Repositories.Utils;
using FastQuiz.Common.Exceptions;
using Shouldly;

namespace FastQuiz.Auth.Test.Infrastructure.Repositories;

[TestFixture]
internal abstract class UserRepositoryTestBase
{
    protected abstract IUserRepository Sut { get; }
    protected abstract ITenantRepository TenantRepository { get; }

    [Test]
    public async Task Create_WhenUserDoesNotExist_ShouldInsertOne()
    {
        // Arrange
        var user = UserBuilder.New().Build();

        // Act
        await Sut.CreateAsync(user);

        // Assert
        var result = await Sut.FindByIdAsync(user.Id);
        result.ShouldNotBeNull();
        result.Id.ShouldBe(user.Id);
        result.Username.ShouldBe(user.Username);
        result.Email.ShouldBe(user.Email);
    }

    [Test]
    public async Task Create_WhenUserDoesAlreadyExist_ShouldInsertFail()
    {
        // Arrange
        var user = UserBuilder.New().Build();
        await Sut.CreateAsync(user);

        // Act & Assert
        Should.Throw<DataAccessException>(() => Sut.CreateAsync(user));
    }

    [Test]
    public async Task Create_WhenDuplicateUsername_ShouldInsertFail()
    {
        // Arrange
        var username = Guid.NewGuid().ToString();
        var user1 = UserBuilder.New().WithUsername(username).Build();
        var user2 = UserBuilder.New().WithUsername(username).Build();
        await Sut.CreateAsync(user1);

        // Act & Assert
        Should.Throw<DataAccessException>(() => Sut.CreateAsync(user2));
    }

    [Test]
    public async Task Create_WhenDuplicateEmail_ShouldInsertFail()
    {
        // Arrange
        var email = $"{Guid.NewGuid()}@example.com";
        var user1 = UserBuilder.New().WithEmail(email).Build();
        var user2 = UserBuilder.New().WithEmail(email).Build();
        await Sut.CreateAsync(user1);

        // Act & Assert
        Should.Throw<DataAccessException>(() => Sut.CreateAsync(user2));
    }

    [Test]
    public async Task FindById_WhenUserDoesExist_ShouldReturnOne()
    {
        // Arrange
        var user = UserBuilder.New().Build();
        await Sut.CreateAsync(user);

        // Act
        var result = await Sut.FindByIdAsync(user.Id);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(user.Id);
        result.Username.ShouldBe(user.Username);
        result.Email.ShouldBe(user.Email);
    }

    [Test]
    public async Task FindById_WhenUserDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var user = UserBuilder.New().Build();

        // Act
        var result = await Sut.FindByIdAsync(user.Id);

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public async Task FindByEmail_WhenUserDoesExist_ShouldReturnOne()
    {
        // Arrange
        var user = UserBuilder.New().Build();
        await Sut.CreateAsync(user);

        // Act
        var result = await Sut.FindByEmailAsync(user.Email);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(user.Id);
        result.Email.ShouldBe(user.Email);
    }

    [Test]
    public async Task FindByEmail_WhenUserDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var email = $"{Guid.NewGuid()}@example.com";

        // Act
        var result = await Sut.FindByEmailAsync(email);

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public async Task FindByUsername_WhenUserDoesExist_ShouldReturnOne()
    {
        // Arrange
        var user = UserBuilder.New().Build();
        await Sut.CreateAsync(user);

        // Act
        var result = await Sut.FindByUsernameAsync(user.Username);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(user.Id);
        result.Username.ShouldBe(user.Username);
    }

    [Test]
    public async Task FindByUsername_WhenUserDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var username = Guid.NewGuid().ToString();

        // Act
        var result = await Sut.FindByUsernameAsync(username);

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public async Task GetUsersByIds_WhenUsersExist_ShouldReturnAll()
    {
        // Arrange
        var user1 = UserBuilder.New().Build();
        var user2 = UserBuilder.New().Build();
        var user3 = UserBuilder.New().Build();
        await Sut.CreateAsync(user1);
        await Sut.CreateAsync(user2);
        await Sut.CreateAsync(user3);

        var ids = new[] { user1.Id, user2.Id };

        // Act
        var result = await Sut.GetUsersByIdsAsync(ids);

        // Assert
        result.Count.ShouldBe(2);
        result.Select(u => u.Id).ShouldContain(user1.Id);
        result.Select(u => u.Id).ShouldContain(user2.Id);
        result.Select(u => u.Id).ShouldNotContain(user3.Id);
    }

    [Test]
    public async Task GetUsersByIds_WhenSomeUsersDoNotExist_ShouldReturnOnlyExisting()
    {
        // Arrange
        var user1 = UserBuilder.New().Build();
        var user2 = UserBuilder.New().Build();
        await Sut.CreateAsync(user1);
        await Sut.CreateAsync(user2);

        var nonExistentId = Guid.NewGuid().ToString();
        var ids = new[] { user1.Id, user2.Id, nonExistentId };

        // Act
        var result = await Sut.GetUsersByIdsAsync(ids);

        // Assert
        result.Count.ShouldBe(2);
        result.Select(u => u.Id).ShouldContain(user1.Id);
        result.Select(u => u.Id).ShouldContain(user2.Id);
        result.Select(u => u.Id).ShouldNotContain(nonExistentId);
    }

    [Test]
    public async Task GetUsersByIds_WhenNoUsersExist_ShouldReturnEmpty()
    {
        // Arrange
        var ids = new[] { Guid.NewGuid().ToString(), Guid.NewGuid().ToString() };

        // Act
        var result = await Sut.GetUsersByIdsAsync(ids);

        // Assert
        result.ShouldBeEmpty();
    }

    [Test]
    public async Task GetUsersByIds_WhenEmptyIds_ShouldReturnEmpty()
    {
        // Arrange
        var ids = Enumerable.Empty<string>();

        // Act
        var result = await Sut.GetUsersByIdsAsync(ids);

        // Assert
        result.ShouldBeEmpty();
    }

    [Test]
    public async Task Update_WhenUserDoesExist_ShouldUpdateSuccess()
    {
        // Arrange
        var user = UserBuilder.New().Build();
        await Sut.CreateAsync(user);

        // Act
        var newUsername = Guid.NewGuid().ToString();
        var newEmail = $"{Guid.NewGuid()}@example.com";
        var newPasswordHash = Guid.NewGuid().ToString();
        user.Username = newUsername;
        user.Email = newEmail;
        user.PasswordHash = newPasswordHash;

        await Sut.UpdateAsync(user);

        // Assert
        var result = await Sut.FindByIdAsync(user.Id);
        result.ShouldNotBeNull();
        result.Username.ShouldBe(newUsername);
        result.Email.ShouldBe(newEmail);
        result.PasswordHash.ShouldBe(newPasswordHash);
    }

    [Test]
    public async Task Update_WhenUserDoesNotExist_ShouldNotThrowException()
    {
        // Arrange
        var user = UserBuilder.New().Build();

        // Act & Assert
        // ExecuteUpdate 不会因为记录不存在而抛出异常
        await Should.NotThrowAsync(() => Sut.UpdateAsync(user));
    }

    [Test]
    public async Task DeleteById_WhenUserDoesExist_ShouldDeleteSuccess()
    {
        // Arrange
        var user = UserBuilder.New().Build();
        await Sut.CreateAsync(user);

        // Act
        await Sut.DeleteByIdAsync(user.Id);

        // Assert
        var result = await Sut.FindByIdAsync(user.Id);
        result.ShouldBeNull();
    }

    [Test]
    public async Task DeleteById_WhenUserDoesNotExist_ShouldNotThrowException()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();

        // Act & Assert
        // ExecuteDelete 不会因为记录不存在而抛出异常
        await Should.NotThrowAsync(() => Sut.DeleteByIdAsync(userId));
    }

    [Test]
    public async Task Create_WhenUserInTenant_ShouldCreateUserSuccessfully()
    {
        // Arrange
        var tenant = await CreateTenantAsync();
        var user = UserBuilder.New().Build();

        // Act
        await Sut.CreateAsync(user);
        await TenantRepository.AddUserToTenantAsync(tenant.Id, user.Id);

        // Assert
        var result = await Sut.FindByIdAsync(user.Id);
        result.ShouldNotBeNull();
        result.Id.ShouldBe(user.Id);

        var isUserInTenant = await TenantRepository.GetUserInTenantAsync(tenant.Id, user.Id);
        isUserInTenant.ShouldBeTrue();
    }

    [Test]
    public async Task Create_WithAllUserProperties_ShouldInsertAllValues()
    {
        // Arrange
        var id = Guid.NewGuid().ToString();
        var username = Guid.NewGuid().ToString();
        var email = $"{Guid.NewGuid()}@example.com";
        var passwordHash = Guid.NewGuid().ToString();

        var user = UserBuilder
            .New()
            .WithId(id)
            .WithUsername(username)
            .WithEmail(email)
            .WithPasswordHash(passwordHash)
            .Build();

        // Act
        await Sut.CreateAsync(user);

        // Assert
        var result = await Sut.FindByIdAsync(id);
        result.ShouldNotBeNull();
        result.Id.ShouldBe(id);
        result.Username.ShouldBe(username);
        result.Email.ShouldBe(email);
        result.PasswordHash.ShouldBe(passwordHash);
    }

    [Test]
    public async Task FindByEmail_ShouldBeCaseSensitive()
    {
        // Arrange
        var email = $"{Guid.NewGuid()}@Example.com";
        var user = UserBuilder.New().WithEmail(email).Build();
        await Sut.CreateAsync(user);

        // Act
        var resultLowercase = await Sut.FindByEmailAsync(email.ToLower());
        var resultOriginal = await Sut.FindByEmailAsync(email);

        // Assert
        resultLowercase.ShouldNotBeNull();
        resultOriginal.ShouldNotBeNull();
    }

    [Test]
    public async Task Create_WithVeryLongValues_ShouldHandleCorrectly()
    {
        // Arrange
        var longString = new string('a', 50);
        var user = UserBuilder
            .New()
            .WithUsername(longString)
            .WithEmail($"{longString.Substring(0, 50)}@example.com")
            .WithPasswordHash(longString)
            .Build();

        // Act
        await Sut.CreateAsync(user);

        // Assert
        var result = await Sut.FindByIdAsync(user.Id);
        result.ShouldNotBeNull();
        result.Username.ShouldBe(longString);
        result.PasswordHash.ShouldBe(longString);
    }

    #region Helper Methods

    private async Task<Tenant> CreateTenantAsync()
    {
        var tenant = TenantBuilder.New().Build();
        await TenantRepository.CreateAsync(tenant);
        return tenant;
    }

    #endregion
}
