using FastQuiz.Question.Domain.Entities;
using FastQuiz.Quiz.Domain.Entities;
using FastQuiz.Quiz.Domain.Exceptions;
using FastQuiz.Quiz.Domain.Repositories;
using FastQuiz.Quiz.Test.Infrastructure.Repositories.Utils;
using Shouldly;
using static System.Guid;

namespace FastQuiz.Quiz.Test.Infrastructure.Repositories;

[TestFixture]
[Category("Sqlite")]
public abstract class QuestionSelectionRuleRepositoryTestsBase
{
    protected abstract IQuestionSelectionRuleRepository Sut { get; }
    protected abstract IQuizTemplateRepository TemplateRepository { get; }

    #region 辅助方法

    protected async Task<QuizTemplate> CreateTemplateAsync(string templateId)
    {
        var template = QuizTemplateBuilder.New().WithId(templateId).Build();

        await TemplateRepository.CreateAsync(template);
        return template;
    }

    #endregion

    #region 测试方法

    [Test]
    public async Task Create_WhenRuleDoesNotExist_ShouldInsertOne()
    {
        // Arrange
        var templateId = NewGuid().ToString();
        await CreateTemplateAsync(templateId);

        var rule = QuestionSelectionRuleBuilder.New().WithQuizTemplateId(templateId).Build();

        // Act
        await Sut.CreateAsync(rule);

        // Assert
        var result = await Sut.GetByIdAsync(rule.Id);
        result.ShouldNotBeNull();
        result.Id.ShouldBe(rule.Id);
        result.QuizeTemplateId.ShouldBe(templateId);
    }

    [Test]
    public void Create_WhenTemplateDoesNotExist_ShouldThrowException()
    {
        // Arrange
        var nonExistentTemplateId = NewGuid().ToString();
        var rule = QuestionSelectionRuleBuilder
            .New()
            .WithQuizTemplateId(nonExistentTemplateId)
            .Build();

        // Act & Assert
        Should.Throw<QuestionSelectionRuleCreateException>(() => Sut.CreateAsync(rule));
    }

    [Test]
    public async Task Create_WhenRuleDoesAlreadyExist_ShouldInsertFail()
    {
        // Arrange
        var templateId = NewGuid().ToString();
        await CreateTemplateAsync(templateId);

        var rule = QuestionSelectionRuleBuilder.New().WithQuizTemplateId(templateId).Build();
        await Sut.CreateAsync(rule);

        // Act & Assert
        Should.Throw<QuestionSelectionRuleCreateException>(() => Sut.CreateAsync(rule));
    }

    [Test]
    public async Task GetByIdAsync_WhenRuleDoesExist_ShouldReturnOne()
    {
        // Arrange
        var templateId = NewGuid().ToString();
        await CreateTemplateAsync(templateId);

        var rule = QuestionSelectionRuleBuilder.New().WithQuizTemplateId(templateId).Build();
        await Sut.CreateAsync(rule);

        // Act
        var result = await Sut.GetByIdAsync(rule.Id);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(rule.Id);
        result.QuizeTemplateId.ShouldBe(templateId);
    }

    [Test]
    public async Task GetByIdAsync_WhenRuleDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var rule = QuestionSelectionRuleBuilder.New().Build();

        // Act
        var result = await Sut.GetByIdAsync(rule.Id);

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public async Task GetByQuizTemplateIdAsync_WithCount_WhenRulesExist_ShouldReturnList()
    {
        // Arrange
        var templateId = NewGuid().ToString();
        await CreateTemplateAsync(templateId);

        var builder = QuestionSelectionRuleBuilder.New().WithQuizTemplateId(templateId);
        var rules = Enumerable
            .Range(1, 5)
            .Select(i => builder.WithId(i.ToString().PadLeft(3, '0')).Build())
            .ToList();

        foreach (var rule in rules)
            await Sut.CreateAsync(rule);

        // Act
        var result = await Sut.GetByQuizTemplateIdAsync(templateId, cursor: null, count: 3);

        // Assert
        result.ShouldNotBeEmpty();
        result.Count.ShouldBe(3);
        result
            .Select(r => r.Id)
            .ShouldBe(rules.OrderByDescending(r => r.Id).Take(3).Select(r => r.Id));
        result.All(r => r.QuizeTemplateId == templateId).ShouldBeTrue();
    }

