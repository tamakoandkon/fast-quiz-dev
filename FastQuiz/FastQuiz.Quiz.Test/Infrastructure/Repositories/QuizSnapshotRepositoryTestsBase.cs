using FastQuiz.Quiz.Domain.Entities;
using FastQuiz.Quiz.Domain.Exceptions;
using FastQuiz.Quiz.Domain.Repositories;
using FastQuiz.Quiz.Test.Infrastructure.Repositories.Utils;
using Shouldly;

namespace FastQuiz.Quiz.Test.Infrastructure.Repositories;

internal abstract class QuizSnapshotRepositoryTestsBase
{
    protected abstract IQuizSnapshotRepository Sut { get; }
    protected abstract IQuizTemplateRepository Templates { get; }
    protected abstract IQuizAttemptRepository Attempts { get; }

    [Test]
    public async Task Create_WhenQuizSnapshotDoesNotExist_ShouldInsertSuceess()
    {
        // Arrange
        var template = await GetCreatedQuizTemplateAsync();
        var quiz = QuizSnapshotBuilder.New().WithQuizTemplateId(template.Id).Build();

        // Act
        await Sut.CreateAsync(quiz);

        // Assert
        var result = await Sut.FindByIdAsync(quiz.Id);
        result.ShouldNotBeNull();
        result.Id.ShouldBe(quiz.Id);
    }

    [Test]
    public async Task Create_WhenQuizSnapshotDoesAlreadyExist_ShouldThrowQuizSnapshotCreateException()
    {
        // Arrange
        var template = await GetCreatedQuizTemplateAsync();
        var quiz = QuizSnapshotBuilder.New().WithQuizTemplateId(template.Id).Build();
        await Sut.CreateAsync(quiz);
        var action = () => Sut.CreateAsync(quiz);

        // Act
        // Assert
        await action.ShouldThrowAsync<QuizSnapshotCreateException>();
    }

