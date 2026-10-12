using FastQuiz.Quiz.Domain.Entities;

namespace FastQuiz.Quiz.Domain.Repositories;

public interface IQuizAttemptRepository
{
    Task CreateAsync(QuizAttempt quizAttempt);
    Task CreateAsync(IEnumerable<QuizAttempt> quizAttempts);
    Task<int> GetAttemptCountAsync(string userId, string templateId);
    Task<QuizAttempt?> FindByIdAsync(string id);
    Task<QuizAttempt?> FindLastUnfinishedByTemplateIdAndUserIdAsync(
        string userId,
        string templateId
    );
    Task<IReadOnlyList<QuizAttempt>> FindByQuizTemplateIdAsync(string userId, string templateId);
    Task<IReadOnlyList<QuizAttempt>> GetAllByUserIdAsync(string userId, string? cursor, int count);
    Task<IReadOnlyList<QuizAttempt>> GetAllByTemplateIdAsync(
        string templateId,
        string? cursor,
        int count
    );
    /// <summary>
    /// 获取指定考试的所有考试记录。
    /// onlyBest 为 true 时，按考生(AssignId)分组，每人只保留得分最高的一条记录(同分取最新)。
    /// isPrevious 为 true 时返回比 cursor 更新的记录(即上一页)，结果仍按 Id 降序。
    /// </summary>
    Task<IReadOnlyList<QuizAttempt>> GetAllRecordsByTemplateIdAsync(
        string templateId,
        bool onlyBest,
        string? cursor,
        int count,
        bool isPrevious
    );
    /// <summary>
    /// 获取指定考生的考试记录(可选限定某个考试模板)。
    /// 仅返回已完成的考试(Status == Graded/已批阅)，未开始/进行中/待批阅的记录不返回。
    /// onlyBest 为 true 时，按考试模板(QuizTemplateId)分组，每场考试只保留该考生得分最高的一条记录(同分取最新)。
    /// isPrevious 为 true 时返回比 cursor 更新的记录(即上一页)，结果仍按 Id 降序。
    /// </summary>
    Task<IReadOnlyList<QuizAttempt>> GetAllRecordsByUserIdAsync(
        string userId,
        string? templateId,
        bool onlyBest,
        string? cursor,
        int count,
        bool isPrevious
    );
    /// <summary>
    /// 获取指定考生在某场考试(模板)下的全部考试记录，包含所有状态(未开始/进行中/待批阅/已批阅)。
    /// 与 <see cref="GetAllRecordsByUserIdAsync"/> 不同：本方法不做 Graded 过滤，
    /// 因为「我的考试」展开区需要展示进行中的记录以便继续作答。
    /// isPrevious 为 true 时返回比 cursor 更新的记录(即上一页)，结果仍按 Id 降序。
    /// </summary>
    Task<IReadOnlyList<QuizAttempt>> GetAllRecordsByTemplateIdAndUserIdAsync(
        string userId,
        string templateId,
        string? cursor,
        int count,
        bool isPrevious
    );
    Task UpdateAsync(QuizAttempt quizAttempt);
    Task DeleteByIdAsync(string id);
    Task DeleteByTemplateIdAsync(string templateId);
}
