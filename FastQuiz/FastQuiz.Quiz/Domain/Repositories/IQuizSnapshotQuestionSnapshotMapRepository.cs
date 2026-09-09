using FastQuiz.Quiz.Domain.Entities;

namespace FastQuiz.Quiz.Domain.Repositories;

public interface IQuizSnapshotQuestionSnapshotMapRepository
{
    Task CreateAsync(QuizSnapshotQuestionSnapshotMap map);
    Task CreateAsync(IEnumerable<QuizSnapshotQuestionSnapshotMap> maps);
    Task<IReadOnlyList<QuizSnapshotQuestionSnapshotMap>> GetAllByQuizSnapshotIdAsync(
        string quizSnapshotId,
        string? cursor,
        int count
    );
    Task<IReadOnlyList<QuizSnapshotQuestionSnapshotMap>> GetAllByQuizSnapshotIdAsync(
        string quizSnapshotId
    );
    Task DeleteByQuizSnapshotIdAsync(string quizSnapshotId);
}
