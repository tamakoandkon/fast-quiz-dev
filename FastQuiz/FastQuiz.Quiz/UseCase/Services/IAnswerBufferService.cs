using FastQuiz.Quiz.Domain.Entities;

namespace FastQuiz.Quiz.UseCase.Services;

public interface IAnswerBufferService
{
    /// <summary>
    /// 请求侧唯一入口：用一组完整答案替换该题在内存缓冲区里的答案。
    /// 空集合表示清空该题答案。
    /// 不触发刷出，不碰数据库。
    /// </summary>
    void SetAnswers(PendingAnswer answer);

    /// <summary>
    /// 把缓冲区里的数据批量落库。
    /// 由后台服务按固定节奏调用，内部用 SemaphoreSlim 保证单线程刷出。
    /// </summary>
    Task FlushAsync(CancellationToken cancellationToken = default);
}
