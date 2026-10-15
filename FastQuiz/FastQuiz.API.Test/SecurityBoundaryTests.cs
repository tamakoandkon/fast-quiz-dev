using System.Net;
using System.Net.Http.Json;
using FastQuiz.API.Test.Utils;
using FastQuiz.Auth.Domain.Entities;
using FastQuiz.Auth.Infrastructure.Repositories.Sqlite;
using FastQuiz.Auth.UseCase.Commands;
using FastQuiz.Question.Domain.Entities;
using FastQuiz.Question.Infrastructure.Repositories.Sqlite;
using FastQuiz.Quiz.Domain.Entities;
using FastQuiz.Quiz.Infrastructure.Repositories.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace FastQuiz.API.Test;

[NonParallelizable]
internal class SecurityBoundaryTests : ApiTestBase
{
    private string a = null!, b = null!, outsider = null!, manager = null!, member = null!, victim = null!;
    private string role = null!, questionRole = null!, template = null!, snapshot = null!, parent = null!, group = null!;
    private IServiceScope scope = null!;
    private SqliteAuthDbContext auth = null!;
    private HttpClient adminA = null!, outside = null!, memberA = null!;

    [SetUp]
    public async Task SeedIndependentTenants()
    {
        using var initialize = Sut.CreateSystemAdminClient();
        scope = Sut.Services.CreateScope();
        auth = scope.ServiceProvider.GetRequiredService<SqliteAuthDbContext>();
        var quiz = scope.ServiceProvider.GetRequiredService<SqliteQuizDbContext>();
        var questions = scope.ServiceProvider.GetRequiredService<SqliteQuestionDbContext>();
        var id = Guid.NewGuid().ToString("N");
        a = "a-" + id; b = "b-" + id; outsider = "out-" + id; manager = "mgr-" + id;
        member = "member-" + id; victim = "victim-" + id; role = "role-" + id;
        questionRole = "qr-" + id; template = "template-" + id; snapshot = "snapshot-" + id;
        parent = "parent-" + id; group = "group-" + id;
        auth.AddRange(new Tenant(a,"Tenant A",""), new Tenant(b,"Tenant B",""),
            new User(outsider,"out_"+id,null,"synthetic"), new User(manager,"mgr_"+id,null,"synthetic"),
            new User(member,"mem_"+id,null,"synthetic"), new User(victim,"vic_"+id,null,"synthetic"),
            new TenantUserMap(a,manager), new TenantUserMap(a,member), new TenantUserMap(b,victim),
            new Role(role,a,"Manager","",false,true), new Role(questionRole,a,"Question","",false,true),
            new UserRole(manager,role,a), new RolePermission(Permissions.Question,questionRole,DateTimeOffset.UtcNow),
            new Organization(parent,b,null,"Private B parent"), new Organization("child-"+id,b,parent,"Private B child"));
        foreach (var permission in new[] { Permissions.Role, Permissions.Tenant, Permissions.QuizTemplate,
            Permissions.QuizAttempt, Permissions.QuizSnapshot, Permissions.Organization })
            auth.Add(new RolePermission(permission,role,DateTimeOffset.UtcNow));
        await auth.SaveChangesAsync();
        quiz.AddRange(new QuizTemplate(template,"Private B exam","description","private remark",b,3,true,
                DateTimeOffset.UtcNow,DateTimeOffset.UtcNow.AddHours(1),60,null) { IsPublished=true },
            new QuizSnapshot { Id=snapshot, QuizTemplateId=template, TotalScore=100 },
            new QuizAttempt { Id="attempt-"+id, QuizTemplateId=template, QuizSnapshotId=snapshot,
                AssignId=victim, Status=QuizAttemptStatus.Graded, Score=95 });
        await quiz.SaveChangesAsync();
        questions.Add(new QuestionGroup(group,a,"Private A group"));
        await questions.SaveChangesAsync();
        adminA = Sut.CreateClient(manager); outside = Sut.CreateClient(outsider); memberA = Sut.CreateClient(member);
    }

    [TearDown]
    public void CloseScope() { adminA.Dispose(); outside.Dispose(); memberA.Dispose(); auth.Dispose(); scope.Dispose(); }

