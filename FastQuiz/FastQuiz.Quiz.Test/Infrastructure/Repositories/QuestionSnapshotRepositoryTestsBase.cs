using FastQuiz.Quiz.Domain.Entities;
using FastQuiz.Quiz.Domain.Exceptions;
using FastQuiz.Quiz.Domain.Repositories;
using FastQuiz.Quiz.Test.Infrastructure.Repositories.Utils;
using Shouldly;

namespace FastQuiz.Quiz.Test.Infrastructure.Repositories;

internal abstract class QuestionSnapshotRepositoryTestsBase
{
    protected abstract IQuestionSnapshotRepository Sut { get; }
    protected abstract IQuizTemplateRepository TemplateRepository { get; }

    [Test]
    public async Task Create_WhenQuestionSnapshotDoesNotExist_ShouldInsertSuccess()
    {
        // Arrange
        var template = await GetCreatedQuizTemplateAsync();
        var question = QuestionSnapshotBuilder.New().WithQuizTemplateId(template.Id).Build();
        var answers = QuestionAnswerSnapshotBuilder
            .New()
            .WithQuestionSnapshotId(question.Id)
            .BuildMany(2);
        question.AnswerSnapshots = answers;
        var exceptIds = answers.OrderBy(x => (x.SortOrder, x.Id)).Select(x => x.Id).ToList();

        // Act
        await Sut.CreateAsync(question);

        // Assert
        var result = await Sut.FindByIdAsync(question.Id);
        result.ShouldNotBeNull();
        result.Id.ShouldBe(question.Id);
        result.AnswerSnapshots.Count.ShouldBe(answers.Count);
        result
            .AnswerSnapshots.OrderBy(x => (x.SortOrder, x.Id))
            .Select(x => x.Id)
            .ShouldBe(exceptIds, true);
    }

    [Test]
    public async Task Create_WhenQuestionSnapshotDoesAlreadyExist_ShouldThrowQuestionSnapshotCreateException()
    {
        // Arrange
        var template = await GetCreatedQuizTemplateAsync();
        var question = QuestionSnapshotBuilder.New().WithQuizTemplateId(template.Id).Build();
        var answers = QuestionAnswerSnapshotBuilder
            .New()
            .WithQuestionSnapshotId(question.Id)
            .BuildMany(2);
        question.AnswerSnapshots = answers;
        await Sut.CreateAsync(question);
        var action = () => Sut.CreateAsync(question);

        // Act
        // Assert
        await action.ShouldThrowAsync<QuestionSnapshotCreateException>();
    }

    [Test]
    public async Task Create_WhenWithManyQuestionSnapshots_ShouldInsertMany()
    {
        // Arrange
        var template = await GetCreatedQuizTemplateAsync();
        var questions = QuestionSnapshotBuilder
            .New()
            .BuildMany(
                2,
                (x, i) =>
                {
                    x.WithQuizTemplateId(template.Id);
                }
            );
        var answers1 = QuestionAnswerSnapshotBuilder
            .New()
            .WithQuestionSnapshotId(questions[0].Id)
            .BuildMany(2);
        questions[0].AnswerSnapshots = answers1;
        var answers2 = QuestionAnswerSnapshotBuilder
            .New()
            .WithQuestionSnapshotId(questions[1].Id)
            .BuildMany(3);
        questions[1].AnswerSnapshots = answers2;
        var exceptIds1 = answers1.OrderBy(x => (x.SortOrder, x.Id)).Select(x => x.Id).ToList();
        var exceptIds2 = answers2.OrderBy(x => (x.SortOrder, x.Id)).Select(x => x.Id).ToList();

        // Act
        await Sut.CreateAsync(questions);

        // Assert
        var result1 = await Sut.FindByIdAsync(questions[0].Id);
        result1.ShouldNotBeNull();
        result1.Id.ShouldBe(questions[0].Id);
        result1.AnswerSnapshots.Count.ShouldBe(answers1.Count);
        result1
            .AnswerSnapshots.OrderBy(x => (x.SortOrder, x.Id))
            .Select(x => x.Id)
            .ShouldBe(exceptIds1, true);

        var result2 = await Sut.FindByIdAsync(questions[1].Id);
        result2.ShouldNotBeNull();
        result2.Id.ShouldBe(questions[1].Id);
        result2.AnswerSnapshots.Count.ShouldBe(answers2.Count);
        result2
            .AnswerSnapshots.OrderBy(x => (x.SortOrder, x.Id))
            .Select(x => x.Id)
            .ShouldBe(exceptIds2, true);
    }

    [Test]
    public async Task FindById_WhenQuestionSnapshotDoesAlreadyExist_ShouldReturnQuestionSnapshot()
    {
        // Arrange
        var template = await GetCreatedQuizTemplateAsync();
        var question = QuestionSnapshotBuilder.New().WithQuizTemplateId(template.Id).Build();
        var answers = QuestionAnswerSnapshotBuilder
            .New()
            .WithQuestionSnapshotId(question.Id)
            .BuildMany(2);
        question.AnswerSnapshots = answers;
        var exceptIds = answers.OrderBy(x => (x.SortOrder, x.Id)).Select(x => x.Id).ToList();
        await Sut.CreateAsync(question);

        // Act
        var result = await Sut.FindByIdAsync(question.Id);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(question.Id);
        result.AnswerSnapshots.Count.ShouldBe(answers.Count);
        result
            .AnswerSnapshots.OrderBy(x => (x.SortOrder, x.Id))
            .Select(x => x.Id)
            .ShouldBe(exceptIds, true);
    }

