using FastQuiz.Quiz.Domain.Entities;

namespace FastQuiz.Quiz.UseCase.Services;

public interface IAnswerBufferService
{
    /// <summary>
    /// 在同一个数据库事务内，用一组完整答案替换该题答案。
    /// 空集合表示清空该题答案；方法返回时数据已经持久化。
    /// </summary>
    Task SetAnswersAsync(
        PendingAnswer answer,
        CancellationToken cancellationToken = default
    );
}
