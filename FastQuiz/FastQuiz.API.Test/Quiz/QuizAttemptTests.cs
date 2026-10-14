using System.Net.Http.Json;
using FastQuiz.API.Test.Utils;
using FastQuiz.API.Test.Utils.Helpers;
using FastQuiz.Auth.Domain.Entities;
using FastQuiz.Auth.Domain.Repositories;
using FastQuiz.Auth.UseCase.Commands;
using FastQuiz.Auth.UseCase.Services;
using FastQuiz.Question.Domain.Entities;
using FastQuiz.Question.UseCase.Command;
using FastQuiz.Question.UseCase.Dto;
using FastQuiz.Quiz.Domain.Entities;
using FastQuiz.Quiz.UseCase.Commands;
using FastQuiz.Quiz.UseCase.Dtos;
using Shouldly;
using Microsoft.Extensions.DependencyInjection;

namespace FastQuiz.API.Test.Quiz;

internal class QuizAttemptTests : ApiTestBase
{
    [SetUp]
    public async Task Setup()
    {
        await Sut.CleanQuizSqliteDb();
    }

    [Test]
    public async Task PostQuizAttemptAsync_WhenValidRequest_ShouldReturnQuizAttemptDto()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(userId);
        var template = await Sut.CreateQuizTemplateAsync(
            userId,
            SeedDefaultCreateQuizTemplateCommand()
        );
        var group = await Sut.CreateQuestionGroupAsync(
            userId,
            new CreateQuestionGroupCommand(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())
        );
        var question = await Sut.CreateQuestionAsync(
            userId,
            group.Id,
            SeedDefaultCreateQuestionCommand()
        );
        var rule = await Sut.CreateQuestionSelectionRuleAsync(
            userId,
            template.Id,
            SeedDefaultCreateQuestionSelectionRuleCommand(group.Id)
        );
        await Sut.CreateQuizSnapshotAsync(userId, template.Id, new CreateQuizSnapshotCommand(1));
        var snapshots = await Sut.GetAllQuizSnapshotAsync(
            userId,
            template.Id,
            new FastQuiz.Quiz.UseCase.Queries.GetAllQuizSnapshotQuery(null, 1)
        );
        await Sut.PublishQuizTempalteAsync(userId, template.Id, true);

        // Act
        var response = await client.PostAsJsonAsync(
            UrlPackage.QuizAttemptWithTemplateId(template.Id),
            new { }
        );

