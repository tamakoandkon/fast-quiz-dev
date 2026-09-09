using FastQuiz.Question.Domain.Entities;
using FastQuiz.Question.Domain.Exceptions;
using FastQuiz.Question.Domain.Repositories;
using FastQuiz.Question.Test.Infrastructure.Repositories.Utils;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Q = FastQuiz.Question.Domain.Entities.Question;

namespace FastQuiz.Question.Test.Infrastructure.Repositories;

[TestFixture]
public abstract class QuestionGroupRepositoryTestsBase
{
    protected abstract IQuestionGroupRepository Sut { get; }

    #region 测试方法

    [Test]
    public async Task Create_WhenGroupDoesNotExist_ShouldInsertOne()
    {
        // Arrange
        var group = QuestionGroupBuilder.New().Build();

        // Act
        await Sut.CreateAsync(group);

        // Assert
        var result = await Sut.FindByIdAsync(group.Id);
        result.ShouldNotBeNull();
        result.Id.ShouldBe(group.Id);
    }

    [Test]
    public async Task Create_WhenGroupDoesAlreadyExist_ShouldInsertFail()
    {
        // Arrange
        var group = QuestionGroupBuilder.New().Build();
        await Sut.CreateAsync(group);

        // Act & Assert
        Should.Throw<CreateQuestionGroupException>(() => Sut.CreateAsync(group));
    }

    [Test]
    public async Task FindById_WhenGroupDoesExist_ShouldReturnOne()
    {
        // Arrange
        var group = QuestionGroupBuilder.New().Build();
        await Sut.CreateAsync(group);

        // Act
        var result = await Sut.FindByIdAsync(group.Id);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(group.Id);
    }

    [Test]
    public async Task FindById_WhenGroupDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var group = QuestionGroupBuilder.New().Build();

        // Act
        var result = await Sut.FindByIdAsync(group.Id);

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public async Task FindByIds_WhenGroupDoesExist_ShouldReturnList()
    {
        // Arrange
        var groups = QuestionGroupBuilder.New().BuildMany(3);
        foreach (var group in groups)
        {
            await Sut.CreateAsync(group);
        }

        // Act
        var result = await Sut.FindByIdAsync([.. groups.Select(x => x.Id)]);

        // Assert
        result.ShouldNotBeEmpty();
        result.Count.ShouldBe(groups.Count);

        foreach (var expectedGroup in groups)
        {
            var actualGroup = result.FirstOrDefault(x => x.Id == expectedGroup.Id);
            actualGroup.ShouldNotBeNull();
        }
    }

    [Test]
    public async Task FindByIds_WhenGroupDoesNotExist_ShouldReturnEmpty()
    {
        // Arrange
        var groups = QuestionGroupBuilder.New().BuildMany(3);

        // Act
        var result = await Sut.FindByIdAsync([.. groups.Select(x => x.Id)]);

        // Assert
        result.ShouldBeEmpty();
    }

    [Test]
    public async Task FindDtoById_WhenGroupDoesExist_ShouldReturnOne()
    {
        // Arrange
        var group = QuestionGroupBuilder.New().Build();
        await Sut.CreateAsync(group);

        // Act
        var result = await Sut.FindDtoByIdAsync(group.Id);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(group.Id);
    }

    [Test]
    public async Task FindDtoById_WhenGroupDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var group = QuestionGroupBuilder.New().Build();

        // Act
        var result = await Sut.FindDtoByIdAsync(group.Id);

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public async Task GetAll_WhenInitialLoad_ShouldReturnCorrectCountAndOrder()
    {
        // Arrange:
        var builder = QuestionGroupBuilder.New().WithRandomOwnerId();
        List<QuestionGroup> groups = [builder.Build(), builder.Build(), builder.Build()];

        foreach (var g in groups)
            await Sut.CreateAsync(g);

        // Act:
        var result = await Sut.GetAllAsync(
            groups[0].OwnerId,
            cursor: null,
            count: 2,
            isPrevious: false,
            null
        );

        // Assert:
        result.Count.ShouldBe(2);
        result.All(r => groups.Select(g => g.Id).Contains(r.Id)).ShouldBeTrue();
    }

