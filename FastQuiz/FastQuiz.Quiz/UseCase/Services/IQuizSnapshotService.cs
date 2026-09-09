using FastQuiz.Quiz.UseCase.Commands;
using FastQuiz.Quiz.UseCase.Dtos;
using FastQuiz.Quiz.UseCase.Queries;

namespace FastQuiz.Quiz.UseCase.Services;

public interface IQuizSnapshotService
{
    Task CreateQuizSnapshotAsync(
        string userId,
        string tenantId,
        string quizTemplateId,
        CreateQuizSnapshotCommand command
    );
    Task<QuizSnapshotDto> GetQuizSnapshotOfMinAttemptCountByQuizTemplateIdAsync(
        string quizTtemplateId
    );
    Task<QuizSnapshotDto> GetQuizSnapshotByIdAsync(
        string userId,
        string tenantId,
        string quizSnapshotId
    );
    Task<IReadOnlyList<QuizSnapshotDto>> GetAllQuizSnapshotAsync(
        string userId,
        string tenantId,
        string quiztemplateId,
        GetAllQuizSnapshotQuery query
    );
    Task DeleteQuizSnapshotByIdAsync(string userId, string tenantId, string id);
    Task<IReadOnlyList<QuestionSnapshotDto>> GetAllQuestionSnapshotAsync(
        string userId,
        string tenantId,
        string quizSnapshotId,
        GetAllQuestionSnapshotQuery query
    );
    Task<IReadOnlyList<QuestionSnapshotDto>> GetAllQuestionSnapshotAsync(
        string userId,
        string tenantId,
        string quizSnapshotId
    );
}
