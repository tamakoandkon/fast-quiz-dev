using FastQuiz.Quiz.Domain.Repositories;
using FastQuiz.Quiz.Infrastructure.Repositories.MariaDb;
using FastQuiz.Quiz.Test.Infrastructure.Repositories.Utils;
using Microsoft.EntityFrameworkCore;

namespace FastQuiz.Quiz.Test.Infrastructure.Repositories.MariaDb;

[TestFixture]
[Category("MariaDb")]
internal class MariaDbQuizAttemptRepositoryTests : QuizAttemptRepositoryTestsBase
{
    private MariaDbQuizDbContext _dbContext = null!;
    private MariaDbQuizAttemptRepository _attemptRepository = null!;
    private MariaDbQuizTemplateRepository _templateRepository = null!;
    private MariaDbQuizSnapshotRepository _quizSnapshotRepository = null!;

    protected override IQuizAttemptRepository Sut => _attemptRepository;

    protected override IQuizTemplateRepository Templates => _templateRepository;

    protected override IQuizSnapshotRepository QuizSnapshots => _quizSnapshotRepository;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        _dbContext = InitialTestDbHelper.GetTestMariaDbQuizDbContext();
        _dbContext.Database.EnsureDeleted();
        _dbContext.Database.EnsureCreated();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        if (_dbContext != null)
        {
            await _dbContext.QuizAttempts.ExecuteDeleteAsync();
            await _dbContext.QuizSnapshots.ExecuteDeleteAsync();
            await _dbContext.QuizTemplates.ExecuteDeleteAsync();
            await _dbContext.DisposeAsync();
        }
    }

    [SetUp]
    public void Setup()
    {
        _dbContext = InitialTestDbHelper.GetTestMariaDbQuizDbContext();

        _attemptRepository = new MariaDbQuizAttemptRepository(_dbContext);
        _templateRepository = new MariaDbQuizTemplateRepository(_dbContext);
        _quizSnapshotRepository = new MariaDbQuizSnapshotRepository(_dbContext);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.QuizAttempts.ExecuteDelete();
        _dbContext.QuizSnapshotQuestionSnapshotMaps.ExecuteDelete();
        _dbContext.QuizSnapshots.ExecuteDelete();
        _dbContext.QuestionAnswerSnapshots.ExecuteDelete();
        _dbContext.QuestionSnapshots.ExecuteDelete();
        _dbContext.QuestionSelecionRules.ExecuteDelete();
        _dbContext.QuizTemplates.ExecuteDelete();
        _dbContext?.Dispose();
    }
}
