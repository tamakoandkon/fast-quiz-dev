using FastQuiz.Quiz.Domain.Entities;
using FastQuiz.Quiz.Domain.Exceptions;
using FastQuiz.Quiz.Domain.Repositories;
using FastQuiz.Quiz.Test.Infrastructure.Repositories.Utils;
using Shouldly;

namespace FastQuiz.Quiz.Test.Infrastructure.Repositories;

internal abstract class QuizSnapshotQuestionSnapshotMapRepositoryTestsBase
{
    protected abstract IQuizSnapshotQuestionSnapshotMapRepository Sut { get; }
    protected abstract IQuizTemplateRepository Templates { get; }
    protected abstract IQuestionSnapshotRepository QuestionSnapshots { get; }
    protected abstract IQuizSnapshotRepository QuizSnapshots { get; }

    [Test]
    public async Task Create_WhenDoesNotExist_ShouldInsertSuccess()
    {
        // Arrange
        var template = await GetCreatedQuizTemplateAsync();
        var question = await GetCreatedQuestionSnapshotAsync(template.Id);
        var quizSnapshot = await GetCreatedQuizSnapshotAsync(template.Id);
        var map = QuizSnapshotQuestionSnapshotMapBuilder
            .New()
            .WithQuestionSnapshotId(question.Id)
            .WithQuizSnapshotId(quizSnapshot.Id)
            .Build();

        // Act
        await Sut.CreateAsync(map);

        // Assert
        var result = await Sut.GetAllByQuizSnapshotIdAsync(map.QuizSnapshotId, null, 1);
        result.ShouldNotBeNull();
        result[0].QuizSnapshotId.ShouldBe(map.QuizSnapshotId);
        result[0].QuestionSnapshotId.ShouldBe(map.QuestionSnapshotId);
    }

    [Test]
    public async Task Create_WhenDoesAlreadyExist_ShouldThrowQuizSnapshotQuestionSnapshotMapCreateException()
    {
        // Arrange
        var template = await GetCreatedQuizTemplateAsync();
        var question = await GetCreatedQuestionSnapshotAsync(template.Id);
        var quizSnapshot = await GetCreatedQuizSnapshotAsync(template.Id);
        var map = QuizSnapshotQuestionSnapshotMapBuilder
            .New()
            .WithQuestionSnapshotId(question.Id)
            .WithQuizSnapshotId(quizSnapshot.Id)
            .Build();
        await Sut.CreateAsync(map);
        var action = () => Sut.CreateAsync(map);

        // Act
        // Assert
        await action.ShouldThrowAsync<QuizSnapshotQuestionSnapshotMapCreateException>();
    }

    [Test]
    public async Task Create_WhenWithManyAndDoesNotExist_ShouldInsertSuccess()
    {
        // Arrange
        var template = await GetCreatedQuizTemplateAsync();
        var question = await GetCreatedQuestionSnapshotAsync(template.Id);
        var quizSnapshot = await GetCreatedQuizSnapshotAsync(template.Id);
        var map = QuizSnapshotQuestionSnapshotMapBuilder
            .New()
            .WithQuestionSnapshotId(question.Id)
            .WithQuizSnapshotId(quizSnapshot.Id)
            .Build();

        // Act
        await Sut.CreateAsync([map]);

        // Assert
        var result = await Sut.GetAllByQuizSnapshotIdAsync(map.QuizSnapshotId, null, 1);
        result.ShouldNotBeNull();
        result[0].QuizSnapshotId.ShouldBe(map.QuizSnapshotId);
        result[0].QuestionSnapshotId.ShouldBe(map.QuestionSnapshotId);
    }

    [Test]
    public async Task Create_WhenWithManyDoesAlreadyExist_ShouldThrowQuizSnapshotQuestionSnapshotMapCreateException()
    {
        // Arrange
        var template = await GetCreatedQuizTemplateAsync();
        var question = await GetCreatedQuestionSnapshotAsync(template.Id);
        var quizSnapshot = await GetCreatedQuizSnapshotAsync(template.Id);
        var map = QuizSnapshotQuestionSnapshotMapBuilder
            .New()
            .WithQuestionSnapshotId(question.Id)
            .WithQuizSnapshotId(quizSnapshot.Id)
            .Build();
        await Sut.CreateAsync([map]);
        var action = () => Sut.CreateAsync([map]);

        // Act
        // Assert
        await action.ShouldThrowAsync<QuizSnapshotQuestionSnapshotMapCreateException>();
    }

    [Test]
    public async Task GetAllByQuizSnapshotIdAsync_WhenWithNoCursor_ShouldReturnLatestItems()
    {
        // Arrange
        var template = await GetCreatedQuizTemplateAsync();
        var question1 = await GetCreatedQuestionSnapshotAsync(template.Id);
        var question2 = await GetCreatedQuestionSnapshotAsync(template.Id);
        var question3 = await GetCreatedQuestionSnapshotAsync(template.Id);

        var quizSnapshot = await GetCreatedQuizSnapshotAsync(template.Id);
        var questionSnapshotIds = new List<string>([question1.Id, question2.Id, question3.Id]);

        var maps = QuizSnapshotQuestionSnapshotMapBuilder
            .New()
            .BuildMany(
                3,
                (x, i) =>
                    x.WithQuestionSnapshotId(questionSnapshotIds[i])
                        .WithQuizSnapshotId(quizSnapshot.Id)
            );
        await Sut.CreateAsync(maps);
        var expectedIds = maps.Select(x => x.QuestionSnapshotId)
            .OrderByDescending(x => x)
            .Take(2)
            .ToList();

        // Act
        var result = await Sut.GetAllByQuizSnapshotIdAsync(quizSnapshot.Id, null, 2);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(expectedIds.Count);
        result.Select(x => x.QuestionSnapshotId).ShouldBe(expectedIds, false);
    }

