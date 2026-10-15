using FastQuiz.Auth.Domain.Entities;
using FastQuiz.Auth.Domain.Repositories;
using FastQuiz.Auth.Infrastructure.Repositories;
using FastQuiz.Auth.Infrastructure.Repositories.Sqlite;
using FastQuiz.Auth.Infrastructure.Repositories.PostgreSql;
using FastQuiz.Common.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Shouldly;

namespace FastQuiz.Auth.Test.Infrastructure.Repositories;

[TestFixture("Sqlite")]
[TestFixture("PostgreSql", Category="PostgreSql")]
internal class SecurityRepositoryTests(string provider)
{
    private string connection = null!;

    [SetUp]
    public async Task Seed()
    {
        var name="fastquiz_security_"+Guid.NewGuid().ToString("N");
        connection=provider=="Sqlite"
            ? $"Data Source={Path.Combine(Path.GetTempPath(),name+".db")};Default Timeout=5"
            : new Npgsql.NpgsqlConnectionStringBuilder(Environment.GetEnvironmentVariable("FASTQUIZ_TEST_POSTGRES")
                ?? "Host=127.0.0.1;Username=postgres;Password=postgres") { Database=name }.ConnectionString;
        await using var db=OpenDb();
        await db.Database.EnsureCreatedAsync();
        db.AddRange(new Tenant("t","tenant",""),new User("u","user",null,"old-hash"),
            new User("outsider","outsider",null,"synthetic"), new TenantUserMap("t","u"),
            new Role("r","t","role","",false,true), new RolePermission(Permissions.Question,"r",DateTimeOffset.UtcNow));
        await db.SaveChangesAsync();
    }

    [TearDown]
    public async Task Cleanup()
    {
        // Both providers use a unique test-owned database; shared databases are never removed.
        await using var db=OpenDb();
        await db.Database.EnsureDeletedAsync();
    }

    [Test]
    public async Task GrantRejectsNonmemberDisabledRoleAndRemovedMember()
    {
        await using var db=OpenDb();
        var roles=Roles(db);
        await Should.ThrowAsync<PermissionException>(()=>roles.AddRoleToUserAsync("outsider","r","t"));
        await db.Set<Role>().Where(x=>x.Id=="r").ExecuteUpdateAsync(x=>x.SetProperty(r=>r.IsEnabled,false));
        await Should.ThrowAsync<PermissionException>(()=>roles.AddRoleToUserAsync("u","r","t"));
        await db.Set<Role>().Where(x=>x.Id=="r").ExecuteUpdateAsync(x=>x.SetProperty(r=>r.IsEnabled,true));
        await roles.AddRoleToUserAsync("u","r","t");
        await roles.AddRoleToUserAsync("u","r","t"); // idempotent, no duplicate mapping
        (await db.Set<UserRole>().CountAsync()).ShouldBe(1);
        await Tenants(db).RemoveUserFromTenantAsync("t","u");
        (await db.Set<UserRole>().CountAsync()).ShouldBe(0);
        await Should.ThrowAsync<PermissionException>(()=>roles.AddRoleToUserAsync("u","r","t"));
    }

    [Test]
    public async Task PermissionLookupRejectsOrphansAndDisabledRolesAndPersistsEdits()
    {
        await using var db=OpenDb();
        var roles=Roles(db);
        db.Add(new UserRole("outsider","r","t"));
        await db.SaveChangesAsync();
        (await roles.FindRolesByUserIdAsync("t","outsider")).ShouldBeEmpty();
        await roles.AddRoleToUserAsync("u","r","t");
        (await roles.FindRolesByUserIdAsync("t","u")).Count.ShouldBe(1);
        var role=(await roles.FindByIdAsync("r"))!;
        role.IsEnabled=false;
        role.Permissions=[Permissions.QuizAttempt];
        await roles.UpdateAsync(role);
        (await roles.FindRolesByUserIdAsync("t","u")).ShouldBeEmpty();
        (await roles.GetPermissionsByRoleIdAsync("r")).ShouldBe([Permissions.QuizAttempt]);
        role.IsEnabled=true; role.Permissions=[];
        await roles.UpdateAsync(role);
        (await roles.GetPermissionsByRoleIdAsync("r")).ShouldBeEmpty();
    }

    [Test]
    public async Task ProfileUpdateAndStalePasswordChangeCannotRestoreOldCredentials()
    {
        await using var db=OpenDb();
        var users=Users(db);
        var stale=(await users.FindByIdAsync("u"))!;
        (await users.TryChangePasswordAsync("u","old-hash","new-hash")).ShouldBeTrue();
        (await users.TryChangePasswordAsync("u","old-hash","attacker-hash")).ShouldBeFalse();
        stale.Username="renamed";
        await users.UpdateProfileAsync(stale);
        var current=(await users.FindByIdAsync("u"))!;
        current.PasswordHash.ShouldBe("new-hash"); current.Username.ShouldBe("renamed");
    }

    private DbContext OpenDb()
    {
        var settings=Options.Create(new AuthDbContextSettings { ConnectionString=connection });
        return provider=="Sqlite"
            ? new SqliteAuthDbContext(new DbContextOptionsBuilder<SqliteAuthDbContext>().UseSqlite(connection).Options,settings)
            : new PostgreSqlAuthDbContext(new DbContextOptionsBuilder<PostgreSqlAuthDbContext>().UseNpgsql(connection).Options,settings);
    }
    private static IRoleRepository Roles(DbContext db)=>db is SqliteAuthDbContext sqlite
        ? new SqliteRoleRepository(sqlite) : new PostgreSqlRoleRepository((PostgreSqlAuthDbContext)db);
    private static ITenantRepository Tenants(DbContext db)=>db is SqliteAuthDbContext sqlite
        ? new SqliteTenantRepository(sqlite) : new PostgreSqlTenantRepository((PostgreSqlAuthDbContext)db);
    private static IUserRepository Users(DbContext db)=>db is SqliteAuthDbContext sqlite
        ? new SqliteUserRepository(sqlite) : new PostgreSqlUserRepository((PostgreSqlAuthDbContext)db);
}
