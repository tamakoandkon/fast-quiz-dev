using FastQuiz.Quiz.Domain.Entities;

namespace FastQuiz.Quiz.Domain.Repositories;

public interface IQuestionSnapshotRepository
{
    Task CreateAsync(QuestionSnapshot questionSnapshot);
    Task CreateAsync(IEnumerable<QuestionSnapshot> questionSnapshots);
    Task<QuestionSnapshot?> FindByIdAsync(string id);
    Task<IReadOnlyList<QuestionSnapshot>> FindByIdAsync(IEnumerable<string> idList);
    Task<IReadOnlyList<QuestionSnapshot>> FindByQuestionIdAsync(
        string quizTemplateId,
        IEnumerable<string> questionIdList
    );
    Task DeleteByTemplateIdAsync(string quizTemplateId);
}
