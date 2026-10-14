using FastQuiz.Common.Exceptions;
using FastQuiz.Quiz.Domain.Entities;
using FastQuiz.Quiz.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FastQuiz.Quiz.Infrastructure.Repositories;

public sealed class EfQuizUnitOfWork(DbContext dbContext) : IQuizUnitOfWork
{
    public Task ExecuteForAttemptAsync(
        string quizAttemptId,
        Func<CancellationToken, Task> action,
        CancellationToken cancellationToken = default
    ) => ExecuteInTransactionAsync(async token =>
    {
        // No-op UPDATE obtains a database write lock, including when no answers exist yet.
        // Unlike a process-local semaphore, this coordinates all application instances.
        var affected = await dbContext.Set<QuizAttempt>()
            .Where(x => x.Id == quizAttemptId)
            .ExecuteUpdateAsync(x => x.SetProperty(a => a.Status, a => a.Status), token)
            .ConfigureAwait(false);
        // MariaDB can report changed rows instead of matched rows for a no-op UPDATE.
        if (affected == 0 && !await dbContext.Set<QuizAttempt>()
            .AnyAsync(x => x.Id == quizAttemptId, token).ConfigureAwait(false))
        {
            throw new ResourceNotFoundException("试卷不存在");
        }

        await action(token).ConfigureAwait(false);
    }, cancellationToken);

    public async Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> action,
        CancellationToken cancellationToken = default
    )
    {
        if (dbContext.Database.CurrentTransaction is not null)
        {
            await action(cancellationToken).ConfigureAwait(false);
            return;
        }

        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy
            .ExecuteAsync(async () =>
            {
                await using var transaction = await dbContext.Database
                    .BeginTransactionAsync(cancellationToken)
                    .ConfigureAwait(false);

                try
                {
                    await action(cancellationToken).ConfigureAwait(false);
                    await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                }
                catch
                {
                    await transaction.RollbackAsync(CancellationToken.None).ConfigureAwait(false);
                    dbContext.ChangeTracker.Clear();
                    throw;
                }
            })
            .ConfigureAwait(false);
    }
}