    [Test]
    public async Task GetByQuizTemplateIdAsync_WithCount_WhenCursorProvided_ShouldReturnNextPage()
    {
        // Arrange
        var templateId = NewGuid().ToString();
        await CreateTemplateAsync(templateId);

        var builder = QuestionSelectionRuleBuilder.New().WithQuizTemplateId(templateId);
        var rules = Enumerable
            .Range(1, 5)
            .Select(i => builder.WithId(i.ToString().PadLeft(3, '0')).Build())
            .ToList();

        foreach (var rule in rules)
            await Sut.CreateAsync(rule);

        // Act
        var cursor = "003";
        var result = await Sut.GetByQuizTemplateIdAsync(templateId, cursor: cursor, count: 2);

        // Assert
        result.Count.ShouldBe(2);
        result.Select(r => r.Id).ShouldBe(["002", "001"]);
        result.All(r => string.Compare(r.Id, cursor, StringComparison.Ordinal) < 0).ShouldBeTrue();
        result.All(r => r.QuizeTemplateId == templateId).ShouldBeTrue();
    }

    [Test]
    public async Task GetByQuizTemplateIdAsync_WithCount_WhenDifferentTemplateId_ShouldReturnOnlyMatching()
    {
        // Arrange
        var templateId1 = NewGuid().ToString();
        var templateId2 = NewGuid().ToString();

        await CreateTemplateAsync(templateId1);
        await CreateTemplateAsync(templateId2);

        var builder1 = QuestionSelectionRuleBuilder.New().WithQuizTemplateId(templateId1);
        var rules1 = Enumerable
            .Range(1, 3)
            .Select(i => builder1.WithId($"A{i:00}").Build())
            .ToList();

        var builder2 = QuestionSelectionRuleBuilder.New().WithQuizTemplateId(templateId2);
        var rules2 = Enumerable
            .Range(1, 2)
            .Select(i => builder2.WithId($"B{i:00}").Build())
            .ToList();

        var allRules = rules1.Concat(rules2);
        foreach (var rule in allRules)
            await Sut.CreateAsync(rule);

        // Act
        var result = await Sut.GetByQuizTemplateIdAsync(templateId1, cursor: null, count: 10);

        // Assert
        result.Count.ShouldBe(3);
        result.Select(r => r.Id).ShouldBe(rules1.Select(r => r.Id), ignoreOrder: true);
        result.All(r => r.QuizeTemplateId == templateId1).ShouldBeTrue();
    }

    [Test]
    public async Task GetByQuizTemplateIdAsync_WithCount_WhenTemplateExistsButNoRules_ShouldReturnEmpty()
    {
        // Arrange
        var templateId = NewGuid().ToString();
        await CreateTemplateAsync(templateId);

        // Act
        var result = await Sut.GetByQuizTemplateIdAsync(templateId, cursor: null, count: 10);

        // Assert
        result.ShouldBeEmpty();
    }

    [Test]
    public async Task GetByQuizTemplateIdAsync_WithCount_WhenTemplateDoesNotExist_ShouldReturnEmpty()
    {
        // Arrange
        var nonExistentTemplateId = NewGuid().ToString();

        // Act
        var result = await Sut.GetByQuizTemplateIdAsync(
            nonExistentTemplateId,
            cursor: null,
            count: 10
        );

        // Assert
        result.ShouldBeEmpty();
    }

