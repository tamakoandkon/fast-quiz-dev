using FastQuiz.Quiz.UseCase.Commands;
using FastQuiz.Quiz.UseCase.Dtos;
using FastQuiz.Quiz.UseCase.Queries;

namespace FastQuiz.Quiz.UseCase.Services;

public interface IQuizTemplateService
{
    Task<QuizTemplateDto> CreateQuizTemplateAsync(
        string userId,
        string tenantId,
        CreateQuizTemplateCommand command
    );
    Task<QuizTemplateDto> GetQuizTemplateAsync(string userId, string tenantId, string templateId);
    Task<IReadOnlyList<QuizTemplateDto>> GetAllQuizTemplateAsync(
        string userId,
        string tenantId,
        GetAllQuizTemplateQuery query
    );
    Task<IReadOnlyList<QuizTemplateDto>> GetPublishedQuizTemplateAsync(
        string userId,
        string tenantId,
        GetPublishedQuizTemplateQuery query
    );
    Task UpdateQuizTemplateAsync(
        string userId,
        string tenantId,
        string templateId,
        UpdateQuizTemplateCommand command
    );
    Task UpdateQuizTemplatePublishedAsync(
        string userId,
        string tenantId,
        string templateId,
        UpdateQuizTemplatePublishedCommand command
    );
    Task DeleteQuizTemplateAsync(string userId, string tenantId, string templateId);

    Task<QuestionSelectionRuleDto> CreateQuestionSelectionRuleAsync(
        string userId,
        string tenantId,
        string quizTemplateId,
        CreateQuestionSelectionRuleCommand command
    );
    Task<IReadOnlyList<QuestionSelectionRuleDto>> GetAllQuestionSelectionRulesAsync(
        string userId,
        string tenantId,
        string quizTemplateId,
        GetAllQuestionSelectionRuleQuery query
    );
    Task<IReadOnlyList<QuestionSelectionRuleDto>> GetAllQuestionSelectionRulesAsync(
        string userId,
        string tenantId,
        string quizTemplateId
    );
    Task<QuestionSelectionRuleDto> GetQuestionSelectionRuleByIdAsync(
        string userId,
        string tenantId,
        string questionSelectionId
    );
    Task UpdateQuestionSelectionRuleAsync(
        string userId,
        string tenantId,
        string ruleId,
        UpdateQuestionSelectionRuleCommand command
    );
    Task DeleteQuestionSelectionRuleByIdAsync(string userId, string tenantId, string id);
}
