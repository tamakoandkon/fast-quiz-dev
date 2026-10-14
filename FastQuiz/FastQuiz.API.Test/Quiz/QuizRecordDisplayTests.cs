using FastQuiz.Auth.Domain.Entities;
using FastQuiz.Auth.Domain.Repositories;
using FastQuiz.Auth.UseCase.Services;
using FastQuiz.Common.Exceptions;
using FastQuiz.Quiz.Domain.Entities;
using FastQuiz.Quiz.Domain.Repositories;
using FastQuiz.Quiz.UseCase.Dtos;
using FastQuiz.Quiz.UseCase.Queries;
using FastQuiz.Quiz.UseCase.Services;
using Moq;
using Shouldly;

namespace FastQuiz.API.Test.Quiz;

public sealed class QuizRecordDisplayTests
{
    private Mock<IQuizAttemptRepository> attempts = null!;
    private Mock<IQuizTemplateRepository> templates = null!;
    private Mock<IQuizTemplateService> templateService = null!;
    private Mock<IPermissionService> permissions = null!;
    private Mock<ITenantService> tenants = null!;
    private Mock<IUserRepository> users = null!;
    private QuizAttemptQueryService service = null!;

    private static QuizTemplate Template(string id = "template", string tenant = "tenant") =>
        new(id, "Exam", "", "", tenant, 3, true, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, 60, null);
    private static QuizTemplateDto TemplateDto() =>
        new("template", "Exam", "", "", 3, true, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, 60, true, "tenant", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, null);
    private static QuizAttempt Attempt(string id = "attempt", string? assignee = "member", string template = "template") =>
        new() { Id = id, AssignId = assignee, QuizTemplateId = template, QuizSnapshotId = "snapshot", Score = 100, Status = QuizAttemptStatus.Graded };

    [SetUp]
    public void Setup()
    {
        attempts = new(); templates = new(); templateService = new(); permissions = new(); tenants = new(); users = new();
        tenants.Setup(x => x.GetUserInTenantAsync("tenant", It.IsAny<string>())).ReturnsAsync(true);
        permissions.Setup(x => x.HasPermissionAsync("tenant", "reviewer", Permissions.QuizAttempt)).ReturnsAsync(true);
        templateService.Setup(x => x.GetQuizTemplateAsync(It.IsAny<string>(), "tenant", "template")).ReturnsAsync(TemplateDto());
        templates.Setup(x => x.FindByIdAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync([Template()]);
        users.Setup(x => x.GetUsersByIdsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync([new User("member", "Alice", null, "hash")]);
        attempts.Setup(x => x.GetAllRecordsByTemplateIdAsync("template", false, null, 30, false)).ReturnsAsync([Attempt(), Attempt("second")]);
        attempts.Setup(x => x.GetAllRecordsByUserIdAsync("member", null, false, null, 30, false, "tenant")).ReturnsAsync([Attempt()]);
        service = new(attempts.Object, templates.Object, templateService.Object, permissions.Object, tenants.Object, users.Object);
    }

    [Test]
    public async Task TemplateRecordsResolveNamesInOneDeduplicatedBatch()
    {
        var result = await service.GetAllRecordsByTemplateIdAsync("reviewer", "tenant", "template", new());
        result.Count.ShouldBe(2);
        result.ShouldAllBe(x => x.AssignUsername == "Alice" && x.AssignId == "member");
        users.Verify(x => x.GetUsersByIdsAsync(It.Is<IEnumerable<string>>(ids => ids.SequenceEqual(new[] { "member" }))), Times.Once);
    }

    [Test]
    public async Task DeletedAccountKeepsTheStableIdWithoutInventingAName()
    {
        users.Setup(x => x.GetUsersByIdsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync([]);
        var result = await service.GetAllRecordsByTemplateIdAsync("reviewer", "tenant", "template", new());
        result.ShouldAllBe(x => x.AssignId == "member" && x.AssignUsername == null);
    }

    [Test]
    public async Task OwnRecordsIncludeNameWithoutUserManagementPermission()
    {
        var result = await service.GetUserRecordsAsync("member", "tenant", "member", new());
        result.Single().AssignUsername.ShouldBe("Alice");
    }

    [Test]
    public async Task AnotherMemberCannotReadRecordsOrResolveNames()
    {
        await Should.ThrowAsync<PermissionException>(() => service.GetUserRecordsAsync("stranger", "tenant", "member", new()));
        users.Verify(x => x.GetUsersByIdsAsync(It.IsAny<IEnumerable<string>>()), Times.Never);
        attempts.Verify(x => x.GetAllRecordsByUserIdAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string?>()), Times.Never);
    }

    [Test]
    public async Task WrongTenantTemplateIsRejectedBeforeReadingRecordsOrNames()
    {
        templateService.Setup(x => x.GetQuizTemplateAsync("reviewer", "tenant", "template")).ThrowsAsync(new PermissionException());
        await Should.ThrowAsync<PermissionException>(() => service.GetAllRecordsByTemplateIdAsync("reviewer", "tenant", "template", new()));
        attempts.Verify(x => x.GetAllRecordsByTemplateIdAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
        users.Verify(x => x.GetUsersByIdsAsync(It.IsAny<IEnumerable<string>>()), Times.Never);
    }

    [Test]
    public async Task UserRecordsDoNotExposeAnotherTenantsExamOrNames()
    {
        templates.Setup(x => x.FindByIdAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync([Template(), Template("foreign", "other-tenant")]);
        attempts.Setup(x => x.GetAllRecordsByUserIdAsync("member", null, false, null, 30, false, "tenant"))
            .ReturnsAsync([Attempt(), Attempt("foreign-attempt", "member", "foreign")]);
        var result = await service.GetUserRecordsAsync("member", "tenant", "member", new());
        result.Count.ShouldBe(1);
        result.Single().QuizTemplateId.ShouldBe("template");
    }

    [Test]
    public async Task EmptyRecordPageDoesNotQueryAccounts()
    {
        attempts.Setup(x => x.GetAllRecordsByTemplateIdAsync("template", false, null, 30, false)).ReturnsAsync([]);
        (await service.GetAllRecordsByTemplateIdAsync("reviewer", "tenant", "template", new())).ShouldBeEmpty();
        users.Verify(x => x.GetUsersByIdsAsync(It.IsAny<IEnumerable<string>>()), Times.Never);
    }

    [Test]
    public async Task TenantNonmemberCannotReadNamesEvenWithAClaimedManagementPermission()
    {
        tenants.Setup(x => x.GetUserInTenantAsync("tenant", "reviewer")).ReturnsAsync(false);
        await Should.ThrowAsync<PermissionException>(() => service.GetAllRecordsByTemplateIdAsync("reviewer", "tenant", "template", new()));
        users.Verify(x => x.GetUsersByIdsAsync(It.IsAny<IEnumerable<string>>()), Times.Never);
    }
}
