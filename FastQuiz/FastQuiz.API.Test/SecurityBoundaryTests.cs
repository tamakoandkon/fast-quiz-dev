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

    [Test]
    public async Task OrganizationEnrollment_RejectsCrossTenantUsers()
    {
        var orgA = "org-a-" + Guid.NewGuid().ToString("N")[..8];
        auth.Add(new Organization(orgA, a, null, "Org A"));
        await auth.SaveChangesAsync();

        var addResp = await adminA.PostAsync($"/tenant/organization/{orgA}/user/{victim}", null);
        addResp.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        auth.OrganizationUserMaps.AsNoTracking().Any(x => x.OrganizationId == orgA && x.UserId == victim).ShouldBeFalse();
    }

    [Test]
    public async Task RoleEscalationAndSystemRoleProtection_AreEnforced()
    {
        var invalidPermResp = await adminA.PostAsync($"/tenant/role/{questionRole}/permission/invalid_perm", null);
        invalidPermResp.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);

        var sysPermResp = await adminA.PostAsync($"/tenant/role/{questionRole}/permission/{Permissions.SystemAdmin}", null);
        sysPermResp.StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        var createSysRole = await adminA.PostAsJsonAsync($"/tenant/{a}/role",
            new CreateRoleCommand("escalate", "", true, true, [Permissions.SystemAdmin]));
        createSysRole.StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        using var sysAdmin = Sut.CreateClient(User.DefaultAdminId);
        (await sysAdmin.DeleteAsync($"/tenant/{Tenant.DefaultTenantId}")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await sysAdmin.DeleteAsync($"/tenant/role/{Role.DefaultAdminRoleId}")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await sysAdmin.PutAsJsonAsync($"/tenant/role/{Role.DefaultAdminRoleId}",
            new UpdateRoleCommand("system admin", "", true, false, [.. Permissions.AllPermissions()]))).StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        // Local tenant admin with Permissions.AllPermissions() equivalent cannot evict default_admin via /user-map
        auth.Add(new TenantUserMap(a, User.DefaultAdminId));
        var localSysRole = "lsys-" + Guid.NewGuid().ToString("N")[..8];
        auth.Add(new Role(localSysRole, a, "local-sys", "", false, true));
        auth.Add(new RolePermission(Permissions.SystemAdmin, localSysRole, DateTimeOffset.UtcNow));
        auth.Add(new UserRole(manager, localSysRole, a));
        await auth.SaveChangesAsync();
        (await adminA.DeleteAsync($"/tenant/{a}/user-map/{User.DefaultAdminId}")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task MyQuizAttempts_AreScopedToCurrentTenant()
    {
        auth.Add(new TenantUserMap(a, victim));
        await auth.SaveChangesAsync();

        using var victimClient = Sut.CreateClient(victim);
        var resp = await victimClient.GetAsync($"/tenant/{a}/quiz-template/quiz-attempt/me?Count=20");
        resp.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await resp.Content.ReadAsStringAsync()).ShouldNotContain("attempt-");
    }

    [Test]
    public async Task DraftTemplateAndRemarkAndAnswerKeys_AreProtectedFromCandidates()
    {
        var draftId = "draft-" + Guid.NewGuid().ToString("N")[..8];
        var expiredAttemptId = "exp-attempt-" + Guid.NewGuid().ToString("N")[..8];
        using (var quizScope = Sut.Services.CreateScope())
        {
            var quiz = quizScope.ServiceProvider.GetRequiredService<SqliteQuizDbContext>();
            quiz.Add(new QuizTemplate(draftId, "Draft B", "desc", "secret teacher remark", b, 1, false,
                DateTimeOffset.UtcNow.AddHours(-2), DateTimeOffset.UtcNow.AddHours(-1), 60, null) { IsPublished = false });
            quiz.Add(new QuizAttempt
            {
                Id = expiredAttemptId,
                QuizTemplateId = draftId,
                QuizSnapshotId = snapshot,
                AssignId = victim,
                Status = QuizAttemptStatus.Pending,
            });
            await quiz.SaveChangesAsync();
        }

        using var candidateB = Sut.CreateClient(victim);
        (await candidateB.GetAsync($"/tenant/{b}/quiz-template/{draftId}")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        var publishedDetail = await candidateB.GetStringAsync($"/tenant/{b}/quiz-template/{template}");
        publishedDetail.ShouldNotContain("private remark");
        var publishedList = await candidateB.GetStringAsync($"/tenant/{b}/quiz-template/published?Count=10");
        publishedList.ShouldNotContain("private remark");

        // Candidate cannot view graded answers while template EndTime is still in the future or attempts remain
        var attemptId = "attempt-" + template["template-".Length..];
        (await candidateB.GetAsync($"/tenant/{b}/quiz-template/quiz-attempt/{attemptId}/question/with-answer"))
            .StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        // Candidate cannot start a Pending attempt once template is unpublished/expired
        (await candidateB.PutAsJsonAsync($"/tenant/{b}/quiz-template/quiz-attempt/{expiredAttemptId}?status=InProgress", new { }))
            .StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        // Admin cannot unpublish a template that already has quiz attempts
        using var sysAdmin = Sut.CreateClient(User.DefaultAdminId);
        auth.Add(new TenantUserMap(b, User.DefaultAdminId));
        await auth.SaveChangesAsync();
        (await sysAdmin.PutAsJsonAsync($"/tenant/{b}/quiz-template/{template}/published",
            new FastQuiz.Quiz.UseCase.Commands.UpdateQuizTemplatePublishedCommand(false)))
            .StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}
