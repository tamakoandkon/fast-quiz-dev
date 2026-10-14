using FastQuiz.Quiz.Domain.Entities;
using FastQuiz.Quiz.Domain.Exceptions;
using FastQuiz.Quiz.Domain.Repositories;
using FastQuiz.Quiz.Test.Infrastructure.Repositories.Utils;
using Shouldly;

namespace FastQuiz.Quiz.Test.Infrastructure.Repositories;

internal abstract class QuestionRecordRepositoryTestsBase
{
    protected abstract IQuestionRecordRepository Sut { get; }
    protected abstract IQuizTemplateRepository Templates { get; }
    protected abstract IQuestionSnapshotRepository QuestionSnapshots { get; }
    protected abstract IQuizSnapshotRepository QuizSnapshots { get; }
    protected abstract IQuizAttemptRepository QuizAttempts { get; }

    protected abstract Task<IReadOnlyList<AnswerRecord>> GetAnswerRecordsByQuestionRecordIdAsync(
        string questionRecordId
    );

    [Test]
    public async Task CreateAsync_WhenWithManyAndDoesNotExist_ShouldInsertSuccess()
    {
        // Arrange
        var template = await GetCreatedQuizTemplateAsync();
        var quizSnapshot = await GetCreatedQuizSnapshotAsync(template.Id);
        var quizAttempt = await GetCreatedQuizAttemptAsync(template.Id, quizSnapshot.Id);
        var question1 = await GetCreatedQuestionSnapshotAsync(template.Id);
        var question2 = await GetCreatedQuestionSnapshotAsync(template.Id);
        var question3 = await GetCreatedQuestionSnapshotAsync(template.Id);
        List<QuestionSnapshot> questions = [question1, question2, question3];
        var records = QuestionRecordBuilder
            .New()
            .BuildMany(
                3,
                (x, i) =>
                    x.WithQuizAttemptId(quizAttempt.Id).WithQuestionSnapshotId(questions[i].Id)
            );

        // Act
        await Sut.CreateAsync(records);

        // Assert
        var result = await Sut.GetAllByQuizAttempIdAsync(quizAttempt.Id, null, 3);
        result.ShouldNotBeNull();
        result.Count.ShouldBe(records.Count);
        result.Select(x => x.Id).ShouldBe(records.Select(x => x.Id), true);
    }

    [Test]
    public async Task CreateAsync_WhenWithManyAndAlreadyExist_ShouldThrowQuestionRecordCreateException()
    {
        // Arrange
        var template = await GetCreatedQuizTemplateAsync();
        var quizSnapshot = await GetCreatedQuizSnapshotAsync(template.Id);
        var quizAttempt = await GetCreatedQuizAttemptAsync(template.Id, quizSnapshot.Id);
        var question1 = await GetCreatedQuestionSnapshotAsync(template.Id);
        var question2 = await GetCreatedQuestionSnapshotAsync(template.Id);
        var question3 = await GetCreatedQuestionSnapshotAsync(template.Id);
        List<QuestionSnapshot> questions = [question1, question2, question3];
        var records = QuestionRecordBuilder
            .New()
            .BuildMany(
                3,
                (x, i) =>
                    x.WithQuizAttemptId(quizAttempt.Id).WithQuestionSnapshotId(questions[i].Id)
            );
        await Sut.CreateAsync(records);
        var action = () => Sut.CreateAsync(records);

        // Act
        // Assert
        await action.ShouldThrowAsync<QuestionRecordCreateException>();
    }

