using System.Net;
using System.Net.Http.Json;
using FastQuiz.API.Test.Utils;
using FastQuiz.API.Test.Utils.Helpers;
using FastQuiz.Auth.Domain.Entities;
using FastQuiz.Question.UseCase.Command;
using FastQuiz.Quiz.Domain.Entities;
using FastQuiz.Quiz.Domain.Repositories;
using FastQuiz.Quiz.UseCase.Commands;
using FastQuiz.Quiz.UseCase.Dtos;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace FastQuiz.API.Test.Quiz;

internal partial class QuizAttemptTests
{
    private static string PrebuiltUrl(string templateId) =>
        $"/tenant/{Tenant.DefaultTenantId}/quiz-template/{templateId}/quiz-attempt";

    private async Task<(string Template, string Snapshot)> SeedPrebuiltAsync(int limit = 3)
    {
        var owner = User.DefaultAdminId;
        var template = await Sut.CreateQuizTemplateAsync(owner,
            SeedDefaultCreateQuizTemplateCommand() with { AttemptLimit = limit });
        var group = await Sut.CreateQuestionGroupAsync(owner,
            new CreateQuestionGroupCommand(Guid.NewGuid().ToString(), "prebuilt"));
        await Sut.CreateQuestionAsync(owner, group.Id, SeedDefaultCreateQuestionCommand());
        await Sut.CreateQuestionSelectionRuleAsync(owner, template.Id,
            SeedDefaultCreateQuestionSelectionRuleCommand(group.Id));
        await Sut.CreateQuizSnapshotAsync(owner, template.Id, new CreateQuizSnapshotCommand(1));
        var snapshots = await Sut.GetAllQuizSnapshotAsync(owner, template.Id,
            new FastQuiz.Quiz.UseCase.Queries.GetAllQuizSnapshotQuery(null, 1));
        await Sut.PublishQuizTempalteAsync(owner, template.Id, true);
        return (template.Id, snapshots.Single().Id);
    }

    private Task<HttpResponseMessage> GenerateAsync(string template, string snapshot, int count) =>
        Sut.CreateClient(User.DefaultAdminId).PostAsJsonAsync(PrebuiltUrl(template) + "/batch",
            new[] { new CreateQuizAttemptCommand(snapshot, count) });

    [Test]
    public async Task BatchCreatesEveryRequestedCopyAndMemberCanClaimStartAndAnswer()
    {
        var (template, snapshot) = await SeedPrebuiltAsync();
        (await GenerateAsync(template, snapshot, 3)).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var admin = Sut.CreateClient(User.DefaultAdminId);
        var pool = await admin.GetFromJsonAsync<List<PrebuiltQuizAttemptDto>>(PrebuiltUrl(template) + "/prebuilt");
        pool!.Count.ShouldBe(3);
        pool.Select(x => x.Id).Distinct().Count().ShouldBe(3);
        var firstPage = (await admin.GetFromJsonAsync<List<PrebuiltQuizAttemptDto>>(
            PrebuiltUrl(template) + "/prebuilt?Count=2"))!;
        firstPage.Count.ShouldBe(2);
        var secondPage = (await admin.GetFromJsonAsync<List<PrebuiltQuizAttemptDto>>(
            PrebuiltUrl(template) + "/prebuilt?Count=2&Cursor=" + firstPage.Last().Id))!;
        secondPage.Count.ShouldBe(1);
        firstPage.Select(x => x.Id).Intersect(secondPage.Select(x => x.Id)).ShouldBeEmpty();
        await using var scope = Sut.Services.CreateAsyncScope();
        var recordRepo = scope.ServiceProvider.GetRequiredService<IQuestionRecordRepository>();
        foreach (var item in pool)
            (await recordRepo.GetAllByQuizAttempIdAsync(item.Id)).Count.ShouldBe(1);

        var memberId = await CreateMemberAsync(false);
        var member = Sut.CreateClient(memberId);
        (await member.GetAsync(PrebuiltUrl(template) + "/prebuilt")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        var response = await member.PostAsync(PrebuiltUrl(template) + "/claim", null);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var attempt = (await response.ReadFromJsonWithEnumAsync<QuizAttempDto>())!;
        attempt.AssignId.ShouldBe(memberId);
        pool.ShouldContain(x => x.Id == attempt.Id);
        (await admin.GetFromJsonAsync<List<PrebuiltQuizAttemptDto>>(PrebuiltUrl(template) + "/prebuilt"))!
            .Count.ShouldBe(2);
        await Sut.UpdateQuizAttemptStatusAsync(memberId, attempt.Id, QuizAttemptStatus.InProgress);
        var question = (await recordRepo.GetAllByQuizAttempIdAsync(attempt.Id)).Single();
        (await member.PutAsJsonAsync(UrlPackage.QuizAttemptQuestionWithQuestionId(question.Id),
            new[] { new SetAnswerRecordCommand("answer") })).StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(101)]
    public async Task BatchRejectsInvalidCountsWithoutWritingAnything(int count)
    {
        var (template, snapshot) = await SeedPrebuiltAsync();
        (await GenerateAsync(template, snapshot, count)).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await Sut.CreateClient(User.DefaultAdminId).GetFromJsonAsync<List<PrebuiltQuizAttemptDto>>(
            PrebuiltUrl(template) + "/prebuilt"))!.ShouldBeEmpty();
    }