    [Test]
    public async Task TenantUpdate_RequiresPermissionAndPreservesRejectedData()
    {
        (await outside.PutAsJsonAsync($"/tenant/{b}",new UpdateTenantCommand("changed",""))).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await auth.Tenants.AsNoTracking().SingleAsync(x=>x.Id==b)).Name.ShouldBe("Tenant B");
        (await memberA.PutAsJsonAsync($"/tenant/{a}",new UpdateTenantCommand("changed",""))).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await adminA.PutAsJsonAsync($"/tenant/{a}",new UpdateTenantCommand("Authorized change","ok"))).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await adminA.PutAsJsonAsync($"/tenant/{a}",new UpdateTenantCommand(new string('x',51),""))).StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
    }

    [Test]
    public async Task PublishedList_RequiresMembershipButAllowsOrdinaryCandidates()
    {
        using var anonymous = Sut.CreateClient();
        var path=$"/tenant/{b}/quiz-template/published";
        (await anonymous.GetAsync(path)).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        (await outside.GetAsync(path)).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        using var candidateB = Sut.CreateClient(victim);
        var allowed = await candidateB.GetAsync(path);
        allowed.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await allowed.Content.ReadAsStringAsync()).ShouldContain(template);
    }

    [TestCase("")]
    [TestCase("/records")]
    public async Task TemplateAttemptLists_RejectAnotherTenantsTemplate(string suffix)
    {
        var response=await adminA.GetAsync($"/tenant/{a}/quiz-template/{template}/quiz-attempt{suffix}");
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await response.Content.ReadAsStringAsync()).ShouldNotContain("Private B exam");
    }

    [Test]
    public async Task UserRecords_DoNotReturnAnotherTenantsGrades()
    {
        var response=await adminA.GetAsync($"/tenant/{a}/quiz-template/quiz-attempt/user/{victim}/records");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync()).ShouldBe("[]");
    }

    [Test]
    public async Task SnapshotList_RejectsAnotherTenantsTemplate()
    {
        (await adminA.GetAsync($"/tenant/{a}/quiz-template/{template}/quiz-snapshot")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        using var candidateB = Sut.CreateClient(victim);
        (await candidateB.GetAsync($"/tenant/{b}/quiz-template/{template}/quiz-snapshot")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task OrganizationTree_RejectsForeignParentOnReadAndCreate()
    {
        (await adminA.GetAsync($"/tenant/{a}/organization/{parent}?Count=10&IsPrevious=false")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await adminA.PostAsJsonAsync($"/tenant/{a}/organization",new CreateOrganizationCommand("bad child",parent))).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await auth.Organizations.AsNoTracking().AnyAsync(x=>x.TenantId==a&&x.ParentId==parent)).ShouldBeFalse();
    }

    [Test]
    public async Task RoleGrant_RejectsNonmemberAndAllowsMember()
    {
        var path=$"/tenant/role/{questionRole}/user/";
        (await adminA.PostAsJsonAsync(path+outsider,new {})).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await auth.UserRoles.AsNoTracking().AnyAsync(x=>x.UserId==outsider&&x.RoleId==questionRole)).ShouldBeFalse();
        (await outside.GetAsync($"/tenant/{a}/question-group/{group}")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await adminA.PostAsJsonAsync(path+member,new {})).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await memberA.GetAsync($"/tenant/{a}/question-group/{group}")).StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Test]
    public async Task RoleChanges_ImmediatelyRevokePermissionsAcrossRequests()
    {
        (await adminA.PostAsJsonAsync($"/tenant/role/{questionRole}/user/{member}",new {})).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var path=$"/tenant/{a}/question-group/{group}";
        (await memberA.GetAsync(path)).StatusCode.ShouldBe(HttpStatusCode.OK);
        var rolePath=$"/tenant/role/{questionRole}";
        (await adminA.PutAsJsonAsync(rolePath,new UpdateRoleCommand("Question","",false,false,[Permissions.Question]))).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await memberA.GetAsync(path)).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await adminA.PutAsJsonAsync(rolePath,new UpdateRoleCommand("Question","",false,true,[Permissions.Question]))).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await memberA.GetAsync(path)).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await adminA.PutAsJsonAsync(rolePath,new UpdateRoleCommand("Question","",false,true,[]))).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await memberA.GetAsync(path)).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task HistoricalOrphanRoleMapping_DoesNotAuthorizeNonmember()
    {
        auth.Add(new UserRole(outsider,questionRole,a));
        await auth.SaveChangesAsync();
        (await outside.GetAsync($"/tenant/{a}/question-group/{group}")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task PublicHangfireDashboard_IsNotExposed()
    {
        using var anonymous=Sut.CreateClient();
        (await anonymous.GetAsync("/hangfire")).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await outside.PutAsJsonAsync("/auth/me",new UpdateUserCommand("renamed","newpass123",null))).StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await anonymous.PutAsJsonAsync("/auth/me",new UpdateUserCommand("renamed","newpass123",null))).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