    [Test]
    public async Task GetAll_WhenWithNoCursor_ShouldReturnLatestItems()
    {
        // Arrange
        var template = await GetCreatedQuizTemplateAsync();
        var quizzes = QuizSnapshotBuilder
            .New()
            .BuildMany(3, (x, i) => x.WithQuizTemplateId(template.Id));
        foreach (var x in quizzes)
        {
            await Sut.CreateAsync(x);
        }
        var expectedIds = quizzes.Select(x => x.Id).OrderByDescending(x => x).Take(2).ToList();

        // Act
        var result = await Sut.GetAllAsync(template.Id, null, 2);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);
        result.Select(x => x.Id).ShouldBe(expectedIds, true);
    }

    [Test]
    public async Task GetAll_WhenWithCursor_ShouldReturnItemsLessThanCursor()
    {
        // Arrange
        var template = await GetCreatedQuizTemplateAsync();
        var quizzes = QuizSnapshotBuilder
            .New()
            .BuildMany(3, (x, i) => x.WithQuizTemplateId(template.Id));
        foreach (var x in quizzes)
        {
            await Sut.CreateAsync(x);
        }
        var expectedIds = quizzes
            .Select(x => x.Id)
            .Where(x => x.CompareTo(quizzes.Max(q => q.Id)) < 0)
            .OrderByDescending(x => x)
            .Take(2)
            .ToList();

        // Act
        var result = await Sut.GetAllAsync(template.Id, quizzes.Max(x => x.Id), 2);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);
        result.Select(x => x.Id).ShouldBe(expectedIds, true);
    }

    [Test]
    public async Task GetAll_WhenQuizSnapshotDoesNotExist_ShouldReturnEmpty()
    {
        // Arrange
        var template = await GetCreatedQuizTemplateAsync();
        var quizzes = QuizSnapshotBuilder
            .New()
            .BuildMany(3, (x, i) => x.WithQuizTemplateId(template.Id));

        // Act
        var result = await Sut.GetAllAsync(template.Id, null, 2);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    [Test]
    public async Task DeleteById_WhenQuizSnapshotDoesAlreadyExist_ShouldDeleteSuccess()
    {
        // Arrange
        var template = await GetCreatedQuizTemplateAsync();
        var quiz = QuizSnapshotBuilder.New().WithQuizTemplateId(template.Id).Build();
        await Sut.CreateAsync(quiz);

        // Act
        await Sut.DeleteByIdAsync(quiz.Id);

        // Assert
        var result = await Sut.FindByIdAsync(quiz.Id);
        result.ShouldBeNull();
    }

    [Test]
    public async Task DeleteById_WhenQuizSnapshotDoesNotExist_ShouldNotThrowException()
    {
        // Arrange
        var template = await GetCreatedQuizTemplateAsync();
        var quiz = QuizSnapshotBuilder.New().WithQuizTemplateId(template.Id).Build();
        await Sut.CreateAsync(quiz);
        var action = () => Sut.DeleteByIdAsync(quiz.Id);

        // Act
        // Assert
        await action.ShouldNotThrowAsync();
    }

    [Test]
    public async Task FindMinAttemptCountAsync_WhenAttemptsExist_ShouldReturnQuizSnapshotIdWithMinAttempts()
    {
        // Arrange
        var template = await GetCreatedQuizTemplateAsync();
        var snapshot1 = QuizSnapshotBuilder.New().WithQuizTemplateId(template.Id).Build();
        var snapshot2 = QuizSnapshotBuilder.New().WithQuizTemplateId(template.Id).Build();
        var snapshot3 = QuizSnapshotBuilder.New().WithQuizTemplateId(template.Id).Build();
        await Sut.CreateAsync(snapshot1);
        await Sut.CreateAsync(snapshot2);
        await Sut.CreateAsync(snapshot3);

        // Create attempts for snapshots
        // snapshot1: 2 attempts, snapshot2: 1 attempt, snapshot3: 3 attempts
        await CreateAttemptsAsync(snapshot1.Id, template.Id, 2);
        await CreateAttemptsAsync(snapshot2.Id, template.Id, 1);
        await CreateAttemptsAsync(snapshot3.Id, template.Id, 3);

        // Act
        var result = await Sut.FindMinAttemptCountAsync(template.Id);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBe(snapshot2.Id); // snapshot2 has minimum attempts (1)
    }

    [Test]
    public async Task FindMinAttemptCountAsync_WhenNoAttemptsExist_ShouldNotReturnNull()
    {
        // Arrange
        var template = await GetCreatedQuizTemplateAsync();
        var snapshot = QuizSnapshotBuilder.New().WithQuizTemplateId(template.Id).Build();
        await Sut.CreateAsync(snapshot);

        // Act
        var result = await Sut.FindMinAttemptCountAsync(template.Id);

        // Assert
        result.ShouldNotBeNull();
    }

    [Test]
    public async Task GetFirstSnapshotByTemplateIdAsync_WhenSnapshotsExist_ShouldReturnSnapshotWithMaxId()
    {
        // Arrange
        var template = await GetCreatedQuizTemplateAsync();
        var snapshots = QuizSnapshotBuilder
            .New()
            .BuildMany(3, (x, i) => x.WithQuizTemplateId(template.Id));
        foreach (var snapshot in snapshots)
        {
            await Sut.CreateAsync(snapshot);
        }
        var expectedId = snapshots.Max(x => x.Id);

        // Act
        var result = await Sut.GetFirstSnapshotByTemplateIdAsync(template.Id);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(expectedId);
        result.QuizTemplateId.ShouldBe(template.Id);
    }

    [Test]
    public async Task GetFirstSnapshotByTemplateIdAsync_WhenNoSnapshotsExist_ShouldReturnNull()
    {
        // Arrange
        var template = await GetCreatedQuizTemplateAsync();

        // Act
        var result = await Sut.GetFirstSnapshotByTemplateIdAsync(template.Id);

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public async Task DeleteByQuizTemplateIdAsync_WhenSnapshotsExist_ShouldDeleteAllSnapshots()
    {
        // Arrange
        var template = await GetCreatedQuizTemplateAsync();
        var snapshots = QuizSnapshotBuilder
            .New()
            .BuildMany(3, (x, i) => x.WithQuizTemplateId(template.Id));
        foreach (var snapshot in snapshots)
        {
            await Sut.CreateAsync(snapshot);
        }

        // Verify snapshots exist
        var beforeDelete = await Sut.GetAllAsync(template.Id, null, 10);
        beforeDelete.Count.ShouldBe(3);

        // Act
        await Sut.DeleteByQuizTemplateIdAsync(template.Id);

        // Assert
        var afterDelete = await Sut.GetAllAsync(template.Id, null, 10);
        afterDelete.ShouldNotBeNull();
        afterDelete.ShouldBeEmpty();
        afterDelete.Count.ShouldBe(0);
    }

    [Test]
    public async Task DeleteByQuizTemplateIdAsync_WhenNoSnapshotsExist_ShouldNotThrowException()
    {
        // Arrange
        var template = await GetCreatedQuizTemplateAsync();
        var action = () => Sut.DeleteByQuizTemplateIdAsync(template.Id);

        // Act
        // Assert
        await action.ShouldNotThrowAsync();
    }

    #region Utils
    private async Task<QuizTemplate> GetCreatedQuizTemplateAsync()
    {
        var template = QuizTemplateBuilder.New().Build();
        await Templates.CreateAsync(template);
        return template;
    }

    private async Task CreateAttemptsAsync(string snapshotId, string templateId, int count)
    {
        for (int i = 0; i < count; i++)
        {
            var attempt = QuizAttemptBuilder
                .New()
                .WithQuizSnapshotId(snapshotId)
                .WithQuizTemplateId(templateId)
                .Build();
            await Attempts.CreateAsync(attempt);
        }
    }
    #endregion
}
