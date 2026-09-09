using BenchmarkDotNet.Attributes;
using FastQuiz.Question.Domain.Entities;
using FastQuiz.Question.Infrastructure.Repositories.Sqlite;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FastQuiz.Question.BenckMark.BenchMarks;

using Q = FastQuiz.Question.Domain.Entities.Question;

[MemoryDiagnoser]
public class RandomQuestionBenchmark
{
    private readonly string _connectionString = "Data Source=:memory:";
    private readonly int _totalQuestions = 100000;
    private readonly int _extractCount = 50;

    // ========== 缓存池：预加载所有 ID（Fisher-Yates 方案专用） ==========
    private List<string> _cachedIds = [];
    private bool _cacheReady = false;

    private SqliteQuestionDbContext _dbContext;
    private SqliteQuestionGroupRepository _questionGroupRepository;
    private SqliteQuestionRepository _quesionRepository;
    private SqliteConnection _connection;

    private QuestionType _testType = QuestionType.SingleChoice;
    private int _testLevel = 0;
    private string _testGroupId = Guid.NewGuid().ToString();

    /// <summary>
    /// 全局初始化：准备测试数据
    /// </summary>
    [GlobalSetup]
    public async Task Setup()
    {
        _connection = new SqliteConnection(_connectionString);
        _connection.Open();

        var options = new DbContextOptionsBuilder<SqliteQuestionDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new SqliteQuestionDbContext(
            options,
            Options.Create(new Question.Infrastructure.Repositories.QuestionDbContextSettings())
        );
        _dbContext.Database.EnsureDeleted();
        _dbContext.Database.EnsureCreated();

        _questionGroupRepository = new SqliteQuestionGroupRepository(_dbContext);
        _quesionRepository = new SqliteQuestionRepository(_dbContext);

        var group = SeedDefaultQuestionGroup(_testGroupId);

        await _questionGroupRepository.CreateAsync(group);
        List<Q> qlist = [];

        for (int i = 0; i < _totalQuestions; i++)
        {
            qlist.Add(SeedDefaultQuestion(_testGroupId));
        }

        _dbContext.Questions.AddRange(qlist);
        _dbContext.Answers.AddRange(qlist.SelectMany(x => x.Answers));
        _dbContext.SaveChanges();

        var count = _dbContext.Questions.Count();
        Console.WriteLine($"question count: {count}");

        var result = await _quesionRepository.GetQuestionIdsAsync(
            _testGroupId,
            _testType,
            _testLevel
        );
        _cachedIds = result.ToList();
        _cacheReady = true;
        Console.WriteLine($"cache is loading completed: {_cachedIds.Count} ");

        QuestionGroup SeedDefaultQuestionGroup(string groupId)
        {
            return new QuestionGroup(groupId, Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
        }

        Q SeedDefaultQuestion(string groupId)
        {
            var qId = Guid.NewGuid().ToString();
            return new Q(
                qId,
                QuestionType.SingleChoice,
                0,
                Guid.NewGuid().ToString(),
                null,
                groupId,
                [
                    new QuestionAnswer(
                        Guid.NewGuid().ToString(),
                        qId,
                        Guid.NewGuid().ToString(),
                        true,
                        0,
                        MatchStrategy.Exact
                    ),
                    new QuestionAnswer(
                        Guid.NewGuid().ToString(),
                        qId,
                        Guid.NewGuid().ToString(),
                        true,
                        0,
                        MatchStrategy.Exact
                    ),
                ]
            );
        }
    }

    [GlobalCleanup]
    public async Task Cleanup()
    {
        if (_connection != null)
        {
            await _connection.DisposeAsync();
        }
        if (_dbContext != null)
        {
            await _dbContext.DisposeAsync();
        }
    }

    // ============================================================
    //  方案一：ORDER BY RANDOM()（数据库原生随机，基线）
    // ============================================================
    [Benchmark(Baseline = true)]
    public async Task<List<string>> OrderByRand()
    {
        var ids = await _dbContext
            .Questions.AsNoTracking()
            .OrderBy(q => EF.Functions.Random()) // SQLite: Random() / PostgreSQL: Random() / SQL Server: NEWID()
            .Take(_extractCount)
            .Select(q => q.Id)
            .ToListAsync();

        return ids;
    }

    // ============================================================
    //  方案二：Fisher-Yates 洗牌（每次从数据库读所有 ID）
    // ============================================================
    [Benchmark]
    public async Task<List<string>> FisherYatesNoCache()
    {
        // 1. 从数据库读取所有 ID
        var allIds = await _dbContext.Questions.AsNoTracking().Select(q => q.Id).ToListAsync();

        // 2. Fisher-Yates 洗牌
        var ids = allIds.ToList(); // 拷贝一份，不破坏原始数据
        for (int i = 0; i < _extractCount; i++)
        {
            int j = i + Random.Shared.Next(ids.Count - i);
            (ids[i], ids[j]) = (ids[j], ids[i]);
        }

        // 3. 取前 _extractCount 个
        return ids.Take(_extractCount).ToList();
    }

    // ============================================================
    //  方案三：Fisher-Yates 洗牌 + ID 缓存（你的亮点）
    // ============================================================
    [Benchmark]
    public async Task<List<string>> FisherYatesWithCache()
    {
        // 确保缓存已加载（_cachedIds 在 GlobalSetup 中已准备好）
        if (_cachedIds == null || !_cachedIds.Any())
        {
            // 兜底：如果缓存没加载，从数据库读取
            _cachedIds = await _dbContext.Questions.AsNoTracking().Select(q => q.Id).ToListAsync();
        }

        // Fisher-Yates 洗牌（使用缓存数据）
        var ids = _cachedIds.ToList(); // 拷贝一份
        for (int i = 0; i < _extractCount; i++)
        {
            int j = i + Random.Shared.Next(ids.Count - i);
            (ids[i], ids[j]) = (ids[j], ids[i]);
        }

        return ids.Take(_extractCount).ToList();
    }
}
