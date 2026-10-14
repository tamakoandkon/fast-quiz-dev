using FastQuiz.Common.Exceptions;
using FastQuiz.Common.Utils;
using FastQuiz.Question.Domain.Entities;
using FastQuiz.Quiz.Domain.Entities;
using FastQuiz.Quiz.Domain.Repositories;
using FastQuiz.Quiz.Domain.Services;
using FastQuiz.Quiz.Infrastructure.Repositories;
using FastQuiz.Quiz.Infrastructure.Repositories.PostgreSQL;
using FastQuiz.Quiz.Infrastructure.Repositories.Sqlite;
using FastQuiz.Quiz.UseCase.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Options;
using Shouldly;

namespace FastQuiz.Quiz.Test.Infrastructure;

[TestFixture("Sqlite")]
[TestFixture("PostgreSql", Category = "PostgreSql")]
internal class QuizTransactionTests(string provider)
{
    private string _database = null!;
    private string _connectionString = null!;

    [SetUp]
    public void Setup()
    {
        var name = "fastquiz_pr13_" + Guid.NewGuid().ToString("N");
        _database = provider == "Sqlite" ? Path.Combine(Path.GetTempPath(), name + ".db") : name;
        _connectionString = provider == "Sqlite"
            ? $"Data Source={_database};Default Timeout=5"
            : new Npgsql.NpgsqlConnectionStringBuilder(
                Environment.GetEnvironmentVariable("FASTQUIZ_TEST_POSTGRES")
                ?? "Host=127.0.0.1;Username=postgres;Password=postgres")
            { Database = _database }.ConnectionString;
    }

    [TearDown]
    public async Task Cleanup()
    {
        // Only the unique database created by this test is ever removed.
        await using var db = OpenDb();
        await db.Database.EnsureDeletedAsync();
    }

    [Test]
    public async Task LegacyMigrationRequiresRecheckingPendingRecordsButPreservesFinalResults()
    {
        await using var db = OpenDb();
        var migrator = db.GetService<IMigrator>();
        await migrator.MigrateAsync(provider == "Sqlite"
            ? "20260802031653_AddTimeConventor" : "20260919055349_InitialDb");
        await SeedAsync(db, manual: true, legacy: true);
        db.AddRange(
            new QuizAttempt
            {
                Id = "graded",
                QuizTemplateId = "template",
                QuizSnapshotId = "snapshot",
                Status = QuizAttemptStatus.Graded,
                AssignId = "owner",
                Score = 10
            },
            new QuestionRecord
            {
                Id = "graded-record",
                QuizAttemptId = "graded",
                QuestionSnapshotId = "question",
                MarkUpContent = "manual",
                IsCorrect = true
            });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        await migrator.MigrateAsync();

        (await db.Set<QuestionRecord>().SingleAsync(x => x.Id == "record")).IsCorrect.ShouldBeNull();
        (await db.Set<QuestionRecord>().SingleAsync(x => x.Id == "graded-record")).IsCorrect.ShouldBe(true);
        (await db.Set<QuizAttempt>().SingleAsync(x => x.Id == "graded")).Score.ShouldBe(10);
        await Should.ThrowAsync<BusinessException>(() => ReviewService(db).CompleteReviewAsync("attempt"));
    }

    [Test]
    public async Task FailedReplacementRollsBackDeletionAndPreservesOldAnswers()
    {
        await using var db = OpenDb();
        await db.Database.EnsureCreatedAsync();
        await SeedAsync(db);
        db.Add(new AnswerRecord { Id = "old", QuestionRecordId = "record", QuizAttemptId = "attempt", Value = "old" });
        await db.SaveChangesAsync();

        await Should.ThrowAsync<DbUpdateException>(() => AnswerService(db).SetAnswersAsync(Pending("new", null!)));
        (await db.Set<AnswerRecord>().AsNoTracking().SingleAsync()).Value.ShouldBe("old");
    }

