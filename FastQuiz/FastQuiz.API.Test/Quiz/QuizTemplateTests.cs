using System.Data;
using System.Net;
using System.Net.Http.Json;
using FastQuiz.API.Test.Utils;
using FastQuiz.API.Test.Utils.Helpers;
using FastQuiz.Auth.Domain.Entities;
using FastQuiz.Question.Domain.Entities;
using FastQuiz.Question.UseCase.Command;
using FastQuiz.Quiz.UseCase.Commands;
using FastQuiz.Quiz.UseCase.Dtos;
using Shouldly;

namespace FastQuiz.API.Test.Quiz;

internal class QuizTemplateTests : ApiTestBase
{
    [SetUp]
    public async Task Setup()
    {
        await Sut.CleanQuizSqliteDb();
    }

    #region QuizTemplate
    [Test]
    public async Task PostCreateQuizTemplate_WhenWithValidaData_ShouldCreateSuccess()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(userId);
        var command = SeedDefaultCreateQuizTemplateCommand();

        // Act
        var response = await client.PostAsJsonAsync(UrlPackage.QuizTemplate(), command);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<QuizTemplateDto>();
        result.ShouldNotBeNull();
        var template = await Sut.GetQuizTemplateByIdAsync(result.Id);
        template.ShouldNotBeNull();
        template.Id.ShouldBe(result.Id);
    }

    [Test]
    [TestCase("", "")] // TODO 补充所有需要的参数约束
    public async Task PostCreateQuizTemplate_WhenGroupWithInvalidaData_ShouldReturnUnprocessEntity(
        string name,
        string description
    )
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(userId);
        var command = SeedDefaultCreateQuizTemplateCommand(name, description);

        // Act
        var response = await client.PostAsJsonAsync(UrlPackage.QuizTemplate(), command);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
    }

    [Test]
    public async Task PostCreateQuizTemplate_WhenUnauthorized_ShouldReturnUnauthorized()
    {
        // Arrange
        var client = Sut.CreateClient();
        var command = SeedDefaultCreateQuizTemplateCommand();

        // Act
        var response = await client.PostAsJsonAsync(UrlPackage.QuizTemplate(), command);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task GetQuizTemplate_WhenTemplateDoesAlreadyExist_ShouldReturnQuizTemplateDto()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(userId);
        var command = SeedDefaultCreateQuizTemplateCommand();
        var createdTemplate = await Sut.CreateQuizTemplateAsync(userId, command);

        // Act
        var response = await client.GetAsync(UrlPackage.QuizTemplate(createdTemplate.Id));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<QuizTemplateDto>();
        result.ShouldNotBeNull();
        result.Id.ShouldBe(createdTemplate.Id);
    }

    [Test]
    public async Task GetQuizTemplate_WhenTemplateDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(userId);

        // Act
        var response = await client.GetAsync(UrlPackage.QuizTemplate(Guid.NewGuid().ToString()));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task GetQuizTemplate_WhenTemplateDoesNotOwnedUser_ShouldReturnForbidden()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(Guid.NewGuid().ToString());
        var command = SeedDefaultCreateQuizTemplateCommand();
        var createdTemplate = await Sut.CreateQuizTemplateAsync(userId, command);

        // Act
        var response = await client.GetAsync(UrlPackage.QuizTemplate(createdTemplate.Id));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task GetQuizTemplate_WhenUnauthorized_ShouldReturnUnauthorized()
    {
        // Arrange
        var client = Sut.CreateClient();

        // Act
        var response = await client.GetAsync(UrlPackage.QuizTemplate(Guid.NewGuid().ToString()));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task GetAllQuizTemplate_WhenCursorIsNull_ShouldReturnLatestTemplates()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(userId);

        var template1 = await Sut.CreateQuizTemplateAsync(
            userId,
            SeedDefaultCreateQuizTemplateCommand()
        );
        var template2 = await Sut.CreateQuizTemplateAsync(
            userId,
            SeedDefaultCreateQuizTemplateCommand()
        );
        var template3 = await Sut.CreateQuizTemplateAsync(
            userId,
            SeedDefaultCreateQuizTemplateCommand()
        );

        var expectedIds = new[] { template1.Id, template2.Id, template3.Id }
            .OrderByDescending(x => x)
            .ToList();

        // Act
        var response = await client.GetAsync(UrlPackage.QuizTemplate());

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var groups = await response.Content.ReadFromJsonAsync<List<QuizTemplateDto>>();
        groups.ShouldNotBeNull();
        groups.Select(g => g.Id).ShouldBe(expectedIds);
    }

    [Test]
    public async Task GetAllQuizTemplate_WhenCursorIsNotNull_ShouldReturnTemplatesOfGreaterThanCursor()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(userId);

        var template1 = await Sut.CreateQuizTemplateAsync(
            userId,
            SeedDefaultCreateQuizTemplateCommand()
        );
        var template2 = await Sut.CreateQuizTemplateAsync(
            userId,
            SeedDefaultCreateQuizTemplateCommand()
        );
        var template3 = await Sut.CreateQuizTemplateAsync(
            userId,
            SeedDefaultCreateQuizTemplateCommand()
        );
        List<string> ids = [template1.Id, template2.Id, template3.Id];
        var expectedIds = ids.OrderByDescending(x => x).Skip(1).ToList();

        // Act
        var response = await client.GetAsync(UrlPackage.QuizTemplate(ids.Max(), 10));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var groups = await response.Content.ReadFromJsonAsync<List<QuizTemplateDto>>();
        groups.ShouldNotBeNull();
        groups.Select(g => g.Id).ShouldBe(expectedIds);
    }

    [Test]
    public async Task GetAllQuizTemplate_WhenWithCount_ShouldReturnTemplatesOfCorrectCount()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(userId);

        var template1 = await Sut.CreateQuizTemplateAsync(
            userId,
            SeedDefaultCreateQuizTemplateCommand()
        );
        var template2 = await Sut.CreateQuizTemplateAsync(
            userId,
            SeedDefaultCreateQuizTemplateCommand()
        );
        var template3 = await Sut.CreateQuizTemplateAsync(
            userId,
            SeedDefaultCreateQuizTemplateCommand()
        );

        var count = 2;

        // Act
        var response = await client.GetAsync(UrlPackage.QuizTemplate("", count));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var templates = await response.Content.ReadFromJsonAsync<List<QuizTemplateDto>>();
        templates.ShouldNotBeNull();
        templates.Count.ShouldBe(count);
    }

    [Test]
    public async Task GetAllQuizTemplate_WhenIsPreviousIsTrue_ShouldReturnTemplatesOfLessThanCursor()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(userId);

        var template1 = await Sut.CreateQuizTemplateAsync(
            userId,
            SeedDefaultCreateQuizTemplateCommand()
        );
        var template2 = await Sut.CreateQuizTemplateAsync(
            userId,
            SeedDefaultCreateQuizTemplateCommand()
        );
        var template3 = await Sut.CreateQuizTemplateAsync(
            userId,
            SeedDefaultCreateQuizTemplateCommand()
        );
        List<string> ids = [template1.Id, template2.Id, template3.Id];
        var expectedIds = ids.OrderByDescending(x => x).Take(2).ToList();

        // Act
        var response = await client.GetAsync(UrlPackage.QuizTemplate(ids.Min(), 10, true));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var templates = await response.Content.ReadFromJsonAsync<List<QuizTemplateDto>>();
        templates.ShouldNotBeNull();
        templates.Select(g => g.Id).ShouldBe(expectedIds);
    }

    [Test]
    public async Task GetAllQuizTemplate_WhenUnauthorized_ShouldReturnUnauthorized()
    {
        // Arrange
        var client = Sut.CreateClient();

        // Act
        var response = await client.GetAsync(UrlPackage.QuizTemplate("", 10));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task PutQuizTemplate_WhenTemplateDoesAlreadyExist_ShouldUpdateSuccess()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(userId);
        var createdTemplate = await Sut.CreateQuizTemplateAsync(
            userId,
            SeedDefaultCreateQuizTemplateCommand()
        );
        var updateCommand = SeedDefaultUpdateQuizTemplateCommand();

        // Act
        var response = await client.PutAsJsonAsync(
            UrlPackage.QuizTemplate(createdTemplate.Id),
            updateCommand
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var result = await Sut.GetQuizTemplateByIdAsync(createdTemplate.Id);
        result.ShouldNotBeNull();
        result.Id.ShouldBe(createdTemplate.Id);
        result.Name.ShouldBe(updateCommand.Name);
        result.Remark.ShouldBe(updateCommand.Remark);
        // TODO: add all fields
    }

    [Test]
    public async Task PutQuizTemplate_WhenTemplateDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(userId);
        var updateCommand = SeedDefaultUpdateQuizTemplateCommand(userId);

        // Act
        var response = await client.PutAsJsonAsync(
            UrlPackage.QuizTemplate(Guid.NewGuid().ToString()),
            updateCommand
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task PutQuizTemplate_WhenTemplateDoesNotOwnedUser_ShouldReturnForbidden()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(Guid.NewGuid().ToString());
        var createdGroup = await Sut.CreateQuizTemplateAsync(
            userId,
            SeedDefaultCreateQuizTemplateCommand()
        );
        var updateCommand = SeedDefaultUpdateQuizTemplateCommand();

        // Act
        var response = await client.PutAsJsonAsync(
            UrlPackage.QuizTemplate(createdGroup.Id),
            updateCommand
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Test]
    [TestCase("", "")]
    public async Task PutQuizTemplate_WhenWithInvalidData_ShouldReturnUnprocessEntity(
        string name,
        string description
    )
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(userId);

        var createdTemplate = await Sut.CreateQuizTemplateAsync(
            userId,
            SeedDefaultCreateQuizTemplateCommand()
        );
        var updateCommand = SeedDefaultUpdateQuizTemplateCommand(name, description);

        // Act
        var response = await client.PutAsJsonAsync(
            UrlPackage.QuizTemplate(createdTemplate.Id),
            updateCommand
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
    }

    [Test]
    public async Task PutQuizTemplate_WhenUnauthorized_ShouldUnauthorized()
    {
        // Arrange
        var client = Sut.CreateClient();
        var updateCommand = SeedDefaultUpdateQuizTemplateCommand();

        // Act
        var response = await client.PutAsJsonAsync(
            UrlPackage.QuizTemplate(Guid.NewGuid().ToString()),
            updateCommand
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task DeleteQuizTemplate_WhenTemplateDoesAlreadyExsit_ShouldDeleteSuccess()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(userId);
        var createdTemplate = await Sut.CreateQuizTemplateAsync(
            userId,
            SeedDefaultCreateQuizTemplateCommand()
        );

        // Act
        var response = await client.DeleteAsync(UrlPackage.QuizTemplate(createdTemplate.Id));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var result = await Sut.GetQuestionGroupByIdAsync(createdTemplate.Id);
        result.ShouldBeNull();
    }

    [Test]
    public async Task DeleteQuizTemplate_WhenTemplateDoesNotExsit_ShouldReturnNoContent()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(userId);

        // Act
        var response = await client.DeleteAsync(UrlPackage.QuizTemplate(Guid.NewGuid().ToString()));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task DeleteQuizTemplate_WhenTemplateDoesNotOwnedUser_ShouldReturnForbidden()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(Guid.NewGuid().ToString());
        var createdTemplate = await Sut.CreateQuizTemplateAsync(
            userId,
            SeedDefaultCreateQuizTemplateCommand()
        );

        // Act
        var response = await client.DeleteAsync(UrlPackage.QuizTemplate(createdTemplate.Id));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task DeleteQuizTemplate_WhenUnauthorized_ShouldReturnUnathorized()
    {
        // Arrange
        var client = Sut.CreateClient();

        // Act
        var response = await client.DeleteAsync(UrlPackage.QuizTemplate(Guid.NewGuid().ToString()));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
    #endregion

    #region QuestionSelectionRule
    [Test]
    public async Task PostCreateQuestionSelectionRule_WhenWithValidaData_ShouldCreateSuccess()
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
        var command = SeedDefaultCreateQuestionSelectionRuleCommand(group.Id);

        // Act
        var response = await client.PostAsJsonAsync(
            UrlPackage.QuestionSelectionRuleWithTemplateId(template.Id),
            command
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.ReadFromJsonWithEnumAsync<QuestionSelectionRuleDto>();
        result.ShouldNotBeNull();
        var rule = await Sut.GetQuestionSelectionRuleByIdAsync(result.Id);
        rule.ShouldNotBeNull();
        rule.Id.ShouldBe(result.Id);
    }

    [Test]
    [TestCase(-1, 1, 1)]
    [TestCase(0, 0, 1)]
    [TestCase(0, 1, 0)]
    public async Task PostCreateQuestionSelectionRule_WhenRuleWithInvalidaData_ShouldReturnUnprocessEntity(
        int level,
        int count,
        int score
    )
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
        var command = SeedDefaultCreateQuestionSelectionRuleCommand(group.Id, level, count, score);

        // Act
        var response = await client.PostAsJsonAsync(
            UrlPackage.QuestionSelectionRuleWithTemplateId(template.Id),
            command
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
    }

    [Test]
    public async Task PostCreateQuestionSelectionRule_WhenQuizTemplateDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(userId);
        var group = await Sut.CreateQuestionGroupAsync(
            userId,
            new CreateQuestionGroupCommand(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())
        );
        var command = SeedDefaultCreateQuestionSelectionRuleCommand(group.Id);

        // Act
        var response = await client.PostAsJsonAsync(
            UrlPackage.QuestionSelectionRuleWithTemplateId(Guid.NewGuid().ToString()),
            command
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task PostCreateQuestionSelectionRule_WhenTemplateDoesNotOwnedUser_ShouldReturnFobidden()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(Guid.NewGuid().ToString());
        var template = await Sut.CreateQuizTemplateAsync(
            userId,
            SeedDefaultCreateQuizTemplateCommand()
        );
        var group = await Sut.CreateQuestionGroupAsync(
            userId,
            new CreateQuestionGroupCommand(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())
        );
        var command = SeedDefaultCreateQuestionSelectionRuleCommand(group.Id);

        // Act
        var response = await client.PostAsJsonAsync(
            UrlPackage.QuestionSelectionRuleWithTemplateId(template.Id),
            command
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task PostCreateQuestionSelectionRule_WhenQuestionGroupDoesNotOwnedUser_ShouldReturnFobidden()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(Guid.NewGuid().ToString());
        var template = await Sut.CreateQuizTemplateAsync(
            userId,
            SeedDefaultCreateQuizTemplateCommand()
        );
        var group = await Sut.CreateQuestionGroupAsync(
            userId,
            new CreateQuestionGroupCommand(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())
        );
        var command = SeedDefaultCreateQuestionSelectionRuleCommand(group.Id);

        // Act
        var response = await client.PostAsJsonAsync(
            UrlPackage.QuestionSelectionRuleWithTemplateId(template.Id),
            command
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task PostCreateQuestionSelectionRule_WhenUnauthorized_ShouldReturnUnauthorized()
    {
        // Arrange
        var client = Sut.CreateClient();
        var command = SeedDefaultCreateQuestionSelectionRuleCommand(Guid.NewGuid().ToString());

        // Act
        var response = await client.PostAsJsonAsync(
            UrlPackage.QuestionSelectionRuleWithTemplateId(Guid.NewGuid().ToString()),
            command
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task GetQuestionSelectionRule_WhenRuleDoesAlreadyExist_ShouldReturnQuestionSelectionRuleDto()
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
        var rule = await Sut.CreateQuestionSelectionRuleAsync(
            userId,
            template.Id,
            SeedDefaultCreateQuestionSelectionRuleCommand(group.Id)
        );

        // Act
        var response = await client.GetAsync(UrlPackage.QuestionSelectionRule(rule.Id));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.ReadFromJsonWithEnumAsync<QuestionSelectionRuleDto>();
        result.ShouldNotBeNull();
        result.Id.ShouldBe(rule.Id);
    }

    [Test]
    public async Task GetQuestionSelectionRule_WhenRuleDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(userId);

        // Act
        var response = await client.GetAsync(
            UrlPackage.QuestionSelectionRule(Guid.NewGuid().ToString())
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task GetQuestionSelectionRule_WhenRuleDoesNotOwnedUser_ShouldReturnForbidden()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var otherUser = Guid.NewGuid().ToString();
        var client = Sut.CreateClient(otherUser);
        var template = await Sut.CreateQuizTemplateAsync(
            userId,
            SeedDefaultCreateQuizTemplateCommand()
        );
        var group = await Sut.CreateQuestionGroupAsync(
            userId,
            new CreateQuestionGroupCommand(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())
        );
        var rule = await Sut.CreateQuestionSelectionRuleAsync(
            userId,
            template.Id,
            SeedDefaultCreateQuestionSelectionRuleCommand(group.Id)
        );

        // Act
        var response = await client.GetAsync(UrlPackage.QuestionSelectionRule(rule.Id));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task GetQuestionSelectionRule_WhenUnauthorized_ShouldReturnUnauthorized()
    {
        // Arrange
        var client = Sut.CreateClient();

        // Act
        var response = await client.GetAsync(
            UrlPackage.QuestionSelectionRule(Guid.NewGuid().ToString())
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task GetAllRules_WhenCursorIsNull_ShouldReturnLatestRules()
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
        var rule1 = await Sut.CreateQuestionSelectionRuleAsync(
            userId,
            template.Id,
            SeedDefaultCreateQuestionSelectionRuleCommand(group.Id, 0)
        );
        var rule2 = await Sut.CreateQuestionSelectionRuleAsync(
            userId,
            template.Id,
            SeedDefaultCreateQuestionSelectionRuleCommand(group.Id, 1)
        );
        var rule3 = await Sut.CreateQuestionSelectionRuleAsync(
            userId,
            template.Id,
            SeedDefaultCreateQuestionSelectionRuleCommand(group.Id, 2)
        );
        var expectedIds = new[] { rule1.Id, rule2.Id, rule3.Id }.OrderByDescending(x => x).ToList();

        // Act
        var response = await client.GetAsync(
            UrlPackage.QuestionSelectionRuleWithTemplateId(template.Id, null)
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var groups = await response.ReadFromJsonWithEnumAsync<List<QuestionSelectionRuleDto>>();
        groups.ShouldNotBeNull();
        groups.Select(g => g.Id).ShouldBe(expectedIds);
    }

    [Test]
    public async Task GetAllRules_WhenCursorIsNotNull_ShouldReturnRulesOfGreaterThanCursor()
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
        var rule1 = await Sut.CreateQuestionSelectionRuleAsync(
            userId,
            template.Id,
            SeedDefaultCreateQuestionSelectionRuleCommand(group.Id, 0)
        );
        var rule2 = await Sut.CreateQuestionSelectionRuleAsync(
            userId,
            template.Id,
            SeedDefaultCreateQuestionSelectionRuleCommand(group.Id, 1)
        );
        var rule3 = await Sut.CreateQuestionSelectionRuleAsync(
            userId,
            template.Id,
            SeedDefaultCreateQuestionSelectionRuleCommand(group.Id, 2)
        );
        var idList = new[] { rule1.Id, rule2.Id, rule3.Id }.OrderByDescending(x => x).ToList();
        var expectedIds = idList.OrderByDescending(x => x).Skip(1).ToList();

        // Act
        var response = await client.GetAsync(
            UrlPackage.QuestionSelectionRuleWithTemplateId(template.Id, idList.Max())
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var groups = await response.ReadFromJsonWithEnumAsync<List<QuestionSelectionRuleDto>>();
        groups.ShouldNotBeNull();
        groups.Select(g => g.Id).ShouldBe(expectedIds);
    }

    [Test]
    public async Task GetAllRules_WhenWithCount_ShouldReturnRulesOfCorrectCount()
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
        var rule1 = await Sut.CreateQuestionSelectionRuleAsync(
            userId,
            template.Id,
            SeedDefaultCreateQuestionSelectionRuleCommand(group.Id, 0)
        );
        var rule2 = await Sut.CreateQuestionSelectionRuleAsync(
            userId,
            template.Id,
            SeedDefaultCreateQuestionSelectionRuleCommand(group.Id, 1)
        );
        var rule3 = await Sut.CreateQuestionSelectionRuleAsync(
            userId,
            template.Id,
            SeedDefaultCreateQuestionSelectionRuleCommand(group.Id, 2)
        );
        var idList = new[] { rule1.Id, rule2.Id, rule3.Id }.OrderByDescending(x => x).ToList();
        var count = 1;

        // Act
        var response = await client.GetAsync(
            UrlPackage.QuestionSelectionRuleWithTemplateId(template.Id, null, count)
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var groups = await response.ReadFromJsonWithEnumAsync<List<QuestionSelectionRuleDto>>();
        groups.ShouldNotBeNull();
        groups.Count.ShouldBe(count);
    }

    [Test]
    public async Task GetAllRules_WhenTemplateDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(userId);

        // Act
        var response = await client.GetAsync(
            UrlPackage.QuestionSelectionRuleWithTemplateId(Guid.NewGuid().ToString())
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task GetAllRules_WhenUnauthorized_ShouldReturnUnauthorized()
    {
        // Arrange
        var client = Sut.CreateClient();

        // Act
        var response = await client.GetAsync(
            UrlPackage.QuestionSelectionRuleWithTemplateId(Guid.NewGuid().ToString())
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task PutQuestionSelectionRule_WhenRuleDoesAlreadyExist_ShouldUpdateSuccess()
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
        var rule = await Sut.CreateQuestionSelectionRuleAsync(
            userId,
            template.Id,
            SeedDefaultCreateQuestionSelectionRuleCommand(group.Id)
        );
        var putCommand = SeedDefaultUpdateQuestionSelectionRuleCommand(group.Id);

        // Act
        var response = await client.PutAsJsonAsync(
            UrlPackage.QuestionSelectionRule(rule.Id),
            putCommand
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var result = await Sut.GetQuestionSelectionRuleByIdAsync(rule.Id);
        result.ShouldNotBeNull();
        result.Id.ShouldBe(rule.Id);
        result.Level.ShouldBe(putCommand.Level);
        result.Type.ShouldBe(putCommand.QuestionType);
        result.Count.ShouldBe(putCommand.Count);
        result.Score.ShouldBe(putCommand.Score);
    }

    [Test]
    public async Task PutQuizQuestionSelectionRule_WhenRuleDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(userId);
        var group = await Sut.CreateQuestionGroupAsync(
            userId,
            new CreateQuestionGroupCommand(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())
        );
        var putCommand = SeedDefaultUpdateQuestionSelectionRuleCommand(group.Id);

        // Act
        var response = await client.PutAsJsonAsync(
            UrlPackage.QuestionSelectionRule(Guid.NewGuid().ToString()),
            putCommand
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task PutQuestionSelectionRule_WhenDoesNotOwnedUser_ShouldReturnForbidden()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var otherUserId = Guid.NewGuid().ToString();
        var client = Sut.CreateClient(otherUserId);

        var template = await Sut.CreateQuizTemplateAsync(
            userId,
            SeedDefaultCreateQuizTemplateCommand()
        );
        var group = await Sut.CreateQuestionGroupAsync(
            userId,
            new CreateQuestionGroupCommand(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())
        );
        var rule = await Sut.CreateQuestionSelectionRuleAsync(
            userId,
            template.Id,
            SeedDefaultCreateQuestionSelectionRuleCommand(group.Id)
        );
        var putCommand = SeedDefaultUpdateQuestionSelectionRuleCommand(group.Id);

        // Act
        var response = await client.PutAsJsonAsync(
            UrlPackage.QuestionSelectionRule(rule.Id),
            putCommand
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Test]
    [TestCase(-1, 1, 1)]
    [TestCase(0, 0, 1)]
    [TestCase(0, 1, 0)]
    public async Task PutCreateQuestionSelectionRule_WhenRuleWithInvalidaData_ShouldReturnUnprocessEntity(
        int level,
        int count,
        int score
    )
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
        var rule = await Sut.CreateQuestionSelectionRuleAsync(
            userId,
            template.Id,
            SeedDefaultCreateQuestionSelectionRuleCommand(group.Id)
        );
        var command = SeedDefaultUpdateQuestionSelectionRuleCommand(group.Id, level, count, score);

        // Act
        var response = await client.PutAsJsonAsync(
            UrlPackage.QuestionSelectionRule(rule.Id),
            command
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
    }

    [Test]
    public async Task PutQuestionSelectionRule_WhenUnauthorized_ShouldUnauthorized()
    {
        // Arrange
        var client = Sut.CreateClient();
        var updateCommand = SeedDefaultUpdateQuestionSelectionRuleCommand();

        // Act
        var response = await client.PutAsJsonAsync(
            UrlPackage.QuestionSelectionRule(Guid.NewGuid().ToString()),
            updateCommand
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task DeleteQuestionSelectionRule_WhenRuleDoesAlreadyExsit_ShouldDeleteSuccess()
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
        var rule = await Sut.CreateQuestionSelectionRuleAsync(
            userId,
            template.Id,
            SeedDefaultCreateQuestionSelectionRuleCommand(group.Id)
        );

        // Act
        var response = await client.DeleteAsync(UrlPackage.QuestionSelectionRule(rule.Id));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var result = await Sut.GetQuestionSelectionRuleByIdAsync(rule.Id);
        result.ShouldBeNull();
    }

    [Test]
    public async Task DeleteQuestionSelectionRule_WhenRuleDoesNotExsit_ShouldReturnNoContent()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var client = Sut.CreateClient(userId);

        // Act
        var response = await client.DeleteAsync(
            UrlPackage.QuestionSelectionRule(Guid.NewGuid().ToString())
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task DeleteQuestionSelelctionRule_WhenRuleDoesNotOwnedUser_ShouldReturnForbidden()
    {
        // Arrange
        var userId = User.DefaultAdminId;
        var otherUserId = Guid.NewGuid().ToString();
        var client = Sut.CreateClient(otherUserId);

        var template = await Sut.CreateQuizTemplateAsync(
            userId,
            SeedDefaultCreateQuizTemplateCommand()
        );
        var group = await Sut.CreateQuestionGroupAsync(
            userId,
            new CreateQuestionGroupCommand(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())
        );
        var rule = await Sut.CreateQuestionSelectionRuleAsync(
            userId,
            template.Id,
            SeedDefaultCreateQuestionSelectionRuleCommand(group.Id)
        );

        // Act
        var response = await client.DeleteAsync(UrlPackage.QuestionSelectionRule(rule.Id));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task DeleteQuestionSelectionRule_WhenUnauthorized_ShouldReturnUnathorized()
    {
        // Arrange
        var client = Sut.CreateClient();

        // Act
        var response = await client.DeleteAsync(
            UrlPackage.QuestionSelectionRule(Guid.NewGuid().ToString())
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
    #endregion


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
            false,
            startTime,
            endTime,
            10,
            null
        );
    }

    private static UpdateQuizTemplateCommand SeedDefaultUpdateQuizTemplateCommand(
        string? name = null,
        string? description = null
    )
    {
        var now = DateTimeOffset.UtcNow;
        var startTime = now.AddMinutes(10);
        var endTime = now.AddMinutes(30);

        return new UpdateQuizTemplateCommand(
            name ?? Guid.NewGuid().ToString(),
            description ?? Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            1,
            false,
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

    private static UpdateQuestionSelectionRuleCommand SeedDefaultUpdateQuestionSelectionRuleCommand(
        string? groupId = null,
        int level = 0,
        int count = 1,
        int score = 1
    )
    {
        return new UpdateQuestionSelectionRuleCommand(
            groupId ?? Guid.NewGuid().ToString(),
            QuestionType.SingleChoice,
            level,
            count,
            score
        );
    }
}