    [Test]
    public async Task GetByQuizTemplateIdAsync_WithoutCount_WhenRulesExist_ShouldReturnAll()
    {
        // Arrange
        var templateId = NewGuid().ToString();
        await CreateTemplateAsync(templateId);

        var builder = QuestionSelectionRuleBuilder.New().WithQuizTemplateId(templateId);
        var rules = Enumerable
            .Range(1, 4)
            .Select(i => builder.WithId(i.ToString().PadLeft(3, '0')).Build())
            .ToList();

        foreach (var rule in rules)
            await Sut.CreateAsync(rule);

        // Act
        var result = await Sut.GetByQuizTemplateIdAsync(templateId);

        // Assert
        result.Count.ShouldBe(4);
        result.Select(r => r.Id).ShouldBe(rules.OrderByDescending(r => r.Id).Select(r => r.Id));
        result.All(r => r.QuizeTemplateId == templateId).ShouldBeTrue();
    }

    [Test]
    public async Task GetByQuizTemplateIdAsync_WithoutCount_WhenDifferentTemplateId_ShouldReturnOnlyMatching()
    {
        // Arrange
        var templateId1 = NewGuid().ToString();
        var templateId2 = NewGuid().ToString();

        await CreateTemplateAsync(templateId1);
        await CreateTemplateAsync(templateId2);

        var builder1 = QuestionSelectionRuleBuilder.New().WithQuizTemplateId(templateId1);
        var rules1 = Enumerable
            .Range(1, 3)
            .Select(i => builder1.WithId($"C{i:00}").Build())
            .ToList();

        var builder2 = QuestionSelectionRuleBuilder.New().WithQuizTemplateId(templateId2);
        var rules2 = Enumerable
            .Range(1, 2)
            .Select(i => builder2.WithId($"D{i:00}").Build())
            .ToList();

        var allRules = rules1.Concat(rules2);
        foreach (var rule in allRules)
            await Sut.CreateAsync(rule);

        // Act
        var result = await Sut.GetByQuizTemplateIdAsync(templateId1);

        // Assert
        result.Count.ShouldBe(3);
        result.Select(r => r.Id).ShouldBe(rules1.Select(r => r.Id), ignoreOrder: true);
        result.All(r => r.QuizeTemplateId == templateId1).ShouldBeTrue();
    }

    [Test]
    public async Task GetByQuizTemplateIdAsync_WithoutCount_WhenTemplateExistsButNoRules_ShouldReturnEmpty()
    {
        // Arrange
        var templateId = NewGuid().ToString();
        await CreateTemplateAsync(templateId);

        // Act
        var result = await Sut.GetByQuizTemplateIdAsync(templateId);

        // Assert
        result.ShouldBeEmpty();
    }

    [Test]
    public async Task GetByQuizTemplateIdAsync_WithoutCount_WhenTemplateDoesNotExist_ShouldReturnEmpty()
    {
        // Arrange
        var nonExistentTemplateId = NewGuid().ToString();

        // Act
        var result = await Sut.GetByQuizTemplateIdAsync(nonExistentTemplateId);

        // Assert
        result.ShouldBeEmpty();
    }

    [Test]
    public async Task GetByTemplateIdAndConditionAsync_WhenRuleExists_ShouldReturnOne()
    {
        // Arrange
        var templateId = NewGuid().ToString();
        await CreateTemplateAsync(templateId);

        var groupId = NewGuid().ToString();
        var type = QuestionType.SingleChoice;
        var level = 2;

        var rule = QuestionSelectionRuleBuilder
            .New()
            .WithQuizTemplateId(templateId)
            .WithGroupId(groupId)
            .WithType(type)
            .WithLevel(level)
            .Build();

        await Sut.CreateAsync(rule);

        // Act
        var result = await Sut.GetByTemplateIdAndConditionAsync(templateId, groupId, type, level);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(rule.Id);
        result.QuizeTemplateId.ShouldBe(templateId);
        result.GroupId.ShouldBe(groupId);
        result.Type.ShouldBe(type);
        result.Level.ShouldBe(level);
    }

