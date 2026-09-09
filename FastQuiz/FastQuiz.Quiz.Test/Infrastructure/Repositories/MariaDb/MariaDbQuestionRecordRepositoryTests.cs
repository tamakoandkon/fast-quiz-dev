using FastQuiz.Quiz.Domain.Entities;
using FastQuiz.Quiz.Domain.Repositories;
using FastQuiz.Quiz.Infrastructure.Repositories.MariaDb;
using FastQuiz.Quiz.Test.Infrastructure.Repositories.Utils;
using Microsoft.EntityFrameworkCore;

namespace FastQuiz.Quiz.Test.Infrastructure.Repositories.MariaDb;

[TestFixture]
[Category("MariaDb")]
internal class MariaDbQuestionRecordRepositoryTests : QuestionRecordRepositoryTestsBase
{
    private MariaDbQuizDbContext _dbContext = null!;
    private MariaDbQuestionRecordRepository _recordRepository = null!;
    private MariaDbQuizTemplateRepository _templateRepository = null!;
    private MariaDbQuestionSnapshotRepository _questionSnapshotRepository = null!;
    private MariaDbQuizSnapshotRepository _quizSnapshotRepository = null!;
    private MariaDbQuizAttemptRepository _quizAttemptRepository = null!;

    protected override IQuestionRecordRepository Sut => _recordRepository;

    protected override IQuizTemplateRepository Templates => _templateRepository;

    protected override IQuestionSnapshotRepository QuestionSnapshots => _questionSnapshotRepository;

    protected override IQuizSnapshotRepository QuizSnapshots => _quizSnapshotRepository;

    protected override IQuizAttemptRepository QuizAttempts => _quizAttemptRepository;

    protected override async Task<
        IReadOnlyList<AnswerRecord>
    > GetAnswerRecordsByQuestionRecordIdAsync(string questionRecordId)
    {
        return await _dbContext
            .AnswerRecords.AsNoTracking()
            .Where(x => x.QuestionRecordId == questionRecordId)
            .ToListAsync();
    }

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
            await _dbContext.AnswerRecords.ExecuteDeleteAsync();
            await _dbContext.QuestionRecords.ExecuteDeleteAsync();
            await _dbContext.QuizAttempts.ExecuteDeleteAsync();
            await _dbContext.QuestionAnswerSnapshots.ExecuteDeleteAsync();
            await _dbContext.QuestionSnapshots.ExecuteDeleteAsync();
            await _dbContext.QuizSnapshots.ExecuteDeleteAsync();
            await _dbContext.QuizTemplates.ExecuteDeleteAsync();
            await _dbContext.DisposeAsync();
        }
    }

    [SetUp]
    public void Setup()
    {
        _dbContext = InitialTestDbHelper.GetTestMariaDbQuizDbContext();

        _recordRepository = new MariaDbQuestionRecordRepository(_dbContext);
        _templateRepository = new MariaDbQuizTemplateRepository(_dbContext);
        _questionSnapshotRepository = new MariaDbQuestionSnapshotRepository(_dbContext);
        _quizSnapshotRepository = new MariaDbQuizSnapshotRepository(_dbContext);
        _quizAttemptRepository = new MariaDbQuizAttemptRepository(_dbContext);
    }

    [TearDown]
    public void TearDown()
    {
        if (_dbContext != null)
        {
            _dbContext.AnswerRecords.ExecuteDelete();
            _dbContext.QuestionRecords.ExecuteDelete();
            _dbContext.QuizAttempts.ExecuteDelete();
            _dbContext.QuestionAnswerSnapshots.ExecuteDelete();
            _dbContext.QuestionSnapshots.ExecuteDelete();
            _dbContext.QuizSnapshotQuestionSnapshotMaps.ExecuteDelete();
            _dbContext.QuizSnapshots.ExecuteDelete();
            _dbContext.QuestionSelecionRules.ExecuteDelete();
            _dbContext.QuizTemplates.ExecuteDelete();
            _dbContext.Dispose();
        }
    }
}