    [Test]
    public async Task GetAllByQuizAttempIdAsync_WhenWithNoCursor_ShouldReturnLatestItems()
    {
        // Arrange
        var template = await GetCreatedQuizTemplateAsync();
        var quizSnapshot = await GetCreatedQuizSnapshotAsync(template.Id);
        var quizAttempt = await GetCreatedQuizAttemptAsync(template.Id, quizSnapshot.Id);
        var question1 = await GetCreatedQuestionSnapshotAsync(template.Id);
        var question2 = await GetCreatedQuestionSnapshotAsync(template.Id);
        var question3 = await GetCreatedQuestionSnapshotAsync(template.Id);
        List<QuestionSnapshot> questions = [question1, question2, question3];
        var records = QuestionRecordBuilder
            .New()
            .BuildMany(
                3,
                (x, i) =>
                    x.WithQuizAttemptId(quizAttempt.Id).WithQuestionSnapshotId(questions[i].Id)
            );
        await Sut.CreateAsync(records);
        var expectedIds = records.Select(x => x.Id).OrderByDescending(x => x).Take(2).ToList();

        // Act
        var result = await Sut.GetAllByQuizAttempIdAsync(quizAttempt.Id, null, 2);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(expectedIds.Count);
        result.Select(x => x.Id).ShouldBe(expectedIds, false);
    }

    [Test]
    public async Task GetAllByQuizAttempIdAsync_WhenWithCursor_ShouldReturnItemsLessThanCursor()
    {
        // Arrange
        var template = await GetCreatedQuizTemplateAsync();
        var quizSnapshot = await GetCreatedQuizSnapshotAsync(template.Id);
        var quizAttempt = await GetCreatedQuizAttemptAsync(template.Id, quizSnapshot.Id);
        var question1 = await GetCreatedQuestionSnapshotAsync(template.Id);
        var question2 = await GetCreatedQuestionSnapshotAsync(template.Id);
        var question3 = await GetCreatedQuestionSnapshotAsync(template.Id);
        List<QuestionSnapshot> questions = [question1, question2, question3];
        var records = QuestionRecordBuilder
            .New()
            .BuildMany(
                3,
                (x, i) =>
                    x.WithQuizAttemptId(quizAttempt.Id).WithQuestionSnapshotId(questions[i].Id)
            );
        await Sut.CreateAsync(records);

        var expectedIds = records
            .Select(x => x.Id)
            .OrderByDescending(x => x)
            .Where(x => x.CompareTo(records.Max(r => r.Id)) < 0)
            .Take(2)
            .ToList();

        // Act
        var result = await Sut.GetAllByQuizAttempIdAsync(quizAttempt.Id, records.Max(x => x.Id), 2);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(expectedIds.Count);
        result.Select(x => x.Id).ShouldBe(expectedIds, false);
    }

