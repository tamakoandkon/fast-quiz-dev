using FastQuiz.Quiz.Domain.Entities;
using FastQuiz.Quiz.Domain.Exceptions;
using FastQuiz.Quiz.Domain.Repositories;
using FastQuiz.Quiz.Test.Infrastructure.Repositories.Utils;
using Shouldly;

namespace FastQuiz.Quiz.Test.Infrastructure.Repositories;

[TestFixture]
internal abstract class QuizTemplateRepositoryTestsBase
{
    protected abstract IQuizTemplateRepository Sut { get; }

    #region 测试方法

    [Test]
    public async Task Create_WhenTemplateDoesNotExist_ShouldInsertOne()
    {
        // Arrange
        var template = QuizTemplateBuilder.New().Build();

        // Act
        await Sut.CreateAsync(template);

        // Assert
        var result = await Sut.FindByIdAsync(template.Id);
        result.ShouldNotBeNull();
        result.Id.ShouldBe(template.Id);
    }

    [Test]
    public async Task Create_WhenTemplateDoesAlreadyExist_ShouldInsertFail()
    {
        // Arrange
        var template = QuizTemplateBuilder.New().Build();
        await Sut.CreateAsync(template);

        // Act & Assert
        Should.Throw<QuizTemplateCreateException>(() => Sut.CreateAsync(template));
    }

    [Test]
    public async Task FindById_WhenTemplateDoesExist_ShouldReturnOne()
    {
        // Arrange
        var template = QuizTemplateBuilder.New().Build();
        await Sut.CreateAsync(template);

        // Act
        var result = await Sut.FindByIdAsync(template.Id);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(template.Id);
    }

    [Test]
    public async Task FindById_WhenTemplateDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var template = QuizTemplateBuilder.New().Build();

        // Act
        var result = await Sut.FindByIdAsync(template.Id);

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public async Task FindByIds_WhenTemplatesDoExist_ShouldReturnList()
    {
        // Arrange
        var templates = QuizTemplateBuilder.New().BuildMany(3);
        foreach (var template in templates)
        {
            await Sut.CreateAsync(template);
        }

        // Act
        var result = await Sut.FindByIdAsync([.. templates.Select(x => x.Id)]);

        // Assert
        result.ShouldNotBeEmpty();
        result.Count.ShouldBe(templates.Count);
        result.Select(x => x.Id).ShouldBe(templates.Select(x => x.Id), true);
    }

    [Test]
    public async Task FindByIds_WhenTemplatesDoNotExist_ShouldReturnEmpty()
    {
        // Arrange
        var templates = QuizTemplateBuilder.New().BuildMany(3);

        // Act
        var result = await Sut.FindByIdAsync([.. templates.Select(x => x.Id)]);

        // Assert
        result.ShouldBeEmpty();
    }

    [Test]
    public async Task GetAll_WhenInitialLoad_ShouldReturnCorrectCountAndOrder()
    {
        // Arrange:
        var builder = QuizTemplateBuilder.New().WithOwnerId(Guid.NewGuid().ToString());
        List<QuizTemplate> templates = [builder.Build(), builder.Build(), builder.Build()];

        foreach (var t in templates)
            await Sut.CreateAsync(t);

        // Act:
        var result = await Sut.GetAllAsync(
            templates[0].OwnerId,
            cursor: null,
            count: 2,
            isPrevious: false,
            null,
            null
        );

        // Assert:
        result.Count.ShouldBe(2);
        result.All(r => templates.Select(t => t.Id).Contains(r.Id)).ShouldBeTrue();
    }

    [Test]
    public async Task GetAll_WhenForwardPagination_ShouldReturnNextPage()
    {
        // Arrange
        var builder = QuizTemplateBuilder.New().WithOwnerId(Guid.NewGuid().ToString());
        var templates = Enumerable
            .Range(1, 5)
            .Select(i => builder.WithId(i.ToString().PadLeft(3, '0')).Build())
            .ToList();

        foreach (var t in templates)
            await Sut.CreateAsync(t);

        // Act:
        var result = await Sut.GetAllAsync(
            templates[0].OwnerId,
            cursor: "004",
            count: 2,
            isPrevious: false,
            null,
            null
        );

        // Assert
        result.Count.ShouldBe(2);
        var inputIds = templates.Select(t => t.Id).ToHashSet();
        result.All(r => inputIds.Contains(r.Id)).ShouldBeTrue();

        result.All(r => string.Compare(r.Id, "004", StringComparison.Ordinal) < 0).ShouldBeTrue();

        var ids = result.Select(r => r.Id).ToList();
        ids[0].ShouldBe("003");
        ids[1].ShouldBe("002");
    }

