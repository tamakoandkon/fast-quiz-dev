using FastQuiz.Quiz.Domain.Entities;
using FastQuiz.Quiz.UseCase.Commands;
using FastQuiz.Quiz.UseCase.Dtos;
using FastQuiz.Quiz.UseCase.Queries;

namespace FastQuiz.Quiz.UseCase.Services;

// Compatibility facade: endpoints and previously persisted Hangfire jobs retain their public entry points.
public class QuizAttemptService(
    IQuizAttemptCreationService _quizAttemptCreationService,
    IQuizAttemptQueryService _quizAttemptQueryService,
    IQuestionRecordQueryService _questionRecordQueryService,
    IQuizAnswerService _quizAnswerService,
    IQuizAttemptLifecycleService _quizAttemptLifecycleService,
    IQuizAttemptReviewService _quizAttemptReviewService
) : IQuizAtteptService
{
    public Task<QuizAttempDto> CreateQuizAttemptAsync(
        string userId,
        string tenantId,
        string quizTemplateId
    )
        => _quizAttemptCreationService.CreateQuizAttemptAsync(userId, tenantId, quizTemplateId);

    public Task CreateQuizAttemptAsync(
        string userId,
        string tenantId,
        string quizTemplateId,
        IEnumerable<CreateQuizAttemptCommand> commands
    )
        => _quizAttemptCreationService.CreateQuizAttemptAsync(userId, tenantId, quizTemplateId, commands);

    public Task AssignQuizAttemptAsync(string userId, string tenantId, string quizAttemptId)
        => _quizAttemptCreationService.AssignQuizAttemptAsync(userId, tenantId, quizAttemptId);

    public Task<IReadOnlyList<QuizAttempDto>> GetAllQuizAttemptAsync(
        string userId,
        string tenantId,
        string quizTemplateId,
        GetAllQuizAttemptQuery query
    )
        => _quizAttemptQueryService.GetAllQuizAttemptAsync(userId, tenantId, quizTemplateId, query);

    public Task<IReadOnlyList<QuizAttempDto>> GetAllRecordsByTemplateIdAsync(
        string userId,
        string tenantId,
        string quizTemplateId,
        GetAllRecordsQuery query
    )
        => _quizAttemptQueryService.GetAllRecordsByTemplateIdAsync(userId, tenantId, quizTemplateId, query);

    public Task<IReadOnlyList<QuizAttempDto>> GetMyQuizAttemptAsync(
        string userId,
        string tenantId,
        GetMyQuizAttemptQuery query
    )
        => _quizAttemptQueryService.GetMyQuizAttemptAsync(userId, tenantId, query);

    public Task<IReadOnlyList<QuizAttempDto>> GetMyTemplateRecordsAsync(
        string userId,
        string tenantId,
        string quizTemplateId,
        GetMyTemplateRecordsQuery query
    )
        => _quizAttemptQueryService.GetMyTemplateRecordsAsync(userId, tenantId, quizTemplateId, query);

    public Task<IReadOnlyList<QuizAttempDto>> GetUserRecordsAsync(
        string operatorId,
        string tenantId,
        string userId,
        GetUserRecordsQuery query
    )
        => _quizAttemptQueryService.GetUserRecordsAsync(operatorId, tenantId, userId, query);

    public Task<QuizAttempDto> GetQuizAttemptByIdAsync(
        string userId,
        string tenantId,
        string quizAttemptId
    )
        => _quizAttemptQueryService.GetQuizAttemptByIdAsync(userId, tenantId, quizAttemptId);

    public Task<
        IReadOnlyList<QuestionRecordWithoutAnswerDto>
    > GetAllQuestionWithoutAnswerDtoAsync(
        string userId,
        string tenantId,
        string attemptId,
        GetAllQuestionRecordDtoQuery query
    )
        => _questionRecordQueryService.GetAllQuestionWithoutAnswerDtoAsync(userId, tenantId, attemptId, query);

    public Task<IReadOnlyList<QuestionRecordWithAnswerDto>> GetAllQuestionWithAnswerDtoAsync(
        string userId,
        string tenantId,
        string attemptId,
        GetAllQuestionRecordDtoQuery query
    )
        => _questionRecordQueryService.GetAllQuestionWithAnswerDtoAsync(userId, tenantId, attemptId, query);

    public Task AnswerQuestionAsync(
        string userId,
        string tenantId,
        string questionRecordId,
        IEnumerable<SetAnswerRecordCommand> commands
    )
        => _quizAnswerService.AnswerQuestionAsync(userId, tenantId, questionRecordId, commands);

    public Task UpdateQuizAttemptStatusAsync(
        string userId,
        string tenantId,
        string quizAttemptId,
        QuizAttemptStatus status
    )
        => _quizAttemptLifecycleService.UpdateQuizAttemptStatusAsync(userId, tenantId, quizAttemptId, status);

    public Task ManualReviewQuestionRecordAsync(
        string tenantId,
        string myId,
        string questionRecordId,
        bool isCorrect
    )
        => _quizAttemptReviewService.ManualReviewQuestionRecordAsync(tenantId, myId, questionRecordId, isCorrect);

    public Task ReviewQuizAttemptByAttemptIdAsync(string quizAttemptId)
        => _quizAttemptReviewService.ReviewQuizAttemptByAttemptIdAsync(quizAttemptId);

    public Task ReviewQuizAttemptsByTemplateIdAsync(string templateId)
        => _quizAttemptReviewService.ReviewQuizAttemptsByTemplateIdAsync(templateId);
}
