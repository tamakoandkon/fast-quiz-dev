using FastQuiz.Common.Utils;
using FastQuiz.Quiz.Domain.Entities;
using FastQuiz.Quiz.Domain.Repositories;
using FastQuiz.Quiz.UseCase.Services;
using Shouldly;

namespace FastQuiz.Quiz.Test.UseCase.Services;

internal class AnswerBufferServiceTests
{
    private InMemoryQuestionRecordRepository _repository = null!;
    private AnswerBufferService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = new InMemoryQuestionRecordRepository();
        _sut = new AnswerBufferService(
            _repository,
            new ImmediateQuizUnitOfWork(),
            new IdGenerator()
        );
    }

    [Test]
    public async Task SetAnswersAsync_WhenMultipleAnswersAreSet_PersistsEveryAnswer()
    {
        await _sut.SetAnswersAsync(CreatePendingAnswer(["answer-a", "answer-b"]));

        _repository.Answers.Select(x => x.Value).ShouldBe(["answer-a", "answer-b"], true);
    }

    [Test]
    public async Task SetAnswersAsync_WhenAnswersAreEmpty_RemovesPreviouslyPersistedAnswers()
    {
        _repository.Answers.AddRange(
            [
                CreateAnswerRecord("old-a"),
                CreateAnswerRecord("old-b"),
            ]
        );
        await _sut.SetAnswersAsync(CreatePendingAnswer([]));

        _repository.Answers.ShouldBeEmpty();
        _repository.DeletedQuestionRecordIds.ShouldContain(QuestionRecordId);
    }

    [Test]
    public async Task SetAnswersAsync_WhenQuestionIsUpdatedTwice_PersistsLatestCompleteSet()
    {
        await _sut.SetAnswersAsync(CreatePendingAnswer(["old-a", "old-b"]));
        await _sut.SetAnswersAsync(CreatePendingAnswer(["new-a", "new-b", "new-c"]));

        _repository.Answers.Select(x => x.Value).ShouldBe(
            ["new-a", "new-b", "new-c"],
            true
        );
    }

    private const string QuestionRecordId = "question-record";
    private const string QuizAttemptId = "quiz-attempt";

    private static PendingAnswer CreatePendingAnswer(IReadOnlyList<string> values) =>
        new()
        {
            UserId = "user",
            TenantId = "tenant",
            QuestionRecordId = QuestionRecordId,
            QuizAttemptId = QuizAttemptId,
            Values = values,
            AddedTime = DateTimeOffset.UtcNow,
        };

    private static AnswerRecord CreateAnswerRecord(string value) =>
        new()
        {
            Id = Guid.NewGuid().ToString(),
            QuestionRecordId = QuestionRecordId,
            QuizAttemptId = QuizAttemptId,
            Value = value,
        };

    private sealed class InMemoryQuestionRecordRepository : IQuestionRecordRepository
    {
        public List<AnswerRecord> Answers { get; } = [];
        public List<string> DeletedQuestionRecordIds { get; } = [];

        public Task CreateAnswerRecordAsync(IEnumerable<AnswerRecord> answerRecords)
        {
            Answers.AddRange(answerRecords);
            return Task.CompletedTask;
        }

        public Task DeleteAnswerRecordByQuestionRecordIdAsync(
            IEnumerable<string> questionRecordIdList
        )
        {
            var ids = questionRecordIdList.ToHashSet();
            DeletedQuestionRecordIds.AddRange(ids);
            Answers.RemoveAll(x => ids.Contains(x.QuestionRecordId));
            return Task.CompletedTask;
        }

        public Task DeleteAnswerRecordByQuestionRecordIdAsync(string questionRecordId) =>
            DeleteAnswerRecordByQuestionRecordIdAsync([questionRecordId]);

        public Task CreateAsync(IEnumerable<QuestionRecord> records) => throw Unused();
        public Task<QuestionRecord?> FindQuestionRecordByIdAsync(string id) => throw Unused();
        public Task<IReadOnlyList<QuestionRecord>> GetAllByQuizAttempIdAsync(
            string quizAttemptId,
            string? cursor,
            int count
        ) => throw Unused();
        public Task<IReadOnlyList<QuestionRecord>> GetAllByQuizAttempIdAsync(
            string quizAttemptId
        ) => throw Unused();
        public Task UpdateAsync(QuestionRecord record) => throw Unused();
        public Task SetQuestionRecordsCorrectAsync(IEnumerable<string> idList, bool isCorrect) =>
            throw Unused();
        public Task DeleteByQuizAttemptId(string quizAttemptId) => throw Unused();

        private static NotSupportedException Unused() =>
            new("This repository member is not used by AnswerBufferService tests.");
    }

    private sealed class ImmediateQuizUnitOfWork : IQuizUnitOfWork
    {
        public Task ExecuteForAttemptAsync(
            string quizAttemptId,
            Func<CancellationToken, Task> action,
            CancellationToken cancellationToken = default
        ) => action(cancellationToken);

        public Task ExecuteInTransactionAsync(
            Func<CancellationToken, Task> action,
            CancellationToken cancellationToken = default
        ) => action(cancellationToken);
    }
}
