using FastQuiz.Quiz.Domain.Entities;
using FastQuiz.Quiz.Domain.Exceptions;
using FastQuiz.Quiz.Domain.Repositories;
using FastQuiz.Quiz.Test.Infrastructure.Repositories.Utils;
using Shouldly;

namespace FastQuiz.Quiz.Test.Infrastructure.Repositories;

internal abstract class QuizAttemptRepositoryTestsBase
{
    protected abstract IQuizAttemptRepository Sut { get; }
    protected abstract IQuizTemplateRepository Templates { get; }
    protected abstract IQuizSnapshotRepository QuizSnapshots { get; }

    [Test]
    public async Task Create_WhenQuizAttemptDoesNotExist_ShouldInsertSuccess()
    {
        // Arrange
        var template = await GetCreatedQuizTemplateAsync();
        var quizSnapshot = await GetCreatedQuizSnapshotAsync(template.Id);
        var attempt = QuizAttemptBuilder
            .New()
            .WithQuizTemplateId(template.Id)
            .WithQuizSnapshotId(quizSnapshot.Id)
            .Build();

        // Act
        await Sut.CreateAsync(attempt);

        // Assert
        var result = await Sut.FindByIdAsync(attempt.Id);
        result.ShouldNotBeNull();
        result.Id.ShouldBe(attempt.Id);
    }

    [Test]
    public async Task Create_WhenQuizAttemptDoesAlreadyExist_ShouldThrowQuizAttemptCreateException()
    {
        // Arrange
        var template = await GetCreatedQuizTemplateAsync();
        var quizSnapshot = await GetCreatedQuizSnapshotAsync(template.Id);
        var attempt = QuizAttemptBuilder
            .New()
            .WithQuizTemplateId(template.Id)
            .WithQuizSnapshotId(quizSnapshot.Id)
            .Build();
        await Sut.CreateAsync(attempt);
        var action = () => Sut.CreateAsync(attempt);

        // Act
        // Assert
        await action.ShouldThrowAsync<QuizAttemptCreateException>();
    }

    [Test]
    public async Task FindById_WhenQuizAttemptDoesAlreadyExist_ShouldReturnQuizAttempt()
    {
        // Arrange
        var template = await GetCreatedQuizTemplateAsync();
        var quizSnapshot = await GetCreatedQuizSnapshotAsync(template.Id);
        var attempt = QuizAttemptBuilder
            .New()
            .WithQuizTemplateId(template.Id)
            .WithQuizSnapshotId(quizSnapshot.Id)
            .Build();
        await Sut.CreateAsync(attempt);

        // Act
        var result = await Sut.FindByIdAsync(attempt.Id);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(attempt.Id);
    }

    [Test]
    public async Task FindById_WhenQuizAttemptDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var attempt = QuizAttemptBuilder.New().Build();

