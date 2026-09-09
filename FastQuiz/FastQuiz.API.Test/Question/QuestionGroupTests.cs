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

internal class QuestionGroupTests : ApiTestBase
{
    [SetUp]
    public async Task Setup()
    {
        await Sut.CleanQuestionSqliteDb();
    }

    [Test]
    public async Task PostCreateQuestionGroup_WhenGroupWithValidaData_ShouldCreateSuccess()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(userId);
        var command = new CreateQuestionGroupCommand(
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString()
        );

        // Act
        var response = await client.PostAsJsonAsync(UrlPackage.QuestionGroup(), command);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<QuestionGroupDto>();
        result.ShouldNotBeNull();
        var group = await Sut.GetQuestionGroupByIdAsync(result.Id);
        group.ShouldNotBeNull();
        group.Id.ShouldBe(result.Id);
    }

    [Test]
    [TestCase("", "")]
    public async Task PostCreateQuestionGroup_WhenGroupWithInvalidaData_ShouldReturnUnprocessEntity(
        string name,
        string remark
    )
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(userId);
        var command = new CreateQuestionGroupCommand(name, remark);

        // Act
        var response = await client.PostAsJsonAsync(UrlPackage.QuestionGroup(), command);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
    }

    [Test]
    public async Task PostCreateQuestionGroup_WhenUnauthorized_ShouldReturnUnauthorized()
    {
        // Arrange
        var client = Sut.CreateClient();
        var command = new CreateQuestionGroupCommand(
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString()
        );

        // Act
        var response = await client.PostAsJsonAsync(UrlPackage.QuestionGroup(), command);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task PostBatchDeleteQuestionGroup_WhenAllGroupsOwnedUser_ShouldDeleteSuccess()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(userId);
        var command1 = new CreateQuestionGroupCommand(
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString()
        );
        var command2 = new CreateQuestionGroupCommand(
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString()
        );
        var group1 = await Sut.CreateQuestionGroupAsync(userId, command1);
        var group2 = await Sut.CreateQuestionGroupAsync(userId, command2);

        // Act
        var response = await client.PostAsJsonAsync(
            UrlPackage.QuiestionGroupBatchDelete(),
            new DeleteQuestionGroupCommand([group1.Id, group2.Id])
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var result1 = await Sut.GetQuestionGroupByIdAsync(group1.Id);
        var result2 = await Sut.GetQuestionGroupByIdAsync(group2.Id);
        result1.ShouldBeNull();
        result2.ShouldBeNull();
    }

    [Test]
    public async Task PostBatchDeleteQuestionGroup_WhenAnyGroupNotOwnedUser_ShouldReturnForbidden()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(Guid.NewGuid().ToString());
        var command1 = new CreateQuestionGroupCommand(
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString()
        );
        var command2 = new CreateQuestionGroupCommand(
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString()
        );
        var group1 = await Sut.CreateQuestionGroupAsync(userId, command1);
        var group2 = await Sut.CreateQuestionGroupAsync(userId, command2);

        // Act
        var response = await client.PostAsJsonAsync(
            UrlPackage.QuiestionGroupBatchDelete(),
            new DeleteQuestionGroupCommand([group1.Id, group2.Id])
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task PostBatchDeleteQuestionGroup_WhenUnauthorized_ShouldReturnUnauthorized()
    {
        // Arrange
        var client = Sut.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync(
            UrlPackage.QuiestionGroupBatchDelete(),
            new DeleteQuestionGroupCommand([])
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task GetQuestionGroup_WhenGroupDoesAlreadyExist_ShouldReturnQuestionGroupDto()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(userId);
        var command = new CreateQuestionGroupCommand(
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString()
        );
        var createdGroup = await Sut.CreateQuestionGroupAsync(userId, command);

        // Act
        var response = await client.GetAsync(UrlPackage.QuestionGroup(createdGroup.Id));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<QuestionGroupDto>();
        result.ShouldNotBeNull();
        result.Id.ShouldBe(createdGroup.Id);
    }

    [Test]
    public async Task GetQuestionGroup_WhenGroupDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(userId);

        // Act
        var response = await client.GetAsync(UrlPackage.QuestionGroup(Guid.NewGuid().ToString()));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task GetQuestionGroup_WhenGroupDoesNotOwnedUser_ShouldReturnForbidden()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(Guid.NewGuid().ToString());
        var command = new CreateQuestionGroupCommand(
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString()
        );
        var createdGroup = await Sut.CreateQuestionGroupAsync(userId, command);

        // Act
        var response = await client.GetAsync(UrlPackage.QuestionGroup(createdGroup.Id));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task GetQuestionGroup_WhenUnauthorized_ShouldReturnUnauthorized()
    {
        // Arrange
        var client = Sut.CreateClient();

        // Act
        var response = await client.GetAsync(UrlPackage.QuestionGroup(Guid.NewGuid().ToString()));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task GetAllQuestionGroup_WhenCursorIsNull_ShouldReturnLatestGroups()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(userId);

        // Create 3 groups
        var group1 = await Sut.CreateQuestionGroupAsync(
            userId,
            new CreateQuestionGroupCommand(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())
        );
        var group2 = await Sut.CreateQuestionGroupAsync(
            userId,
            new CreateQuestionGroupCommand(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())
        );
        var group3 = await Sut.CreateQuestionGroupAsync(
            userId,
            new CreateQuestionGroupCommand(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())
        );

        var expectedIds = new[] { group3.Id, group2.Id, group1.Id }.OrderByDescending(x => x);

        // Act
        var response = await client.GetAsync(UrlPackage.QuestionGroup());

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var groups = await response.Content.ReadFromJsonAsync<List<QuestionGroupDto>>();
        groups.ShouldNotBeNull();
        groups.Select(g => g.Id).ShouldBe(expectedIds);
    }

    [Test]
    public async Task GetAllQuestionGroup_WhenCursorIsNotNull_ShouldReturnGroupsOfGreaterThanCursor()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(userId);

        var group1 = await Sut.CreateQuestionGroupAsync(
            userId,
            new CreateQuestionGroupCommand(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())
        );
        var group2 = await Sut.CreateQuestionGroupAsync(
            userId,
            new CreateQuestionGroupCommand(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())
        );
        var group3 = await Sut.CreateQuestionGroupAsync(
            userId,
            new CreateQuestionGroupCommand(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())
        );
        List<string> ids = [group1.Id, group2.Id, group3.Id];
        var expectedIds = ids.OrderByDescending(x => x).Skip(1).ToList();

        // Act
        var response = await client.GetAsync(UrlPackage.QuestionGroup(ids.Max(), 10));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var groups = await response.Content.ReadFromJsonAsync<List<QuestionGroupDto>>();
        groups.ShouldNotBeNull();
        groups.Select(g => g.Id).ShouldBe(expectedIds);
    }

    [Test]
    public async Task GetAllQuestionGroup_WhenWithCount_ShouldReturnGroupsOfCorrectCount()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(userId);

        await Sut.CreateQuestionGroupAsync(
            userId,
            new CreateQuestionGroupCommand(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())
        );
        await Sut.CreateQuestionGroupAsync(
            userId,
            new CreateQuestionGroupCommand(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())
        );
        await Sut.CreateQuestionGroupAsync(
            userId,
            new CreateQuestionGroupCommand(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())
        );

        var count = 2;

        // Act
        var response = await client.GetAsync(UrlPackage.QuestionGroup("", count));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var groups = await response.Content.ReadFromJsonAsync<List<QuestionGroupDto>>();
        groups.ShouldNotBeNull();
        groups.Count.ShouldBe(count);
    }

    [Test]
    public async Task GetAllQuestionGroup_WhenIsPreviousIsTrue_ShouldReturnGroupsOfLessThanCursor()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(userId);

        var group1 = await Sut.CreateQuestionGroupAsync(
            userId,
            new CreateQuestionGroupCommand(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())
        );
        var group2 = await Sut.CreateQuestionGroupAsync(
            userId,
            new CreateQuestionGroupCommand(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())
        );
        var group3 = await Sut.CreateQuestionGroupAsync(
            userId,
            new CreateQuestionGroupCommand(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())
        );
        List<string> ids = [group1.Id, group2.Id, group3.Id];
        var expectedIds = ids.OrderByDescending(x => x).Take(2).ToList();

        // Act
        var response = await client.GetAsync(UrlPackage.QuestionGroup(ids.Min(), 10, true));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var groups = await response.Content.ReadFromJsonAsync<List<QuestionGroupDto>>();
        groups.ShouldNotBeNull();
        groups.Select(g => g.Id).ShouldBe(expectedIds);
    }

    [Test]
    public async Task GetAllQuestionGroup_WhenNamePrefixIsNotNull_ShouldReturnGroupsOfStartWithNamePrefix()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(userId);
        var prefix = Guid.NewGuid().ToString().Substring(0, 8);

        var group1 = await Sut.CreateQuestionGroupAsync(
            userId,
            new CreateQuestionGroupCommand($"{prefix}{Guid.NewGuid()}", Guid.NewGuid().ToString())
        );
        var group2 = await Sut.CreateQuestionGroupAsync(
            userId,
            new CreateQuestionGroupCommand($"{prefix}{Guid.NewGuid()}", Guid.NewGuid().ToString())
        );
        var group3 = await Sut.CreateQuestionGroupAsync(
            userId,
            new CreateQuestionGroupCommand($"{prefix}{Guid.NewGuid()}", Guid.NewGuid().ToString())
        );

        await Sut.CreateQuestionGroupAsync(
            userId,
            new CreateQuestionGroupCommand(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())
        );
        await Sut.CreateQuestionGroupAsync(
            userId,
            new CreateQuestionGroupCommand(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())
        );

        var expectedIds = new[] { group3.Id, group2.Id, group1.Id }.OrderByDescending(x => x);

        // Act
        var response = await client.GetAsync(UrlPackage.QuestionGroup(null, 10, false, prefix));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var groups = await response.Content.ReadFromJsonAsync<List<QuestionGroupDto>>();
        groups.ShouldNotBeNull();
        groups.Select(g => g.Id).ShouldBe(expectedIds);
        groups.All(g => g.Name.StartsWith(prefix)).ShouldBeTrue();
    }

    [Test]
    public async Task GetAllQuestionGroup_WhenUnauthorized_ShouldReturnUnauthorized()
    {
        // Arrange
        var client = Sut.CreateClient();

        // Act
        var response = await client.GetAsync(UrlPackage.QuestionGroup());

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task GetQuestionCount_WhenAuthorized_ShouldReturnQuestionCount()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(userId);
        var group = await CreateRamdomQuestionGroupAsync(userId);
        var questions = new List<QuestionDto>();

        var type = QuestionType.SingleChoice;

        for (int i = 0; i < 5; i++)
        {
            var command = SeedDefaultCreateQuestionCommand(type, 0);
            var question = await Sut.CreateQuestionAsync(userId, group.Id, command);
            questions.Add(question);
        }
        var expectedCount = questions.Count;

        // Act
        var response = await client.GetAsync(UrlPackage.QuestionCount(group.Id, type, 0));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var count = await response.Content.ReadFromJsonAsync<int>();
        count.ShouldBe(expectedCount);
    }

    [Test]
    public async Task GetQuestionCount_WhenUnauthorized_ShouldFail()
    {
        // Arrange
        var client = Sut.CreateClient();

        // Act
        var response = await client.GetAsync(
            UrlPackage.QuestionCount(Guid.NewGuid().ToString(), QuestionType.SingleChoice, 0)
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task PutQuestionGroup_WhenGroupDoesAlreadyExist_ShouldUpdateSuccess()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(userId);
        var createCommand = new CreateQuestionGroupCommand(
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString()
        );
        var createdGroup = await Sut.CreateQuestionGroupAsync(userId, createCommand);
        var updateCommand = new UpdateQuestionGroupCommand(
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString()
        );

        // Act
        var response = await client.PutAsJsonAsync(
            UrlPackage.QuestionGroup(createdGroup.Id),
            updateCommand
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var result = await Sut.GetQuestionGroupByIdAsync(createdGroup.Id);
        result.ShouldNotBeNull();
        result.Id.ShouldBe(createdGroup.Id);
        result.Name.ShouldBe(updateCommand.Name);
        result.Remark.ShouldBe(updateCommand.Remark);
    }

    [Test]
    public async Task PutQuestionGroup_WhenGroupDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(userId);
        var updateCommand = new UpdateQuestionGroupCommand(
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString()
        );

        // Act
        var response = await client.PutAsJsonAsync(
            UrlPackage.QuestionGroup(Guid.NewGuid().ToString()),
            updateCommand
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task PutQuestionGroup_WhenGroupDoesNotOwnedUser_ShouldReturnForbidden()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(Guid.NewGuid().ToString());
        var createCommand = new CreateQuestionGroupCommand(userId, Guid.NewGuid().ToString());
        var createdGroup = await Sut.CreateQuestionGroupAsync(userId, createCommand);
        var updateCommand = new UpdateQuestionGroupCommand(
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString()
        );

        // Act
        var response = await client.PutAsJsonAsync(
            UrlPackage.QuestionGroup(createdGroup.Id),
            updateCommand
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Test]
    [TestCase("", "")]
    public async Task PutQuestionGroup_WhenWithInvalidData_ShouldReturnUnprocessEntity(
        string name,
        string remark
    )
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(userId);
        var createCommand = new CreateQuestionGroupCommand(
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString()
        );
        var createdGroup = await Sut.CreateQuestionGroupAsync(userId, createCommand);
        var updateCommand = new UpdateQuestionGroupCommand(name, remark);

        // Act
        var response = await client.PutAsJsonAsync(
            UrlPackage.QuestionGroup(createdGroup.Id),
            updateCommand
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
    }

    [Test]
    public async Task PutQuestionGroup_WhenUnauthorized_ShouldUnauthorized()
    {
        // Arrange
        var client = Sut.CreateClient();
        var updateCommand = new UpdateQuestionGroupCommand(
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString()
        );

        // Act
        var response = await client.PutAsJsonAsync(
            UrlPackage.QuestionGroup(Guid.NewGuid().ToString()),
            updateCommand
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task DeleteQuestionGroup_WhenGroupDoesAlreadyExsit_ShouldDeleteSuccess()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(userId);
        var command = new CreateQuestionGroupCommand(
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString()
        );
        var createdGroup = await Sut.CreateQuestionGroupAsync(userId, command);

        // Act
        var response = await client.DeleteAsync(UrlPackage.QuestionGroup(createdGroup.Id));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var result = await Sut.GetQuestionGroupByIdAsync(createdGroup.Id);
        result.ShouldBeNull();
    }

    [Test]
    public async Task DeleteQuestionGroup_WhenGroupDoesNotExsit_ShouldReturnNoContent()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(userId);

        // Act
        var response = await client.DeleteAsync(
            UrlPackage.QuestionGroup(Guid.NewGuid().ToString())
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task DeleteQuestionGroup_WhenGroupDoesNotOwnedUser_ShouldReturnForbidden()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(Guid.NewGuid().ToString());
        var command = new CreateQuestionGroupCommand(
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString()
        );
        var createdGroup = await Sut.CreateQuestionGroupAsync(userId, command);

        // Act
        var response = await client.DeleteAsync(UrlPackage.QuestionGroup(createdGroup.Id));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task DeleteQuestionGroup_WhenUnauthorized_ShouldReturnUnathorized()
    {
        // Arrange
        var client = Sut.CreateClient();

        // Act
        var response = await client.DeleteAsync(
            UrlPackage.QuestionGroup(Guid.NewGuid().ToString())
        );

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
}