    [Test]
    public async Task GetAll_WhenForwardPagination_ShouldReturnNextPage()
    {
        // Arrange
        var builder = QuestionGroupBuilder.New().WithRandomOwnerId();
        var groups = Enumerable
            .Range(1, 5)
            .Select(i => builder.WithId(i.ToString().PadLeft(3, '0')).Build())
            .ToList();

        foreach (var g in groups)
            await Sut.CreateAsync(g);

        // Act:
        var result = await Sut.GetAllAsync(
            groups[0].OwnerId,
            cursor: "004",
            count: 2,
            isPrevious: false,
            null
        );

        // Assert
        result.Count.ShouldBe(2);
        var inputIds = groups.Select(g => g.Id).ToHashSet();
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
        var builder = QuestionGroupBuilder.New().WithRandomOwnerId();
        var groups = Enumerable
            .Range(1, 5)
            .Select(i => builder.WithId(i.ToString().PadLeft(3, '0')).Build())
            .ToList();

        foreach (var g in groups)
            await Sut.CreateAsync(g);

        // Act:
        var cursor = "002";
        var count = 2;
        var result = await Sut.GetAllAsync(
            groups[0].OwnerId,
            cursor: cursor,
            count: count,
            isPrevious: true,
            null
        );

        // Assert:
        result.Count.ShouldBe(count);

        var expectedIds = groups
            .Where(g => string.Compare(g.Id, cursor, StringComparison.Ordinal) > 0)
            .OrderBy(g => g.Id)
            .Take(count)
            .Select(g => g.Id)
            .ToList();

        expectedIds.Reverse();

        result.Select(r => r.Id).ShouldBe(expectedIds);
    }

    [Test]
    public async Task GetAll_WhenNamePrefixProvided_ShouldReturnOnlyGroupsWithMatchingNamePrefix()
    {
        // Arrange
        var builder = QuestionGroupBuilder.New().WithRandomOwnerId();

        var groups = new List<QuestionGroup>
        {
            builder.WithId("001").WithName("Math_Primary").Build(),
            builder.WithId("002").WithName("Math_Junior").Build(),
            builder.WithId("003").WithName("Chinese_Primary").Build(),
            builder.WithId("004").WithName("English_High").Build(),
            builder.WithId("005").WithName("Math_High").Build(),
        };

        foreach (var g in groups)
            await Sut.CreateAsync(g);

        // Act: Get only groups with name starting with "Math_"
        var result = await Sut.GetAllAsync(
            groups[0].OwnerId,
            cursor: null,
            count: 10,
            isPrevious: false,
            namePrefix: "Math_"
        );

        // Assert
        result.Count.ShouldBe(3);
        result.Select(r => r.Id).ShouldBe(["005", "002", "001"]);
        result.All(r => r.Name.StartsWith("Math_")).ShouldBeTrue();
    }

    [Test]
    public async Task GetCountByGroupIdAsync_WhenQuestionsExist_ShouldReturnCorrectCount()
    {
        // Arrange
        var groupId = "TEST_GROUP_001";
        var questionType = QuestionType.SingleChoice;
        var level = 2;

        // 创建题目组
        var group = QuestionGroupBuilder.New().WithId(groupId).Build();
        await Sut.CreateAsync(group);

        // 手动创建题目：3条匹配的，2条不匹配的
        var answers = new List<QuestionAnswer>();

        var questions = new List<Q>
        {
            // 匹配的题目（3条）
            new Q("Q001", QuestionType.SingleChoice, 2, "Test Q1 Content", null, groupId, answers),
            new Q("Q002", QuestionType.SingleChoice, 2, "Test Q2 Content", null, groupId, answers),
            new Q("Q003", QuestionType.SingleChoice, 2, "Test Q3 Content", null, groupId, answers),
            // 不匹配的题目（2条）
            new Q(
                "Q004",
                QuestionType.MultipleChoice,
                2,
                "Test Q4 Content",
                null,
                groupId,
                answers
            ),
            new Q("Q005", QuestionType.SingleChoice, 3, "Test Q5 Content", null, groupId, answers),
        };

        await AddQuestionsAsync(questions);

        // Act
        var result = await Sut.GetCountByGroupIdAsync(groupId, questionType, level);

        // Assert
        result.ShouldBe(3);
    }

