using FastQuiz.Quiz.Domain.Entities;
using FastQuiz.Quiz.UseCase.Commands;
using FastQuiz.Quiz.UseCase.Dtos;
using FastQuiz.Quiz.UseCase.Queries;

namespace FastQuiz.Quiz.UseCase.Services;

public interface IQuizAtteptService
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
    Task<IReadOnlyList<QuizAttempDto>> GetAllQuizAttemptAsync(
        string userId,
        string tenantId,
        string quizTemplateId,
        GetAllQuizAttemptQuery query
    );
    Task<IReadOnlyList<QuizAttempDto>> GetMyQuizAttemptAsync(
        string userId,
        string tenantId,
        GetMyQuizAttemptQuery query
    );
    Task<QuizAttempDto> GetQuizAttemptByIdAsync(
        string userId,
        string tenantId,
        string quizAttemptId
    );
    Task AssignQuizAttemptAsync(string userId, string tenantId, string quizAttemptId);
    Task UpdateQuizAttemptStatusAsync(
        string userId,
        string tenantId,
        string quizAttemptId,
        QuizAttemptStatus status
    );
    Task AnswerQuestionAsync(
        string userId,
        string tenantId,
        string questionRecordId,
        IEnumerable<SetAnswerRecordCommand> commands
    );
    Task<IReadOnlyList<QuestionRecordWithoutAnswerDto>> GetAllQuestionWithoutAnswerDtoAsync(
        string userId,
        string tenantId,
        string attemptId,
        GetAllQuestionRecordDtoQuery query
    );
    Task<IReadOnlyList<QuestionRecordWithAnswerDto>> GetAllQuestionWithAnswerDtoAsync(
        string userId,
        string tenantId,
        string attemptId,
        GetAllQuestionRecordDtoQuery query
    );
    Task ManualReviewQuestionRecordAsync(
        string tenantId,
        string myId,
        string questionRecordId,
        bool isCorrect
    );
}