    [Test]
    public async Task GetByTemplateIdAndConditionAsync_WhenTemplateDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var nonExistentTemplateId = NewGuid().ToString();
        var groupId = NewGuid().ToString();
        var type = QuestionType.MultipleChoice;
        var level = 3;

        // Act
        var result = await Sut.GetByTemplateIdAndConditionAsync(
            nonExistentTemplateId,
            groupId,
            type,
            level
        );

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public async Task GetByTemplateIdAndConditionAsync_WhenRuleDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var templateId = NewGuid().ToString();
        await CreateTemplateAsync(templateId);

        var groupId = NewGuid().ToString();
        var type = QuestionType.MultipleChoice;
        var level = 3;

        // Act
        var result = await Sut.GetByTemplateIdAndConditionAsync(templateId, groupId, type, level);

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public async Task GetByTemplateIdAndConditionAsync_WhenPartialMatchExists_ShouldReturnNull()
    {
        // Arrange
        var templateId = NewGuid().ToString();
        await CreateTemplateAsync(templateId);

        var groupId = NewGuid().ToString();
        var type = QuestionType.SingleChoice;
        var level = 2;

        // 创建一条只有部分条件匹配的规则
        var rule = QuestionSelectionRuleBuilder
            .New()
            .WithQuizTemplateId(templateId)
            .WithGroupId(groupId)
            .WithType(QuestionType.MultipleChoice) // 不同的类型
            .WithLevel(level)
            .Build();

        await Sut.CreateAsync(rule);

        // Act
        var result = await Sut.GetByTemplateIdAndConditionAsync(templateId, groupId, type, level);

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public async Task GetByTemplateIdAndConditionAsync_WithDifferentTemplateId_ShouldReturnNull()
    {
        // Arrange
        var templateId1 = NewGuid().ToString();
        var templateId2 = NewGuid().ToString();

        await CreateTemplateAsync(templateId1);
        await CreateTemplateAsync(templateId2);

        var groupId = NewGuid().ToString();
        var type = QuestionType.SingleChoice;
        var level = 1;

        var rule = QuestionSelectionRuleBuilder
            .New()
            .WithQuizTemplateId(templateId1)
            .WithGroupId(groupId)
            .WithType(type)
            .WithLevel(level)
            .Build();

        await Sut.CreateAsync(rule);

        // Act
        var result = await Sut.GetByTemplateIdAndConditionAsync(templateId2, groupId, type, level);

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public async Task Update_WhenRuleExists_ShouldUpdateFieldsCorrectly()
    {
        // Arrange
        var templateId = NewGuid().ToString();
        await CreateTemplateAsync(templateId);

        var original = QuestionSelectionRuleBuilder.New().WithQuizTemplateId(templateId).Build();
        await Sut.CreateAsync(original);

        // Act
        var updated = QuestionSelectionRuleBuilder
            .New()
            .WithId(original.Id)
            .WithQuizTemplateId(templateId) // 保持同一个 template
            .WithGroupId(NewGuid().ToString())
            .WithType(QuestionType.MultipleChoice)
            .WithLevel(5)
            .WithCount(10)
            .WithScore(20)
            .Build();

        await Sut.UpdateAsync(updated);

        // Assert
        var result = await Sut.GetByIdAsync(original.Id);
        result.ShouldNotBeNull();
        result.GroupId.ShouldBe(updated.GroupId);
        result.Type.ShouldBe(QuestionType.MultipleChoice);
        result.Level.ShouldBe(5);
        result.Count.ShouldBe(10);
        result.Score.ShouldBe(20);
        // 不应更新的字段保持不变
        result.QuizeTemplateId.ShouldBe(original.QuizeTemplateId);
    }

    [Test]
    public async Task Update_WhenRuleDoesNotExist_ShouldNotThrow()
    {
        // Arrange
        var rule = QuestionSelectionRuleBuilder.New().Build();

        // Act & Assert
        await Should.NotThrowAsync(async () => await Sut.UpdateAsync(rule));
    }

    [Test]
    public async Task Update_ShouldOnlyUpdateSpecificFields()
    {
        // Arrange
        var templateId = NewGuid().ToString();
        await CreateTemplateAsync(templateId);

        var original = QuestionSelectionRuleBuilder
            .New()
            .WithQuizTemplateId(templateId)
            .WithGroupId(NewGuid().ToString())
            .WithType(QuestionType.SingleChoice)
            .WithLevel(3)
            .WithCount(5)
            .WithScore(10)
            .Build();

        await Sut.CreateAsync(original);

        // Act
        var updated = QuestionSelectionRuleBuilder
            .New()
            .WithId(original.Id)
            .WithQuizTemplateId(NewGuid().ToString()) // 这个字段不应该被更新
            .WithGroupId(NewGuid().ToString())
            .WithType(QuestionType.FillIn)
            .WithLevel(7)
            .WithCount(15)
            .WithScore(25)
            .Build();

        await Sut.UpdateAsync(updated);

        // Assert
        var result = await Sut.GetByIdAsync(original.Id);
        result.ShouldNotBeNull();
        result.GroupId.ShouldBe(updated.GroupId);
        result.Type.ShouldBe(QuestionType.FillIn);
        result.Level.ShouldBe(7);
        result.Count.ShouldBe(15);
        result.Score.ShouldBe(25);
        // QuizeTemplateId 不应该被 Update 方法更新
        result.QuizeTemplateId.ShouldBe(original.QuizeTemplateId);
    }

    [Test]
    public async Task DeleteByIdAsync_WhenRuleExists_ShouldRemoveFromDatabase()
    {
        // Arrange
        var templateId = NewGuid().ToString();
        await CreateTemplateAsync(templateId);

        var rule = QuestionSelectionRuleBuilder.New().WithQuizTemplateId(templateId).Build();
        await Sut.CreateAsync(rule);

        // Act
        await Sut.DeleteByIdAsync(rule.Id);

        // Assert
        var result = await Sut.GetByIdAsync(rule.Id);
        result.ShouldBeNull();

        // 验证 Template 仍然存在
        var template = await TemplateRepository.FindByIdAsync(templateId);
        template.ShouldNotBeNull();
    }

    [Test]
    public async Task DeleteByIdAsync_WhenRuleDoesNotExist_ShouldNotThrow()
    {
        // Act & Assert
        await Should.NotThrowAsync(async () => await Sut.DeleteByIdAsync(NewGuid().ToString()));
    }

    [Test]
    public async Task DeleteByIdAsync_ShouldNotAffectOtherRules()
    {
        // Arrange
        var templateId = NewGuid().ToString();
        await CreateTemplateAsync(templateId);

        var rule1 = QuestionSelectionRuleBuilder
            .New()
            .WithId(NewGuid().ToString())
            .WithQuizTemplateId(templateId)
            .Build();

        var rule2 = QuestionSelectionRuleBuilder
            .New()
            .WithId(NewGuid().ToString())
            .WithQuizTemplateId(templateId)
            .Build();

        await Sut.CreateAsync(rule1);
        await Sut.CreateAsync(rule2);

        // Act
        await Sut.DeleteByIdAsync(rule1.Id);

        // Assert
        var result1 = await Sut.GetByIdAsync(rule1.Id);
        result1.ShouldBeNull();

        var result2 = await Sut.GetByIdAsync(rule2.Id);
        result2.ShouldNotBeNull();
        result2.Id.ShouldBe(rule2.Id);

        // 验证 Template 仍然存在
        var template = await TemplateRepository.FindByIdAsync(templateId);
        template.ShouldNotBeNull();
    }

    #endregion
}
