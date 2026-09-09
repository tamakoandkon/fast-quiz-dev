using FastQuiz.Question.Domain.Entities;
using FastQuiz.Question.Domain.Exceptions;
using FastQuiz.Question.Domain.Repositories;
using Shouldly;
using Q = FastQuiz.Question.Domain.Entities.Question;

namespace FastQuiz.Question.Test.Infrastructure.Repositories;

[TestFixture]
public abstract class QuestionRepositoryTestsBase
{
    protected abstract IQuestionRepository Sut { get; }
    protected abstract IQuestionGroupRepository GroupRepository { get; }

    [Test]
    public async Task Create_WhenQuestionDoesNotExist_ShouldInsertQuestionAndAnswers()
    {
        // Arrange
        var group = new QuestionGroup(
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString()
        );
        await GroupRepository.CreateAsync(group);

        var questionId = Guid.NewGuid().ToString();

        List<QuestionAnswer> answers =
        [
            new QuestionAnswer(
                Guid.NewGuid().ToString(),
                questionId,
                Guid.NewGuid().ToString(),
                true,
                0,
                MatchStrategy.Manual
            ),
            new QuestionAnswer(
                Guid.NewGuid().ToString(),
                questionId,
                Guid.NewGuid().ToString(),
                true,
                0,
                MatchStrategy.Manual
            ),
        ];
        var question = new Q(
            questionId,
            QuestionType.SingleChoice,
            0,
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
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
    public async Task Create_WhenIdAlreadyExists_ShouldThrowCreateQuestionException()
    {
        // Arrange
        var group = new QuestionGroup(
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString()
        );
        await GroupRepository.CreateAsync(group);

        var questionId = Guid.NewGuid().ToString();
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

    [Test]
    public async Task Delete_WhenQuestionExists_ShouldDeleteQuestionAndRelatedAnswers()
    {
        // Arrange
        var group = new QuestionGroup(
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString()
        );
        await GroupRepository.CreateAsync(group);

        var questionId = Guid.NewGuid().ToString();
        var answers = new List<QuestionAnswer>
        {
            new(Guid.NewGuid().ToString(), questionId, "A1", true, 0, MatchStrategy.Manual),
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
    public async Task Delete_WhenQuestionDoesNotExist_ShouldNotThrowException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act & Assert
        await Should.NotThrowAsync(async () => await Sut.DeleteAsync(nonExistentId));
    }

    [Test]
    public async Task FindById_WhenQuestionExists_ShouldReturnQuestionWithAnswers()
    {
        // Arrange
        var questionId = Guid.NewGuid().ToString();
        var group = new QuestionGroup(
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString()
        );
        await GroupRepository.CreateAsync(group);

        var answers = new List<QuestionAnswer>
        {
            new(Guid.NewGuid().ToString(), questionId, "Answer 1", true, 0, MatchStrategy.Manual),
            new(Guid.NewGuid().ToString(), questionId, "Answer 2", false, 1, MatchStrategy.Manual),
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
        var nonExistentId = "NonExistentId";

        // Act
        var result = await Sut.FindByIdAsync(nonExistentId);

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public async Task FindById_WhenQuestionHasNoAnswers_ShouldReturnQuestionWithEmptyList()
    {
        // Arrange
        var group = new QuestionGroup(
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString()
        );
        await GroupRepository.CreateAsync(group);
        var questionId = Guid.NewGuid().ToString();
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

    [Test]
    public async Task Update_AllFields_ShouldBeUpdatedInDatabase()
    {
        // Arrange
        var id = Guid.NewGuid().ToString();
        var group = new QuestionGroup(
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString()
        );
        await GroupRepository.CreateAsync(group);
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
    public async Task Update_WhenExceptionOccurs_ShouldNotLoseOldAnswers()
    {
        // Arrange
        var id = Guid.NewGuid().ToString();
        var group = new QuestionGroup(
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString()
        );
        await GroupRepository.CreateAsync(group);
        var oldAnswerId = Guid.NewGuid().ToString();
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
        var id = Guid.NewGuid().ToString();
        var group = new QuestionGroup(
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString()
        );
        await GroupRepository.CreateAsync(group);
        var answerId = Guid.NewGuid().ToString();
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
    public async Task GetAll_Pagination_ShouldReturnCorrectItemsAndOrder()
    {
        // Arrange
        var group = new QuestionGroup(
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString()
        );
        await GroupRepository.CreateAsync(group);
        var type = QuestionType.SingleChoice;
        var level = 1;

        var qs = Enumerable
            .Range(1, 5)
            .Select(i => new Q($"ID00{i}", type, level, $"Markup{i}", "Remark", group.Id, []))
            .ToList();

        foreach (var item in qs)
            await Sut.CreateAsync(item);

        // Act 1: 首页查询
        var page1 = await Sut.GetAllAsync(group.Id, type, level, null, 3, false);

        // Assert 1
        page1.Count.ShouldBe(3);
        page1[0].Id.ShouldBe("ID005");
        page1[2].Id.ShouldBe("ID003");

        // Act 2: 向后翻页
        var cursor = page1.Last().Id;
        var page2 = await Sut.GetAllAsync(group.Id, type, level, cursor, 3, false);

        // Assert 2
        page2.Count.ShouldBe(2);
        page2[0].Id.ShouldBe("ID002");
        page2[1].Id.ShouldBe("ID001");

        // Act 3: 向前翻页
        var prevCursor = page2.First().Id;
        var pagePrev = await Sut.GetAllAsync(group.Id, type, level, prevCursor, 2, true);

        // Assert 3
        pagePrev.Count.ShouldBe(2);
        pagePrev[0].Id.ShouldBe("ID004");
        pagePrev[1].Id.ShouldBe("ID003");
    }

    [Test]
    public async Task GetAll_Filters_ShouldExcludeMismatchingItems()
    {
        // Arrange
        var group1 = new QuestionGroup(
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString()
        );
        await GroupRepository.CreateAsync(group1);
        var group2 = new QuestionGroup(
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString()
        );
        await GroupRepository.CreateAsync(group2);
        await Sut.CreateAsync(new Q("Q1", QuestionType.SingleChoice, 1, "C", "R", group1.Id, []));
        await Sut.CreateAsync(new Q("Q2", QuestionType.SingleChoice, 1, "C", "R", group2.Id, []));
        await Sut.CreateAsync(new Q("Q3", QuestionType.MultipleChoice, 1, "C", "R", group1.Id, []));

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
        result[0].Id.ShouldBe("Q1");
    }

    [Test]
    public async Task GetAll_AnswersMapping_ShouldMapCorrectAnswersToCorrectQuestions()
    {
        // Arrange
        var qId1 = "Q1";
        var qId2 = "Q2";
        var group1 = new QuestionGroup(
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString()
        );
        await GroupRepository.CreateAsync(group1);
        var a1 = new QuestionAnswer("A1", qId1, "Ans1", true, 0, MatchStrategy.Manual);
        var a2 = new QuestionAnswer("A2", qId2, "Ans2", true, 0, MatchStrategy.Manual);

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
        q1Result.Answers.First().Id.ShouldBe("A1");
    }

    [Test]
    public async Task GetQuestionIdsAsync_WithValidFilters_ShouldReturnAllMatchingIds()
    {
        // Arrange
        var groupId = Guid.NewGuid().ToString();
        var type = QuestionType.SingleChoice;
        var level = 1;

        var group = new QuestionGroup(
            groupId,
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString()
        );
        await GroupRepository.CreateAsync(group);

        var expectedIds = new List<string>();
        for (int i = 1; i <= 3; i++)
        {
            var id = $"Q{i}";
            expectedIds.Add(id);
            await Sut.CreateAsync(new Q(id, type, level, "C", "R", groupId, []));
        }

        var otherGroup = new QuestionGroup(
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString()
        );
        await GroupRepository.CreateAsync(otherGroup);
        await Sut.CreateAsync(new Q("Q4", type, level, "C", "R", otherGroup.Id, []));

        // Act
        var result = await Sut.GetQuestionIdsAsync(groupId, type, level);

        // Assert
        result.Count.ShouldBe(3);
        result.ShouldBe(expectedIds);
    }

    [Test]
    public async Task GetQuestionIdsAsync_WithNoMatchingItems_ShouldReturnEmptyCollection()
    {
        // Arrange
        var groupId = Guid.NewGuid().ToString();
        var type = QuestionType.SingleChoice;
        var level = 1;

        var group = new QuestionGroup(
            groupId,
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString()
        );
        await GroupRepository.CreateAsync(group);

        var otherGroup = new QuestionGroup(
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString()
        );
        await GroupRepository.CreateAsync(otherGroup);
        await Sut.CreateAsync(new Q("Q1", type, level, "C", "R", otherGroup.Id, []));

        await Sut.CreateAsync(new Q("Q2", QuestionType.MultipleChoice, 2, "C", "R", groupId, []));

        // Act
        var result = await Sut.GetQuestionIdsAsync(groupId, type, level);

        // Assert
        result.ShouldBeEmpty();
    }

    [Test]
    public async Task GetByIdAsync_WithValidIds_ShouldReturnMatchingQuestions()
    {
        // Arrange
        var groupId = Guid.NewGuid().ToString();
        var group = new QuestionGroup(
            groupId,
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString()
        );
        await GroupRepository.CreateAsync(group);

        var question1 = new Q("Q1", QuestionType.SingleChoice, 1, "C", "R", groupId, []);
        var question2 = new Q("Q2", QuestionType.SingleChoice, 1, "C", "R", groupId, []);
        var question3 = new Q("Q3", QuestionType.MultipleChoice, 2, "C", "R", groupId, []);

        await Sut.CreateAsync(question1);
        await Sut.CreateAsync(question2);
        await Sut.CreateAsync(question3);

        var ids = new[] { "Q2", "Q1", "Q3" };

        // Act
        var result = await Sut.GetByIdAsync(ids);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(3);
        result.Select(q => q.Id).ShouldBe(ids, ignoreOrder: true);
    }

    [Test]
    public async Task GetByIdAsync_WithNoMatchingIds_ShouldReturnEmptyCollection()
    {
        // Arrange
        var ids = new[] { "NonExistent1", "NonExistent2" };

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
}