    [Test]
    public async Task GetAllByQuizSnapshotIdAsync_WhenWithCursor_ShouldReturnItemsLessThanCursor()
    {
        // Arrange
        var template = await GetCreatedQuizTemplateAsync();
        var question1 = await GetCreatedQuestionSnapshotAsync(template.Id);
        var question2 = await GetCreatedQuestionSnapshotAsync(template.Id);
        var question3 = await GetCreatedQuestionSnapshotAsync(template.Id);

        var quizSnapshot = await GetCreatedQuizSnapshotAsync(template.Id);
        var questionSnapshotIds = new List<string>([question1.Id, question2.Id, question3.Id]);

        var maps = QuizSnapshotQuestionSnapshotMapBuilder
            .New()
            .BuildMany(
                3,
                (x, i) =>
                    x.WithQuestionSnapshotId(questionSnapshotIds[i])
                        .WithQuizSnapshotId(quizSnapshot.Id)
            );
        await Sut.CreateAsync(maps);
        var expectedIds = maps.Select(x => x.QuestionSnapshotId)
            .OrderByDescending(x => x)
            .Skip(1)
            .Take(2)
            .ToList();

        // Act
        var result = await Sut.GetAllByQuizSnapshotIdAsync(
            quizSnapshot.Id,
            maps.Max(x => x.QuestionSnapshotId),
            2
        );

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(expectedIds.Count);
        result.Select(x => x.QuestionSnapshotId).ShouldBe(expectedIds, false);
    }

    [Test]
    public async Task GetAllByQuizSnapshotIdAsync_WhenMapDoesNotExist_ShouldReturnEmpty()
    {
        // Arrange
        var quizSnapshotId = Guid.NewGuid().ToString();

        // Act
        var result = await Sut.GetAllByQuizSnapshotIdAsync(quizSnapshotId, null, 2);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    [Test]
    public async Task GetAllByQuizSnapshotIdAsync_WhenSnapshotDoesAlreadyExist_ShouldReturnAllItems()
    {
        // Arrange
        var template = await GetCreatedQuizTemplateAsync();
        var question1 = await GetCreatedQuestionSnapshotAsync(template.Id);
        var question2 = await GetCreatedQuestionSnapshotAsync(template.Id);
        var question3 = await GetCreatedQuestionSnapshotAsync(template.Id);

        var quizSnapshot = await GetCreatedQuizSnapshotAsync(template.Id);
        var questionSnapshotIds = new List<string>([question1.Id, question2.Id, question3.Id]);

        var maps = QuizSnapshotQuestionSnapshotMapBuilder
            .New()
            .BuildMany(
                3,
                (x, i) =>
                    x.WithQuestionSnapshotId(questionSnapshotIds[i])
                        .WithQuizSnapshotId(quizSnapshot.Id)
            );
        await Sut.CreateAsync(maps);
        var expectedIds = maps.Select(x => x.QuestionSnapshotId).OrderByDescending(x => x).ToList();

        // Act
        var result = await Sut.GetAllByQuizSnapshotIdAsync(quizSnapshot.Id);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(expectedIds.Count);
        result.Select(x => x.QuestionSnapshotId).ShouldBe(expectedIds, false);
    }

    [Test]
    public async Task GetAllByQuizSnapshotIdAsync_WhenSnapshotDoesNotExist_ShouldReturnEmpty()
    {
        // Arrange
        var quizSnapshotId = Guid.NewGuid().ToString();

        // Act
        var result = await Sut.GetAllByQuizSnapshotIdAsync(quizSnapshotId);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    [Test]
    public async Task DeleteByQuizSnapshotId_WhenMapDoesAlreadyExist_ShouldDeleteSuccess()
    {
        // Arrange
        var template = await GetCreatedQuizTemplateAsync();
        var question = await GetCreatedQuestionSnapshotAsync(template.Id);
        var quizSnapshot = await GetCreatedQuizSnapshotAsync(template.Id);
        var map = QuizSnapshotQuestionSnapshotMapBuilder
            .New()
            .WithQuestionSnapshotId(question.Id)
            .WithQuizSnapshotId(quizSnapshot.Id)
            .Build();
        await Sut.CreateAsync([map]);

        // Act
        await Sut.DeleteByQuizSnapshotIdAsync(map.QuizSnapshotId);

        // Assert
        var result = await Sut.GetAllByQuizSnapshotIdAsync(map.QuizSnapshotId, null, 1);
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    [Test]
    public async Task DeleteByQuizSnapshotId_WhenMapDoesNotExist_ShouldNotThrowException()
    {
        // Arrange
        var template = await GetCreatedQuizTemplateAsync();
        var question = await GetCreatedQuestionSnapshotAsync(template.Id);
        var quizSnapshot = await GetCreatedQuizSnapshotAsync(template.Id);
        var map = QuizSnapshotQuestionSnapshotMapBuilder
            .New()
            .WithQuestionSnapshotId(question.Id)
            .WithQuizSnapshotId(quizSnapshot.Id)
            .Build();
        var action = async () => await Sut.DeleteByQuizSnapshotIdAsync(map.QuizSnapshotId);

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
    #endregion
}
