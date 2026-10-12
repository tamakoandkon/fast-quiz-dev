using FastQuiz.Question.Domain.Entities;
using FastQuiz.Question.Domain.Exceptions;
using FastQuiz.Question.Domain.Repositories;
using FastQuiz.Test.Common;
using Shouldly;
using Q = FastQuiz.Question.Domain.Entities.Question;

namespace FastQuiz.Question.Test.Infrastructure.Repositories;

[TestFixture]
public abstract class QuestionRepositoryTestsBase
{
    protected abstract IQuestionRepository Sut { get; }
    protected abstract IQuestionGroupRepository GroupRepository { get; }

    #region 辅助方法

    private async Task<QuestionGroup> CreateGroupAsync()
    {
        var group = new QuestionGroup(TestId.New(), TestId.New(), TestId.New(), TestId.New());
        await GroupRepository.CreateAsync(group);
        return group;
    }

    #endregion

    #region Create

    [Test]
    public async Task Create_WhenQuestionDoesNotExist_ShouldInsertQuestionAndAnswers()
    {
        // Arrange
        var group = await CreateGroupAsync();

        var questionId = TestId.New();

        List<QuestionAnswer> answers =
        [
            new QuestionAnswer(TestId.New(), questionId, TestId.New(), true, 0, MatchStrategy.Manual),
            new QuestionAnswer(TestId.New(), questionId, TestId.New(), true, 0, MatchStrategy.Manual),
        ];
        var question = new Q(
            questionId,
            QuestionType.SingleChoice,
            0,
            TestId.New(),
            TestId.New(),
            group.Id,
            answers
        );

        // Act
        await Sut.CreateAsync(question);

        // Assert
        var result = await Sut.FindByIdAsync(question.Id);
        result.ShouldNotBeNull();
        result.Id.ShouldBe(question.Id);
        result.Answers.Count.ShouldBe(answers.Count);
        foreach (var expectedAnswer in answers)
        {
            var actualAnswer = result.Answers.FirstOrDefault(x => x.Id == expectedAnswer.Id);
            actualAnswer.ShouldNotBeNull($"Answer with ID {expectedAnswer.Id} was not found.");
            actualAnswer.QuestionId.ShouldBe(question.Id);
            actualAnswer.MarkupContent.ShouldBe(expectedAnswer.MarkupContent);
            actualAnswer.IsCorrect.ShouldBe(expectedAnswer.IsCorrect);
            actualAnswer.MatchStrategy.ShouldBe(expectedAnswer.MatchStrategy);
        }
    }

    [Test]
    public async Task Create_ShouldPersistAllFieldsAndAnswerSortOrder()
    {
        // Arrange
        var group = await CreateGroupAsync();

        var questionId = TestId.New();
        var answers = new List<QuestionAnswer>
        {
            new(TestId.New(), questionId, "A-third", false, 3, MatchStrategy.Regex),
            new(TestId.New(), questionId, "A-first", true, 1, MatchStrategy.Exact),
            new(TestId.New(), questionId, "A-second", false, 2, MatchStrategy.Contains),
        };
        var question = new Q(
            questionId,
            QuestionType.MultipleChoice,
            7,
            "Markup",
            "Remark",
            group.Id,
            answers
        );

        // Act
        await Sut.CreateAsync(question);

        // Assert
        var result = await Sut.FindByIdAsync(questionId);
        result.ShouldNotBeNull();
        result.Type.ShouldBe(QuestionType.MultipleChoice);
        result.Level.ShouldBe(7);
        result.MarkupContent.ShouldBe("Markup");
        result.Remark.ShouldBe("Remark");
        result.GroupId.ShouldBe(group.Id);
        result.Answers.Count.ShouldBe(3);

        // MariaDb/PostgreSQL 实现按 SortOrder 排序返回答案 —— 校验顺序与序号都持久化成功。
        result.Answers.Select(a => a.SortOrder).ShouldBe([1, 2, 3]);
        result.Answers.Select(a => a.MarkupContent).ShouldBe(["A-first", "A-second", "A-third"]);

        var first = result.Answers.Single(a => a.SortOrder == 1);
        first.IsCorrect.ShouldBeTrue();
        first.MatchStrategy.ShouldBe(MatchStrategy.Exact);
        first.QuestionId.ShouldBe(questionId);
    }