    [Test]
    public async Task ConcurrentCompleteAnswerSetsMustNotBeMerged()
    {
        await using (var seed = OpenDb())
        {
            await seed.Database.EnsureCreatedAsync();
            await SeedAsync(seed);
        }
        var firstLocked = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var secondStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var first = Task.Run(async () =>
        {
            await using var db = OpenDb();
            await new EfQuizUnitOfWork(db).ExecuteForAttemptAsync("attempt", async _ =>
            {
                firstLocked.SetResult();
                await release.Task;
                await AnswerService(db).SetAnswersAsync(Pending("a", "b"));
            });
        });
        await firstLocked.Task.WaitAsync(TimeSpan.FromSeconds(10));
        var second = Task.Run(async () =>
        {
            await using var db = OpenDb();
            secondStarted.SetResult();
            await AnswerService(db).SetAnswersAsync(Pending("c", "d"));
        });
        try
        {
            await secondStarted.Task.WaitAsync(TimeSpan.FromSeconds(10));
            await Task.Delay(100);
            second.IsCompleted.ShouldBeFalse("the second request must wait for the attempt lock");
        }
        finally
        {
            release.TrySetResult();
        }
        await Task.WhenAll(first, second).WaitAsync(TimeSpan.FromSeconds(15));
        await using var verify = OpenDb();
        (await verify.Set<AnswerRecord>().OrderBy(x => x.Value).Select(x => x.Value).ToListAsync())
            .ShouldBe(["c", "d"]);
    }

    [Test]
    public async Task CompletingMixedAttemptRechecksObjectiveQuestionsAndKeepsManualDecisions()
    {
        await using var db = OpenDb();
        await db.Database.EnsureCreatedAsync();
        await SeedAsync(db);
        db.AddRange(
            new QuestionSnapshot
            {
                Id = "manual",
                QuestionId = "manual-source",
                QuizTemplateId = "template",
                GroupId = "group",
                MarkUpContent = "manual",
                Type = QuestionType.FillIn
            },
            new QuestionAnswerSnapshot
            {
                Id = "manual-option",
                QuestionSnapshotId = "manual",
                MarkUpContent = "reference",
                IsCorrect = true,
                Strategy = MatchStrategy.Manual
            },
            new QuestionRecord
            {
                Id = "manual-record",
                QuizAttemptId = "attempt",
                QuestionSnapshotId = "manual",
                MarkUpContent = "manual",
                IsCorrect = false
            },
            new QuestionSelectionRule
            {
                Id = "manual-rule",
                QuizeTemplateId = "template",
                GroupId = "group",
                Type = QuestionType.FillIn,
                Count = 1,
                Score = 10
            },
            new AnswerRecord { Id = "random-record-id", QuestionRecordId = "record", QuizAttemptId = "attempt", Value = "correct-option" });
        await db.SaveChangesAsync();
        await db.Set<QuizAttempt>().ExecuteUpdateAsync(x => x.SetProperty(a => a.Status, QuizAttemptStatus.Submitted));
        await ReviewService(db).CompleteReviewAsync("attempt");
        var attempt = await db.Set<QuizAttempt>().AsNoTracking().SingleAsync();
        attempt.Status.ShouldBe(QuizAttemptStatus.Graded);
        attempt.Score.ShouldBe(10);
        (await db.Set<QuestionRecord>().AsNoTracking().SingleAsync(x => x.Id == "manual-record")).IsCorrect.ShouldBe(false);
    }

    [Test]
    public async Task ExpiredManualAttemptIsSubmittedButNotMarkedComplete()
    {
        await using var db = OpenDb();
        await db.Database.EnsureCreatedAsync();
        await SeedAsync(db, manual: true);
        var startedAt = DateTimeOffset.UtcNow.AddHours(-2);
        await db.Set<QuizAttempt>().ExecuteUpdateAsync(x => x
            .SetProperty(a => a.Status, QuizAttemptStatus.InProgress)
            .SetProperty(a => a.StartTime, startedAt));
        await ReviewService(db).ReviewAttemptAsync("attempt");
        var attempt = await db.Set<QuizAttempt>().AsNoTracking().SingleAsync();
        attempt.Status.ShouldBe(QuizAttemptStatus.Submitted);
        attempt.SubmitTime.ShouldNotBeNull();
        (await db.Set<QuestionRecord>().AsNoTracking().SingleAsync()).IsCorrect.ShouldBeNull();
    }

