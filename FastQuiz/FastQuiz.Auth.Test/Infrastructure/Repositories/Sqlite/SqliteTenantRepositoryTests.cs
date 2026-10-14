using FastQuiz.Auth.Domain.Repositories;
using FastQuiz.Auth.Infrastructure.Repositories.Sqlite;
using FastQuiz.Auth.Test.Infrastructure.Repositories.Utils;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using FastQuiz.Auth.Domain.Entities;
using Shouldly;

namespace FastQuiz.Auth.Test.Infrastructure.Repositories.Sqlite;

[TestFixture]
[Category("Sqlite")]
internal class SqliteTenantRepositoryTests : TenantRepositoryTestBase
{
    protected override ITenantRepository Sut => _repository;
    protected override IUserRepository UserRepository => _userRepository;
    private SqliteAuthDbContext _dbContext;
    private SqliteTenantRepository _repository;
    private SqliteUserRepository _userRepository;
    private SqliteConnection _connection;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        (_connection, _dbContext) = InitialTestDbHelper.GetTestSqliteAuthDbContext();

        _dbContext.Database.EnsureDeleted();
        _dbContext.Database.EnsureCreated();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        if (_dbContext != null)
        {
            _dbContext?.Dispose();
            _connection?.Dispose();
        }
    }

    [SetUp]
    public void Setup()
    {
        (_connection, _dbContext) = InitialTestDbHelper.GetTestSqliteAuthDbContext();
        _dbContext.Database.EnsureCreated();

        _repository = new SqliteTenantRepository(_dbContext);
        _userRepository = new(_dbContext);
    }

    [TearDown]
    public async Task TearDown()
    {
        await _dbContext.OrganizationUserMaps.ExecuteDeleteAsync();
        await _dbContext.TenantUserMaps.ExecuteDeleteAsync();
        await _dbContext.RolePermissions.ExecuteDeleteAsync();
        await _dbContext.UserRoles.ExecuteDeleteAsync();
        await _dbContext.Roles.ExecuteDeleteAsync();
        await _dbContext.Organizations.Where(x => x.ParentId != null).ExecuteDeleteAsync();
        await _dbContext.Organizations.ExecuteDeleteAsync();
        await _dbContext.Users.ExecuteDeleteAsync();
        await _dbContext.Tenants.ExecuteDeleteAsync();
        _dbContext?.Dispose();
        _connection?.Dispose();
    }

    [Test]
    public async Task RemovingMemberPreservesSharedAccountAndOtherTenantRolesAndGroups()
    {
        var user = new User(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), null, "hash");
        var tenantA = new Tenant(Guid.NewGuid().ToString(), "A", "");
        var tenantB = new Tenant(Guid.NewGuid().ToString(), "B", "");
        var groupA = new Organization(Guid.NewGuid().ToString(), tenantA.Id, null, "A");
        var groupB = new Organization(Guid.NewGuid().ToString(), tenantB.Id, null, "B");
        var roleA = new Role(Guid.NewGuid().ToString(), tenantA.Id, "A", "", false, true);
        var roleB = new Role(Guid.NewGuid().ToString(), tenantB.Id, "B", "", false, true);
        _dbContext.AddRange(user, tenantA, tenantB, groupA, groupB, roleA, roleB,
            new TenantUserMap(tenantA.Id, user.Id), new TenantUserMap(tenantB.Id, user.Id),
            new UserRole(user.Id, roleA.Id, tenantA.Id), new UserRole(user.Id, roleB.Id, tenantB.Id),
            new OrganizationUserMap(groupA.Id, user.Id), new OrganizationUserMap(groupB.Id, user.Id));
        await _dbContext.SaveChangesAsync();

        await Sut.RemoveUserFromTenantAsync(tenantA.Id, user.Id);

        (await _dbContext.Users.AsNoTracking().SingleAsync()).Id.ShouldBe(user.Id);
        (await _dbContext.TenantUserMaps.AsNoTracking().SingleAsync()).TenantId.ShouldBe(tenantB.Id);
        (await _dbContext.UserRoles.AsNoTracking().SingleAsync()).RoleId.ShouldBe(roleB.Id);
        (await _dbContext.OrganizationUserMaps.AsNoTracking().SingleAsync()).OrganizationId.ShouldBe(groupB.Id);
    }
}
