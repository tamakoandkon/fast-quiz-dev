using System.Net.Http.Json;
using FastQuiz.API.Test.Utils;
using FastQuiz.API.Test.Utils.Helpers;
using FastQuiz.Auth.Domain.Entities;
using FastQuiz.Question.Domain.Entities;
using FastQuiz.Question.UseCase.Command;
using FastQuiz.Question.UseCase.Dto;
using FastQuiz.Quiz.UseCase.Commands;
using FastQuiz.Quiz.UseCase.Dtos;
using Shouldly;

namespace FastQuiz.API.Test.Quiz;

internal class QuizSnapshotTests : ApiTestBase
{
    [SetUp]
    public async Task Setup()
    {
        await Sut.CleanQuizSqliteDb();
    }

    [Test]
    public async Task PostQuizSnapshotAsync_WhenValidRequest_ShouldReturnNoContent()
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

        // Act
        var response = await client.PostAsJsonAsync(
            UrlPackage.QuizSnapshotWithTemplate(template.Id),
            new CreateQuizSnapshotCommand(1)
        );

        // Assert
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.NoContent);
        var result = await Sut.GetAllQuizSnapshotAsync(
            userId,
            template.Id,
            new FastQuiz.Quiz.UseCase.Queries.GetAllQuizSnapshotQuery(null, 10)
        );
        result.Count.ShouldBe(1);
    }

    [Test]
    public async Task PostQuizSnapshotAsync_WhenUnauthorized_ShouldReturnUnauthorized()
    {
        // Arrange
        var client = Sut.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync(
            UrlPackage.QuizSnapshotWithTemplate(Guid.NewGuid().ToString()),
            new CreateQuizSnapshotCommand(1)
        );

        // Assert
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task GetAllQuizSnapshotAsync_WhenValidRequest_ShouldReturnList()
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

        // Act
        var response = await client.GetAsync(
            UrlPackage.QuizSnapshotWithTemplate(template.Id, null)
        );

        // Assert
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        var result = await response.ReadFromJsonWithEnumAsync<List<QuizSnapshotDto>>();
        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);
    }

    [Test]
    public async Task GetAllQuizSnapshotAsync_WhenUnauthorized_ShouldReturnUnauthorized()
    {
        // Arrange
        var client = Sut.CreateClient();

        // Act
        var response = await client.GetAsync(
            UrlPackage.QuizSnapshotWithTemplate(Guid.NewGuid().ToString(), null)
        );

        // Assert
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task GetQuizSnapshotByIdAsync_WhenValidRequest_ShouldReturnDto()
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

        // Act
        var response = await client.GetAsync(UrlPackage.QuizSnapshotWithId(snapshots[0].Id));

        // Assert
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        var result = await response.ReadFromJsonWithEnumAsync<QuizSnapshotDto>();
        result.ShouldNotBeNull();
        result.Id.ShouldBe(snapshots[0].Id);
    }

    [Test]
    public async Task GetQuizSnapshotByIdAsync_WhenNotFound_ShouldThrowException()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(userId);

        // Act
        var response = await client.GetAsync(
            UrlPackage.QuizSnapshotWithId(Guid.NewGuid().ToString())
        );

        // Assert
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.NotFound);
    }

    [Test]
    public async Task GetQuizSnapshotByIdAsync_WhenUnauthorized_ShouldReturnUnauthorized()
    {
        // Arrange
        var client = Sut.CreateClient();

        // Act
        var response = await client.GetAsync(
            UrlPackage.QuizSnapshotWithId(Guid.NewGuid().ToString())
        );

        // Assert
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task DeleteQuizSnapshotByIdAsync_WhenValidRequest_ShouldReturnNoContent()
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

        // Act
        var response = await client.DeleteAsync(UrlPackage.QuizSnapshotWithId(snapshots[0].Id));

        // Assert
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.NoContent);
        var result = await Sut.GetAllQuizSnapshotAsync(
            userId,
            template.Id,
            new FastQuiz.Quiz.UseCase.Queries.GetAllQuizSnapshotQuery(null, 1)
        );
        result.ShouldNotBeNull();
        result.Count.ShouldBe(0);
    }

    [Test]
    public async Task DeleteQuizSnapshotByIdAsync_WhenNotFound_ShouldThrowException()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(userId);

        // Act
        var response = await client.DeleteAsync(
            UrlPackage.QuizSnapshotWithId(Guid.NewGuid().ToString())
        );

        // Assert
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.NoContent);
    }

    [Test]
    public async Task DeleteQuizSnapshotByIdAsync_WhenUnauthorized_ShouldReturnUnauthorized()
    {
        // Arrange
        var client = Sut.CreateClient();

        // Act
        var response = await client.DeleteAsync(
            UrlPackage.QuizSnapshotWithId(Guid.NewGuid().ToString())
        );

        // Assert
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task GetAllQuestionSnapshotByQuizSnapshotIdAsync_WhenValidRequest_ShouldReturnList()
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

        // Act
        var response = await client.GetAsync(
            UrlPackage.QuestionRecordWithQuizSnapshotId(snapshots[0].Id, null)
        );

        // Assert
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        var result = await response.ReadFromJsonWithEnumAsync<List<QuestionRecordWithAnswerDto>>();
        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);
    }

    [Test]
    public async Task GetAllQuestionSnapshotByQuizSnapshotIdAsync_WhenUnauthorized_ShouldReturnUnauthorzied()
    {
        // Arrange
        var client = Sut.CreateClient();

        // Act
        var response = await client.GetAsync(
            UrlPackage.QuestionRecordWithQuizSnapshotId(Guid.NewGuid().ToString(), null)
        );

        // Assert
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.Unauthorized);
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