    [Test]
    public async Task GetAll_WhenPreviousPagination_ShouldReturnPreviousPageInCorrectOrder()
    {
        // Arrange
        var builder = QuizTemplateBuilder.New().WithOwnerId(Guid.NewGuid().ToString());
        var templates = Enumerable
            .Range(1, 5)
            .Select(i => builder.WithId(i.ToString().PadLeft(3, '0')).Build())
            .ToList();

        foreach (var t in templates)
            await Sut.CreateAsync(t);

        // Act:
        var cursor = "002";
        var count = 2;
        var result = await Sut.GetAllAsync(
            templates[0].OwnerId,
            cursor: cursor,
            count: count,
            isPrevious: true,
            null,
            null
        );

        // Assert:
        result.Count.ShouldBe(count);

        var expectedIds = templates
            .Where(t => string.Compare(t.Id, cursor, StringComparison.Ordinal) > 0)
            .OrderBy(t => t.Id)
            .Take(count)
            .Select(t => t.Id)
            .ToList();

        expectedIds.Reverse();

        result.Select(r => r.Id).ShouldBe(expectedIds);
    }

    [Test]
    public async Task GetAll_WhenNamePrefixProvided_ShouldReturnOnlyTemplatesWithMatchingNamePrefix()
    {
        // Arrange
        var builder = QuizTemplateBuilder.New().WithOwnerId(Guid.NewGuid().ToString());

        var templates = new List<QuizTemplate>
        {
            builder.WithId("001").WithName("Math_Primary").Build(),
            builder.WithId("002").WithName("Math_Junior").Build(),
            builder.WithId("003").WithName("Chinese_Primary").Build(),
            builder.WithId("004").WithName("English_High").Build(),
            builder.WithId("005").WithName("Math_High").Build(),
        };

        foreach (var t in templates)
            await Sut.CreateAsync(t);

        // Act: Get only templates with name starting with "Math_"
        var result = await Sut.GetAllAsync(
            templates[0].OwnerId,
            cursor: null,
            count: 10,
            isPrevious: false,
            namePrefix: "Math_",
            null
        );

        // Assert
        result.Count.ShouldBe(3);
        result.Select(r => r.Id).ShouldBe(["005", "002", "001"]);
        result.All(r => r.Name.StartsWith("Math_")).ShouldBeTrue();
    }

    [Test]
    public async Task GetAll_WhenIsPublishedFilterProvided_ShouldReturnOnlyPublishedTemplates()
    {
        // Arrange
        var builder = QuizTemplateBuilder.New().WithOwnerId(Guid.NewGuid().ToString());
        var now = DateTimeOffset.UtcNow;

        var templates = new List<QuizTemplate>
        {
            builder.WithId("001").WithIsPublished(true).Build(),
            builder.WithId("002").WithIsPublished(true).Build(),
            builder.WithId("003").WithIsPublished(false).Build(),
            builder.WithId("004").WithIsPublished(false).Build(),
            builder.WithId("005").WithIsPublished(false).Build(),
        };

        foreach (var t in templates)
            await Sut.CreateAsync(t);

        // Act: Get only published templates (excluding expired ones)
        var result = await Sut.GetAllAsync(
            templates[0].OwnerId,
            cursor: null,
            count: 10,
            isPrevious: false,
            null,
            true
        );

        // Assert
        result.Count.ShouldBe(2);
        result.Select(r => r.Id).ShouldContain("001");
        result.Select(r => r.Id).ShouldContain("002");
        result.Select(r => r.Id).ShouldNotContain("003");
        result.Select(r => r.Id).ShouldNotContain("004");
        result.Select(r => r.Id).ShouldNotContain("005");
        result.All(r => r.IsPublished).ShouldBeTrue();
    }

    [Test]
    public async Task GetAll_WhenIsPublishedFilterIsFalse_ShouldReturnOnlyUnpublishedTemplates()
    {
        // Arrange
        var builder = QuizTemplateBuilder.New().WithOwnerId(Guid.NewGuid().ToString());

        var templates = new List<QuizTemplate>
        {
            builder.WithId("001").WithIsPublished(true).Build(),
            builder.WithId("002").WithIsPublished(false).Build(),
            builder.WithId("003").WithIsPublished(false).Build(),
            builder.WithId("004").WithIsPublished(true).Build(),
        };

        foreach (var t in templates)
            await Sut.CreateAsync(t);

        // Act: Get only unpublished templates
        var result = await Sut.GetAllAsync(
            templates[0].OwnerId,
            cursor: null,
            count: 10,
            isPrevious: false,
            null,
            false
        );

        // Assert
        result.Count.ShouldBe(2);
    }