        // Assert
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        var result = await response.ReadFromJsonWithEnumAsync<QuizAttempDto>();
        result.ShouldNotBeNull();
        result.QuizSampleId.ShouldBe(snapshots[0].Id);
    }

    [Test]
    [Ignore("方法没有实现")]
    public async Task PostBatchQuizAttemptAsync_WhenValidRequest_ShouldReturnNoContent()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(userId);
        var template = await Sut.CreateQuizTemplateAsync(
            userId,
            SeedDefaultCreateQuizTemplateCommand()
        );
        var group = await Sut.CreateQuestionGroupAsync(
            userId,
            new CreateQuestionGroupCommand(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())
        );
        var question = await Sut.CreateQuestionAsync(
            userId,
            group.Id,
            SeedDefaultCreateQuestionCommand()
        );
        var rule = await Sut.CreateQuestionSelectionRuleAsync(
            userId,
            template.Id,
            SeedDefaultCreateQuestionSelectionRuleCommand(group.Id)
        );
        await Sut.CreateQuizSnapshotAsync(userId, template.Id, new CreateQuizSnapshotCommand(1));
        var snapshots = await Sut.GetAllQuizSnapshotAsync(
            userId,
            template.Id,
            new FastQuiz.Quiz.UseCase.Queries.GetAllQuizSnapshotQuery(null, 1)
        );
        await Sut.PublishQuizTempalteAsync(userId, template.Id, true);

        // Act
        var response = await client.PostAsJsonAsync(
            UrlPackage.QuizAttemptBatchWithTemplateId(template.Id),
            new List<CreateQuizAttemptCommand>()
            {
                new CreateQuizAttemptCommand(snapshots[0].Id, 1),
            }
        );

        // Assert
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.NoContent);
        var result = await Sut.GetAllQuizAttemptByTemplateIdAsync(
            userId,
            template.Id,
            new FastQuiz.Quiz.UseCase.Queries.GetAllQuizAttemptQuery(null, 1)
        );
        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);
        result[0].QuizSampleId.ShouldBe(snapshots[0].Id);
    }

    [Test]
    public async Task GetAllQuizAttemptAsync_WhenValidRequest_ShouldReturnListOfQuizAttemptDtos()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(userId);
        var template = await Sut.CreateQuizTemplateAsync(
            userId,
            SeedDefaultCreateQuizTemplateCommand()
        );
        var group = await Sut.CreateQuestionGroupAsync(
            userId,
            new CreateQuestionGroupCommand(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())
        );
        var question = await Sut.CreateQuestionAsync(
            userId,
            group.Id,
            SeedDefaultCreateQuestionCommand()
        );
        var rule = await Sut.CreateQuestionSelectionRuleAsync(
            userId,
            template.Id,
            SeedDefaultCreateQuestionSelectionRuleCommand(group.Id)
        );
        await Sut.CreateQuizSnapshotAsync(userId, template.Id, new CreateQuizSnapshotCommand(1));
        var snapshots = await Sut.GetAllQuizSnapshotAsync(
            userId,
            template.Id,
            new FastQuiz.Quiz.UseCase.Queries.GetAllQuizSnapshotQuery(null, 1)
        );
        await Sut.PublishQuizTempalteAsync(userId, template.Id, true);
        var createdAttempt = await Sut.CreateQuizAttemptAsync(userId, template.Id);

        // Act
        var response = await client.GetAsync(
            UrlPackage.QuizAttemptWithTemplateId(template.Id, null)
        );

        // Assert
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        var result = await response.ReadFromJsonWithEnumAsync<List<QuizAttempDto>>();
        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);
        result[0].Id.ShouldBe(createdAttempt.Id);
    }

    [Test]
    public async Task GetMyQuizAttemptsAsync_WhenValidRequest_ShouldReturnListOfQuizAttemptDtos()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(userId);
        var template = await Sut.CreateQuizTemplateAsync(
            userId,
            SeedDefaultCreateQuizTemplateCommand()
        );
        var group = await Sut.CreateQuestionGroupAsync(
            userId,
            new CreateQuestionGroupCommand(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())
        );
        var question = await Sut.CreateQuestionAsync(
            userId,
            group.Id,
            SeedDefaultCreateQuestionCommand()
        );
        var rule = await Sut.CreateQuestionSelectionRuleAsync(
            userId,
            template.Id,
            SeedDefaultCreateQuestionSelectionRuleCommand(group.Id)
        );
        await Sut.CreateQuizSnapshotAsync(userId, template.Id, new CreateQuizSnapshotCommand(1));
        var snapshots = await Sut.GetAllQuizSnapshotAsync(
            userId,
            template.Id,
            new FastQuiz.Quiz.UseCase.Queries.GetAllQuizSnapshotQuery(null, 1)
        );
        await Sut.PublishQuizTempalteAsync(userId, template.Id, true);
        var createdAttempt = await Sut.CreateQuizAttemptAsync(userId, template.Id);

        // Act
        var response = await client.GetAsync(UrlPackage.MyQuizAttemptWithTemplateId());

        // Assert
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        var result = await response.ReadFromJsonWithEnumAsync<List<QuizAttempDto>>();
        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);
        result[0].Id.ShouldBe(createdAttempt.Id);
    }

    [Test]
    public async Task GetQuizAttemptByIdAsync_WhenValidRequest_ShouldReturnQuizAttemptDto()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(userId);
        var template = await Sut.CreateQuizTemplateAsync(
            userId,
            SeedDefaultCreateQuizTemplateCommand()
        );
        var group = await Sut.CreateQuestionGroupAsync(
            userId,
            new CreateQuestionGroupCommand(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())
        );
        var question = await Sut.CreateQuestionAsync(
            userId,
            group.Id,
            SeedDefaultCreateQuestionCommand()
        );
        var rule = await Sut.CreateQuestionSelectionRuleAsync(
            userId,
            template.Id,
            SeedDefaultCreateQuestionSelectionRuleCommand(group.Id)
        );
        await Sut.CreateQuizSnapshotAsync(userId, template.Id, new CreateQuizSnapshotCommand(1));
        var snapshots = await Sut.GetAllQuizSnapshotAsync(
            userId,
            template.Id,
            new FastQuiz.Quiz.UseCase.Queries.GetAllQuizSnapshotQuery(null, 1)
        );
        await Sut.PublishQuizTempalteAsync(userId, template.Id, true);
        var createdAttempt = await Sut.CreateQuizAttemptAsync(userId, template.Id);

        // Act
        var response = await client.GetAsync(UrlPackage.QuizAttemptWithId(createdAttempt.Id));

        // Assert
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        var result = await response.ReadFromJsonWithEnumAsync<QuizAttempDto>();
        result.ShouldNotBeNull();
        result.Id.ShouldBe(createdAttempt.Id);
    }

    [Test]
    public async Task PutQuizAttemptStatusAsync_WhenValidRequest_ShouldReturnNoContent()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(userId);
        var template = await Sut.CreateQuizTemplateAsync(
            userId,
            SeedDefaultCreateQuizTemplateCommand()
        );
        var group = await Sut.CreateQuestionGroupAsync(
            userId,
            new CreateQuestionGroupCommand(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())
        );
        var question = await Sut.CreateQuestionAsync(
            userId,
            group.Id,
            SeedDefaultCreateQuestionCommand()
        );
        var rule = await Sut.CreateQuestionSelectionRuleAsync(
            userId,
            template.Id,
            SeedDefaultCreateQuestionSelectionRuleCommand(group.Id)
        );
        await Sut.CreateQuizSnapshotAsync(userId, template.Id, new CreateQuizSnapshotCommand(1));
        var snapshots = await Sut.GetAllQuizSnapshotAsync(
            userId,
            template.Id,
            new FastQuiz.Quiz.UseCase.Queries.GetAllQuizSnapshotQuery(null, 1)
        );
        await Sut.PublishQuizTempalteAsync(userId, template.Id, true);
        var createdAttempt = await Sut.CreateQuizAttemptAsync(userId, template.Id);

        // Act
        var response = await client.PutAsJsonAsync(
            UrlPackage.QuizAttemptWithId(createdAttempt.Id)
                + $"?status={QuizAttemptStatus.InProgress}",
            new { }
        );

        // Assert
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.NoContent);
        var result = await Sut.GetQuizAttemptByIdAsync(userId, createdAttempt.Id);
        result.ShouldNotBeNull();
        result.Id.ShouldBe(createdAttempt.Id);
        result.Status.ShouldBe(QuizAttemptStatus.InProgress);
    }

    [Test]
    public async Task GetQuestionWithOutAnswerByAttemptIdAsync_WhenValidRequest_ShouldReturnListOfQuestionRecordWithoutAnswerDtos()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(userId);
        var template = await Sut.CreateQuizTemplateAsync(
            userId,
            SeedDefaultCreateQuizTemplateCommand()
        );
        var group = await Sut.CreateQuestionGroupAsync(
            userId,
            new CreateQuestionGroupCommand(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())
        );
        var question = await Sut.CreateQuestionAsync(
            userId,
            group.Id,
            SeedDefaultCreateQuestionCommand()
        );
        var rule = await Sut.CreateQuestionSelectionRuleAsync(
            userId,
            template.Id,
            SeedDefaultCreateQuestionSelectionRuleCommand(group.Id)
        );
        await Sut.CreateQuizSnapshotAsync(userId, template.Id, new CreateQuizSnapshotCommand(1));
        var snapshots = await Sut.GetAllQuizSnapshotAsync(
            userId,
            template.Id,
            new FastQuiz.Quiz.UseCase.Queries.GetAllQuizSnapshotQuery(null, 1)
        );
        await Sut.PublishQuizTempalteAsync(userId, template.Id, true);
        var createdAttempt = await Sut.CreateQuizAttemptAsync(userId, template.Id);
        await Sut.UpdateQuizAttemptStatusAsync(
            userId,
            createdAttempt.Id,
            QuizAttemptStatus.InProgress
        );

        // Act
        var response = await client.GetAsync(
            UrlPackage.QuizAttemptQuestionWithOutAnswerWithId(createdAttempt.Id)
        );

        // Assert
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        var result = await response.ReadFromJsonWithEnumAsync<
            List<QuestionRecordWithoutAnswerDto>
        >();
        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);
    }

    [Test]
    public async Task GetQuestionWithAnswerByAttemptIdAsync_WhenValidRequest_ShouldReturnListOfQuestionRecordWithAnswerDtos()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(userId);
        var template = await Sut.CreateQuizTemplateAsync(
            userId,
            SeedDefaultCreateQuizTemplateCommand()
        );
        var group = await Sut.CreateQuestionGroupAsync(
            userId,
            new CreateQuestionGroupCommand(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())
        );
        var question = await Sut.CreateQuestionAsync(
            userId,
            group.Id,
            SeedDefaultCreateQuestionCommand()
        );
        var rule = await Sut.CreateQuestionSelectionRuleAsync(
            userId,
            template.Id,
            SeedDefaultCreateQuestionSelectionRuleCommand(group.Id)
        );
        await Sut.CreateQuizSnapshotAsync(userId, template.Id, new CreateQuizSnapshotCommand(1));
        var snapshots = await Sut.GetAllQuizSnapshotAsync(
            userId,
            template.Id,
            new FastQuiz.Quiz.UseCase.Queries.GetAllQuizSnapshotQuery(null, 1)
        );
        await Sut.PublishQuizTempalteAsync(userId, template.Id, true);
        var createdAttempt = await Sut.CreateQuizAttemptAsync(userId, template.Id);
        await Sut.UpdateQuizAttemptStatusAsync(
            userId,
            createdAttempt.Id,
            QuizAttemptStatus.InProgress
        );
        await Sut.UpdateQuizAttemptStatusAsync(
            userId,
            createdAttempt.Id,
            QuizAttemptStatus.Submitted
        );
        await Sut.UpdateQuizAttemptStatusAsync(userId, createdAttempt.Id, QuizAttemptStatus.Graded);

        // Act
        var response = await client.GetAsync(
            UrlPackage.QuizAttemptQuestionWithAnswerWithId(createdAttempt.Id)
        );

        // Assert
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        var result = await response.ReadFromJsonWithEnumAsync<
            List<QuestionRecordWithoutAnswerDto>
        >();
        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);
    }

    [Test]
    public async Task PutAnswerQuestionAsync_WhenValidRequest_ShouldReturnNoContent()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(userId);
        var template = await Sut.CreateQuizTemplateAsync(
            userId,
            SeedDefaultCreateQuizTemplateCommand()
        );
        var group = await Sut.CreateQuestionGroupAsync(
            userId,
            new CreateQuestionGroupCommand(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())
        );
        var question = await Sut.CreateQuestionAsync(
            userId,
            group.Id,
            SeedDefaultCreateQuestionCommand()
        );
        var rule = await Sut.CreateQuestionSelectionRuleAsync(
            userId,
            template.Id,
            SeedDefaultCreateQuestionSelectionRuleCommand(group.Id)
        );
        await Sut.CreateQuizSnapshotAsync(userId, template.Id, new CreateQuizSnapshotCommand(1));
        var snapshots = await Sut.GetAllQuizSnapshotAsync(
            userId,
            template.Id,
            new FastQuiz.Quiz.UseCase.Queries.GetAllQuizSnapshotQuery(null, 1)
        );
        await Sut.PublishQuizTempalteAsync(userId, template.Id, true);
        var createdAttempt = await Sut.CreateQuizAttemptAsync(userId, template.Id);
        await Sut.UpdateQuizAttemptStatusAsync(
            userId,
            createdAttempt.Id,
            QuizAttemptStatus.InProgress
        );
        var response1 = await client.GetAsync(
            UrlPackage.QuizAttemptQuestionWithOutAnswerWithId(createdAttempt.Id)
        );
        response1.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        var questions = await response1.ReadFromJsonWithEnumAsync<
            List<QuestionRecordWithoutAnswerDto>
        >();
        questions.ShouldNotBeNull();
        questions.Count.ShouldBe(1);

        // Act
        var response2 = await client.PutAsJsonAsync(
            UrlPackage.QuizAttemptQuestionWithQuestionId(questions[0].Id),
            new List<SetAnswerRecordCommand>()
            {
                new SetAnswerRecordCommand(questions[0].Answers[0].Id),
                new SetAnswerRecordCommand(questions[0].Answers[1].Id),
            }
        );

        // Assert
        response2.StatusCode.ShouldBe(System.Net.HttpStatusCode.NoContent);

        var response3 = await client.GetAsync(
            UrlPackage.QuizAttemptQuestionWithOutAnswerWithId(createdAttempt.Id)
        );

        response3.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        var questions2 = await response3.ReadFromJsonWithEnumAsync<
            List<QuestionRecordWithoutAnswerDto>
        >();
        questions2.ShouldNotBeNull();
        questions2.Count.ShouldBe(1);
        questions2[0].AnswerRecords.Count.ShouldBe(2);

        var clearResponse = await client.PutAsJsonAsync(
            UrlPackage.QuizAttemptQuestionWithQuestionId(questions[0].Id),
            Array.Empty<SetAnswerRecordCommand>()
        );
        clearResponse.StatusCode.ShouldBe(System.Net.HttpStatusCode.NoContent);

        var response4 = await client.GetAsync(
            UrlPackage.QuizAttemptQuestionWithOutAnswerWithId(createdAttempt.Id)
        );
        var questionsAfterClear = await response4.ReadFromJsonWithEnumAsync<
            List<QuestionRecordWithoutAnswerDto>
        >();
        questionsAfterClear.ShouldNotBeNull();
        questionsAfterClear[0].AnswerRecords.ShouldBeEmpty();
    }

    [Test]
    public async Task PutAnswerQuestionAsync_WhenAttemptBelongsToAnotherUser_ShouldReturnForbidden()
    {
        var ownerId = User.DefaultAdminId;
        var (_, question) = await CreateInProgressAttemptWithQuestionAsync(ownerId);
        var otherUserClient = Sut.CreateClient(Guid.NewGuid().ToString());

        var response = await otherUserClient.PutAsJsonAsync(
            UrlPackage.QuizAttemptQuestionWithQuestionId(question.Id),
            new[] { new SetAnswerRecordCommand(question.Answers[0].Id) }
        );

        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task PutReviewQuestionRecordAsync_WhenValidRequest_ShouldReturnNoContent()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(userId);
        var template = await Sut.CreateQuizTemplateAsync(
            userId,
            SeedDefaultCreateQuizTemplateCommand()
        );
        var group = await Sut.CreateQuestionGroupAsync(
            userId,
            new CreateQuestionGroupCommand(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())
        );
        var question = await Sut.CreateQuestionAsync(
            userId,
            group.Id,
            SeedDefaultCreateQuestionCommand(QuestionType.FillIn)
        );
        var rule = await Sut.CreateQuestionSelectionRuleAsync(
            userId,
            template.Id,
            SeedDefaultCreateQuestionSelectionRuleCommand(group.Id, type: QuestionType.FillIn)
        );
        await Sut.CreateQuizSnapshotAsync(userId, template.Id, new CreateQuizSnapshotCommand(1));
        var snapshots = await Sut.GetAllQuizSnapshotAsync(
            userId,
            template.Id,
            new FastQuiz.Quiz.UseCase.Queries.GetAllQuizSnapshotQuery(null, 1)
        );
        await Sut.PublishQuizTempalteAsync(userId, template.Id, true);
        var createdAttempt = await Sut.CreateQuizAttemptAsync(userId, template.Id);
        await Sut.UpdateQuizAttemptStatusAsync(
            userId,
            createdAttempt.Id,
            QuizAttemptStatus.InProgress
        );
        var response1 = await client.GetAsync(
            UrlPackage.QuizAttemptQuestionWithOutAnswerWithId(createdAttempt.Id)
        );
        response1.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        var questions = await response1.ReadFromJsonWithEnumAsync<
            List<QuestionRecordWithoutAnswerDto>
        >();
        questions.ShouldNotBeNull();
        questions.Count.ShouldBe(1);

        await Sut.UpdateQuizAttemptStatusAsync(
            userId,
            createdAttempt.Id,
            QuizAttemptStatus.Submitted
        );

        // Act
        var response2 = await client.PutAsJsonAsync(
            UrlPackage.QuizAttemptQuestionStatusWithQuestionId(questions[0].Id, true),
            new { }
        );

        // Assert
        response2.StatusCode.ShouldBe(System.Net.HttpStatusCode.NoContent);
        var response3 = await client.GetAsync(
            UrlPackage.QuizAttemptQuestionWithAnswerWithId(createdAttempt.Id)
        );

        response3.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        await Sut.UpdateQuizAttemptStatusAsync(userId, createdAttempt.Id, QuizAttemptStatus.Graded);
        var questions2 = await response3.ReadFromJsonWithEnumAsync<
            List<QuestionRecordWithAnswerDto>
        >();
        questions2.ShouldNotBeNull();
        questions2.Count.ShouldBe(1);
        (questions2[0].IsCorrect == true).ShouldBeTrue();
    }

    [Test]
    public async Task PutReviewQuestionRecordAsync_WhenReviewerHasNoPermission_ShouldReturnForbidden()
    {
        var ownerId = User.DefaultAdminId;
        var (attempt, question) = await CreateInProgressAttemptWithQuestionAsync(ownerId);
        await Sut.UpdateQuizAttemptStatusAsync(
            ownerId,
            attempt.Id,
            QuizAttemptStatus.Submitted
        );
        var unauthorizedClient = Sut.CreateClient(Guid.NewGuid().ToString());

        var response = await unauthorizedClient.PutAsJsonAsync(
            UrlPackage.QuizAttemptQuestionStatusWithQuestionId(question.Id, true),
            new { }
        );

        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task PutQuizAttemptStatusAsync_WhenManualQuestionIsUnreviewed_ShouldRejectCompletion()
    {
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(userId);
        var (attempt, question) = await CreateInProgressAttemptWithQuestionAsync(userId);
        await Sut.UpdateQuizAttemptStatusAsync(
            userId,
            attempt.Id,
            QuizAttemptStatus.Submitted
        );

        var beforeReviewResponse = await client.GetAsync(
            UrlPackage.QuizAttemptQuestionWithAnswerWithId(attempt.Id)
        );
        var beforeReview = await beforeReviewResponse.ReadFromJsonWithEnumAsync<
            List<QuestionRecordWithAnswerDto>
        >();
        beforeReview.ShouldNotBeNull();
        beforeReview[0].IsCorrect.ShouldBeNull();

        var completeResponse = await client.PutAsJsonAsync(
            UrlPackage.QuizAttemptWithId(attempt.Id) + $"?status={QuizAttemptStatus.Graded}",
            new { }
        );
        completeResponse.StatusCode.ShouldBe(System.Net.HttpStatusCode.BadRequest);

        var reviewResponse = await client.PutAsJsonAsync(
            UrlPackage.QuizAttemptQuestionStatusWithQuestionId(question.Id, false),
            new { }
        );
        reviewResponse.StatusCode.ShouldBe(System.Net.HttpStatusCode.NoContent);

        var afterReviewResponse = await client.GetAsync(
            UrlPackage.QuizAttemptQuestionWithAnswerWithId(attempt.Id)
        );
        var afterReview = await afterReviewResponse.ReadFromJsonWithEnumAsync<
            List<QuestionRecordWithAnswerDto>
        >();
        afterReview.ShouldNotBeNull();
        afterReview[0].IsCorrect.ShouldBe(false);
    }

    [Test]
    public async Task AnswerAfterSubmission_ShouldRejectAndKeepCommittedAnswer()
    {
        var (attempt, question) = await CreateInProgressAttemptWithQuestionAsync(User.DefaultAdminId);
        var client = Sut.CreateClient(User.DefaultAdminId);
        var url = UrlPackage.QuizAttemptQuestionWithQuestionId(question.Id);
        (await client.PutAsJsonAsync(url, new[] { new SetAnswerRecordCommand("first") }))
            .StatusCode.ShouldBe(System.Net.HttpStatusCode.NoContent);
        await Sut.UpdateQuizAttemptStatusAsync(User.DefaultAdminId, attempt.Id, QuizAttemptStatus.Submitted);

        (await client.PutAsJsonAsync(url, new[] { new SetAnswerRecordCommand("after-submit") }))
            .StatusCode.ShouldBe(System.Net.HttpStatusCode.BadRequest);
        var response = await client.GetAsync(UrlPackage.QuizAttemptQuestionWithAnswerWithId(attempt.Id));
        var records = await response.ReadFromJsonWithEnumAsync<List<QuestionRecordWithAnswerDto>>();
        records!.Single().AnswerRecords.Single().Value.ShouldBe("first");
    }

    [Test]
    public async Task OtherMember_ShouldNotReadActiveAnswers()
    {
        var (attempt, _) = await CreateInProgressAttemptWithQuestionAsync(User.DefaultAdminId);
        var otherUser = await CreateMemberAsync(false);
        var response = await Sut.CreateClient(otherUser).GetAsync(
            UrlPackage.QuizAttemptQuestionWithOutAnswerWithId(attempt.Id));
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task OtherMember_ShouldNotReadGradedResults()
    {
        var (attempt, _) = await CreateInProgressAttemptWithQuestionAsync(
            User.DefaultAdminId, QuestionType.SingleChoice);
        await Sut.UpdateQuizAttemptStatusAsync(User.DefaultAdminId, attempt.Id, QuizAttemptStatus.Submitted);
        await Sut.UpdateQuizAttemptStatusAsync(User.DefaultAdminId, attempt.Id, QuizAttemptStatus.Graded);
        var otherUser = await CreateMemberAsync(false);
        var response = await Sut.CreateClient(otherUser).GetAsync(
            UrlPackage.QuizAttemptQuestionWithAnswerWithId(attempt.Id));
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task ReviewerWithoutSystemPermission_CanOpenJudgeAndCompleteAnotherUsersAttempt()
    {
        var (attempt, question) = await CreateInProgressAttemptWithQuestionAsync(User.DefaultAdminId);
        await Sut.UpdateQuizAttemptStatusAsync(User.DefaultAdminId, attempt.Id, QuizAttemptStatus.Submitted);
        var reviewerId = await CreateMemberAsync(true);
        var client = Sut.CreateClient(reviewerId);
        (await client.GetAsync(UrlPackage.QuizAttemptWithId(attempt.Id)))
            .StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        (await client.GetAsync(UrlPackage.QuizAttemptQuestionWithAnswerWithId(attempt.Id)))
            .StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        (await client.PutAsJsonAsync(UrlPackage.QuizAttemptQuestionStatusWithQuestionId(question.Id, false), new { }))
            .StatusCode.ShouldBe(System.Net.HttpStatusCode.NoContent);
        (await client.PutAsJsonAsync(UrlPackage.QuizAttemptWithId(attempt.Id) + "?status=Graded", new { }))
            .StatusCode.ShouldBe(System.Net.HttpStatusCode.NoContent);
    }

    [Test]
    public async Task QuestionReadsAndWrites_RejectMismatchedTenant()
    {
        var (attempt, question) = await CreateInProgressAttemptWithQuestionAsync(User.DefaultAdminId);
        var client = Sut.CreateClient(User.DefaultAdminId);
        string ForeignTenant(string url) => url.Replace(Tenant.DefaultTenantId, "unrelated-tenant");
        (await client.GetAsync(ForeignTenant(UrlPackage.QuizAttemptQuestionWithOutAnswerWithId(attempt.Id))))
            .StatusCode.ShouldBe(System.Net.HttpStatusCode.Forbidden);
        (await client.PutAsJsonAsync(ForeignTenant(UrlPackage.QuizAttemptQuestionWithQuestionId(question.Id)),
            new[] { new SetAnswerRecordCommand("answer") }))
            .StatusCode.ShouldBe(System.Net.HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task CorrectChoice_ShouldReceiveFullScoreThroughActualAnswerAndReviewFlow()
    {
        var (attempt, question) = await CreateInProgressAttemptWithQuestionAsync(
            User.DefaultAdminId, QuestionType.SingleChoice);
        var client = Sut.CreateClient(User.DefaultAdminId);
        (await client.PutAsJsonAsync(UrlPackage.QuizAttemptQuestionWithQuestionId(question.Id),
            new[] { new SetAnswerRecordCommand(question.Answers.Single(x => x.SortOrder == 1).Id) }))
            .StatusCode.ShouldBe(System.Net.HttpStatusCode.NoContent);
        await Sut.UpdateQuizAttemptStatusAsync(User.DefaultAdminId, attempt.Id, QuizAttemptStatus.Submitted);
        await Sut.UpdateQuizAttemptStatusAsync(User.DefaultAdminId, attempt.Id, QuizAttemptStatus.Graded);
        var result = await Sut.GetQuizAttemptByIdAsync(User.DefaultAdminId, attempt.Id);
        result.Score.ShouldBe(1);
        result.Status.ShouldBe(QuizAttemptStatus.Graded);
    }

    private async Task<string> CreateMemberAsync(bool canReview)
    {
        var auth = await Sut.RegisterAsync(new UserRegisterCommand(Guid.NewGuid().ToString("N")[..16], "test-password", null));
        await using var scope = Sut.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<ITenantRepository>()
            .AddUserToTenantAsync(Tenant.DefaultTenantId, auth.User.Id);
        var permissionService = scope.ServiceProvider.GetRequiredService<IPermissionService>();
        (await permissionService.HasPermissionAsync(Tenant.DefaultTenantId, auth.User.Id, Permissions.QuizAttempt))
            .ShouldBeFalse(); // Fill the negative cache before granting a role.
        if (canReview)
        {
            var roles = scope.ServiceProvider.GetRequiredService<IRoleService>();
            var role = await roles.CreateAsync(User.DefaultAdminId, Tenant.DefaultTenantId,
                new CreateRoleCommand(Guid.NewGuid().ToString(), "review only", false, true, [Permissions.QuizAttempt]));
            await roles.AddRoleToUserAsync(User.DefaultAdminId, auth.User.Id, role.Id);
            (await permissionService.HasPermissionAsync(Tenant.DefaultTenantId, auth.User.Id, Permissions.SystemAdmin))
                .ShouldBeFalse();
        }
        return auth.User.Id;
    }

    private async Task<(
        QuizAttempDto Attempt,
        QuestionRecordWithoutAnswerDto Question
    )> CreateInProgressAttemptWithQuestionAsync(string userId, QuestionType type = QuestionType.FillIn)
    {
        var template = await Sut.CreateQuizTemplateAsync(
            userId,
            SeedDefaultCreateQuizTemplateCommand()
        );
        var group = await Sut.CreateQuestionGroupAsync(
            userId,
            new CreateQuestionGroupCommand(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())
        );
        await Sut.CreateQuestionAsync(userId, group.Id, SeedDefaultCreateQuestionCommand(type));
        await Sut.CreateQuestionSelectionRuleAsync(
            userId,
            template.Id,
            SeedDefaultCreateQuestionSelectionRuleCommand(group.Id, type: type)
        );
        await Sut.CreateQuizSnapshotAsync(userId, template.Id, new CreateQuizSnapshotCommand(1));
        await Sut.PublishQuizTempalteAsync(userId, template.Id, true);
        var attempt = await Sut.CreateQuizAttemptAsync(userId, template.Id);
        await Sut.UpdateQuizAttemptStatusAsync(userId, attempt.Id, QuizAttemptStatus.InProgress);

        var client = Sut.CreateClient(userId);
        var response = await client.GetAsync(
            UrlPackage.QuizAttemptQuestionWithOutAnswerWithId(attempt.Id)
        );
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        var questions = await response.ReadFromJsonWithEnumAsync<
            List<QuestionRecordWithoutAnswerDto>
        >();
        questions.ShouldNotBeNull();
        questions.Count.ShouldBe(1);
        return (attempt, questions[0]);
    }

    private static CreateQuizTemplateCommand SeedDefaultCreateQuizTemplateCommand(
        string? name = null,
        string? description = null
    )
    {
        var now = DateTimeOffset.UtcNow;
        var startTime = now.AddMinutes(10);
        var endTime = now.AddMinutes(30);

        return new CreateQuizTemplateCommand(
            name ?? Guid.NewGuid().ToString(),
            description ?? Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            1,
            true,
            startTime,
            endTime,
            10,
            null
        );
    }

    private static CreateQuestionSelectionRuleCommand SeedDefaultCreateQuestionSelectionRuleCommand(
        string? groupId = null,
        int level = 0,
        int count = 1,
        int score = 1,
        QuestionType type = QuestionType.SingleChoice
    )
    {
        return new CreateQuestionSelectionRuleCommand(
            groupId ?? Guid.NewGuid().ToString(),
            type,
            level,
            count,
            score
        );
    }

    private async Task<QuestionGroupDto> CreateRamdomQuestionGroupAsync(string userId)
    {
        var command = new CreateQuestionGroupCommand(
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString()
        );
        return await Sut.CreateQuestionGroupAsync(userId, command);
    }

    private static CreateQuestionCommand SeedDefaultCreateQuestionCommand(
        QuestionType type = QuestionType.SingleChoice,
        int level = 0
    )
    {
        return new CreateQuestionCommand(
            Guid.NewGuid().ToString(),
            level,
            type,
            [
                new CreateQuestionAnswerCommand(
                    Guid.NewGuid().ToString(),
                    false,
                    0,
                    type == QuestionType.FillIn ? MatchStrategy.Manual : MatchStrategy.Exact
                ),
                new CreateQuestionAnswerCommand(
                    Guid.NewGuid().ToString(),
                    true,
                    1,
                    type == QuestionType.FillIn ? MatchStrategy.Manual : MatchStrategy.Exact
                ),
            ]
        );
    }
}
