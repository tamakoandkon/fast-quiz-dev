using System.Collections.Concurrent;
using FastQuiz.Common.Utils;
using FastQuiz.Quiz.Domain.Entities;
using FastQuiz.Quiz.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FastQuiz.Quiz.UseCase.Services;

public class AnswerBufferService : IAnswerBufferService
{
    // ============ 1. 核心数据 ============
    private ConcurrentDictionary<string, PendingAnswer> _writeBuffer = new();

    // ============ 2. 同步控制 ============
    // 只允许一个刷出任务在执行；TryAcquire 失败说明已有刷出在跑
    private readonly SemaphoreSlim _flushSemaphore = new(1, 1);
    private readonly object _bufferLock = new();

    // ============ 3. 配置参数 ============
    private readonly int _maxBufferSize = 10000;

    // ============ 4. 状态追踪 ============
    private long _pendingCount = 0; // 独立计数，避免 ConcurrentDictionary.Count 的 O(n)
    private readonly IServiceScopeFactory scopeFactory;
    private readonly IIdGenerator idGenerator;
    private readonly ILogger<AnswerBufferService> logger;

    public AnswerBufferService(
        IServiceScopeFactory _scopeFactory,
        IIdGenerator _idGenerator,
        ILogger<AnswerBufferService> _logger
    )
    {
        scopeFactory = _scopeFactory;
        idGenerator = _idGenerator;
        logger = _logger;
    }

    /// <summary>
    /// 请求侧唯一入口：只写内存，不触发刷出，不碰数据库。
    /// </summary>
    public void SetAnswers(PendingAnswer answer)
    {
        lock (_bufferLock)
        {
            if (_writeBuffer.TryAdd(answer.QuestionRecordId, answer))
            {
                Interlocked.Increment(ref _pendingCount);
            }
            else
            {
                _writeBuffer[answer.QuestionRecordId] = answer;
            }
        }

        if (Interlocked.Read(ref _pendingCount) > _maxBufferSize)
        {
            // 缓冲区超限时只记一条警告，刷出交给后台服务
            logger.LogWarning(
                "缓冲区超过限制 {Count}/{MaxBufferSize}",
                Interlocked.Read(ref _pendingCount),
                _maxBufferSize
            );
        }
    }

    /// <summary>
    /// 刷出：由后台服务按固定节奏调用。
    /// 用 SemaphoreSlim(0) 做非阻塞互斥，避免并发刷出。
    /// </summary>
    public async Task FlushAsync(CancellationToken cancellationToken = default)
    {
        // 拿不到锁说明已有刷出在跑，直接跳过本轮
        if (!await _flushSemaphore.WaitAsync(0, cancellationToken))
        {
            logger.LogDebug("已有刷出任务在执行，跳过本轮");
            return;
        }

        try
        {
            ConcurrentDictionary<string, PendingAnswer> bufferToFlush;
            lock (_bufferLock)
            {
                if (_writeBuffer.IsEmpty)
                    return;

                bufferToFlush = _writeBuffer;
                _writeBuffer = new ConcurrentDictionary<string, PendingAnswer>();
                Interlocked.Exchange(ref _pendingCount, 0);
            }

            if (bufferToFlush.IsEmpty)
                return;

            logger.LogInformation("开始刷出 {Count} 条数据", bufferToFlush.Count);

            try
            {
                await PersistBufferAsync(bufferToFlush, cancellationToken);
                logger.LogInformation("刷出成功，{Count}条", bufferToFlush.Count);
            }
            catch (Exception ex)
            {
                // 刷出失败：把数据合并回缓冲区，等下一轮重试，避免丢失
                MergeBack(bufferToFlush);
                logger.LogError(ex, "刷出失败，{Count}条数据已回滚到缓冲区", bufferToFlush.Count);
            }
        }
        finally
        {
            _flushSemaphore.Release();
        }
    }

    /// <summary>
    /// 刷出失败时把数据放回缓冲区。刷出期间收到的新答案优先，
    /// 避免旧快照覆盖用户随后提交的答案。
    /// </summary>
    private void MergeBack(ConcurrentDictionary<string, PendingAnswer> buffer)
    {
        lock (_bufferLock)
        {
            foreach (var kv in buffer)
            {
                if (_writeBuffer.TryAdd(kv.Key, kv.Value))
                {
                    Interlocked.Increment(ref _pendingCount);
                }
            }
        }
    }

    private async Task PersistBufferAsync(
        ConcurrentDictionary<string, PendingAnswer> buffer,
        CancellationToken cancellationToken
    )
    {
        if (buffer.IsEmpty)
            return;

        var items = buffer.Values.ToList();

        using var scope = scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IQuestionRecordRepository>();

        var questionRecordIds = items.Select(x => x.QuestionRecordId).Distinct().ToList();
        await repository
            .DeleteAnswerRecordByQuestionRecordIdAsync(questionRecordIds)
            .ConfigureAwait(false);

        var answerRecords = items
            .SelectMany(x =>
                x.Values.Select(value => new AnswerRecord
                {
                    Id = idGenerator.GenerateUlId(),
                    QuestionRecordId = x.QuestionRecordId,
                    QuizAttemptId = x.QuizAttemptId,
                    Value = value,
                })
            )
            .ToList();

        if (answerRecords.Count > 0)
        {
            await repository.CreateAnswerRecordAsync(answerRecords).ConfigureAwait(false);
        }
    }
}
