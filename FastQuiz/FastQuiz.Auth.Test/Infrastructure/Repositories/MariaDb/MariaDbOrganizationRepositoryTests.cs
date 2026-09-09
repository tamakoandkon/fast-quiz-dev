using FastQuiz.Auth.Domain.Repositories;
using FastQuiz.Auth.Infrastructure.Repositories.MariaDb;
using FastQuiz.Auth.Test.Infrastructure.Repositories.Utils;
using Microsoft.EntityFrameworkCore;

namespace FastQuiz.Auth.Test.Infrastructure.Repositories.MariaDb;

[TestFixture]
[Category("MariaDb")]
internal class MariaDbOrganizationRepositoryTests : OrganizationRepositoryTestBase
{
    protected override IOrganizationRepository Sut => _repository;
    protected override IUserRepository UserRepository => _userRepository;
    protected override ITenantRepository TenantRepository => _tenantRepository;
    private MariaDbAuthDbContext _dbContext;
    private MariaDbOrganizationRepository _repository;
    private MariaDbUserRepository _userRepository;
    private MariaDbTenantRepository _tenantRepository;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        _dbContext = InitialTestDbHelper.GetTestMariaDbAuthDbContext();
        _dbContext.Database.EnsureDeleted();
        _dbContext.Database.EnsureCreated();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        if (_dbContext != null)
        {
            await _dbContext.DisposeAsync();
        }
    }

    [SetUp]
    public void Setup()
    {
        _dbContext = InitialTestDbHelper.GetTestMariaDbAuthDbContext();
        _repository = new(_dbContext);
        _userRepository = new(_dbContext);
        _tenantRepository = new(_dbContext);
    }

    [TearDown]
    public async Task TearDown()
    {
        if (_dbContext != null)
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
        }
    }
}