        // Act
        var result = await Sut.FindByIdAsync(attempt.Id);

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public async Task FindByQuizTemplateId_WhenQuizAttemptDoesAlreadyExist_ShouldReturnAllItems()
    {
        // Arrange
        var template = await GetCreatedQuizTemplateAsync();
        var quizSnapshot = await GetCreatedQuizSnapshotAsync(template.Id);
        var attemp = QuizAttemptBuilder
            .New()
            .WithQuizTemplateId(template.Id)
            .WithQuizSnapshotId(quizSnapshot.Id)
            .Build();
        await Sut.CreateAsync(attemp);

        // Act
        var result = await Sut.FindByQuizTemplateIdAsync(attemp.AssignId!, attemp.QuizTemplateId);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);
        result[0].Id.ShouldBe(attemp.Id);
    }

    [Test]
    public async Task FindByQuizTemplateId_WhenQuizAttemptDoesNotExist_ShouldReturnEmpty()
    {
        // Arrange
        var attempt = QuizAttemptBuilder.New().Build();

        // Act
        var result = await Sut.FindByQuizTemplateIdAsync(attempt.AssignId!, attempt.QuizTemplateId);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(0);
    }

    [Test]
    public async Task GetAll_WhenWithNoCursor_ShouldReturnLatestItems()
    {
        // Arrange
        var template = await GetCreatedQuizTemplateAsync();
        var quizSnapshot = await GetCreatedQuizSnapshotAsync(template.Id);
        var attempts = QuizAttemptBuilder
            .New()
            .BuildMany(
                3,
                (x, i) => x.WithQuizTemplateId(template.Id).WithQuizSnapshotId(quizSnapshot.Id)
            );
        foreach (var attempt in attempts)
        {
            await Sut.CreateAsync(attempt);
        }
        var expectedIds = attempts.Select(x => x.Id).OrderByDescending(x => x).Take(2).ToList();

        // Act
        var result = await Sut.GetAllByTemplateIdAsync(template.Id, null, 2);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);
        result.Select(x => x.Id).ShouldBe(expectedIds, false);
    }

    [Test]
    public async Task GetAll_WhenWithCursor_ShouldReturnItemsLessThanCursor()
    {
        // Arrange
        var template = await GetCreatedQuizTemplateAsync();
        var quizSnapshot = await GetCreatedQuizSnapshotAsync(template.Id);
        var attempts = QuizAttemptBuilder
            .New()
            .BuildMany(
                3,
                (x, i) => x.WithQuizTemplateId(template.Id).WithQuizSnapshotId(quizSnapshot.Id)
            );
        foreach (var attempt in attempts)
        {
            await Sut.CreateAsync(attempt);
        }
        var expectedIds = attempts
            .Where(x => x.Id.CompareTo(attempts.Max(a => a.Id)) < 0)
            .Select(x => x.Id)
            .OrderByDescending(x => x)
            .Take(2)
            .ToList();

        // Act
        var result = await Sut.GetAllByTemplateIdAsync(template.Id, attempts.Max(x => x.Id), 2);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);
        result.Select(x => x.Id).ShouldBe(expectedIds, false);
    }

    [Test]
    public async Task GetAll_WhenQuizAttemptDoesNotExist_ShouldReturnEmpty()
    {
        // Arrange
        var quizTemplateId = Guid.NewGuid().ToString();

        // Act
        var result = await Sut.GetAllByTemplateIdAsync(quizTemplateId, null, 2);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    [Test]
    public async Task GetAllByUserId_WhenNoCursor_ShouldReturnLatestItems()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var template = await GetCreatedQuizTemplateAsync();
        var quizSnapshot = await GetCreatedQuizSnapshotAsync(template.Id);
        var attempts = QuizAttemptBuilder
            .New()
            .BuildMany(
                3,
                (x, i) =>
                    x.WithQuizTemplateId(template.Id)
                        .WithQuizSnapshotId(quizSnapshot.Id)
                        .WithAssignId(userId)
            );
        foreach (var attempt in attempts)
        {
            await Sut.CreateAsync(attempt);
        }
        var expectedIds = attempts.Select(x => x.Id).OrderByDescending(x => x).Take(2).ToList();

        // Act
        var result = await Sut.GetAllByUserIdAsync(userId, null, 2);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);
        result.Select(x => x.Id).ShouldBe(expectedIds, false);
        result.All(x => x.AssignId == userId).ShouldBeTrue();
    }

    [Test]
    public async Task GetAllByUserId_WhenWithCursor_ShouldReturnItemsLessThanCursor()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var template = await GetCreatedQuizTemplateAsync();
        var quizSnapshot = await GetCreatedQuizSnapshotAsync(template.Id);
        var attempts = QuizAttemptBuilder
            .New()
            .BuildMany(
                3,
                (x, i) =>
                    x.WithQuizTemplateId(template.Id)
                        .WithQuizSnapshotId(quizSnapshot.Id)
                        .WithAssignId(userId)
            );
        foreach (var attempt in attempts)
        {
            await Sut.CreateAsync(attempt);
        }
        var expectedIds = attempts
            .Where(x => x.Id.CompareTo(attempts.Max(a => a.Id)) < 0)
            .Select(x => x.Id)
            .OrderByDescending(x => x)
            .Take(2)
            .ToList();

        // Act
        var result = await Sut.GetAllByUserIdAsync(userId, attempts.Max(x => x.Id), 2);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);
        result.Select(x => x.Id).ShouldBe(expectedIds, false);
        result.All(x => x.Id.CompareTo(attempts.Max(a => a.Id)) < 0).ShouldBeTrue();
    }

    [Test]
    public async Task GetAllByUserId_WhenQuizAttemptDoesNotExist_ShouldReturnEmpty()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();

        // Act
        var result = await Sut.GetAllByUserIdAsync(userId, null, 2);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    [Test]
    public async Task GetAllByUserId_WhenAttemptsExistForDifferentUser_ShouldReturnEmpty()
    {
        // Arrange
        var userId1 = Guid.NewGuid().ToString();
        var userId2 = Guid.NewGuid().ToString();
        var template = await GetCreatedQuizTemplateAsync();
        var quizSnapshot = await GetCreatedQuizSnapshotAsync(template.Id);
        var attempts = QuizAttemptBuilder
            .New()
            .BuildMany(
                2,
                (x, i) =>
                    x.WithQuizTemplateId(template.Id)
                        .WithQuizSnapshotId(quizSnapshot.Id)
                        .WithAssignId(userId1)
            );
        foreach (var attempt in attempts)
        {
            await Sut.CreateAsync(attempt);
        }

        // Act
        var result = await Sut.GetAllByUserIdAsync(userId2, null, 2);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    [Test]
    public async Task Update_WhenQuizAttempDoesAlreadyExist_ShouldUpdateSuccess()
    {
        // Arrange
        var template = await GetCreatedQuizTemplateAsync();
        var quizSnapshot = await GetCreatedQuizSnapshotAsync(template.Id);
        var attempt = QuizAttemptBuilder
            .New()
            .WithQuizTemplateId(template.Id)
            .WithQuizSnapshotId(quizSnapshot.Id)
            .Build();
        await Sut.CreateAsync(attempt);
        var updatedAttempt = QuizAttemptBuilder.New().WithId(attempt.Id).Build();

        // Act
        await Sut.UpdateAsync(updatedAttempt);

        // Assert
        var result = await Sut.FindByIdAsync(attempt.Id);
        result.ShouldNotBeNull();
        result.Id.ShouldBe(attempt.Id);
        result.QuizSnapshotId.ShouldBe(attempt.QuizSnapshotId);
        result.QuizTemplateId.ShouldBe(attempt.QuizTemplateId);
        result.AssignId.ShouldBe(updatedAttempt.AssignId);
        result.Status.ShouldBe(updatedAttempt.Status);
        result.StartTime.ShouldNotBeNull();
        result.StartTime.Value.UtcDateTime.ShouldBe(
            updatedAttempt.StartTime!.Value.UtcDateTime,
            TimeSpan.FromSeconds(1)
        );
        result.SubmitTime.ShouldNotBeNull();
        result.SubmitTime.Value.UtcDateTime.ShouldBe(
            updatedAttempt.SubmitTime!.Value.UtcDateTime,
            TimeSpan.FromSeconds(1)
        );
        result.Score.ShouldBe(updatedAttempt.Score);
    }

    [Test]
    public async Task DeleteById_WhenQuizAttempDoesAlreadyExist_ShouldDeleteSuccess()
    {
        // Arrange
        var template = await GetCreatedQuizTemplateAsync();
        var quizSnapshot = await GetCreatedQuizSnapshotAsync(template.Id);
        var attempt = QuizAttemptBuilder
            .New()
            .WithQuizTemplateId(template.Id)
            .WithQuizSnapshotId(quizSnapshot.Id)
            .Build();
        await Sut.CreateAsync(attempt);

        // Act
        await Sut.DeleteByIdAsync(attempt.Id);

        // Assert
        var result = await Sut.FindByIdAsync(attempt.Id);
        result.ShouldBeNull();
    }

    [Test]
    public async Task DeleteById_WhenQuizAttempDoesNotExist_ShouldNotThrowException()
    {
        // Arrange
        var template = await GetCreatedQuizTemplateAsync();
        var quizSnapshot = await GetCreatedQuizSnapshotAsync(template.Id);
        var attempt = QuizAttemptBuilder
            .New()
            .WithQuizTemplateId(template.Id)
            .WithQuizSnapshotId(quizSnapshot.Id)
            .Build();
        var action = () => Sut.DeleteByIdAsync(attempt.Id);

        // Act
        // Assert
        await action.ShouldNotThrowAsync();
    }

    [Test]
    public async Task DeleteByQuizTemplateId_WhenQuizAttempDoesAlreadyExist_ShouldDeleteSuccess()
    {
        // Arrange
        var template = await GetCreatedQuizTemplateAsync();
        var quizSnapshot = await GetCreatedQuizSnapshotAsync(template.Id);
        var attempt = QuizAttemptBuilder
            .New()
            .WithQuizTemplateId(template.Id)
            .WithQuizSnapshotId(quizSnapshot.Id)
            .Build();
        await Sut.CreateAsync(attempt);

        // Act
        await Sut.DeleteByTemplateIdAsync(attempt.QuizTemplateId);

        // Assert
        var result = await Sut.FindByIdAsync(attempt.Id);
        result.ShouldBeNull();
    }

    [Test]
    public async Task DeleteByQuizTemplateId_WhenQuizAttempDoesNotExist_ShouldNotThrowException()
    {
        // Arrange
        var template = await GetCreatedQuizTemplateAsync();
        var quizSnapshot = await GetCreatedQuizSnapshotAsync(template.Id);
        var attempt = QuizAttemptBuilder
            .New()
            .WithQuizTemplateId(template.Id)
            .WithQuizSnapshotId(quizSnapshot.Id)
            .Build();
        var action = () => Sut.DeleteByTemplateIdAsync(attempt.QuizTemplateId);

        // Act
        // Assert
        await action.ShouldNotThrowAsync();
    }

    [Test]
    public async Task CreateAsync_WhenMultipleQuizAttemptsDoNotExist_ShouldInsertAllSuccess()
    {
        // Arrange
        var template = await GetCreatedQuizTemplateAsync();
        var quizSnapshot = await GetCreatedQuizSnapshotAsync(template.Id);
        var userId = Guid.NewGuid().ToString();
        var attempts = QuizAttemptBuilder
            .New()
            .BuildMany(
                3,
                (x, i) =>
                    x.WithQuizTemplateId(template.Id)
                        .WithQuizSnapshotId(quizSnapshot.Id)
                        .WithAssignId(userId)
            );
        var expectedIds = attempts.Select(x => x.Id).ToList();

        // Act
        await Sut.CreateAsync(attempts);

        // Assert
        var result = await Sut.GetAllByTemplateIdAsync(template.Id, null, 10);
        result.ShouldNotBeNull();
        result.Count.ShouldBe(3);
        result.Select(x => x.Id).ShouldBe(expectedIds, true);
    }

    [Test]
    public async Task CreateAsync_WhenSomeQuizAttemptsAlreadyExist_ShouldThrowQuizAttemptCreateException()
    {
        // Arrange
        var template = await GetCreatedQuizTemplateAsync();
        var quizSnapshot = await GetCreatedQuizSnapshotAsync(template.Id);
        var existingAttempt = QuizAttemptBuilder
            .New()
            .WithQuizTemplateId(template.Id)
            .WithQuizSnapshotId(quizSnapshot.Id)
            .Build();
        await Sut.CreateAsync(existingAttempt);

        var attempts = QuizAttemptBuilder
            .New()
            .BuildMany(
                2,
                (x, i) => x.WithQuizTemplateId(template.Id).WithQuizSnapshotId(quizSnapshot.Id)
            );
        var duplicateAttempt = QuizAttemptBuilder
            .New()
            .WithId(existingAttempt.Id)
            .WithQuizTemplateId(template.Id)
            .WithQuizSnapshotId(quizSnapshot.Id)
            .Build();
        var allAttempts = attempts.Concat(new[] { duplicateAttempt });
        var action = () => Sut.CreateAsync(allAttempts);

        // Act
        // Assert
        await action.ShouldThrowAsync<QuizAttemptCreateException>();
    }

    [Test]
    public async Task GetAttemptCountAsync_WhenAttemptsExist_ShouldReturnCorrectCount()
    {
        // Arrange
        var template = await GetCreatedQuizTemplateAsync();
        var quizSnapshot = await GetCreatedQuizSnapshotAsync(template.Id);
        var userId = Guid.NewGuid().ToString();
        var attempts = QuizAttemptBuilder
            .New()
            .BuildMany(
                3,
                (x, i) =>
                    x.WithQuizTemplateId(template.Id)
                        .WithQuizSnapshotId(quizSnapshot.Id)
                        .WithAssignId(userId)
            );
        foreach (var attempt in attempts)
        {
            await Sut.CreateAsync(attempt);
        }

        // Act
        var result = await Sut.GetAttemptCountAsync(userId, template.Id);

        // Assert
        result.ShouldBe(3);
    }

    [Test]
    public async Task GetAttemptCountAsync_WhenNoAttemptsExist_ShouldReturnZero()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var template = await GetCreatedQuizTemplateAsync();

        // Act
        var result = await Sut.GetAttemptCountAsync(userId, template.Id);

        // Assert
        result.ShouldBe(0);
    }

    [Test]
    public async Task GetAttemptCountAsync_WhenAttemptsExistForDifferentUser_ShouldReturnZero()
    {
        // Arrange
        var template = await GetCreatedQuizTemplateAsync();
        var quizSnapshot = await GetCreatedQuizSnapshotAsync(template.Id);
        var userId1 = Guid.NewGuid().ToString();
        var userId2 = Guid.NewGuid().ToString();
        var attempts = QuizAttemptBuilder
            .New()
            .BuildMany(
                2,
                (x, i) =>
                    x.WithQuizTemplateId(template.Id)
                        .WithQuizSnapshotId(quizSnapshot.Id)
                        .WithAssignId(userId1)
            );
        foreach (var attempt in attempts)
        {
            await Sut.CreateAsync(attempt);
        }

        // Act
        var result = await Sut.GetAttemptCountAsync(userId2, template.Id);

        // Assert
        result.ShouldBe(0);
    }

    [Test]
    public async Task FindLastUnfinishedByTemplateIdAndUserIdAsync_WhenUnfinishedAttemptExists_ShouldReturnLatestUnfinishedAttempt()
    {
        // Arrange
        var template = await GetCreatedQuizTemplateAsync();
        var quizSnapshot = await GetCreatedQuizSnapshotAsync(template.Id);
        var userId = Guid.NewGuid().ToString();

        // Create a finished attempt
        var finishedAttempt = QuizAttemptBuilder
            .New()
            .WithQuizTemplateId(template.Id)
            .WithQuizSnapshotId(quizSnapshot.Id)
            .WithAssignId(userId)
            .WithStatus(QuizAttemptStatus.Submitted)
            .Build();
        await Sut.CreateAsync(finishedAttempt);

        // Create two unfinished attempts
        var unfinishedAttempts = QuizAttemptBuilder
            .New()
            .BuildMany(
                2,
                (x, i) =>
                    x.WithQuizTemplateId(template.Id)
                        .WithQuizSnapshotId(quizSnapshot.Id)
                        .WithAssignId(userId)
                        .WithStatus(QuizAttemptStatus.InProgress)
            );
        foreach (var attempt in unfinishedAttempts)
        {
            await Sut.CreateAsync(attempt);
        }

        var expectedId = unfinishedAttempts.Max(x => x.Id);

        // Act
        var result = await Sut.FindLastUnfinishedByTemplateIdAndUserIdAsync(userId, template.Id);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(expectedId);
        result.Status.ShouldNotBe(QuizAttemptStatus.Submitted);
        result.Status.ShouldNotBe(QuizAttemptStatus.Graded);
    }

    [Test]
    public async Task FindLastUnfinishedByTemplateIdAndUserIdAsync_WhenNoUnfinishedAttemptExists_ShouldReturnNull()
    {
        // Arrange
        var template = await GetCreatedQuizTemplateAsync();
        var quizSnapshot = await GetCreatedQuizSnapshotAsync(template.Id);
        var userId = Guid.NewGuid().ToString();

        // Create only finished attempts
        var finishedAttempt = QuizAttemptBuilder
            .New()
            .WithQuizTemplateId(template.Id)
            .WithQuizSnapshotId(quizSnapshot.Id)
            .WithAssignId(userId)
            .WithStatus(QuizAttemptStatus.Submitted)
            .Build();
        await Sut.CreateAsync(finishedAttempt);

        // Act
        var result = await Sut.FindLastUnfinishedByTemplateIdAndUserIdAsync(userId, template.Id);

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public async Task FindLastUnfinishedByTemplateIdAndUserIdAsync_WhenAttemptsExistForDifferentUser_ShouldReturnNull()
    {
        // Arrange
        var template = await GetCreatedQuizTemplateAsync();
        var quizSnapshot = await GetCreatedQuizSnapshotAsync(template.Id);
        var userId1 = Guid.NewGuid().ToString();
        var userId2 = Guid.NewGuid().ToString();

        var unfinishedAttempt = QuizAttemptBuilder
            .New()
            .WithQuizTemplateId(template.Id)
            .WithQuizSnapshotId(quizSnapshot.Id)
            .WithAssignId(userId1)
            .WithStatus(QuizAttemptStatus.InProgress)
            .Build();
        await Sut.CreateAsync(unfinishedAttempt);

        // Act
        var result = await Sut.FindLastUnfinishedByTemplateIdAndUserIdAsync(userId2, template.Id);

        // Assert
        result.ShouldBeNull();
    }

    #region Utils
    private async Task<QuizTemplate> GetCreatedQuizTemplateAsync()
    {
        var template = QuizTemplateBuilder.New().Build();
        await Templates.CreateAsync(template);
        return template;
    }

    private async Task<QuizSnapshot> GetCreatedQuizSnapshotAsync(string quizTemplateId)
    {
        var snapshot = QuizSnapshotBuilder.New().WithQuizTemplateId(quizTemplateId).Build();
        await QuizSnapshots.CreateAsync(snapshot);
        return snapshot;
    }
    #endregion
}