    [Test]
    public async Task GetAll_WhenCombinedFiltersProvided_ShouldReturnFilteredResults()
    {
        // Arrange
        var builder = QuizTemplateBuilder.New().WithOwnerId(Guid.NewGuid().ToString());

        var templates = new List<QuizTemplate>
        {
            builder.WithId("001").WithName("Math_Primary").WithIsPublished(true).Build(),
            builder.WithId("002").WithName("Math_Junior").WithIsPublished(false).Build(),
            builder.WithId("003").WithName("Chinese_Primary").WithIsPublished(true).Build(),
            builder.WithId("004").WithName("Math_High").WithIsPublished(true).Build(),
            builder.WithId("005").WithName("Math_Middle").WithIsPublished(false).Build(),
        };

        foreach (var t in templates)
            await Sut.CreateAsync(t);

        // Act: Get published templates with name starting with "Math_"
        var result = await Sut.GetAllAsync(
            templates[0].OwnerId,
            cursor: null,
            count: 10,
            isPrevious: false,
            namePrefix: "Math_",
            true
        );

        // Assert
        result.Count.ShouldBe(2);
        result.Select(r => r.Id).ShouldContain("001");
        result.Select(r => r.Id).ShouldContain("004");
        result.All(r => r.Name.StartsWith("Math_")).ShouldBeTrue();
        result.All(r => r.IsPublished).ShouldBeTrue();
    }

    [Test]
    public async Task Update_WhenTemplateExists_ShouldUpdateFieldsCorrectly()
    {
        // Arrange
        var original = QuizTemplateBuilder.New().Build();
        await Sut.CreateAsync(original);

        // Act
        var updated = QuizTemplateBuilder
            .New()
            .WithId(original.Id)
            .WithName(Guid.NewGuid().ToString())
            .WithDescription(Guid.NewGuid().ToString())
            .WithRemark(Guid.NewGuid().ToString())
            .WithAttemptLimit(5)
            .WithIsUnlimitedTime(false)
            .WithBeginTime(DateTimeOffset.UtcNow.AddDays(-1))
            .WithEndTime(DateTimeOffset.UtcNow.AddDays(7))
            .WithDurationMinutes(60)
            .WithIsPublished(true)
            .WithUserGroupId(Guid.NewGuid().ToString())
            .WithUpdatedAt(DateTimeOffset.UtcNow.AddDays(1))
            .Build();

        await Sut.UpdateAsync(updated);

        // Assert
        var result = await Sut.FindByIdAsync(original.Id);
        result.ShouldNotBeNull();
        result.Id.ShouldBe(original.Id);
        result.Name.ShouldBe(updated.Name);
        result.Description.ShouldBe(updated.Description);
        result.Remark.ShouldBe(updated.Remark);
        result.AttemptLimit.ShouldBe(updated.AttemptLimit);
        result.IsUnlimitedTime.ShouldBe(updated.IsUnlimitedTime);
        result.BeginTime.ShouldBe(updated.BeginTime);
        result.EndTime.ShouldBe(updated.EndTime);
        result.DurationMinutes.ShouldBe(updated.DurationMinutes);
        result.IsPublished.ShouldBe(updated.IsPublished);
        result.CreatedAt.ShouldBe(original.CreatedAt);
        result.OwnerId.ShouldBe(original.OwnerId);
        result.UserGroupId.ShouldBe(updated.UserGroupId);
        result.UpdatedAt.ShouldBe(updated.UpdatedAt, TimeSpan.FromSeconds(1));
    }

    [Test]
    public async Task Update_WhenTemplateDoesNotExist_ShouldNotThrow()
    {
        // Arrange
        var template = QuizTemplateBuilder.New().Build();

        // Act & Assert
        await Should.NotThrowAsync(async () => await Sut.UpdateAsync(template));
    }

    [Test]
    public async Task DeleteByIdAsync_WhenTemplateExists_ShouldRemoveFromDatabase()
    {
        // Arrange
        var template = QuizTemplateBuilder.New().Build();
        await Sut.CreateAsync(template);

        // Act
        await Sut.DeleteByIdAsync(template.Id);

        // Assert
        var result = await Sut.FindByIdAsync(template.Id);
        result.ShouldBeNull();
    }

    [Test]
    public async Task DeleteByIdAsync_WhenTemplateDoesNotExist_ShouldNotThrow()
    {
        // Act & Assert
        await Should.NotThrowAsync(
            async () => await Sut.DeleteByIdAsync(Guid.NewGuid().ToString())
        );
    }

    #endregion
}