    [Test]
    public async Task GetCountByGroupIdAsync_WhenNoMatchingQuestions_ShouldReturnZero()
    {
        // Arrange
        var groupId = "TEST_GROUP_002";
        var questionType = QuestionType.SingleChoice;
        var level = 2;

        var group = QuestionGroupBuilder.New().WithId(groupId).Build();
        await Sut.CreateAsync(group);

        // 添加一些不匹配的题目
        var answers = new List<QuestionAnswer>();
        var question = new Q(
            "Q006",
            QuestionType.MultipleChoice,
            2,
            "Non-matching question content",
            null,
            groupId,
            answers
        );

        await AddQuestionsAsync([question]);

        // Act
        var result = await Sut.GetCountByGroupIdAsync(groupId, questionType, level);

        // Assert
        result.ShouldBe(0);
    }

    [Test]
    public async Task GetCountByGroupIdAsync_WhenGroupDoesNotExist_ShouldReturnZero()
    {
        // Arrange
        var nonExistentGroupId = "NON_EXISTENT_GROUP";
        var questionType = QuestionType.SingleChoice;
        var level = 2;

        // Act
        var result = await Sut.GetCountByGroupIdAsync(nonExistentGroupId, questionType, level);

        // Assert
        result.ShouldBe(0);
    }

    [Test]
    public async Task GetCountByGroupIdAsync_WithDifferentLevels_ShouldCountCorrectly()
    {
        // Arrange
        var groupId = "TEST_GROUP_003";
        var questionType = QuestionType.SingleChoice;
        var answers = new List<QuestionAnswer>();

        var group = QuestionGroupBuilder.New().WithId(groupId).Build();
        await Sut.CreateAsync(group);

        // 添加不同等级的题目
        var questions = new List<Q>
        {
            new Q(
                "Q007",
                QuestionType.SingleChoice,
                1,
                "Level 1 Q1 Content",
                null,
                groupId,
                answers
            ),
            new Q(
                "Q008",
                QuestionType.SingleChoice,
                1,
                "Level 1 Q2 Content",
                null,
                groupId,
                answers
            ),
            new Q(
                "Q009",
                QuestionType.SingleChoice,
                2,
                "Level 2 Q1 Content",
                null,
                groupId,
                answers
            ),
            new Q(
                "Q010",
                QuestionType.SingleChoice,
                3,
                "Level 3 Q1 Content",
                null,
                groupId,
                answers
            ),
        };

        await AddQuestionsAsync(questions);

        // Act
        var resultLevel1 = await Sut.GetCountByGroupIdAsync(groupId, questionType, 1);
        var resultLevel2 = await Sut.GetCountByGroupIdAsync(groupId, questionType, 2);
        var resultLevel3 = await Sut.GetCountByGroupIdAsync(groupId, questionType, 3);

        // Assert
        resultLevel1.ShouldBe(2);
        resultLevel2.ShouldBe(1);
        resultLevel3.ShouldBe(1);
    }

    [Test]
    public async Task GetCountByGroupIdAsync_WithDifferentTypes_ShouldCountCorrectly()
    {
        // Arrange
        var groupId = "TEST_GROUP_004";
        var level = 2;
        var answers = new List<QuestionAnswer>();

        var group = QuestionGroupBuilder.New().WithId(groupId).Build();
        await Sut.CreateAsync(group);

        // 添加不同类型的题目
        var questions = new List<Q>
        {
            new Q(
                "Q011",
                QuestionType.SingleChoice,
                level,
                "Single choice Q1 Content",
                null,
                groupId,
                answers
            ),
            new Q(
                "Q012",
                QuestionType.SingleChoice,
                level,
                "Single choice Q2 Content",
                null,
                groupId,
                answers
            ),
            new Q(
                "Q013",
                QuestionType.MultipleChoice,
                level,
                "Multiple choice Q1 Content",
                null,
                groupId,
                answers
            ),
            new Q(
                "Q014",
                QuestionType.FillIn,
                level,
                "True false Q1 Content",
                null,
                groupId,
                answers
            ),
        };

        await AddQuestionsAsync(questions);

        // Act
        var resultSingleChoice = await Sut.GetCountByGroupIdAsync(
            groupId,
            QuestionType.SingleChoice,
            level
        );
        var resultMultipleChoice = await Sut.GetCountByGroupIdAsync(
            groupId,
            QuestionType.MultipleChoice,
            level
        );
        var resultTrueFalse = await Sut.GetCountByGroupIdAsync(groupId, QuestionType.FillIn, level);

        // Assert
        resultSingleChoice.ShouldBe(2);
        resultMultipleChoice.ShouldBe(1);
        resultTrueFalse.ShouldBe(1);
    }

