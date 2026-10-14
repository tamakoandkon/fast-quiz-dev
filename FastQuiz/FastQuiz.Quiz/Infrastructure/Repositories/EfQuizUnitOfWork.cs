using FastQuiz.Quiz.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FastQuiz.Quiz.Infrastructure.Repositories;

public sealed class EfQuizUnitOfWork(DbContext dbContext) : IQuizUnitOfWork
{
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
