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

    #region GetAllRecordsByTemplateId

    [Test]
    public async Task GetAllRecords_WhenOnlyBestIsFalse_ShouldReturnAllRecordsOfTemplate()
    {
        // Arrange
        var template = await GetCreatedQuizTemplateAsync();
        var quizSnapshot = await GetCreatedQuizSnapshotAsync(template.Id);
        var otherTemplate = await GetCreatedQuizTemplateAsync();
        var otherSnapshot = await GetCreatedQuizSnapshotAsync(otherTemplate.Id);
        var userId = Guid.NewGuid().ToString();
        var attempts = QuizAttemptBuilder
            .New()
            .BuildMany(
                3,
                (x, i) =>
                    x.WithQuizTemplateId(template.Id)
                        .WithQuizSnapshotId(quizSnapshot.Id)
                        .WithAssignId(userId)
                        .WithScore(i)
            );
        await Sut.CreateAsync(attempts);
        // 其他考试的记录不应被返回
        await Sut.CreateAsync(
            QuizAttemptBuilder
                .New()
                .WithQuizTemplateId(otherTemplate.Id)
                .WithQuizSnapshotId(otherSnapshot.Id)
                .WithAssignId(userId)
                .WithScore(100)
                .Build()
        );

        // Act
        var result = await Sut.GetAllRecordsByTemplateIdAsync(template.Id, false, null, 10, false);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(3);
        result.All(x => x.QuizTemplateId == template.Id).ShouldBeTrue();
    }

    [Test]
    public async Task GetAllRecords_WhenOnlyBestIsTrue_ShouldReturnOnlyBestPerUser()
    {
        // Arrange
        var template = await GetCreatedQuizTemplateAsync();
        var quizSnapshot = await GetCreatedQuizSnapshotAsync(template.Id);
        var userId1 = Guid.NewGuid().ToString();
        var userId2 = Guid.NewGuid().ToString();
        // user1: 三次成绩 10 / 90 / 50 -> 最好 90
        var user1Attempts = QuizAttemptBuilder
            .New()
            .BuildMany(
                3,
                (x, i) =>
                    x.WithQuizTemplateId(template.Id)
                        .WithQuizSnapshotId(quizSnapshot.Id)
                        .WithAssignId(userId1)
                        .WithScore(i == 1 ? 90 : (i == 0 ? 10 : 50))
            );
        // user2: 一次成绩 70
        var user2Attempts = QuizAttemptBuilder
            .New()
            .BuildMany(
                1,
                (x, i) =>
                    x.WithQuizTemplateId(template.Id)
                        .WithQuizSnapshotId(quizSnapshot.Id)
                        .WithAssignId(userId2)
                        .WithScore(70)
            );
        await Sut.CreateAsync(user1Attempts);
        await Sut.CreateAsync(user2Attempts);
        var expectedUser1Id = user1Attempts.Single(x => x.Score == 90).Id;

        // Act
        var result = await Sut.GetAllRecordsByTemplateIdAsync(template.Id, true, null, 10, false);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);
        var best1 = result.Single(x => x.AssignId == userId1);
        best1.Score.ShouldBe(90);
        best1.Id.ShouldBe(expectedUser1Id);
        result.Single(x => x.AssignId == userId2).Score.ShouldBe(70);
    }

    [Test]
    public async Task GetAllRecords_WhenOnlyBestIsTrueAndSameScore_ShouldReturnLatestRecord()
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
                        .WithScore(80)
            );
        await Sut.CreateAsync(attempts);
        var expectedId = attempts.Max(x => x.Id);

        // Act
        var result = await Sut.GetAllRecordsByTemplateIdAsync(template.Id, true, null, 10, false);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);
        result[0].Id.ShouldBe(expectedId);
    }

    [Test]
    public async Task GetAllRecords_WhenOnlyBestIsTrue_ShouldExcludeUnassignedAttempts()
    {
        // Arrange
        var template = await GetCreatedQuizTemplateAsync();
        var quizSnapshot = await GetCreatedQuizSnapshotAsync(template.Id);
        var userId = Guid.NewGuid().ToString();
        await Sut.CreateAsync(
            QuizAttemptBuilder
                .New()
                .WithQuizTemplateId(template.Id)
                .WithQuizSnapshotId(quizSnapshot.Id)
                .WithAssignId(userId)
                .WithScore(60)
                .Build()
        );
        await Sut.CreateAsync(
            QuizAttemptBuilder
                .New()
                .WithQuizTemplateId(template.Id)
                .WithQuizSnapshotId(quizSnapshot.Id)
                .WithAssignId(null)
                .WithScore(200)
                .Build()
        );

        // Act
        var result = await Sut.GetAllRecordsByTemplateIdAsync(template.Id, true, null, 10, false);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);
        result[0].AssignId.ShouldBe(userId);
    }

    [Test]
    public async Task GetAllRecords_WhenQuizAttemptDoesNotExist_ShouldReturnEmpty()
    {
        // Arrange
        var quizTemplateId = Guid.NewGuid().ToString();

        // Act
        var result = await Sut.GetAllRecordsByTemplateIdAsync(quizTemplateId, true, null, 10, false);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    #endregion

    #region GetAllRecordsByUserId

    [Test]
    public async Task GetUserRecords_WhenNoTemplateFilter_ShouldReturnAllRecordsOfUser()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var otherUserId = Guid.NewGuid().ToString();
        var template = await GetCreatedQuizTemplateAsync();
        var templateSnapshot = await GetCreatedQuizSnapshotAsync(template.Id);
        var otherTemplate = await GetCreatedQuizTemplateAsync();
        var otherTemplateSnapshot = await GetCreatedQuizSnapshotAsync(otherTemplate.Id);

        // 该考生在两场考试中共 3 条记录
        await Sut.CreateAsync(
            QuizAttemptBuilder
                .New()
                .BuildMany(
                    2,
                    (x, i) =>
                        x.WithQuizTemplateId(template.Id)
                            .WithQuizSnapshotId(templateSnapshot.Id)
                            .WithAssignId(userId)
                            .WithStatus(QuizAttemptStatus.Graded)
                            .WithScore(i)
                )
        );
        await Sut.CreateAsync(
            QuizAttemptBuilder
                .New()
                .WithQuizTemplateId(otherTemplate.Id)
                .WithQuizSnapshotId(otherTemplateSnapshot.Id)
                .WithAssignId(userId)
                .WithStatus(QuizAttemptStatus.Graded)
                .WithScore(60)
                .Build()
        );
        // 其他考生的记录不应被返回
        await Sut.CreateAsync(
            QuizAttemptBuilder
                .New()
                .WithQuizTemplateId(template.Id)
                .WithQuizSnapshotId(templateSnapshot.Id)
                .WithAssignId(otherUserId)
                .WithStatus(QuizAttemptStatus.Graded)
                .WithScore(100)
                .Build()
        );

        // Act
        var result = await Sut.GetAllRecordsByUserIdAsync(userId, null, false, null, 10, false);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(3);
        result.All(x => x.AssignId == userId).ShouldBeTrue();
    }

    [Test]
    public async Task GetUserRecords_WhenTemplateFilterProvided_ShouldReturnOnlyThatTemplate()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var template = await GetCreatedQuizTemplateAsync();
        var templateSnapshot = await GetCreatedQuizSnapshotAsync(template.Id);
        var otherTemplate = await GetCreatedQuizTemplateAsync();
        var otherTemplateSnapshot = await GetCreatedQuizSnapshotAsync(otherTemplate.Id);

        var attempts = QuizAttemptBuilder
            .New()
            .BuildMany(
                2,
                (x, i) =>
                    x.WithQuizTemplateId(template.Id)
                        .WithQuizSnapshotId(templateSnapshot.Id)
                        .WithAssignId(userId)
                        .WithStatus(QuizAttemptStatus.Graded)
                        .WithScore(i)
            );
        await Sut.CreateAsync(attempts);
        await Sut.CreateAsync(
            QuizAttemptBuilder
                .New()
                .WithQuizTemplateId(otherTemplate.Id)
                .WithQuizSnapshotId(otherTemplateSnapshot.Id)
                .WithAssignId(userId)
                .WithStatus(QuizAttemptStatus.Graded)
                .WithScore(99)
                .Build()
        );
        var expectedIds = attempts.Select(x => x.Id).OrderByDescending(x => x).ToList();

        // Act
        var result = await Sut.GetAllRecordsByUserIdAsync(userId, template.Id, false, null, 10, false);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);
        result.All(x => x.QuizTemplateId == template.Id).ShouldBeTrue();
        result.Select(x => x.Id).ShouldBe(expectedIds, false);
    }

    [Test]
    public async Task GetUserRecords_WhenOnlyBestIsFalse_ShouldReturnEveryRecord()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var template = await GetCreatedQuizTemplateAsync();
        var snapshot = await GetCreatedQuizSnapshotAsync(template.Id);
        var attempts = QuizAttemptBuilder
            .New()
            .BuildMany(
                3,
                (x, i) =>
                    x.WithQuizTemplateId(template.Id)
                        .WithQuizSnapshotId(snapshot.Id)
                        .WithAssignId(userId)
                        .WithStatus(QuizAttemptStatus.Graded)
                        .WithScore(i == 1 ? 90 : (i == 0 ? 10 : 50))
            );
        await Sut.CreateAsync(attempts);

        // Act
        var result = await Sut.GetAllRecordsByUserIdAsync(userId, null, false, null, 10, false);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(3);
    }

    [Test]
    public async Task GetUserRecords_WhenOnlyBestIsTrue_ShouldReturnBestPerTemplate()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var template1 = await GetCreatedQuizTemplateAsync();
        var snapshot1 = await GetCreatedQuizSnapshotAsync(template1.Id);
        var template2 = await GetCreatedQuizTemplateAsync();
        var snapshot2 = await GetCreatedQuizSnapshotAsync(template2.Id);

        // 考试1：三次成绩 10 / 90 / 50 -> 最好 90
        var template1Attempts = QuizAttemptBuilder
            .New()
            .BuildMany(
                3,
                (x, i) =>
                    x.WithQuizTemplateId(template1.Id)
                        .WithQuizSnapshotId(snapshot1.Id)
                        .WithAssignId(userId)
                        .WithStatus(QuizAttemptStatus.Graded)
                        .WithScore(i == 1 ? 90 : (i == 0 ? 10 : 50))
            );
        // 考试2：一次成绩 70
        var template2Attempts = QuizAttemptBuilder
            .New()
            .BuildMany(
                1,
                (x, i) =>
                    x.WithQuizTemplateId(template2.Id)
                        .WithQuizSnapshotId(snapshot2.Id)
                        .WithAssignId(userId)
                        .WithStatus(QuizAttemptStatus.Graded)
                        .WithScore(70)
            );
        await Sut.CreateAsync(template1Attempts);
        await Sut.CreateAsync(template2Attempts);
        var expectedBestTemplate1Id = template1Attempts.Single(x => x.Score == 90).Id;

        // Act
        var result = await Sut.GetAllRecordsByUserIdAsync(userId, null, true, null, 10, false);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);
        var bestOfTemplate1 = result.Single(x => x.QuizTemplateId == template1.Id);
        bestOfTemplate1.Score.ShouldBe(90);
        bestOfTemplate1.Id.ShouldBe(expectedBestTemplate1Id);
        result.Single(x => x.QuizTemplateId == template2.Id).Score.ShouldBe(70);
    }

    [Test]
    public async Task GetUserRecords_WhenOnlyBestIsTrueAndSameScore_ShouldReturnLatestRecord()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var template = await GetCreatedQuizTemplateAsync();
        var snapshot = await GetCreatedQuizSnapshotAsync(template.Id);
        var attempts = QuizAttemptBuilder
            .New()
            .BuildMany(
                3,
                (x, i) =>
                    x.WithQuizTemplateId(template.Id)
                        .WithQuizSnapshotId(snapshot.Id)
                        .WithAssignId(userId)
                        .WithStatus(QuizAttemptStatus.Graded)
                        .WithScore(80)
            );
        await Sut.CreateAsync(attempts);
        var expectedId = attempts.Max(x => x.Id);

        // Act
        var result = await Sut.GetAllRecordsByUserIdAsync(userId, null, true, null, 10, false);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);
        result[0].Id.ShouldBe(expectedId);
    }

    [Test]
    public async Task GetUserRecords_WhenWithCursor_ShouldReturnItemsLessThanCursor()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var template = await GetCreatedQuizTemplateAsync();
        var snapshot = await GetCreatedQuizSnapshotAsync(template.Id);
        var attempts = QuizAttemptBuilder
            .New()
            .BuildMany(
                3,
                (x, i) =>
                    x.WithQuizTemplateId(template.Id)
                        .WithQuizSnapshotId(snapshot.Id)
                        .WithAssignId(userId)
                        .WithStatus(QuizAttemptStatus.Graded)
            );
        await Sut.CreateAsync(attempts);
        var maxId = attempts.Max(x => x.Id);
        var expectedIds = attempts
            .Where(x => x.Id.CompareTo(maxId) < 0)
            .Select(x => x.Id)
            .OrderByDescending(x => x)
            .Take(2)
            .ToList();

        // Act
        var result = await Sut.GetAllRecordsByUserIdAsync(userId, null, false, maxId, 2, false);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);
        result.Select(x => x.Id).ShouldBe(expectedIds, false);
    }

    [Test]
    public async Task GetUserRecords_WhenIsPreviousTrue_ShouldReturnItemsGreaterThanCursorInDescOrder()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var template = await GetCreatedQuizTemplateAsync();
        var snapshot = await GetCreatedQuizSnapshotAsync(template.Id);
        var attempts = QuizAttemptBuilder
            .New()
            .BuildMany(
                5,
                (x, i) =>
                    x.WithQuizTemplateId(template.Id)
                        .WithQuizSnapshotId(snapshot.Id)
                        .WithAssignId(userId)
                        .WithStatus(QuizAttemptStatus.Graded)
            );
        await Sut.CreateAsync(attempts);

        // 模拟「第 2 页」：游标 = 倒数第 3 条(共 5 条、每页 2 条时的 page2 首条)
        var allIdsDesc = attempts.Select(x => x.Id).OrderByDescending(x => x).ToList();
        var page2First = allIdsDesc[2]; // 比它新的有 2 条(page1)
        // 上一页应回到 page1：比 page2First 更新的那 2 条，仍按 Id 降序
        var expectedIds = allIdsDesc.Take(2).ToList();

        // Act
        var result = await Sut.GetAllRecordsByUserIdAsync(
            userId,
            null,
            false,
            page2First,
            2,
            true
        );

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);
        result.Select(x => x.Id).ShouldBe(expectedIds, false);
        result.All(x => x.Id.CompareTo(page2First) > 0).ShouldBeTrue();
    }

    [Test]
    public async Task GetUserRecords_WhenUserDoesNotExist_ShouldReturnEmpty()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();

        // Act
        var result = await Sut.GetAllRecordsByUserIdAsync(userId, null, false, null, 10, false);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    [Test]
    public async Task GetUserRecords_WhenAttemptIsNotGraded_ShouldExcludeIt()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var template = await GetCreatedQuizTemplateAsync();
        var snapshot = await GetCreatedQuizSnapshotAsync(template.Id);

        // 未完成的三种状态都不应被返回
        foreach (
            var status in new[]
            {
                QuizAttemptStatus.Pending,
                QuizAttemptStatus.InProgress,
                QuizAttemptStatus.Submitted,
            }
        )
        {
            await Sut.CreateAsync(
                QuizAttemptBuilder
                    .New()
                    .WithQuizTemplateId(template.Id)
                    .WithQuizSnapshotId(snapshot.Id)
                    .WithAssignId(userId)
                    .WithStatus(status)
                    .WithScore(88)
                    .Build()
            );
        }
        // 只有已批阅的一条会被返回
        var graded = QuizAttemptBuilder
            .New()
            .WithQuizTemplateId(template.Id)
            .WithQuizSnapshotId(snapshot.Id)
            .WithAssignId(userId)
            .WithStatus(QuizAttemptStatus.Graded)
            .WithScore(66)
            .Build();
        await Sut.CreateAsync(graded);

        // Act
        var result = await Sut.GetAllRecordsByUserIdAsync(userId, null, false, null, 10, false);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);
        result[0].Id.ShouldBe(graded.Id);
        result.All(x => x.Status == QuizAttemptStatus.Graded).ShouldBeTrue();
    }

    #endregion

    #region GetAllRecordsByTemplateIdAndUserId

    [Test]
    public async Task GetMyTemplateRecords_ShouldIncludeAllStatuses()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var template = await GetCreatedQuizTemplateAsync();
        var snapshot = await GetCreatedQuizSnapshotAsync(template.Id);

        // 四种状态各一条 —— 与 by-user 的 Graded-only 行为相反，这里全都要返回
        foreach (
            var status in new[]
            {
                QuizAttemptStatus.Pending,
                QuizAttemptStatus.InProgress,
                QuizAttemptStatus.Submitted,
                QuizAttemptStatus.Graded,
            }
        )
        {
            await Sut.CreateAsync(
                QuizAttemptBuilder
                    .New()
                    .WithQuizTemplateId(template.Id)
                    .WithQuizSnapshotId(snapshot.Id)
                    .WithAssignId(userId)
                    .WithStatus(status)
                    .WithScore(50)
                    .Build()
            );
        }

        // Act
        var result = await Sut.GetAllRecordsByTemplateIdAndUserIdAsync(
            userId,
            template.Id,
            null,
            10,
            false
        );

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(4);
        result.Select(x => x.Status).Distinct().Count().ShouldBe(4);
    }

    [Test]
    public async Task GetMyTemplateRecords_WhenOtherUserOrOtherTemplate_ShouldExcludeThem()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var otherUserId = Guid.NewGuid().ToString();
        var template = await GetCreatedQuizTemplateAsync();
        var snapshot = await GetCreatedQuizSnapshotAsync(template.Id);
        var otherTemplate = await GetCreatedQuizTemplateAsync();
        var otherSnapshot = await GetCreatedQuizSnapshotAsync(otherTemplate.Id);

        var mine = QuizAttemptBuilder
            .New()
            .WithQuizTemplateId(template.Id)
            .WithQuizSnapshotId(snapshot.Id)
            .WithAssignId(userId)
            .WithStatus(QuizAttemptStatus.InProgress)
            .Build();
        await Sut.CreateAsync(mine);
        // 同模板但别的考生
        await Sut.CreateAsync(
            QuizAttemptBuilder
                .New()
                .WithQuizTemplateId(template.Id)
                .WithQuizSnapshotId(snapshot.Id)
                .WithAssignId(otherUserId)
                .WithStatus(QuizAttemptStatus.Graded)
                .Build()
        );
        // 本人但别的考试
        await Sut.CreateAsync(
            QuizAttemptBuilder
                .New()
                .WithQuizTemplateId(otherTemplate.Id)
                .WithQuizSnapshotId(otherSnapshot.Id)
                .WithAssignId(userId)
                .WithStatus(QuizAttemptStatus.Graded)
                .Build()
        );

        // Act
        var result = await Sut.GetAllRecordsByTemplateIdAndUserIdAsync(
            userId,
            template.Id,
            null,
            10,
            false
        );

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);
        result[0].Id.ShouldBe(mine.Id);
    }

    [Test]
    public async Task GetMyTemplateRecords_ShouldPageForwardThenBackToSamePage()
    {
        // Arrange: 5 条记录，每页 2 条
        var userId = Guid.NewGuid().ToString();
        var template = await GetCreatedQuizTemplateAsync();
        var snapshot = await GetCreatedQuizSnapshotAsync(template.Id);
        var attempts = QuizAttemptBuilder
            .New()
            .BuildMany(
                5,
                (x, i) =>
                    x.WithQuizTemplateId(template.Id)
                        .WithQuizSnapshotId(snapshot.Id)
                        .WithAssignId(userId)
                        .WithStatus(QuizAttemptStatus.Graded)
                        .WithScore(i)
            );
        await Sut.CreateAsync(attempts);

        var allIdsDesc = attempts.Select(x => x.Id).OrderByDescending(x => x).ToList();
        var page1Ids = allIdsDesc.Take(2).ToList();
        var page2First = allIdsDesc[2];

        // Act: 下一页(游标 = page1 最后一条)
        var page2 = await Sut.GetAllRecordsByTemplateIdAndUserIdAsync(
            userId,
            template.Id,
            page1Ids[1],
            2,
            false
        );
        // Act: 上一页(游标 = page2 第一条)
        var back = await Sut.GetAllRecordsByTemplateIdAndUserIdAsync(
            userId,
            template.Id,
            page2First,
            2,
            true
        );

        // Assert
        page2.Select(x => x.Id).ShouldBe(allIdsDesc.Skip(2).Take(2).ToList(), false);
        back.Select(x => x.Id).ShouldBe(page1Ids, false);
        back.All(x => x.Id.CompareTo(page2First) > 0).ShouldBeTrue();
    }

    [Test]
    public async Task GetMyTemplateRecords_WhenNoRecords_ShouldReturnEmpty()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var template = await GetCreatedQuizTemplateAsync();

        // Act
        var result = await Sut.GetAllRecordsByTemplateIdAndUserIdAsync(
            userId,
            template.Id,
            null,
            10,
            false
        );

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    #endregion

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