    [Test]
    public async Task Create_WithNullRemark_ShouldPersistNullRemark()
    {
        // Arrange
        var group = await CreateGroupAsync();

        var questionId = TestId.New();
        var question = new Q(questionId, QuestionType.FillIn, 0, "M", null, group.Id, []);

        // Act
        await Sut.CreateAsync(question);

        // Assert
        var result = await Sut.FindByIdAsync(questionId);
        result.ShouldNotBeNull();
        result.Remark.ShouldBeNull();
    }

    [Test]
    public async Task Create_WhenIdAlreadyExists_ShouldThrowCreateQuestionException()
    {
        // Arrange
        var group = await CreateGroupAsync();

        var questionId = TestId.New();
        var question = new Q(
            questionId,
            QuestionType.SingleChoice,
            0,
            "Title",
            "Content",
            group.Id,
            []
        );

        await Sut.CreateAsync(question);

        // Act & Assert
        await Should.ThrowAsync<CreateQuestionException>(async () =>
        {
            await Sut.CreateAsync(question);
        });
    }

    #endregion

    #region FindById

    [Test]
    public async Task FindById_WhenQuestionExists_ShouldReturnQuestionWithAnswers()
    {
        // Arrange
        var group = await CreateGroupAsync();
        var questionId = TestId.New();

        var answers = new List<QuestionAnswer>
        {
            new(TestId.New(), questionId, "Answer 1", true, 0, MatchStrategy.Manual),
            new(TestId.New(), questionId, "Answer 2", false, 1, MatchStrategy.Manual),
        };
        var question = new Q(
            questionId,
            QuestionType.SingleChoice,
            0,
            "Title",
            "Content",
            group.Id,
            answers
        );

        await Sut.CreateAsync(question);

        // Act
        var result = await Sut.FindByIdAsync(questionId);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(questionId);
        result.Answers.Count.ShouldBe(2);
        result.Answers.ShouldContain(x => x.MarkupContent == "Answer 1");
        result.Answers.ShouldContain(x => x.MarkupContent == "Answer 2");
    }

    [Test]
    public async Task FindById_WhenQuestionDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var nonExistentId = TestId.New();

        // Act
        var result = await Sut.FindByIdAsync(nonExistentId);

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public async Task FindById_WhenQuestionHasNoAnswers_ShouldReturnQuestionWithEmptyList()
    {
        // Arrange
        var group = await CreateGroupAsync();
        var questionId = TestId.New();
        var question = new Q(
            questionId,
            QuestionType.SingleChoice,
            0,
            "Title",
            "Content",
            group.Id,
            []
        );
        await Sut.CreateAsync(question);

        // Act
        var result = await Sut.FindByIdAsync(questionId);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(questionId);
        result.Answers.ShouldNotBeNull();
        result.Answers.ShouldBeEmpty();
    }

    #endregion

    #region GetByIdAsync

    [Test]
    public async Task GetByIdAsync_WithValidIds_ShouldReturnMatchingQuestions()
    {
        // Arrange
        var group = await CreateGroupAsync();

        var question1 = new Q(TestId.New(), QuestionType.SingleChoice, 1, "C", "R", group.Id, []);
        var question2 = new Q(TestId.New(), QuestionType.SingleChoice, 1, "C", "R", group.Id, []);
        var question3 = new Q(TestId.New(), QuestionType.MultipleChoice, 2, "C", "R", group.Id, []);

        await Sut.CreateAsync(question1);
        await Sut.CreateAsync(question2);
        await Sut.CreateAsync(question3);

        var ids = new[] { question2.Id, question1.Id, question3.Id };

        // Act
        var result = await Sut.GetByIdAsync(ids);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(3);
        result.Select(q => q.Id).ShouldBe(ids, ignoreOrder: true);
    }

