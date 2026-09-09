using FastQuiz.Auth.Domain.Repositories;
using FastQuiz.Auth.Infrastructure.Repositories.Sqlite;
using FastQuiz.Auth.Test.Infrastructure.Repositories.Utils;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace FastQuiz.Auth.Test.Infrastructure.Repositories.Sqlite;

internal class SqliteOrganizationRepositoryTests : OrganizationRepositoryTestBase
{
    protected override IOrganizationRepository Sut => _organizationRepository;

    protected override ITenantRepository TenantRepository => _tenantRepository;

    protected override IUserRepository UserRepository => _userRepository;
    private SqliteAuthDbContext _dbContext;
    private SqliteTenantRepository _tenantRepository;
    private SqliteUserRepository _userRepository;
    private SqliteOrganizationRepository _organizationRepository;
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

        _organizationRepository = new(_dbContext);
        _tenantRepository = new(_dbContext);
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
}
