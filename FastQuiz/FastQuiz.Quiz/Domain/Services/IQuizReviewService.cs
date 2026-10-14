namespace FastQuiz.Quiz.Domain.Services;

/// <summary>
/// 试卷自动阅卷领域服务。
/// 统一只暴露“通过 quizAttemptId”驱动的入口，阅卷/评分逻辑与 UseCase(权限/编排)解耦。
/// </summary>
public interface IQuizReviewService
{
    /// <summary>
    /// 按 attemptId 执行自动阅卷扫描：
    /// 对不需要人工判断的题调用判分器写入每题 IsCorrect；跳过需要人工判断(Manual)的题，不写入其 IsCorrect。
    /// - 整卷没有 Manual 题且当前为已提交(Submitted)：判完后置为 Graded(自动完成阅卷)。
    /// - 存在 Manual 题：得分为已自动判定的部分，保持 Submitted 语义(待人工阅卷)，不自动完成阅卷。
    /// </summary>
    Task ReviewAttemptAsync(string quizAttemptId);

    /// <summary>
    /// 仅在所有题目的 IsCorrect 都已落定后把整卷置为 Graded；null 表示尚未批阅。
    /// 得分不做重判(re-decode)，直接按“每题的持久化 IsCorrect × 对应抽题规则分数”求和。
    /// </summary>
    Task CompleteReviewAsync(string quizAttemptId);

    /// <summary>给定试卷是否存在“需要人工判断(Manual)”的题。</summary>
    Task<bool> HasManualQuestionAsync(string quizAttemptId);
}