    [Test]
    public async Task GetByIdAsync_ShouldMapAnswersToTheirOwnQuestions()
    {
        // Arrange
        var group = await CreateGroupAsync();
        var groupId = group.Id;

        var qId1 = TestId.New();
        var qId2 = TestId.New();
        var q1Answers = new List<QuestionAnswer>
        {
            new(TestId.New(), qId1, "Q1-Ans1", true, 1, MatchStrategy.Exact),
            new(TestId.New(), qId1, "Q1-Ans2", false, 2, MatchStrategy.Contains),
        };
        var q2Answers = new List<QuestionAnswer>
        {
            new(TestId.New(), qId2, "Q2-Ans1", true, 1, MatchStrategy.Manual),
        };

        await Sut.CreateAsync(new Q(qId1, QuestionType.SingleChoice, 1, "C", "R", groupId, q1Answers));
        await Sut.CreateAsync(new Q(qId2, QuestionType.FillIn, 2, "C", "R", groupId, q2Answers));

        // Act
        var result = await Sut.GetByIdAsync([qId1, qId2]);

        // Assert
        result.Count.ShouldBe(2);

        var question1 = result.Single(x => x.Id == qId1);
        question1.Answers.Count.ShouldBe(2);
        question1.Answers.ShouldAllBe(a => a.QuestionId == qId1);

        var question2 = result.Single(x => x.Id == qId2);
        question2.Answers.Count.ShouldBe(1);
        question2.Answers.Single().QuestionId.ShouldBe(qId2);
    }

    [Test]
    public async Task GetByIdAsync_WhenSomeIdsMissing_ShouldReturnOnlyExisting()
    {
        // Arrange
        var group = await CreateGroupAsync();

        var questionId = TestId.New();
        await Sut.CreateAsync(new Q(questionId, QuestionType.SingleChoice, 1, "C", "R", group.Id, []));

        // Act
        var result = await Sut.GetByIdAsync([questionId, TestId.New(), TestId.New()]);

        // Assert
        result.Count.ShouldBe(1);
        result.Single().Id.ShouldBe(questionId);
    }

    [Test]
    public async Task GetByIdAsync_WhenQuestionHasNoAnswers_ShouldReturnEmptyAnswers()
    {
        // Arrange
        var group = await CreateGroupAsync();

        var questionId = TestId.New();
        await Sut.CreateAsync(new Q(questionId, QuestionType.SingleChoice, 1, "C", "R", group.Id, []));

        // Act
        var result = await Sut.GetByIdAsync([questionId]);

        // Assert
        result.Count.ShouldBe(1);
        result.Single().Answers.ShouldNotBeNull();
        result.Single().Answers.ShouldBeEmpty();
    }

