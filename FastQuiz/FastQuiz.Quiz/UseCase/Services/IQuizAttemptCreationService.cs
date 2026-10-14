using FastQuiz.Quiz.UseCase.Commands;
using FastQuiz.Quiz.UseCase.Dtos;

namespace FastQuiz.Quiz.UseCase.Services;

public interface IQuizAttemptCreationService
{
    Task<QuizAttempDto> CreateQuizAttemptAsync(
        string userId,
        string tenantId,
        string quizTemplateId
    );

    Task CreateQuizAttemptAsync(
        string userId,
        string tenantId,
        string quizTemplateId,
        IEnumerable<CreateQuizAttemptCommand> commands
    );

    Task AssignQuizAttemptAsync(string userId, string tenantId, string quizAttemptId);
}
