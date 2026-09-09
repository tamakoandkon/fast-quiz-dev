using System.Net.Http.Json;
using FastQuiz.API.Test.Utils;
using FastQuiz.API.Test.Utils.Helpers;
using FastQuiz.Auth.Domain.Entities;
using FastQuiz.Question.Domain.Entities;
using FastQuiz.Question.UseCase.Command;
using FastQuiz.Question.UseCase.Dto;
using FastQuiz.Quiz.Domain.Entities;
using FastQuiz.Quiz.UseCase.Commands;
using FastQuiz.Quiz.UseCase.Dtos;
using Shouldly;

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
    [Ignore("底层方法有改动")]
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
        questions2[0].AnswerRecords.Count.ShouldBe(1);
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
        questions2[0].IsCorrect.ShouldBeTrue();
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
        int score = 1
    )
    {
        return new CreateQuestionSelectionRuleCommand(
            groupId ?? Guid.NewGuid().ToString(),
            QuestionType.SingleChoice,
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
                    MatchStrategy.Manual
                ),
                new CreateQuestionAnswerCommand(
                    Guid.NewGuid().ToString(),
                    true,
                    1,
                    MatchStrategy.Manual
                ),
            ]
        );
    }
}