    [Test]
    public async Task GetAllByQuizAttempIdAsync_WhenRecordDoesNotExist_ShouldReturnEmpty()
    {
        // Arrange
        var quizAttemptId = Guid.NewGuid().ToString();

        // Act
        var result = await Sut.GetAllByQuizAttempIdAsync(quizAttemptId, null, 2);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    [Test]
    public async Task DeleteByQuizAttemptId_WhenRecordDoesAlreadyExist_ShouldDeleteSuccess()
    {
        // Arrange
        var template = await GetCreatedQuizTemplateAsync();
        var quizSnapshot = await GetCreatedQuizSnapshotAsync(template.Id);
        var quizAttempt = await GetCreatedQuizAttemptAsync(template.Id, quizSnapshot.Id);
        var question1 = await GetCreatedQuestionSnapshotAsync(template.Id);
        var question2 = await GetCreatedQuestionSnapshotAsync(template.Id);
        var question3 = await GetCreatedQuestionSnapshotAsync(template.Id);
        List<QuestionSnapshot> questions = [question1, question2, question3];
        var records = QuestionRecordBuilder
            .New()
            .BuildMany(
                3,
                (x, i) =>
                    x.WithQuizAttemptId(quizAttempt.Id).WithQuestionSnapshotId(questions[i].Id)
            );
        await Sut.CreateAsync(records);

        // Act
        await Sut.DeleteByQuizAttemptId(records[0].QuizAttemptId);

        // Assert
        var result = await Sut.GetAllByQuizAttempIdAsync(records[0].QuizAttemptId, null, 3);
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    [Test]
    public async Task DeleteByQuizAttemptId_WhenRecordDoesNotExist_ShouldNotThrowException()
    {
        // Arrange
        var quizAttemptId = Guid.NewGuid().ToString();
        var action = () => Sut.DeleteByQuizAttemptId(quizAttemptId);

        // Act
        // Assert
        await action.ShouldNotThrowAsync();
    }

    [Test]
    public async Task CreateAnswerRecordAsync_WhenAnswerRecordsDoNotExist_ShouldInsertSuccess()
    {
        // Arrange
        var template = await GetCreatedQuizTemplateAsync();
        var quizSnapshot = await GetCreatedQuizSnapshotAsync(template.Id);
        var quizAttempt = await GetCreatedQuizAttemptAsync(template.Id, quizSnapshot.Id);
        var question = await GetCreatedQuestionSnapshotAsync(template.Id);
        var questionRecord = QuestionRecordBuilder
            .New()
            .WithQuizAttemptId(quizAttempt.Id)
            .WithQuestionSnapshotId(question.Id)
            .Build();
        await Sut.CreateAsync(new List<QuestionRecord> { questionRecord });

        var answerRecords = AnswerRecordBuilder
            .New()
            .BuildMany(
                3,
                (x, i) =>
                    x.WithQuestionRecordId(questionRecord.Id)
                        .WithQuizAttemptId(quizAttempt.Id)
                        .WithValue($"Answer {i + 1}")
            );

        // Act
        await Sut.CreateAnswerRecordAsync(answerRecords);

        // Assert
        var result = await Sut.FindQuestionRecordByIdAsync(questionRecord.Id);
        result.ShouldNotBeNull();

        // Verify answer records were created
        var answerRecordsResult = await GetAnswerRecordsByQuestionRecordIdAsync(
            questionRecord.Id
        );
        answerRecordsResult.ShouldNotBeNull();
        answerRecordsResult.Count.ShouldBe(3);
        answerRecordsResult.Select(x => x.Id).ShouldBe(answerRecords.Select(x => x.Id), true);
        answerRecordsResult.Select(x => x.Value).ShouldBe(answerRecords.Select(x => x.Value), true);
    }

    [Test]
    public async Task UpdateAsync_WhenQuestionRecordExists_ShouldUpdateSuccess()
    {
        // Arrange
        var template = await GetCreatedQuizTemplateAsync();
        var quizSnapshot = await GetCreatedQuizSnapshotAsync(template.Id);
        var quizAttempt = await GetCreatedQuizAttemptAsync(template.Id, quizSnapshot.Id);
        var question = await GetCreatedQuestionSnapshotAsync(template.Id);
        var questionRecord = QuestionRecordBuilder
            .New()
            .WithQuizAttemptId(quizAttempt.Id)
            .WithQuestionSnapshotId(question.Id)
            .WithIsCorrect(false)
            .Build();
        await Sut.CreateAsync(new List<QuestionRecord> { questionRecord });

        var updatedRecord = QuestionRecordBuilder
            .New()
            .WithId(questionRecord.Id)
            .WithQuizAttemptId(quizAttempt.Id)
            .WithQuestionSnapshotId(question.Id)
            .WithIsCorrect(true)
            .Build();

        // Act
        await Sut.UpdateAsync(updatedRecord);

        // Assert
        var result = await Sut.FindQuestionRecordByIdAsync(questionRecord.Id);
        result.ShouldNotBeNull();
        result.Id.ShouldBe(questionRecord.Id);
        (result.IsCorrect == true).ShouldBeTrue();
    }

    [Test]
    public async Task UpdateAsync_WhenQuestionRecordDoesNotExist_ShouldNotThrowException()
    {
        // Arrange
        var nonExistentRecord = QuestionRecordBuilder.New().Build();
        var action = () => Sut.UpdateAsync(nonExistentRecord);

        // Act
        // Assert
        await action.ShouldNotThrowAsync();
    }

    [Test]
    public async Task DeleteAnswerRecordByQuestionRecordIdAsync_WhenAnswerRecordsExist_ShouldDeleteSuccess()
    {
        // Arrange
        var template = await GetCreatedQuizTemplateAsync();
        var quizSnapshot = await GetCreatedQuizSnapshotAsync(template.Id);
        var quizAttempt = await GetCreatedQuizAttemptAsync(template.Id, quizSnapshot.Id);
        var question = await GetCreatedQuestionSnapshotAsync(template.Id);
        var questionRecord = QuestionRecordBuilder
            .New()
            .WithQuizAttemptId(quizAttempt.Id)
            .WithQuestionSnapshotId(question.Id)
            .Build();
        await Sut.CreateAsync(new List<QuestionRecord> { questionRecord });

        var answerRecords = AnswerRecordBuilder
            .New()
            .BuildMany(
                3,
                (x, i) =>
                    x.WithQuestionRecordId(questionRecord.Id)
                        .WithQuizAttemptId(quizAttempt.Id)
                        .WithValue($"Answer {i + 1}")
            );
        await Sut.CreateAnswerRecordAsync(answerRecords);

        // Verify answer records exist
        var beforeDelete = await GetAnswerRecordsByQuestionRecordIdAsync(questionRecord.Id);
        beforeDelete.Count.ShouldBe(3);

        // Act
        await Sut.DeleteAnswerRecordByQuestionRecordIdAsync(questionRecord.Id);

        // Assert
        var afterDelete = await GetAnswerRecordsByQuestionRecordIdAsync(questionRecord.Id);
        afterDelete.ShouldNotBeNull();
        afterDelete.ShouldBeEmpty();
    }

    [Test]
    public async Task DeleteAnswerRecordByQuestionRecordIdAsync_WhenNoAnswerRecordsExist_ShouldNotThrowException()
    {
        // Arrange
        var template = await GetCreatedQuizTemplateAsync();
        var quizSnapshot = await GetCreatedQuizSnapshotAsync(template.Id);
        var quizAttempt = await GetCreatedQuizAttemptAsync(template.Id, quizSnapshot.Id);
        var question = await GetCreatedQuestionSnapshotAsync(template.Id);
        var questionRecord = QuestionRecordBuilder
            .New()
            .WithQuizAttemptId(quizAttempt.Id)
            .WithQuestionSnapshotId(question.Id)
            .Build();
        await Sut.CreateAsync(new List<QuestionRecord> { questionRecord });
        var action = () => Sut.DeleteAnswerRecordByQuestionRecordIdAsync(questionRecord.Id);

        // Act
        // Assert
        await action.ShouldNotThrowAsync();
    }

    [Test]
    public async Task FindQuestionRecordByIdAsync_WhenQuestionRecordExists_ShouldReturnQuestionRecord()
    {
        // Arrange
        var template = await GetCreatedQuizTemplateAsync();
        var quizSnapshot = await GetCreatedQuizSnapshotAsync(template.Id);
        var quizAttempt = await GetCreatedQuizAttemptAsync(template.Id, quizSnapshot.Id);
        var question = await GetCreatedQuestionSnapshotAsync(template.Id);
        var questionRecord = QuestionRecordBuilder
            .New()
            .WithQuizAttemptId(quizAttempt.Id)
            .WithQuestionSnapshotId(question.Id)
            .WithIsCorrect(true)
            .Build();
        await Sut.CreateAsync(new List<QuestionRecord> { questionRecord });

        // Act
        var result = await Sut.FindQuestionRecordByIdAsync(questionRecord.Id);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(questionRecord.Id);
        result.QuizAttemptId.ShouldBe(questionRecord.QuizAttemptId);
        result.QuestionSnapshotId.ShouldBe(questionRecord.QuestionSnapshotId);
        (result.IsCorrect == true).ShouldBeTrue();
    }

    [Test]
    public async Task FindQuestionRecordByIdAsync_WhenQuestionRecordDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act
        var result = await Sut.FindQuestionRecordByIdAsync(nonExistentId);

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public async Task GetAllByQuizAttempIdAsync_WhenRecordsExist_ShouldReturnAllWithAnswerRecords()
    {
        // Arrange
        var template = await GetCreatedQuizTemplateAsync();
        var quizSnapshot = await GetCreatedQuizSnapshotAsync(template.Id);
        var quizAttempt = await GetCreatedQuizAttemptAsync(template.Id, quizSnapshot.Id);
        var question1 = await GetCreatedQuestionSnapshotAsync(template.Id);
        var question2 = await GetCreatedQuestionSnapshotAsync(template.Id);
        var question3 = await GetCreatedQuestionSnapshotAsync(template.Id);
        List<QuestionSnapshot> questions = [question1, question2, question3];
        var records = QuestionRecordBuilder
            .New()
            .BuildMany(
                3,
                (x, i) =>
                    x.WithQuizAttemptId(quizAttempt.Id).WithQuestionSnapshotId(questions[i].Id)
            );
        await Sut.CreateAsync(records);

        // 为每个题目记录挂上答案
        var answerRecords = new List<AnswerRecord>();
        foreach (var record in records)
        {
            answerRecords.AddRange(
                AnswerRecordBuilder
                    .New()
                    .BuildMany(
                        2,
                        (x, i) =>
                            x.WithQuestionRecordId(record.Id)
                                .WithQuizAttemptId(quizAttempt.Id)
                                .WithValue($"{Guid.NewGuid()}")
                    )
            );
        }
        await Sut.CreateAnswerRecordAsync(answerRecords);

        // Act：无分页重载应返回该答题记录下的全部题目记录
        var result = await Sut.GetAllByQuizAttempIdAsync(quizAttempt.Id);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(records.Count);
        result.Select(x => x.Id).ShouldBe(records.Select(x => x.Id), ignoreOrder: true);
        result.All(r => r.AnswerRecords.Count == 2).ShouldBeTrue();
    }

    [Test]
    public async Task GetAllByQuizAttempIdAsync_WithoutPagination_WhenRecordDoesNotExist_ShouldReturnEmpty()
    {
        // Arrange
        var quizAttemptId = Guid.NewGuid().ToString();

        // Act
        var result = await Sut.GetAllByQuizAttempIdAsync(quizAttemptId);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    [Test]
    public async Task SetQuestionRecordsCorrectAsync_WhenRecordsExist_ShouldUpdateIsCorrectForGivenIds()
    {
        // Arrange
        var template = await GetCreatedQuizTemplateAsync();
        var quizSnapshot = await GetCreatedQuizSnapshotAsync(template.Id);
        var quizAttempt = await GetCreatedQuizAttemptAsync(template.Id, quizSnapshot.Id);
        var question1 = await GetCreatedQuestionSnapshotAsync(template.Id);
        var question2 = await GetCreatedQuestionSnapshotAsync(template.Id);
        var question3 = await GetCreatedQuestionSnapshotAsync(template.Id);
        List<QuestionSnapshot> questions = [question1, question2, question3];
        var records = QuestionRecordBuilder
            .New()
            .BuildMany(
                3,
                (x, i) =>
                    x.WithQuizAttemptId(quizAttempt.Id)
                        .WithQuestionSnapshotId(questions[i].Id)
                        .WithIsCorrect(false)
            );
        await Sut.CreateAsync(records);

        var correctIds = records.Take(2).Select(x => x.Id).ToList();

        // Act：只把前两条标记为正确
        await Sut.SetQuestionRecordsCorrectAsync(correctIds, true);

        // Assert
        var updated = await Sut.GetAllByQuizAttempIdAsync(quizAttempt.Id, null, 3);
        updated.ShouldNotBeNull();
        updated.Count.ShouldBe(3);
        updated.Where(r => correctIds.Contains(r.Id)).All(r => r.IsCorrect == true).ShouldBeTrue();
        updated
            .Where(r => !correctIds.Contains(r.Id))
            .All(r => r.IsCorrect == false)
            .ShouldBeTrue();
    }

    [Test]
    public async Task SetQuestionRecordsCorrectAsync_WhenRecordsDoNotExist_ShouldNotThrowException()
    {
        // Arrange
        var nonExistentIds = new[] { Guid.NewGuid().ToString(), Guid.NewGuid().ToString() };
        var action = () => Sut.SetQuestionRecordsCorrectAsync(nonExistentIds, true);

        // Act & Assert
        await action.ShouldNotThrowAsync();
    }

    [Test]
    public async Task DeleteAnswerRecordByQuestionRecordIdAsync_WhenWithMany_ShouldDeleteOnlyGiveRecordsAnswers()
    {
        // Arrange
        var template = await GetCreatedQuizTemplateAsync();
        var quizSnapshot = await GetCreatedQuizSnapshotAsync(template.Id);
        var quizAttempt = await GetCreatedQuizAttemptAsync(template.Id, quizSnapshot.Id);
        var question1 = await GetCreatedQuestionSnapshotAsync(template.Id);
        var question2 = await GetCreatedQuestionSnapshotAsync(template.Id);
        List<QuestionSnapshot> questions = [question1, question2];
        var records = QuestionRecordBuilder
            .New()
            .BuildMany(
                2,
                (x, i) =>
                    x.WithQuizAttemptId(quizAttempt.Id).WithQuestionSnapshotId(questions[i].Id)
            );
        await Sut.CreateAsync(records);

        var answersForRecord1 = AnswerRecordBuilder
            .New()
            .BuildMany(
                2,
                (x, i) =>
                    x.WithQuestionRecordId(records[0].Id)
                        .WithQuizAttemptId(quizAttempt.Id)
                        .WithValue($"{Guid.NewGuid()}")
            );
        var answersForRecord2 = AnswerRecordBuilder
            .New()
            .BuildMany(
                2,
                (x, i) =>
                    x.WithQuestionRecordId(records[1].Id)
                        .WithQuizAttemptId(quizAttempt.Id)
                        .WithValue($"{Guid.NewGuid()}")
            );
        await Sut.CreateAnswerRecordAsync(answersForRecord1.Concat(answersForRecord2));

        // Act：批量删除第一条记录的所有答案
        await Sut.DeleteAnswerRecordByQuestionRecordIdAsync([records[0].Id]);

        // Assert
        var result1 = await GetAnswerRecordsByQuestionRecordIdAsync(records[0].Id);
        result1.ShouldNotBeNull();
        result1.ShouldBeEmpty();

        var result2 = await GetAnswerRecordsByQuestionRecordIdAsync(records[1].Id);
        result2.ShouldNotBeNull();
        result2.Count.ShouldBe(answersForRecord2.Count);
    }

    [Test]
    public async Task DeleteAnswerRecordByQuestionRecordIdAsync_WhenWithManyAndNoAnswerRecordsExist_ShouldNotThrowException()
    {
        // Arrange
        var nonExistentIds = new[] { Guid.NewGuid().ToString(), Guid.NewGuid().ToString() };
        var action = () => Sut.DeleteAnswerRecordByQuestionRecordIdAsync(nonExistentIds);

        // Act & Assert
        await action.ShouldNotThrowAsync();
    }

    #region Utils
    private async Task<QuizTemplate> GetCreatedQuizTemplateAsync()
    {
        var template = QuizTemplateBuilder.New().Build();
        await Templates.CreateAsync(template);
        return template;
    }

    private async Task<QuestionSnapshot> GetCreatedQuestionSnapshotAsync(string quizTemplateId)
    {
        var snapshot = QuestionSnapshotBuilder.New().WithQuizTemplateId(quizTemplateId).Build();
        await QuestionSnapshots.CreateAsync(snapshot);
        return snapshot;
    }

    private async Task<QuizSnapshot> GetCreatedQuizSnapshotAsync(string quizTemplateId)
    {
        var snapshot = QuizSnapshotBuilder.New().WithQuizTemplateId(quizTemplateId).Build();
        await QuizSnapshots.CreateAsync(snapshot);
        return snapshot;
    }

    private async Task<QuizAttempt> GetCreatedQuizAttemptAsync(
        string quizTemplateId,
        string quizSnapshotId
    )
    {
        var attempt = QuizAttemptBuilder
            .New()
            .WithQuizTemplateId(quizTemplateId)
            .WithQuizSnapshotId(quizSnapshotId)
            .Build();
        await QuizAttempts.CreateAsync(attempt);
        return attempt;
    }
    #endregion
}
