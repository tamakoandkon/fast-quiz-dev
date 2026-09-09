using FastQuiz.Quiz.Domain.Repositories;
using FastQuiz.Quiz.Infrastructure.Repositories.PostgreSQL;
using FastQuiz.Quiz.Test.Infrastructure.Repositories.Utils;
using Microsoft.EntityFrameworkCore;

namespace FastQuiz.Quiz.Test.Infrastructure.Repositories.PostgreSql;

[TestFixture]
[Category("PostgreSql")]
internal class PostgreSQLQuizTemplateRepositoryTests : QuizTemplateRepositoryTestsBase
{
    private PostgreSQLQuizDbContext _dbContext = null!;
    private PostgreSQLQuizTemplateRepository _quizTemplateRepository = null!;

    protected override IQuizTemplateRepository Sut => _quizTemplateRepository;

    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        _dbContext = InitialTestDbHelper.GetTestPostgreSqlQuizDbContext();
        await _dbContext.Database.EnsureDeletedAsync();
        await _dbContext.Database.EnsureCreatedAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        if (_dbContext != null)
        {
            await _dbContext.Database.EnsureDeletedAsync();
            await _dbContext.DisposeAsync();
        }
    }

    [SetUp]
    public void Setup()
    {
        _dbContext = InitialTestDbHelper.GetTestPostgreSqlQuizDbContext();

        _quizTemplateRepository = new PostgreSQLQuizTemplateRepository(_dbContext);
    }

    [TearDown]
    public void TearDown()
    {
        if (_dbContext != null)
        {
            _dbContext.AnswerRecords.ExecuteDelete();
            _dbContext.QuestionRecords.ExecuteDelete();
            _dbContext.QuizSnapshotQuestionSnapshotMaps.ExecuteDelete();
            _dbContext.QuestionAnswerSnapshots.ExecuteDelete();
            _dbContext.QuestionSnapshots.ExecuteDelete();
            _dbContext.QuizAttempts.ExecuteDelete();
            _dbContext.QuizSnapshots.ExecuteDelete();
            _dbContext.QuestionSelecionRules.ExecuteDelete();
            _dbContext.QuizTemplates.ExecuteDelete();
            _dbContext.Dispose();
        }
    }
}