    [Test]
    public async Task BatchRejectsEmptyOversizedTotalAndNullItems()
    {
        var (template, snapshot) = await SeedPrebuiltAsync();
        var admin = Sut.CreateClient(User.DefaultAdminId);
        foreach (var batch in new object[] {
            Array.Empty<CreateQuizAttemptCommand>(),
            new[] { new CreateQuizAttemptCommand(snapshot, 60), new CreateQuizAttemptCommand(snapshot, 60) },
            new CreateQuizAttemptCommand?[] { null } })
            (await admin.PostAsJsonAsync(PrebuiltUrl(template) + "/batch", batch))
                .StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task BatchRollsBackEarlierCopiesWhenLaterSnapshotIsInvalid()
    {
        var (template, snapshot) = await SeedPrebuiltAsync();
        var response = await Sut.CreateClient(User.DefaultAdminId).PostAsJsonAsync(PrebuiltUrl(template) + "/batch",
            new[] { new CreateQuizAttemptCommand(snapshot, 2), new CreateQuizAttemptCommand("missing", 1) });
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        await using var scope = Sut.Services.CreateAsyncScope();
        (await scope.ServiceProvider.GetRequiredService<IQuizAttemptRepository>()
            .GetAllByTemplateIdAsync(template, null, 100)).ShouldBeEmpty();
    }

    [Test]
    public async Task BatchRejectsSnapshotFromAnotherTemplateAndWrongTenant()
    {
        var (first, _) = await SeedPrebuiltAsync();
        var (_, foreignSnapshot) = await SeedPrebuiltAsync();
        (await GenerateAsync(first, foreignSnapshot, 1)).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await Sut.CreateClient(User.DefaultAdminId).PostAsJsonAsync(
            PrebuiltUrl(first).Replace(Tenant.DefaultTenantId, "wrong-tenant") + "/batch",
            new[] { new CreateQuizAttemptCommand(foreignSnapshot, 1) })).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task OnlyManagersCanGenerateAndOnlyMembersCanClaim()
    {
        var (template, snapshot) = await SeedPrebuiltAsync();
        var member = Sut.CreateClient(await CreateMemberAsync(false));
        (await member.PostAsJsonAsync(PrebuiltUrl(template) + "/batch",
            new[] { new CreateQuizAttemptCommand(snapshot, 1) })).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await Sut.CreateClient(Guid.NewGuid().ToString()).PostAsync(PrebuiltUrl(template) + "/claim", null))
            .StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await member.PostAsync(PrebuiltUrl(template) + "/claim", null)).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task TwoMembersCannotClaimTheSamePrebuiltAttempt()
    {
        var (template, snapshot) = await SeedPrebuiltAsync();
        (await GenerateAsync(template, snapshot, 1)).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var pool = await Sut.CreateClient(User.DefaultAdminId)
            .GetFromJsonAsync<List<PrebuiltQuizAttemptDto>>(PrebuiltUrl(template) + "/prebuilt");
        var url = $"/tenant/{Tenant.DefaultTenantId}/quiz-template/quiz-attempt/{pool!.Single().Id}";
        var users = new[] { await CreateMemberAsync(false), await CreateMemberAsync(false) };
        var responses = await Task.WhenAll(users.Select(id => Sut.CreateClient(id).PostAsync(url, null)));
        responses.Count(x => x.StatusCode == HttpStatusCode.NoContent).ShouldBe(1);
        responses.Count(x => x.StatusCode == HttpStatusCode.BadRequest).ShouldBe(1);
    }

