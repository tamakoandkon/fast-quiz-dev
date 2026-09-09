using FastQuiz.Quiz.UseCase.Commands;
using FastQuiz.Quiz.UseCase.Dtos;
using FastQuiz.Quiz.UseCase.Queries;

namespace FastQuiz.Quiz.UseCase.Services;

public interface IQuizService
{
    Task GenerateQuizSnapshotAsync(
        string userId,
        string quizTemplateId,
        CreateQuizSnapshotCommand command
    );
    Task<IReadOnlyList<QuizSnapshotDto>> GetAllQuizSnapshotAsync(
        string userId,
        string quiztemplateId,
        GetAllQuizSnapshotQuery query
    );
    Task DeleteQuizSnapshotByIdAsync(string userId, string id);
    Task CreateQuizAttemptAsync(
        string user,
        string quizTemplateId,
        CreateQuizAttemptCommand command
    );
    Task<QuizAttempSecretDto> GetQuizAttemptByIdAsync(string userId, string quizAttemptId);
    Task<QuizAttempSecretDto> AssignQuizAttemptAsync(string userId, string quizTemplateId);
    Task SetAnswerRecordAsync(
        string questionRecordId,
        IReadOnlyList<SetAnswerRecordCommand> commands
    );
}
