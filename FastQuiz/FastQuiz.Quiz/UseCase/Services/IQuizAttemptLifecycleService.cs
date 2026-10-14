using FastQuiz.Quiz.Domain.Entities;

namespace FastQuiz.Quiz.UseCase.Services;

public interface IQuizAttemptLifecycleService
{
    Task UpdateQuizAttemptStatusAsync(
        string userId,
        string tenantId,
        string quizAttemptId,
        QuizAttemptStatus status
    );
}