    [Test]
    public async Task FindById_WhenQuestionSnapshotDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        // Act
        var result = await Sut.FindByIdAsync(Guid.NewGuid().ToString());

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public async Task FindById_WhenWithIdList_ShouldReturnQuestionSnapshots()
    {
        // Arrange
        var template = await GetCreatedQuizTemplateAsync();
        var question = QuestionSnapshotBuilder.New().WithQuizTemplateId(template.Id).Build();
        var answers = QuestionAnswerSnapshotBuilder
            .New()
            .WithQuestionSnapshotId(question.Id)
            .BuildMany(2);
        question.AnswerSnapshots = answers;
        var exceptIds = answers.OrderBy(x => (x.SortOrder, x.Id)).Select(x => x.Id).ToList();
        await Sut.CreateAsync(question);

        // Act
        var result = await Sut.FindByIdAsync([question.Id]);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);
        result[0].Id.ShouldBe(question.Id);
        result[0].AnswerSnapshots.Count.ShouldBe(answers.Count);
        result[0]
            .AnswerSnapshots.OrderBy(x => (x.SortOrder, x.Id))
            .Select(x => x.Id)
            .ShouldBe(exceptIds, true);
    }

    [Test]
    public async Task FindById_WhenWithIdListAndQuestionSnapshotDoesNotExist_ShouldReturnEmpty()
    {
        // Arrange
        // Act
        var result = await Sut.FindByIdAsync([Guid.NewGuid().ToString()]);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(0);
    }

    [Test]
    public async Task FindByQuestionId_WhenQuestionSnapshotDoesAlreadyExist_ShouldReturnQuestionSnapshots()
    {
        // Arrange
        var template = await GetCreatedQuizTemplateAsync();
        var question = QuestionSnapshotBuilder.New().WithQuizTemplateId(template.Id).Build();
        var answers = QuestionAnswerSnapshotBuilder
            .New()
            .WithQuestionSnapshotId(question.Id)
            .BuildMany(2);
        question.AnswerSnapshots = answers;
        var exceptIds = answers.OrderBy(x => (x.SortOrder, x.Id)).Select(x => x.Id).ToList();
        await Sut.CreateAsync(question);

        // Act
        var result = await Sut.FindByQuestionIdAsync(
            question.QuizTemplateId,
            [question.QuestionId]
        );

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);
        result[0].Id.ShouldBe(question.Id);
        result[0].AnswerSnapshots.Count.ShouldBe(answers.Count);
        result[0]
            .AnswerSnapshots.OrderBy(x => (x.SortOrder, x.Id))
            .Select(x => x.Id)
            .ShouldBe(exceptIds, true);
    }

    [Test]
    public async Task FindByQuestionId_WhenQuestionSnapshotDoesNotExist_ShouldReturnEmpty()
    {
        // Arrange
        var template = await GetCreatedQuizTemplateAsync();
        var question = QuestionSnapshotBuilder.New().WithQuizTemplateId(template.Id).Build();
        var answers = QuestionAnswerSnapshotBuilder
            .New()
            .WithQuestionSnapshotId(question.Id)
            .BuildMany(2);

        // Act
        var result = await Sut.FindByQuestionIdAsync(
            question.QuizTemplateId,
            [question.QuestionId]
        );

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(0);
    }

    [Test]
    public async Task DeleteByQuizTemplateId_WhenQuestionDoesAlreadyExist_ShouldDeleteSuccess()
    {
        // Arrange
        var template = await GetCreatedQuizTemplateAsync();
        var question = QuestionSnapshotBuilder.New().WithQuizTemplateId(template.Id).Build();
        var answers = QuestionAnswerSnapshotBuilder
            .New()
            .WithQuestionSnapshotId(question.Id)
            .BuildMany(2);
        question.AnswerSnapshots = answers;
        var exceptIds = answers.OrderBy(x => (x.SortOrder, x.Id)).Select(x => x.Id).ToList();
        await Sut.CreateAsync(question);

        // Act
        await Sut.DeleteByTemplateIdAsync(question.QuizTemplateId);

        // Assert
        var result = await Sut.FindByIdAsync(question.Id);
        result.ShouldBeNull();

        var resultAnswers = await GetQuestionAnswerSnapshotsAsync(question.Id);
        resultAnswers.ShouldNotBeNull();
        resultAnswers.Count.ShouldBe(0);
    }

    [Test]
    public async Task DeleteByQuizTemplateId_WhenQuestionDoesNotExist_ShouldNotThrowAsync()
    {
        // Arrange
        var action = () => Sut.DeleteByTemplateIdAsync(Guid.NewGuid().ToString());

        // Act
        // Assert
        await action.ShouldNotThrowAsync();
    }

    #region Utils
    private async Task<QuizTemplate> GetCreatedQuizTemplateAsync()
    {
        var template = QuizTemplateBuilder.New().Build();
        await TemplateRepository.CreateAsync(template);
        return template;
    }

    protected abstract Task<List<QuestionAnswerSnapshot>> GetQuestionAnswerSnapshotsAsync(
        string questionId
    );
    #endregion
}
