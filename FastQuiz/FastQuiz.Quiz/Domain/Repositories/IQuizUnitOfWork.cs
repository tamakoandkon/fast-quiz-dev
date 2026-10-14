namespace FastQuiz.Quiz.Domain.Repositories;

public interface IQuizUnitOfWork
{
    Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> action,
        CancellationToken cancellationToken = default
    );
}