    [Test]
    public async Task ConcurrentClaimsAndOrdinaryCreationShareTheSameAttemptLimit()
    {
        var (template, snapshot) = await SeedPrebuiltAsync(1);
        (await GenerateAsync(template, snapshot, 2)).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var user = await CreateMemberAsync(false);
        var client = Sut.CreateClient(user);
        var responses = await Task.WhenAll(client.PostAsync(PrebuiltUrl(template) + "/claim", null),
            client.PostAsync(PrebuiltUrl(template), null));
        responses.Count(x => x.StatusCode == HttpStatusCode.OK).ShouldBe(1);
        responses.Count(x => x.StatusCode == HttpStatusCode.BadRequest).ShouldBe(1);
        await using var scope = Sut.Services.CreateAsyncScope();
        (await scope.ServiceProvider.GetRequiredService<IQuizAttemptRepository>()
            .GetAttemptCountAsync(user, template)).ShouldBe(1);
    }

    [TestCase(QuizAttemptStatus.Pending)]
    [TestCase(QuizAttemptStatus.Graded)]
    public async Task ClaimRejectsUnfinishedAttemptsAndExhaustedCompletedQuota(QuizAttemptStatus previousStatus)
    {
        var (template, snapshot) = await SeedPrebuiltAsync(1);
        (await GenerateAsync(template, snapshot, 2)).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var member = Sut.CreateClient(await CreateMemberAsync(false));
        var firstResponse = await member.PostAsync(PrebuiltUrl(template) + "/claim", null);
        firstResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var first = (await firstResponse.ReadFromJsonWithEnumAsync<QuizAttempDto>())!;
        await using var scope = Sut.Services.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IQuizAttemptRepository>();
        var attempt = (await repository.FindByIdAsync(first.Id))!;
        attempt.Status = previousStatus;
        await repository.UpdateAsync(attempt);
        (await member.PostAsync(PrebuiltUrl(template) + "/claim", null))
            .StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await repository.GetUnassignedByTemplateIdAsync(template, null, 10)).Count.ShouldBe(1);
    }

    [Test]
    public async Task ClaimEndpointsRejectWrongTenantAndUnauthenticatedCalls()
    {
        var (template, snapshot) = await SeedPrebuiltAsync();
        (await GenerateAsync(template, snapshot, 1)).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var pool = (await Sut.CreateClient(User.DefaultAdminId)
            .GetFromJsonAsync<List<PrebuiltQuizAttemptDto>>(PrebuiltUrl(template) + "/prebuilt"))!;
        (await GetClient().PostAsync(PrebuiltUrl(template) + "/claim", null))
            .StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        var admin = Sut.CreateClient(User.DefaultAdminId);
        (await admin.PostAsync(PrebuiltUrl(template).Replace(Tenant.DefaultTenantId, "wrong-tenant") + "/claim", null))
            .StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await admin.PostAsync($"/tenant/wrong-tenant/quiz-template/quiz-attempt/{pool.Single().Id}", null))
            .StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [TestCase("unpublished", HttpStatusCode.BadRequest)]
    [TestCase("future", HttpStatusCode.BadRequest)]
    [TestCase("expired", HttpStatusCode.BadRequest)]
    [TestCase("organization", HttpStatusCode.Forbidden)]
    [TestCase("zero-limit", HttpStatusCode.BadRequest)]
    public async Task ClaimEnforcesEligibilityWithoutConsumingPool(string restriction, HttpStatusCode expected)
    {
        var (template, snapshot) = await SeedPrebuiltAsync();
        (await GenerateAsync(template, snapshot, 1)).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        await using var scope = Sut.Services.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IQuizTemplateRepository>();
        var entity = (await repository.FindByIdAsync(template))!;
        if (restriction == "unpublished") entity.IsPublished = false;
        if (restriction == "zero-limit") entity.AttemptLimit = 0;
        if (restriction == "organization") entity.UserGroupId = "restricted-organization";
        if (restriction is "future" or "expired")
        {
            entity.IsUnlimitedTime = false;
            entity.BeginTime = DateTimeOffset.UtcNow.AddDays(restriction == "future" ? 1 : -2);
            entity.EndTime = entity.BeginTime.AddDays(1).AddMinutes(-1);
        }
        await repository.UpdateAsync(entity);
        var response = await Sut.CreateClient(await CreateMemberAsync(false))
            .PostAsync(PrebuiltUrl(template) + "/claim", null);
        response.StatusCode.ShouldBe(expected);
        (await scope.ServiceProvider.GetRequiredService<IQuizAttemptRepository>()
            .GetUnassignedByTemplateIdAsync(template, null, 10)).Count.ShouldBe(1);
    }
}
