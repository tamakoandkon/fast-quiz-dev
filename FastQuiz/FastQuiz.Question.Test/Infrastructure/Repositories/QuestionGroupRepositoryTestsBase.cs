using FastQuiz.Question.Domain.Entities;
using FastQuiz.Question.Domain.Exceptions;
using FastQuiz.Question.Domain.Repositories;
using FastQuiz.Question.Test.Infrastructure.Repositories.Utils;
using FastQuiz.Test.Common;
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
    public async Task Create_ShouldPersistAllFields()
    {
        // Arrange
        var id = TestId.New();
        var ownerId = TestId.New();
        var group = QuestionGroupBuilder
            .New()
            .WithId(id)
            .WithOwnerId(ownerId)
            .WithName("GroupName")
            .WithRemark("GroupRemark")
            .Build();

        // Act
        await Sut.CreateAsync(group);

        // Assert
        var result = await Sut.FindByIdAsync(id);
        result.ShouldNotBeNull();
        result.OwnerId.ShouldBe(ownerId);
        result.Name.ShouldBe("GroupName");
        result.Remark.ShouldBe("GroupRemark");
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
    public async Task FindByIds_WhenSomeIdsMissing_ShouldReturnOnlyExisting()
    {
        // Arrange
        var groups = QuestionGroupBuilder.New().BuildMany(2);
        foreach (var group in groups)
        {
            await Sut.CreateAsync(group);
        }

        // Act
        var result = await Sut.FindByIdAsync(
            [.. groups.Select(x => x.Id), TestId.New(), TestId.New()]
        );

        // Assert
        result.Count.ShouldBe(2);
        result.Select(x => x.Id).ShouldBe(groups.Select(g => g.Id), ignoreOrder: true);
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
    public async Task FindDtoById_WhenGroupHasQuestions_ShouldReturnQuestionCount()
    {
        // Arrange
        var groupId = TestId.New();
        var group = QuestionGroupBuilder.New().WithId(groupId).Build();
        await Sut.CreateAsync(group);

        var answers = new List<QuestionAnswer>();
        await AddQuestionsAsync(
            [
                new Q(TestId.New(), QuestionType.SingleChoice, 1, "C", null, groupId, answers),
                new Q(TestId.New(), QuestionType.SingleChoice, 1, "C", null, groupId, answers),
                new Q(TestId.New(), QuestionType.FillIn, 1, "C", null, groupId, answers),
            ]
        );

        // Act
        var result = await Sut.FindDtoByIdAsync(groupId);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(groupId);
        result.Count.ShouldBe(3);
    }

    [Test]
    public async Task FindDtoById_WhenGroupHasNoQuestions_ShouldReturnZeroCount()
    {
        // Arrange
        var groupId = TestId.New();
        await Sut.CreateAsync(QuestionGroupBuilder.New().WithId(groupId).Build());

        // Act
        var result = await Sut.FindDtoByIdAsync(groupId);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(0);
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
        var prefix = TestId.RandomToken();
        var builder = QuestionGroupBuilder.New().WithRandomOwnerId();
        var groups = Enumerable
            .Range(1, 5)
            .Select(i => builder.WithId(TestId.Sortable(i, prefix)).Build())
            .ToList();

        foreach (var g in groups)
            await Sut.CreateAsync(g);

        // 取第 4 条（倒序第 2 条）的 Id 作为游标
        var cursor = TestId.Sortable(4, prefix);

        // Act:
        var result = await Sut.GetAllAsync(
            groups[0].OwnerId,
            cursor: cursor,
            count: 2,
            isPrevious: false,
            null
        );

        // Assert
        result.Count.ShouldBe(2);
        var inputIds = groups.Select(g => g.Id).ToHashSet();
        result.All(r => inputIds.Contains(r.Id)).ShouldBeTrue();

        result.All(r => string.Compare(r.Id, cursor, StringComparison.Ordinal) < 0).ShouldBeTrue();

        var ids = result.Select(r => r.Id).ToList();
        ids[0].ShouldBe(TestId.Sortable(3, prefix));
        ids[1].ShouldBe(TestId.Sortable(2, prefix));
    }

    [Test]
    public async Task GetAll_WhenPreviousPagination_ShouldReturnPreviousPageInCorrectOrder()
    {
        // Arrange
        var prefix = TestId.RandomToken();
        var builder = QuestionGroupBuilder.New().WithRandomOwnerId();
        var groups = Enumerable
            .Range(1, 5)
            .Select(i => builder.WithId(TestId.Sortable(i, prefix)).Build())
            .ToList();

        foreach (var g in groups)
            await Sut.CreateAsync(g);

        // Act:
        var cursor = TestId.Sortable(2, prefix);
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
        var prefix = TestId.RandomToken();
        var builder = QuestionGroupBuilder.New().WithRandomOwnerId();

        var groups = new List<QuestionGroup>
        {
            builder.WithId(TestId.Sortable(1, prefix)).WithName("Math_Primary").Build(),
            builder.WithId(TestId.Sortable(2, prefix)).WithName("Math_Junior").Build(),
            builder.WithId(TestId.Sortable(3, prefix)).WithName("Chinese_Primary").Build(),
            builder.WithId(TestId.Sortable(4, prefix)).WithName("English_High").Build(),
            builder.WithId(TestId.Sortable(5, prefix)).WithName("Math_High").Build(),
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

        // Assert — 命中 3 条，按 Id 倒序：第 5、2、1 条
        result.Count.ShouldBe(3);
        result
            .Select(r => r.Id)
            .ShouldBe([TestId.Sortable(5, prefix), TestId.Sortable(2, prefix), TestId.Sortable(1, prefix)]);
        result.All(r => r.Name.StartsWith("Math_")).ShouldBeTrue();
    }

    [Test]
    public async Task GetAll_ShouldExposeQuestionCountPerGroup()
    {
        // Arrange
        var ownerId = TestId.New();
        var groupId1 = TestId.New();
        var groupId2 = TestId.New();
        await Sut.CreateAsync(QuestionGroupBuilder.New().WithId(groupId1).WithOwnerId(ownerId).Build());
        await Sut.CreateAsync(QuestionGroupBuilder.New().WithId(groupId2).WithOwnerId(ownerId).Build());

        var answers = new List<QuestionAnswer>();
        await AddQuestionsAsync(
            [
                new Q(TestId.New(), QuestionType.SingleChoice, 1, "C", null, groupId1, answers),
                new Q(TestId.New(), QuestionType.SingleChoice, 1, "C", null, groupId1, answers),
                new Q(TestId.New(), QuestionType.SingleChoice, 1, "C", null, groupId2, answers),
            ]
        );

        // Act
        var result = await Sut.GetAllAsync(
            ownerId,
            cursor: null,
            count: 10,
            isPrevious: false,
            null
        );

        // Assert
        result.Count.ShouldBe(2);
        result.Single(x => x.Id == groupId1).Count.ShouldBe(2);
        result.Single(x => x.Id == groupId2).Count.ShouldBe(1);
    }

    [Test]
    public async Task GetAll_ShouldNotReturnOtherOwnersGroups()
    {
        // Arrange
        var myOwnerId = TestId.New();
        var otherOwnerId = TestId.New();

        await Sut.CreateAsync(QuestionGroupBuilder.New().WithOwnerId(myOwnerId).Build());
        await Sut.CreateAsync(QuestionGroupBuilder.New().WithOwnerId(myOwnerId).Build());
        await Sut.CreateAsync(QuestionGroupBuilder.New().WithOwnerId(otherOwnerId).Build());

        // Act
        var result = await Sut.GetAllAsync(
            myOwnerId,
            cursor: null,
            count: 10,
            isPrevious: false,
            null
        );

        // Assert
        result.Count.ShouldBe(2);
        result.ShouldAllBe(x => x.OwnerId == myOwnerId);
    }

    [Test]
    public async Task GetAll_WhenOwnerHasNoGroups_ShouldReturnEmpty()
    {
        // Arrange
        var ownerId = TestId.New();

        // Act
        var result = await Sut.GetAllAsync(
            ownerId,
            cursor: null,
            count: 10,
            isPrevious: false,
            null
        );

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    [Test]
    public async Task GetAll_WhenNamePrefixAndPagingCombined_ShouldApplyBoth()
    {
        // Arrange
        var ownerId = TestId.New();
        var idPrefix = TestId.RandomToken();
        var mathPrefix = $"Math_{TestId.RandomToken()}_";
        var groups = new List<QuestionGroup>
        {
            QuestionGroupBuilder.New().WithId(TestId.Sortable(1, idPrefix)).WithOwnerId(ownerId).WithName($"{mathPrefix}P1").Build(),
            QuestionGroupBuilder.New().WithId(TestId.Sortable(2, idPrefix)).WithOwnerId(ownerId).WithName($"{mathPrefix}P2").Build(),
            QuestionGroupBuilder.New().WithId(TestId.Sortable(3, idPrefix)).WithOwnerId(ownerId).WithName($"{mathPrefix}P3").Build(),
            QuestionGroupBuilder.New().WithId(TestId.Sortable(4, idPrefix)).WithOwnerId(ownerId).WithName("Chinese").Build(),
        };
        foreach (var g in groups)
            await Sut.CreateAsync(g);

        // Act — 只取 Math_ 前缀，按 Id 倒序，每页 2 条
        var page1 = await Sut.GetAllAsync(
            ownerId,
            cursor: null,
            count: 2,
            isPrevious: false,
            mathPrefix
        );

        // Assert — 命中 Math_ 的三条按 Id 倒序取前 2 条
        page1.Count.ShouldBe(2);
        page1.Select(x => x.Id).ShouldBe([TestId.Sortable(3, idPrefix), TestId.Sortable(2, idPrefix)]);
        page1.ShouldAllBe(x => x.Name.StartsWith(mathPrefix));
    }

    [Test]
    public async Task GetAll_WhenPreviousPageAfterForwardRoundTrip_ShouldReturnFirstPageIds()
    {
        // Arrange
        var ownerId = TestId.New();
        var prefix = TestId.RandomToken();
        var groups = Enumerable
            .Range(1, 5)
            .Select(i =>
                QuestionGroupBuilder
                    .New()
                    .WithId(TestId.Sortable(i, prefix))
                    .WithOwnerId(ownerId)
                    .Build()
            )
            .ToList();
        foreach (var g in groups)
            await Sut.CreateAsync(g);

        var descendingIds = groups.Select(g => g.Id).OrderByDescending(x => x).ToList();

        // Act — page1（首页 2 条）→ page2（下一页）→ 再向前翻回
        var page1 = await Sut.GetAllAsync(ownerId, cursor: null, count: 2, isPrevious: false, null);
        page1.Select(x => x.Id).ShouldBe(descendingIds.Take(2));

        var page2 = await Sut.GetAllAsync(
            ownerId,
            cursor: page1[^1].Id,
            count: 2,
            isPrevious: false,
            null
        );
        page2.Select(x => x.Id).ShouldBe(descendingIds.Skip(2).Take(2));

        var back = await Sut.GetAllAsync(
            ownerId,
            cursor: page2[0].Id,
            count: 2,
            isPrevious: true,
            null
        );

        // Assert — 向前翻回首页，顺序保持 Id 倒序
        back.Select(x => x.Id).ShouldBe(descendingIds.Take(2));
    }

    [Test]
    public async Task GetCountByGroupIdAsync_WhenQuestionsExist_ShouldReturnCorrectCount()
    {
        // Arrange
        var groupId = TestId.New();
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
            new Q(TestId.New(), QuestionType.SingleChoice, 2, "Test Q1 Content", null, groupId, answers),
            new Q(TestId.New(), QuestionType.SingleChoice, 2, "Test Q2 Content", null, groupId, answers),
            new Q(TestId.New(), QuestionType.SingleChoice, 2, "Test Q3 Content", null, groupId, answers),
            // 不匹配的题目（2条）
            new Q(
                TestId.New(),
                QuestionType.MultipleChoice,
                2,
                "Test Q4 Content",
                null,
                groupId,
                answers
            ),
            new Q(TestId.New(), QuestionType.SingleChoice, 3, "Test Q5 Content", null, groupId, answers),
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
        var groupId = TestId.New();
        var questionType = QuestionType.SingleChoice;
        var level = 2;

        var group = QuestionGroupBuilder.New().WithId(groupId).Build();
        await Sut.CreateAsync(group);

        // 添加一些不匹配的题目
        var answers = new List<QuestionAnswer>();
        var question = new Q(
            TestId.New(),
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
        var nonExistentGroupId = TestId.New();
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
        var groupId = TestId.New();
        var questionType = QuestionType.SingleChoice;
        var answers = new List<QuestionAnswer>();

        var group = QuestionGroupBuilder.New().WithId(groupId).Build();
        await Sut.CreateAsync(group);

        // 添加不同等级的题目
        var questions = new List<Q>
        {
            new Q(
                TestId.New(),
                QuestionType.SingleChoice,
                1,
                "Level 1 Q1 Content",
                null,
                groupId,
                answers
            ),
            new Q(
                TestId.New(),
                QuestionType.SingleChoice,
                1,
                "Level 1 Q2 Content",
                null,
                groupId,
                answers
            ),
            new Q(
                TestId.New(),
                QuestionType.SingleChoice,
                2,
                "Level 2 Q1 Content",
                null,
                groupId,
                answers
            ),
            new Q(
                TestId.New(),
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
        var groupId = TestId.New();
        var level = 2;
        var answers = new List<QuestionAnswer>();

        var group = QuestionGroupBuilder.New().WithId(groupId).Build();
        await Sut.CreateAsync(group);

        // 添加不同类型的题目
        var questions = new List<Q>
        {
            new Q(
                TestId.New(),
                QuestionType.SingleChoice,
                level,
                "Single choice Q1 Content",
                null,
                groupId,
                answers
            ),
            new Q(
                TestId.New(),
                QuestionType.SingleChoice,
                level,
                "Single choice Q2 Content",
                null,
                groupId,
                answers
            ),
            new Q(
                TestId.New(),
                QuestionType.MultipleChoice,
                level,
                "Multiple choice Q1 Content",
                null,
                groupId,
                answers
            ),
            new Q(
                TestId.New(),
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
        var groupId = TestId.New();
        var otherGroupId = TestId.New();
        var answers = new List<QuestionAnswer>();

        var group = QuestionGroupBuilder.New().WithId(groupId).Build();
        await Sut.CreateAsync(group);

        var otherGroup = QuestionGroupBuilder.New().WithId(otherGroupId).Build();
        await Sut.CreateAsync(otherGroup);

        // 创建复杂混合数据
        var questions = new List<Q>
        {
            // Group A: SingleChoice, Level 1 (2条)
            new Q(TestId.New(), QuestionType.SingleChoice, 1, "SC L1 Q1 Content", null, groupId, answers),
            new Q(TestId.New(), QuestionType.SingleChoice, 1, "SC L1 Q2 Content", null, groupId, answers),
            // Group A: SingleChoice, Level 2 (3条)
            new Q(TestId.New(), QuestionType.SingleChoice, 2, "SC L2 Q1 Content", null, groupId, answers),
            new Q(TestId.New(), QuestionType.SingleChoice, 2, "SC L2 Q2 Content", null, groupId, answers),
            new Q(TestId.New(), QuestionType.SingleChoice, 2, "SC L2 Q3 Content", null, groupId, answers),
            // Group A: MultipleChoice, Level 1 (1条)
            new Q(
                TestId.New(),
                QuestionType.MultipleChoice,
                1,
                "MC L1 Q1 Content",
                null,
                groupId,
                answers
            ),
            // 不同组ID的题目（不应被统计）
            new Q(
                TestId.New(),
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
    public async Task GetStatistics_WithMixedQuestions_ShouldAggregateTotalTypeAndLevel()
    {
        // Arrange
        var groupId = TestId.New();
        await Sut.CreateAsync(QuestionGroupBuilder.New().WithId(groupId).Build());

        var answers = new List<QuestionAnswer>();
        await AddQuestionsAsync(
            [
                // SingleChoice: 3 题（Level 1 ×2, Level 2 ×1）
                new Q(TestId.New(), QuestionType.SingleChoice, 1, "C", null, groupId, answers),
                new Q(TestId.New(), QuestionType.SingleChoice, 1, "C", null, groupId, answers),
                new Q(TestId.New(), QuestionType.SingleChoice, 2, "C", null, groupId, answers),
                // MultipleChoice: 1 题（Level 2）
                new Q(TestId.New(), QuestionType.MultipleChoice, 2, "C", null, groupId, answers),
                // FillIn: 2 题（Level 3）
                new Q(TestId.New(), QuestionType.FillIn, 3, "C", null, groupId, answers),
                new Q(TestId.New(), QuestionType.FillIn, 3, "C", null, groupId, answers),
            ]
        );

        // Act
        var result = await Sut.GetStatisticsByGroupIdAsync(groupId);

        // Assert — 总数
        result.TotalCount.ShouldBe(6);

        // 题型分项：按 Type 升序（SingleChoice0 / MultipleChoice1 / FillIn2）
        result
            .Types.Select(x => x.Type)
            .ShouldBe(
                [QuestionType.SingleChoice, QuestionType.MultipleChoice, QuestionType.FillIn]
            );
        result.Types.Single(x => x.Type == QuestionType.SingleChoice).Count.ShouldBe(3);
        result.Types.Single(x => x.Type == QuestionType.MultipleChoice).Count.ShouldBe(1);
        result.Types.Single(x => x.Type == QuestionType.FillIn).Count.ShouldBe(2);

        // 难度分项：按 Level 升序，且基于题库全量题目（不受题型筛选影响）
        result.Levels.Select(x => x.Level).ShouldBe([1, 2, 3]);
        result.Levels.Single(x => x.Level == 1).Count.ShouldBe(2);
        result.Levels.Single(x => x.Level == 2).Count.ShouldBe(2);
        result.Levels.Single(x => x.Level == 3).Count.ShouldBe(2);
    }

    [Test]
    public async Task GetStatistics_WhenGroupHasNoQuestions_ShouldReturnZeroTotals()
    {
        // Arrange
        var groupId = TestId.New();
        await Sut.CreateAsync(QuestionGroupBuilder.New().WithId(groupId).Build());

        // Act
        var result = await Sut.GetStatisticsByGroupIdAsync(groupId);

        // Assert
        result.TotalCount.ShouldBe(0);
        result.Types.ShouldBeEmpty();
        result.Levels.ShouldBeEmpty();
    }

    [Test]
    public async Task GetStatistics_ShouldOnlyCountOwnGroup()
    {
        // Arrange
        var groupId = TestId.New();
        var otherGroupId = TestId.New();
        await Sut.CreateAsync(QuestionGroupBuilder.New().WithId(groupId).Build());
        await Sut.CreateAsync(QuestionGroupBuilder.New().WithId(otherGroupId).Build());

        var answers = new List<QuestionAnswer>();
        await AddQuestionsAsync(
            [
                new Q(TestId.New(), QuestionType.SingleChoice, 1, "C", null, groupId, answers),
                new Q(TestId.New(), QuestionType.SingleChoice, 1, "C", null, otherGroupId, answers),
                new Q(TestId.New(), QuestionType.MultipleChoice, 2, "C", null, otherGroupId, answers),
            ]
        );

        // Act
        var result = await Sut.GetStatisticsByGroupIdAsync(groupId);

        // Assert — 只统计本组，不串组
        result.TotalCount.ShouldBe(1);
        result.Types.Single().Type.ShouldBe(QuestionType.SingleChoice);
        result.Types.Single().Count.ShouldBe(1);
        result.Levels.Single().Level.ShouldBe(1);
        result.Levels.Single().Count.ShouldBe(1);
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
    public async Task Update_ShouldOnlyChangeNameAndRemark()
    {
        // Arrange
        var id = TestId.New();
        var keepOwnerId = TestId.New();
        var original = QuestionGroupBuilder
            .New()
            .WithId(id)
            .WithOwnerId(keepOwnerId)
            .WithName("OldName")
            .WithRemark("OldRemark")
            .Build();
        await Sut.CreateAsync(original);

        // Act — 换一个 OwnerId，但仓储只更新 Name/Remark
        var updated = QuestionGroupBuilder
            .New()
            .WithId(id)
            .WithOwnerId(TestId.New())
            .WithName("NewName")
            .WithRemark("NewRemark")
            .Build();
        await Sut.UpdateAsync(updated);

        // Assert
        var result = await Sut.FindByIdAsync(id);
        result.ShouldNotBeNull();
        result.Name.ShouldBe("NewName");
        result.Remark.ShouldBe("NewRemark");
        result.OwnerId.ShouldBe(keepOwnerId);
    }

    [Test]
    public async Task Update_WhenGroupDoesNotExist_ShouldNotThrow()
    {
        // Arrange
        var ghost = QuestionGroupBuilder.New().WithId(TestId.New()).Build();

        // Act & Assert — ExecuteUpdate 影响 0 行，不应抛异常
        await Should.NotThrowAsync(async () => await Sut.UpdateAsync(ghost));
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
        await Should.NotThrowAsync(async () => await Sut.DeleteAsync(TestId.New()));
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

    [Test]
    public async Task DeleteMany_WhenSomeIdsMissing_ShouldDeleteOnlyExisting()
    {
        // Arrange
        var groups = QuestionGroupBuilder.New().BuildMany(2);
        foreach (var group in groups)
            await Sut.CreateAsync(group);

        // Act
        await Sut.DeleteAsync([.. groups.Select(x => x.Id), TestId.New()]);

        // Assert
        foreach (var group in groups)
        {
            (await Sut.FindByIdAsync(group.Id)).ShouldBeNull();
        }
    }

    [Test]
    public async Task DeleteMany_WhenIdsEmpty_ShouldNotThrow()
    {
        // Act & Assert
        await Should.NotThrowAsync(async () => await Sut.DeleteAsync(new List<string>()));
    }

    #endregion


    #region 辅助方法

    protected abstract Task AddQuestionsAsync(IEnumerable<Q> questions);

    #endregion
}