    [Test]
    public async Task GetCountByGroupIdAsync_WithMixedData_ShouldReturnAccurateCount()
    {
        // Arrange
        var groupId = "TEST_GROUP_005";
        var otherGroupId = Guid.NewGuid().ToString();
        var answers = new List<QuestionAnswer>();

        var group = QuestionGroupBuilder.New().WithId(groupId).Build();
        await Sut.CreateAsync(group);

        var otherGroup = QuestionGroupBuilder.New().WithId(otherGroupId).Build();
        await Sut.CreateAsync(otherGroup);

        // 创建复杂混合数据
        var questions = new List<Q>
        {
            // Group A: SingleChoice, Level 1 (2条)
            new Q("Q015", QuestionType.SingleChoice, 1, "SC L1 Q1 Content", null, groupId, answers),
            new Q("Q016", QuestionType.SingleChoice, 1, "SC L1 Q2 Content", null, groupId, answers),
            // Group A: SingleChoice, Level 2 (3条)
            new Q("Q017", QuestionType.SingleChoice, 2, "SC L2 Q1 Content", null, groupId, answers),
            new Q("Q018", QuestionType.SingleChoice, 2, "SC L2 Q2 Content", null, groupId, answers),
            new Q("Q019", QuestionType.SingleChoice, 2, "SC L2 Q3 Content", null, groupId, answers),
            // Group A: MultipleChoice, Level 1 (1条)
            new Q(
                "Q020",
                QuestionType.MultipleChoice,
                1,
                "MC L1 Q1 Content",
                null,
                groupId,
                answers
            ),
            // 不同组ID的题目（不应被统计）
            new Q(
                "Q021",
                QuestionType.FillIn,
                2,
                "Other group Q Content",
                null,
                otherGroup.Id,
                answers
            ),
        };

        await AddQuestionsAsync(questions);

        // Act
        var resultSC_L1 = await Sut.GetCountByGroupIdAsync(groupId, QuestionType.SingleChoice, 1);
        var resultSC_L2 = await Sut.GetCountByGroupIdAsync(groupId, QuestionType.SingleChoice, 2);
        var resultMC_L1 = await Sut.GetCountByGroupIdAsync(groupId, QuestionType.MultipleChoice, 1);

        // Assert
        resultSC_L1.ShouldBe(2);
        resultSC_L2.ShouldBe(3);
        resultMC_L1.ShouldBe(1);
    }

    [Test]
    public async Task Update_WhenGroupExists_ShouldUpdateFieldsCorrectly()
    {
        // Arrange
        var original = QuestionGroupBuilder.New().Build();
        await Sut.CreateAsync(original);

        // Act
        var updated = QuestionGroupBuilder.New().WithId(original.Id).Build();
        await Sut.UpdateAsync(updated);

        // Assert
        var result = await Sut.FindByIdAsync(original.Id);
        result.ShouldNotBeNull();
        result.Name.ShouldBe(updated.Name);
        result.Remark.ShouldBe(updated.Remark);
        result.OwnerId.ShouldBe(original.OwnerId);
    }

    [Test]
    public async Task Delete_WhenGroupExists_ShouldRemoveFromDatabase()
    {
        // Arrange
        var group = QuestionGroupBuilder.New().Build();
        await Sut.CreateAsync(group);

        // Act
        await Sut.DeleteAsync(group.Id);

        // Assert
        var result = await Sut.FindByIdAsync(group.Id);
        result.ShouldBeNull();
    }

    [Test]
    public async Task Delete_WhenGroupDoesNotExist_ShouldNotThrow()
    {
        // Act & Assert
        await Should.NotThrowAsync(async () => await Sut.DeleteAsync("NON_EXISTENT_ID"));
    }

    [Test]
    public async Task DeleteMany_WhenGroupsExists_ShouldRemoveFromDatabase()
    {
        // Arrange
        var groups = QuestionGroupBuilder.New().BuildMany(3);
        foreach (var group in groups)
        {
            await Sut.CreateAsync(group);
        }

        // Act
        await Sut.DeleteAsync(groups.Select(x => x.Id).ToList());

        // Assert
        foreach (var group in groups)
        {
            var result = await Sut.FindByIdAsync(group.Id);
            result.ShouldBeNull();
        }
    }

    #endregion

    #region 辅助方法

    protected abstract Task AddQuestionsAsync(IEnumerable<Q> questions);

    #endregion
}