    [Test]
    public async Task GetByIdAsync_WithNoMatchingIds_ShouldReturnEmptyCollection()
    {
        // Arrange
        var ids = new[] { TestId.New(), TestId.New() };

        // Act
        var result = await Sut.GetByIdAsync(ids);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    [Test]
    public async Task GetByIdAsync_WithEmptyIds_ShouldReturnEmptyCollection()
    {
        // Arrange
        var ids = Enumerable.Empty<string>();

        // Act
        var result = await Sut.GetByIdAsync(ids);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    #endregion

    #region GetAllAsync

    [Test]
    public async Task GetAll_Pagination_ShouldReturnCorrectItemsAndOrder()
    {
        // Arrange
        var group = await CreateGroupAsync();
        var type = QuestionType.SingleChoice;
        var level = 1;
        var prefix = TestId.RandomToken();

        var qs = Enumerable
            .Range(1, 5)
            .Select(i => new Q(TestId.Sortable(i, prefix), type, level, $"Markup{i}", "Remark", group.Id, []))
            .ToList();

        foreach (var item in qs)
            await Sut.CreateAsync(item);

        var descendingIds = qs.Select(q => q.Id).OrderByDescending(x => x).ToList();

        // Act 1: 首页查询
        var page1 = await Sut.GetAllAsync(group.Id, type, level, null, 3, false);

        // Assert 1
        page1.Count.ShouldBe(3);
        page1.Select(x => x.Id).ShouldBe(descendingIds.Take(3));

        // Act 2: 向后翻页
        var cursor = page1.Last().Id;
        var page2 = await Sut.GetAllAsync(group.Id, type, level, cursor, 3, false);

        // Assert 2
        page2.Count.ShouldBe(2);
        page2.Select(x => x.Id).ShouldBe(descendingIds.Skip(3).Take(3));

        // Act 3: 向前翻页（从 page2 首项 002 往前，取 2 条 → 003,004，再反转为倒序）
        var prevCursor = page2.First().Id;
        var pagePrev = await Sut.GetAllAsync(group.Id, type, level, prevCursor, 2, true);

        // Assert 3
        pagePrev.Count.ShouldBe(2);
        pagePrev.Select(x => x.Id).ShouldBe([TestId.Sortable(4, prefix), TestId.Sortable(3, prefix)]);
    }

    [Test]
    public async Task GetAll_Filters_ShouldExcludeMismatchingItems()
    {
        // Arrange
        var group1 = await CreateGroupAsync();
        var group2 = await CreateGroupAsync();

        var targetId = TestId.New();
        await Sut.CreateAsync(new Q(targetId, QuestionType.SingleChoice, 1, "C", "R", group1.Id, []));
        await Sut.CreateAsync(new Q(TestId.New(), QuestionType.SingleChoice, 1, "C", "R", group2.Id, []));
        await Sut.CreateAsync(new Q(TestId.New(), QuestionType.MultipleChoice, 1, "C", "R", group1.Id, []));

        // Act
        var result = await Sut.GetAllAsync(
            group1.Id,
            QuestionType.SingleChoice,
            1,
            null,
            10,
            false
        );

        // Assert
        result.Count.ShouldBe(1);
        result[0].Id.ShouldBe(targetId);
    }

    [Test]
    public async Task GetAll_AnswersMapping_ShouldMapCorrectAnswersToCorrectQuestions()
    {
        // Arrange
        var group1 = await CreateGroupAsync();
        var qId1 = TestId.New();
        var qId2 = TestId.New();
        var a1Id = TestId.New();
        var a1 = new QuestionAnswer(a1Id, qId1, "Ans1", true, 0, MatchStrategy.Manual);
        var a2 = new QuestionAnswer(TestId.New(), qId2, "Ans2", true, 0, MatchStrategy.Manual);

        await Sut.CreateAsync(new Q(qId1, QuestionType.SingleChoice, 1, "C", "R", group1.Id, [a1]));
        await Sut.CreateAsync(new Q(qId2, QuestionType.SingleChoice, 1, "C", "R", group1.Id, [a2]));

        // Act
        var result = await Sut.GetAllAsync(
            group1.Id,
            QuestionType.SingleChoice,
            1,
            null,
            10,
            false
        );

        // Assert
        var q1Result = result.First(x => x.Id == qId1);
        q1Result.Answers.Count.ShouldBe(1);
        q1Result.Answers.First().Id.ShouldBe(a1Id);
    }

    [Test]
    public async Task GetAll_WhenTypeIsNull_ShouldReturnAllTypes()
    {
        // Arrange
        var group = await CreateGroupAsync();
        var groupId = group.Id;

        await Sut.CreateAsync(new Q(TestId.New(), QuestionType.SingleChoice, 1, "C", "R", groupId, []));
        await Sut.CreateAsync(new Q(TestId.New(), QuestionType.MultipleChoice, 1, "C", "R", groupId, []));
        await Sut.CreateAsync(new Q(TestId.New(), QuestionType.FillIn, 1, "C", "R", groupId, []));

        // Act — type 传 null 表示不按类型过滤
        var result = await Sut.GetAllAsync(groupId, null, 1, null, 10, false);

        // Assert
        result.Count.ShouldBe(3);
    }

    [Test]
    public async Task GetAll_WhenLevelIsNegative_ShouldReturnAllLevels()
    {
        // Arrange
        var group = await CreateGroupAsync();
        var groupId = group.Id;

        await Sut.CreateAsync(new Q(TestId.New(), QuestionType.SingleChoice, 1, "C", "R", groupId, []));
        await Sut.CreateAsync(new Q(TestId.New(), QuestionType.SingleChoice, 2, "C", "R", groupId, []));
        await Sut.CreateAsync(new Q(TestId.New(), QuestionType.SingleChoice, 3, "C", "R", groupId, []));

        // Act — level < 0 表示不按等级过滤
        var result = await Sut.GetAllAsync(groupId, QuestionType.SingleChoice, -1, null, 10, false);

        // Assert
        result.Count.ShouldBe(3);
    }

    [Test]
    public async Task GetAll_WhenNoMatchingQuestions_ShouldReturnEmptyCollection()
    {
        // Arrange
        var group = await CreateGroupAsync();
        var groupId = group.Id;

        await Sut.CreateAsync(new Q(TestId.New(), QuestionType.MultipleChoice, 5, "C", "R", groupId, []));

        // Act
        var result = await Sut.GetAllAsync(groupId, QuestionType.SingleChoice, 1, null, 10, false);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    [Test]
    public async Task GetAll_ShouldMapAnswersForEveryPagedQuestion()
    {
        // Arrange
        var group = await CreateGroupAsync();
        var groupId = group.Id;
        var prefix = TestId.RandomToken();

        for (var i = 1; i <= 3; i++)
        {
            var qId = TestId.Sortable(i, prefix);
            var answers = new List<QuestionAnswer>
            {
                new(TestId.New(), qId, $"{qId}-AnsA", true, 1, MatchStrategy.Exact),
                new(TestId.New(), qId, $"{qId}-AnsB", false, 2, MatchStrategy.Contains),
            };
            await Sut.CreateAsync(new Q(qId, QuestionType.SingleChoice, 1, "C", "R", groupId, answers));
        }

        // Act — 两页都要带回各自的答案
        var page1 = await Sut.GetAllAsync(groupId, QuestionType.SingleChoice, 1, null, 2, false);
        var page2 = await Sut.GetAllAsync(
            groupId,
            QuestionType.SingleChoice,
            1,
            page1.Last().Id,
            2,
            false
        );

        // Assert
        page1.Count.ShouldBe(2);
        page2.Count.ShouldBe(1);

        foreach (var q in page1.Concat(page2))
        {
            q.Answers.Count.ShouldBe(2);
            q.Answers.ShouldAllBe(a => a.QuestionId == q.Id);
        }

        page1
            .Single(x => x.Id == TestId.Sortable(3, prefix))
            .Answers.Select(a => a.SortOrder)
            .ShouldBe([1, 2]);
    }

    [Test]
    public async Task GetAll_WhenGroupHasNoQuestions_ShouldReturnEmptyCollection()
    {
        // Arrange
        var group = await CreateGroupAsync();

        // Act
        var result = await Sut.GetAllAsync(
            group.Id,
            QuestionType.SingleChoice,
            1,
            null,
            10,
            false
        );

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    #endregion

    #region GetQuestionIdsAsync

    [Test]
    public async Task GetQuestionIdsAsync_WithValidFilters_ShouldReturnAllMatchingIds()
    {
        // Arrange
        var groupId = TestId.New();
        var type = QuestionType.SingleChoice;
        var level = 1;

        var group = new QuestionGroup(
            groupId,
            TestId.New(),
            TestId.New(),
            TestId.New()
        );
        await GroupRepository.CreateAsync(group);

        var expectedIds = new List<string>();
        for (int i = 1; i <= 3; i++)
        {
            var id = TestId.New();
            expectedIds.Add(id);
            await Sut.CreateAsync(new Q(id, type, level, "C", "R", groupId, []));
        }

        var otherGroup = new QuestionGroup(
            TestId.New(),
            TestId.New(),
            TestId.New(),
            TestId.New()
        );
        await GroupRepository.CreateAsync(otherGroup);
        await Sut.CreateAsync(new Q(TestId.New(), type, level, "C", "R", otherGroup.Id, []));

        // Act
        var result = await Sut.GetQuestionIdsAsync(groupId, type, level);

        // Assert
        result.Count.ShouldBe(3);
        result.ShouldBe(expectedIds, ignoreOrder: true);
    }

    [Test]
    public async Task GetQuestionIdsAsync_WithNoMatchingItems_ShouldReturnEmptyCollection()
    {
        // Arrange
        var groupId = TestId.New();
        var type = QuestionType.SingleChoice;
        var level = 1;

        var group = new QuestionGroup(
            groupId,
            TestId.New(),
            TestId.New(),
            TestId.New()
        );
        await GroupRepository.CreateAsync(group);

        var otherGroup = new QuestionGroup(
            TestId.New(),
            TestId.New(),
            TestId.New(),
            TestId.New()
        );
        await GroupRepository.CreateAsync(otherGroup);
        await Sut.CreateAsync(new Q(TestId.New(), type, level, "C", "R", otherGroup.Id, []));

        await Sut.CreateAsync(
            new Q(TestId.New(), QuestionType.MultipleChoice, 2, "C", "R", groupId, [])
        );

        // Act
        var result = await Sut.GetQuestionIdsAsync(groupId, type, level);

        // Assert
        result.ShouldBeEmpty();
    }

    [Test]
    public async Task GetQuestionIdsAsync_ShouldApplyTypeAndLevelAndGroupTogether()
    {
        // Arrange
        var groupId = TestId.New();
        var group = new QuestionGroup(
            groupId,
            TestId.New(),
            TestId.New(),
            TestId.New()
        );
        await GroupRepository.CreateAsync(group);

        var hit1 = TestId.New();
        var hit2 = TestId.New();
        await Sut.CreateAsync(new Q(hit1, QuestionType.SingleChoice, 2, "C", "R", groupId, []));
        await Sut.CreateAsync(new Q(hit2, QuestionType.SingleChoice, 2, "C", "R", groupId, []));
        // 类型不符
        await Sut.CreateAsync(new Q(TestId.New(), QuestionType.MultipleChoice, 2, "C", "R", groupId, []));
        // 等级不符
        await Sut.CreateAsync(new Q(TestId.New(), QuestionType.SingleChoice, 3, "C", "R", groupId, []));
        // 组不符
        var otherGroup = new QuestionGroup(
            TestId.New(),
            TestId.New(),
            TestId.New(),
            TestId.New()
        );
        await GroupRepository.CreateAsync(otherGroup);
        await Sut.CreateAsync(new Q(TestId.New(), QuestionType.SingleChoice, 2, "C", "R", otherGroup.Id, []));

        // Act
        var result = await Sut.GetQuestionIdsAsync(groupId, QuestionType.SingleChoice, 2);

        // Assert
        result.Count.ShouldBe(2);
        result.ShouldBe([hit1, hit2], ignoreOrder: true);
    }

    #endregion

    #region Update

    [Test]
    public async Task Update_AllFields_ShouldBeUpdatedInDatabase()
    {
        // Arrange
        var group = await CreateGroupAsync();
        var id = TestId.New();
        var original = new Q(
            id,
            QuestionType.SingleChoice,
            1,
            "Old Content",
            "Old Remark",
            group.Id,
            []
        );
        await Sut.CreateAsync(original);

        var updated = new Q(
            id,
            QuestionType.MultipleChoice,
            5,
            "New Content",
            "New Remark",
            group.Id,
            []
        );

        // Act
        await Sut.UpdateAsync(updated);

        // Assert
        var result = await Sut.FindByIdAsync(id);
        result.ShouldNotBeNull();
        result.MarkupContent.ShouldBe("New Content");
        result.Remark.ShouldBe("New Remark");
        result.Level.ShouldBe(5);
        result.Type.ShouldBe(QuestionType.MultipleChoice);
    }

    [Test]
    public async Task Update_ShouldReplaceAnswersCompletely()
    {
        // Arrange
        var group = await CreateGroupAsync();
        var groupId = group.Id;
        var id = TestId.New();

        var originalAnswers = new List<QuestionAnswer>
        {
            new(TestId.New(), id, "Old-1", true, 1, MatchStrategy.Exact),
            new(TestId.New(), id, "Old-2", false, 2, MatchStrategy.Contains),
        };
        await Sut.CreateAsync(new Q(id, QuestionType.SingleChoice, 1, "O", "O", groupId, originalAnswers));

        var newAnswerId = TestId.New();
        var newAnswers = new List<QuestionAnswer>
        {
            new(newAnswerId, id, "New-1", false, 1, MatchStrategy.Regex),
        };
        var updated = new Q(id, QuestionType.FillIn, 4, "N", "N", groupId, newAnswers);

        // Act
        await Sut.UpdateAsync(updated);

        // Assert
        var result = await Sut.FindByIdAsync(id);
        result.ShouldNotBeNull();
        result.Type.ShouldBe(QuestionType.FillIn);
        result.Level.ShouldBe(4);
        result.MarkupContent.ShouldBe("N");
        result.Remark.ShouldBe("N");

        // 旧答案被整体删除，只保留新答案
        result.Answers.Count.ShouldBe(1);
        result.Answers.Single().Id.ShouldBe(newAnswerId);
    }

    [Test]
    public async Task Update_WhenExceptionOccurs_ShouldNotLoseOldAnswers()
    {
        // Arrange
        var group = await CreateGroupAsync();
        var id = TestId.New();
        var oldAnswerId = TestId.New();
        var oldAnswers = new List<QuestionAnswer>
        {
            new(oldAnswerId, id, "Old", true, 0, MatchStrategy.Manual),
        };
        await Sut.CreateAsync(
            new Q(id, QuestionType.SingleChoice, 1, "O", "O", group.Id, oldAnswers)
        );

        // Act
        var invalidQuestion = new Q(
            id,
            QuestionType.SingleChoice,
            1,
            "M",
            "M",
            group.Id,
            [
                new QuestionAnswer(oldAnswerId, id, "Duplicate", true, 0, MatchStrategy.Manual),
                new(oldAnswerId, id, "Old", true, 0, MatchStrategy.Manual),
            ]
        );

        await Should.ThrowAsync<UpdateQuestionException>(
            async () => await Sut.UpdateAsync(invalidQuestion)
        );

        // Assert
        var result = await Sut.FindByIdAsync(id);
        result!.Answers.Count.ShouldBe(1);
        result.Answers.First().MarkupContent.ShouldBe("Old");
    }

    [Test]
    public async Task Update_WithTrackedAnswers_ShouldStillSucceed()
    {
        // Arrange
        var group = await CreateGroupAsync();
        var id = TestId.New();
        var answerId = TestId.New();
        var initialAnswer = new QuestionAnswer(answerId, id, "C1", true, 0, MatchStrategy.Manual);
        var question = new Q(id, QuestionType.SingleChoice, 1, "C", "R", group.Id, [initialAnswer]);

        await Sut.CreateAsync(question);

        var newAnswer = new QuestionAnswer(
            answerId,
            id,
            "Updated Content",
            true,
            0,
            MatchStrategy.Manual
        );
        var updatedQuestion = new Q(
            id,
            QuestionType.SingleChoice,
            1,
            "C",
            "R",
            group.Id,
            [newAnswer]
        );

        // Act & Assert
        await Should.NotThrowAsync(async () => await Sut.UpdateAsync(updatedQuestion));

        var result = await Sut.FindByIdAsync(id);
        result!.Answers.First().MarkupContent.ShouldBe("Updated Content");
    }

    [Test]
    public async Task Update_WhenQuestionDoesNotExist_ShouldNotThrow()
    {
        // Arrange
        var group = await CreateGroupAsync();

        var ghost = new Q(TestId.New(), QuestionType.SingleChoice, 1, "C", "R", group.Id, []);

        // Act & Assert — ExecuteUpdate 影响 0 行，不应抛异常
        await Should.NotThrowAsync(async () => await Sut.UpdateAsync(ghost));
    }

    #endregion

    #region Delete

    [Test]
    public async Task Delete_WhenQuestionExists_ShouldDeleteQuestionAndRelatedAnswers()
    {
        // Arrange
        var group = await CreateGroupAsync();

        var questionId = TestId.New();
        var answers = new List<QuestionAnswer>
        {
            new(TestId.New(), questionId, "A1", true, 0, MatchStrategy.Manual),
        };
        var question = new Q(
            questionId,
            QuestionType.SingleChoice,
            0,
            "Title",
            "Content",
            group.Id,
            answers
        );

        await Sut.CreateAsync(question);

        var existing = await Sut.FindByIdAsync(questionId);
        existing.ShouldNotBeNull();

        // Act
        await Sut.DeleteAsync(questionId);

        // Assert
        var deletedQuestion = await Sut.FindByIdAsync(questionId);
        deletedQuestion.ShouldBeNull();
    }

    [Test]
    public async Task Delete_WhenQuestionExists_ShouldAlsoRemoveItsAnswers()
    {
        // Arrange
        var group = await CreateGroupAsync();
        var groupId = group.Id;

        var questionId = TestId.New();
        var answers = new List<QuestionAnswer>
        {
            new(TestId.New(), questionId, "A1", true, 1, MatchStrategy.Exact),
            new(TestId.New(), questionId, "A2", false, 2, MatchStrategy.Contains),
        };
        await Sut.CreateAsync(new Q(questionId, QuestionType.SingleChoice, 1, "C", "R", groupId, answers));

        // Act
        await Sut.DeleteAsync(questionId);

        // Assert — 题目消失，且其答案不再被任何查询带回
        var deletedQuestion = await Sut.FindByIdAsync(questionId);
        deletedQuestion.ShouldBeNull();

        var byIds = await Sut.GetByIdAsync([questionId]);
        byIds.ShouldBeEmpty();

        var all = await Sut.GetAllAsync(groupId, QuestionType.SingleChoice, 1, null, 10, false);
        all.ShouldBeEmpty();
    }

    [Test]
    public async Task Delete_ShouldNotRemoveOtherQuestionsOrTheirAnswers()
    {
        // Arrange
        var group = await CreateGroupAsync();
        var groupId = group.Id;

        var keepId = TestId.New();
        var keepAnswerId = TestId.New();
        var removeId = TestId.New();
        await Sut.CreateAsync(
            new Q(
                keepId,
                QuestionType.SingleChoice,
                1,
                "C",
                "R",
                groupId,
                [new QuestionAnswer(keepAnswerId, keepId, "Keep-Ans", true, 1, MatchStrategy.Exact)]
            )
        );
        await Sut.CreateAsync(
            new Q(
                removeId,
                QuestionType.SingleChoice,
                1,
                "C",
                "R",
                groupId,
                [new QuestionAnswer(TestId.New(), removeId, "Remove-Ans", true, 1, MatchStrategy.Exact)]
            )
        );

        // Act
        await Sut.DeleteAsync(removeId);

        // Assert
        var kept = await Sut.FindByIdAsync(keepId);
        kept.ShouldNotBeNull();
        kept.Answers.Count.ShouldBe(1);
        kept.Answers.Single().Id.ShouldBe(keepAnswerId);

        (await Sut.FindByIdAsync(removeId)).ShouldBeNull();
    }

    [Test]
    public async Task Delete_WhenQuestionDoesNotExist_ShouldNotThrowException()
    {
        // Arrange
        var nonExistentId = TestId.New();

        // Act & Assert
        await Should.NotThrowAsync(async () => await Sut.DeleteAsync(nonExistentId));
    }

    #endregion
}
