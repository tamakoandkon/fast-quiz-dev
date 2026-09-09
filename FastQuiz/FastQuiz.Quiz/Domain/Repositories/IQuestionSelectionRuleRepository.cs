using FastQuiz.Question.Domain.Entities;
using FastQuiz.Quiz.Domain.Entities;

namespace FastQuiz.Quiz.Domain.Repositories;

public interface IQuestionSelectionRuleRepository
{
    Task CreateAsync(QuestionSelectionRule rule);
    Task<IReadOnlyList<QuestionSelectionRule>> GetByQuizTemplateIdAsync(
        string quizTemplateId,
        string? cursor,
        int count
    );
    Task<IReadOnlyList<QuestionSelectionRule>> GetByQuizTemplateIdAsync(string quizTemplateId);
    Task<QuestionSelectionRule?> GetByIdAsync(string id);
    Task<QuestionSelectionRule?> GetByTemplateIdAndConditionAsync(
        string templateId,
        string groupId,
        QuestionType type,
        int level
    );
    Task UpdateAsync(QuestionSelectionRule rule);
    Task DeleteByIdAsync(string id);
}