    [Test]
    public async Task TemplateLockSerializesCreationAndClaimingAcrossIndependentContexts()
    {
        await using (var seed = OpenDb())
        {
            await seed.Database.EnsureCreatedAsync();
            await SeedAsync(seed);
        }
        var firstLocked = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var secondStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var secondEntered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var first = Task.Run(async () =>
        {
            await using var db = OpenDb();
            await new EfQuizUnitOfWork(db).ExecuteForTemplateAsync("template", async _ =>
            {
                firstLocked.SetResult();
                await release.Task;
                await db.Set<QuizTemplate>().ExecuteUpdateAsync(x => x.SetProperty(t => t.AttemptLimit, 2));
            });
        });
        await firstLocked.Task.WaitAsync(TimeSpan.FromSeconds(10));
        var second = Task.Run(async () =>
        {
            await using var db = OpenDb();
            secondStarted.SetResult();
            await new EfQuizUnitOfWork(db).ExecuteForTemplateAsync("template", async _ =>
            {
                secondEntered.SetResult();
                (await db.Set<QuizTemplate>().AsNoTracking().SingleAsync()).AttemptLimit.ShouldBe(2);
            });
        });
        try
        {
            await secondStarted.Task.WaitAsync(TimeSpan.FromSeconds(10));
            await Task.WhenAny(secondEntered.Task, Task.Delay(100));
            secondEntered.Task.IsCompleted.ShouldBeFalse();
        }
        finally { release.TrySetResult(); }
        await Task.WhenAll(first, second).WaitAsync(TimeSpan.FromSeconds(15));
    }

    private DbContext OpenDb()
    {
        var settings = Options.Create(new QuizDbContextSettings { ConnectionString = _connectionString });
        return provider == "Sqlite"
            ? new SqliteQuizDbContext(new DbContextOptionsBuilder<SqliteQuizDbContext>().UseSqlite(_connectionString).Options, settings)
            : new PostgreSQLQuizDbContext(new DbContextOptionsBuilder<PostgreSQLQuizDbContext>().UseNpgsql(_connectionString).Options, settings);
    }

    private IQuestionRecordRepository Records(DbContext db) => db is SqliteQuizDbContext sqlite
        ? new SqliteQuestionRecordRepository(sqlite) : new PostgreSQLQuestionRecordRepository((PostgreSQLQuizDbContext)db);

    private AnswerBufferService AnswerService(DbContext db) => new(Records(db), new EfQuizUnitOfWork(db), new IdGenerator());

    private QuizReviewService ReviewService(DbContext db) => db is SqliteQuizDbContext sqlite
        ? new(new SqliteQuizAttemptRepository(sqlite), new SqliteQuizTemplateRepository(sqlite), Records(db),
            new SqliteQuestionSnapshotRepository(sqlite), new SqliteQuestionSelectionRuleRepository(sqlite), new QuestionGradingDecider(), new EfQuizUnitOfWork(db))
        : new(new PostgreSQLQuizAttemptRepository((PostgreSQLQuizDbContext)db), new PostgreSQLQuizTemplateRepository((PostgreSQLQuizDbContext)db), Records(db),
            new PostgreSQLQuestionSnapshotRepository((PostgreSQLQuizDbContext)db), new PostgreSQLQuestionSelectionRuleRepository((PostgreSQLQuizDbContext)db), new QuestionGradingDecider(), new EfQuizUnitOfWork(db));

    private static PendingAnswer Pending(params string[] values) => new()
    { QuestionRecordId = "record", QuizAttemptId = "attempt", Values = values };

    private static async Task SeedAsync(DbContext db, bool manual = false, bool legacy = false)
    {
        var type = manual ? QuestionType.FillIn : QuestionType.SingleChoice;
        db.AddRange(
            new QuizTemplate("template", "test", "", "", "tenant", 3, true, DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow.AddHours(1), 60, null),
            new QuizSnapshot { Id = "snapshot", QuizTemplateId = "template", TotalScore = 10 },
            new QuizAttempt
            {
                Id = "attempt",
                AssignId = "owner",
                QuizTemplateId = "template",
                QuizSnapshotId = "snapshot",
                Status = QuizAttemptStatus.Submitted
            },
            new QuestionSnapshot
            {
                Id = "question",
                QuestionId = "source",
                QuizTemplateId = "template",
                GroupId = "group",
                MarkUpContent = "question",
                Type = type
            },
            new QuestionAnswerSnapshot
            {
                Id = "correct-option",
                QuestionSnapshotId = "question",
                MarkUpContent = "reference",
                IsCorrect = true,
                Strategy = manual ? MatchStrategy.Manual : MatchStrategy.Exact
            },
            new QuestionRecord
            {
                Id = "record",
                QuizAttemptId = "attempt",
                QuestionSnapshotId = "question",
                MarkUpContent = "question",
                IsCorrect = legacy ? false : null
            },
            new QuestionSelectionRule { Id = "rule", QuizeTemplateId = "template", GroupId = "group", Type = type, Count = 1, Score = 10 });
        await db.SaveChangesAsync();
    }
}
