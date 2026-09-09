using System.Net;
using System.Net.Http.Json;
using FastQuiz.API.Test.Utils;
using FastQuiz.API.Test.Utils.Helpers;
using FastQuiz.Auth.Domain.Entities;
using FastQuiz.Question.Domain.Entities;
using FastQuiz.Question.UseCase.Command;
using FastQuiz.Question.UseCase.Dto;
using Shouldly;

namespace FastQuiz.API.Test.Question;

internal class QuestionTests : ApiTestBase
{
    [SetUp]
    public async Task Setup()
    {
        await Sut.CleanQuestionSqliteDb();
    }

    [Test]
    public async Task PostQuestion_WhenQuestionGroupDoesAlreadyExist_ShouldCreateQuestionSuccess()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(userId);
        var group = await CreateRamdomQuestionGroupAsync(userId);
        var question = SeedDefaultCreateQuestionCommand();

        // Act
        var response = await client.PostAsJsonAsync(
            UrlPackage.QuestionWithGroupId(group.Id),
            question
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.ReadFromJsonWithEnumAsync<QuestionDto>();
        result.ShouldNotBeNull();
        var createdQuestion = await Sut.GetQuestionByIdAsync(result.Id);
        createdQuestion.ShouldNotBeNull();
        createdQuestion.Id.ShouldBe(result.Id);
    }

    [Test]
    public async Task PostQuestion_WhenQuestionGroupDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(userId);

        var question = SeedDefaultCreateQuestionCommand();

        // Act
        var response = await client.PostAsJsonAsync(
            UrlPackage.QuestionWithGroupId(Guid.NewGuid().ToString()),
            question
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Test]
    [TestCase("", 0)]
    [TestCase("any content", -1)]
    public async Task PostQuestion_WhenWithInvalidData_ShouldReturnUnprocessEntity(
        string content,
        int level
    )
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(userId);
        var group = await CreateRamdomQuestionGroupAsync(userId);
        var question = new CreateQuestionCommand(content, level, QuestionType.SingleChoice, []);

        // Act
        var response = await client.PostAsJsonAsync(
            UrlPackage.QuestionWithGroupId(group.Id),
            question
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
    }

    [Test]
    public async Task PostQuestion_WhenQuestionGroupDoesNotOwnedUser_ShouldReturnFobidden()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(Guid.NewGuid().ToString());
        var group = await CreateRamdomQuestionGroupAsync(userId);
        var question = SeedDefaultCreateQuestionCommand();

