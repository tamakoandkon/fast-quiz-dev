namespace FastQuiz.Quiz.Domain.Repositories;

public interface IQuizUnitOfWork
{
    /// <summary>
    /// 在事务中锁定试卷行，串行化该卷的答题、交卷和阅卷操作。
    /// </summary>
    Task ExecuteForAttemptAsync(
        string quizAttemptId,
        Func<CancellationToken, Task> action,
        CancellationToken cancellationToken = default
    );

    Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> action,
        CancellationToken cancellationToken = default
    );
}