        // Act
        var response = await client.PostAsJsonAsync(
            UrlPackage.QuestionWithGroupId(group.Id),
            question
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task PostQuestion_WhenUnauthorized_ShouldReturnUnauthorized()
    {
        // Arrange
        var client = Sut.CreateClient();
        var question = SeedDefaultCreateQuestionCommand();

        // Act
        var response = await client.PostAsJsonAsync(
            UrlPackage.QuestionWithGroupId(Guid.NewGuid().ToString()),
            question
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task GetQuestion_WhenQuestionDoesAlreadyExist_ShouldReturnQuestionDto()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(userId);
        var group = await CreateRamdomQuestionGroupAsync(userId);
        var command = SeedDefaultCreateQuestionCommand();
        var createdQuestion = await Sut.CreateQuestionAsync(userId, group.Id, command);

        // Act
        var response = await client.GetAsync(UrlPackage.Question(createdQuestion.Id));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.ReadFromJsonWithEnumAsync<QuestionDto>();
        result.ShouldNotBeNull();
        createdQuestion.ShouldNotBeNull();
        createdQuestion.Id.ShouldBe(result.Id);
    }

    [Test]
    public async Task GetQuestion_WhenQuestionDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(userId);

        // Act
        var response = await client.GetAsync(UrlPackage.Question(Guid.NewGuid().ToString()));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task GetQuestion_WhenQuestionDoesNotOwnedUser_ShouldReturnForbidden()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var otherUserId = Guid.NewGuid().ToString();
        var client = Sut.CreateClient(otherUserId);
        var group = await CreateRamdomQuestionGroupAsync(userId);
        var command = SeedDefaultCreateQuestionCommand();
        var createdQuestion = await Sut.CreateQuestionAsync(userId, group.Id, command);

        // Act
        var response = await client.GetAsync(UrlPackage.Question(createdQuestion.Id));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task GetQuestion_WhenUnauthorized_ShouldReturnUnauthorized()
    {
        // Arrange
        var client = Sut.CreateClient();

        // Act
        var response = await client.GetAsync(
            UrlPackage.QuestionWithGroupId(Guid.NewGuid().ToString())
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task GetAllQuestion_WhenCursorIsNull_ShouldReturnLatestQuestions()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(userId);
        var group = await CreateRamdomQuestionGroupAsync(userId);
        var questions = new List<QuestionDto>();

        for (int i = 0; i < 5; i++)
        {
            var command = SeedDefaultCreateQuestionCommand();
            var question = await Sut.CreateQuestionAsync(userId, group.Id, command);
            questions.Add(question);
        }
        var expectedIds = questions.OrderByDescending(q => q.Id).Select(q => q.Id).ToList();

        // Act
        var response = await client.GetAsync(
            UrlPackage.Question(group.Id, cursor: null, count: 10)
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.ReadFromJsonWithEnumAsync<List<QuestionDto>>();
        result.ShouldNotBeNull();
        result.Count.ShouldBe(5);
        result.Select(q => q.Id).ShouldBe(expectedIds);
    }

    [Test]
    public async Task GetAllQuestion_WhenCursorIsNotNull_ShouldReturnQuestionsOfLessThanCursor()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(userId);
        var group = await CreateRamdomQuestionGroupAsync(userId);
        var questions = new List<QuestionDto>();

        // Create 5 questions
        for (int i = 0; i < 5; i++)
        {
            var command = SeedDefaultCreateQuestionCommand();
            var question = await Sut.CreateQuestionAsync(userId, group.Id, command);
            questions.Add(question);
        }

        var sortedQuestions = questions.OrderByDescending(q => q.Id).ToList();
        var cursor = sortedQuestions[2].Id;
        var expectedIds = sortedQuestions.Skip(3).Select(q => q.Id).ToList();

        // Act
        var response = await client.GetAsync(
            UrlPackage.Question(group.Id, cursor: cursor, count: 10)
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.ReadFromJsonWithEnumAsync<List<QuestionDto>>();
        result.ShouldNotBeNull();
        result.Select(q => q.Id).ShouldBe(expectedIds);
    }

    [Test]
    public async Task GetAllQuestion_WhenGroupDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(userId);
        var nonExistentGroupId = Guid.NewGuid().ToString();

        // Act
        var response = await client.GetAsync(
            UrlPackage.Question(nonExistentGroupId, cursor: null, count: 10)
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task GetAllQuestion_WhenWithCondition_ShouldReturnQuestionsOfMatchCondition()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(userId);
        var group = await CreateRamdomQuestionGroupAsync(userId);
        var questions = new List<QuestionDto>();

        // Create questions with different types and levels
        var command1 = SeedDefaultCreateQuestionCommand(QuestionType.SingleChoice, 1);
        var question1 = await Sut.CreateQuestionAsync(userId, group.Id, command1);

        var command2 = SeedDefaultCreateQuestionCommand(QuestionType.MultipleChoice, 2);
        var question2 = await Sut.CreateQuestionAsync(userId, group.Id, command2);

        var command3 = SeedDefaultCreateQuestionCommand(QuestionType.SingleChoice, 3);
        var question3 = await Sut.CreateQuestionAsync(userId, group.Id, command3);

        var expectedIds = new List<string> { question1.Id, question3.Id }
            .OrderByDescending(x => x)
            .ToList();

        // Act
        var response = await client.GetAsync(
            UrlPackage.Question(
                group.Id,
                cursor: null,
                count: 10,
                type: QuestionType.SingleChoice,
                level: null
            )
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.ReadFromJsonWithEnumAsync<List<QuestionDto>>();
        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);
        result.Select(q => q.Id).OrderBy(id => id).ShouldBe(expectedIds.OrderBy(id => id));
    }

    [Test]
    public async Task GetAllQuestion_WhenIsPreviousIsTrue_ShouldReturnQuestionsOfGreaterThanCursor()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(userId);
        var group = await CreateRamdomQuestionGroupAsync(userId);
        var questions = new List<QuestionDto>();

        // Create 5 questions
        for (int i = 0; i < 5; i++)
        {
            var command = SeedDefaultCreateQuestionCommand();
            var question = await Sut.CreateQuestionAsync(userId, group.Id, command);
            questions.Add(question);
        }

        var sortedQuestions = questions.OrderByDescending(q => q.Id).ToList();
        var cursor = sortedQuestions[2].Id;
        var expectedIds = sortedQuestions.Take(2).Select(q => q.Id).ToList();

        // Act
        var response = await client.GetAsync(
            UrlPackage.Question(
                group.Id,
                cursor: cursor,
                count: 10,
                type: null,
                level: null,
                isPrevious: true
            )
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.ReadFromJsonWithEnumAsync<List<QuestionDto>>();
        result.ShouldNotBeNull();
        result.Select(q => q.Id).ShouldBe(expectedIds);
    }

    [Test]
    public async Task GetAllQuestion_WhenUnauthorized_ShouldReturnUnauthorized()
    {
        // Arrange
        var client = Sut.CreateClient(); // Create unauthenticated client
        var groupId = Guid.NewGuid().ToString();

        // Act
        var response = await client.GetAsync(UrlPackage.Question(groupId, cursor: null, count: 10));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task PutQuestion_WhenQuestionDoesAlreadyExist_ShouldUpdateSuccess()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(userId);
        var group = await CreateRamdomQuestionGroupAsync(userId);
        var command = SeedDefaultCreateQuestionCommand();
        var createdQuestion = await Sut.CreateQuestionAsync(userId, group.Id, command);
        var putCommand = SeedDefaultUpdateQuestionCommand();

        // Act
        var response = await client.PutAsJsonAsync(
            UrlPackage.Question(createdQuestion.Id),
            putCommand
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var result = await Sut.GetQuestionByIdAsync(createdQuestion.Id);
        result.ShouldNotBeNull();
        result.Id.ShouldBe(createdQuestion.Id);
        result.MarkupContent.ShouldBe(putCommand.Content);
        result.Level.ShouldBe(putCommand.Level);
        result.Type.ShouldBe(putCommand.Type);
        // TODO Assert Answers
    }

    [Test]
    public async Task PutQuestion_WhenQuestionDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(userId);
        var putCommand = SeedDefaultUpdateQuestionCommand();

        // Act
        var response = await client.PutAsJsonAsync(
            UrlPackage.Question(Guid.NewGuid().ToString()),
            putCommand
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Test]
    [TestCase("", 0)]
    [TestCase("any content", -1)]
    public async Task PutQuestion_WhenWithInvalidData_ShouldReturnUnprocessEntity(
        string content,
        int level
    )
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(userId);
        var putCommand = new UpdateQuestionCommand(
            content,
            level,
            QuestionType.MultipleChoice,
            [
                new CreateQuestionAnswerCommand(
                    Guid.NewGuid().ToString(),
                    true,
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

        // Act
        var response = await client.PutAsJsonAsync(
            UrlPackage.Question(Guid.NewGuid().ToString()),
            putCommand
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
    }

    [Test]
    public async Task PutQuestion_WhenQuestionDoesNotOwnedUser_ShouldReturnForbidden()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var otherUserId = Guid.NewGuid().ToString();
        var client = Sut.CreateClient(otherUserId);
        var group = await CreateRamdomQuestionGroupAsync(userId);
        var command = SeedDefaultCreateQuestionCommand();
        var createdQuestion = await Sut.CreateQuestionAsync(userId, group.Id, command);
        var putCommand = SeedDefaultUpdateQuestionCommand();

        // Act
        var response = await client.PutAsJsonAsync(
            UrlPackage.Question(createdQuestion.Id),
            putCommand
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task PutQuestion_WhenUnauthorized_ShouldReturnUnauthorized()
    {
        // Arrange
        var client = Sut.CreateClient();
        var putCommand = SeedDefaultUpdateQuestionCommand();

        // Act
        var response = await client.PutAsJsonAsync(
            UrlPackage.Question(Guid.NewGuid().ToString()),
            putCommand
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task DeleteQuestion_WhenQuestionDoesAlreadyExist_ShouldDeleteSuccess()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(userId);
        var group = await CreateRamdomQuestionGroupAsync(userId);
        var command = SeedDefaultCreateQuestionCommand();
        var createdQuestion = await Sut.CreateQuestionAsync(userId, group.Id, command);

        // Act
        var response = await client.DeleteAsync(UrlPackage.Question(createdQuestion.Id));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var result = await Sut.GetQuestionByIdAsync(createdQuestion.Id);
        result.ShouldBeNull();
    }

    [Test]
    public async Task DeleteQuestion_WhenQuestionDoesNotExist_ShouldReturnNoContent()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(userId);

        // Act
        var response = await client.DeleteAsync(UrlPackage.Question(Guid.NewGuid().ToString()));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task DeleteQuestion_WhenQuestionDoesNotOwnedUser_ShouldReturnPermission()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var otherUserId = Guid.NewGuid().ToString();
        var client = Sut.CreateClient(otherUserId);
        var group = await CreateRamdomQuestionGroupAsync(userId);
        var command = SeedDefaultCreateQuestionCommand();
        var createdQuestion = await Sut.CreateQuestionAsync(userId, group.Id, command);

        // Act
        var response = await client.DeleteAsync(UrlPackage.Question(createdQuestion.Id));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task DeleteQuestion_WhenUnauthorized_ShouldReturnUnauthorized()
    {
        // Arrange
        var client = Sut.CreateClient();

        // Act
        var response = await client.DeleteAsync(UrlPackage.Question(Guid.NewGuid().ToString()));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
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

    private static UpdateQuestionCommand SeedDefaultUpdateQuestionCommand()
    {
        return new UpdateQuestionCommand(
            Guid.NewGuid().ToString(),
            1,
            QuestionType.MultipleChoice,
            [
                new CreateQuestionAnswerCommand(
                    Guid.NewGuid().ToString(),
                    true,
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
