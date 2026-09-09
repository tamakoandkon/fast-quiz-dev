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
    private readonly SemaphoreSlim _flushSemaphore = new(1, 1); // 控制并发刷出
    private int _isFlushing = 0; // 0=空闲, 1=刷出中或等待中

    // ============ 3. 配置参数 ============
    private readonly int _maxBatchSize = 100; // 最大批量条数
    private readonly int _maxBufferSize = 10000;
    private readonly int _flushDelayMs = 50; // 延迟50ms，等待更多数据聚合

    // ============ 4. 状态追踪 ============
    private DateTimeOffset _lastFlushTime = DateTimeOffset.UtcNow;
    private readonly Timer _timer; // 兜底定时器
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
        _timer = new Timer(_ => _ = CheckAndFlushAsync(), null, 5000, 5000);
    }

    public void AddAnswer(PendingAnswer answer)
    {
        // 写入缓冲区（覆盖写）
        _writeBuffer[answer.QuestionRecordId] = answer;

        logger.LogDebug("添加答案，当前缓冲区大小: {Count}", _writeBuffer.Count);

        // 尝试触发刷出（非阻塞）
        TryTriggerFlush();
    }

    /// <summary>
    /// 尝试触发刷出任务
    /// 如果已有刷出任务在运行，则不重复触发
    /// </summary>
    private void TryTriggerFlush()
    {
        // 如果已经有刷出任务在运行，直接返回
        if (Interlocked.CompareExchange(ref _isFlushing, 1, 0) != 0)
        {
            logger.LogDebug("已有刷出任务在执行，跳过触发");
            return;
        }

        // 异步启动刷出循环（使用Task.Run避免阻塞调用线程）
        _ = Task.Run(async () => await FlushLoopAsync());
    }

    /// <summary>
    /// 刷出循环 - 使用循环代替递归，避免栈溢出
    /// </summary>
    private async Task FlushLoopAsync()
    {
        // 防止无限循环的安全计数器（极端情况下的保护）
        const int maxLoopIterations = 10000;
        int iterationCount = 0;

        try
        {
            while (iterationCount++ < maxLoopIterations)
            {
                // 1. 检查是否有数据
                if (_writeBuffer.IsEmpty)
                {
                    logger.LogTrace("缓冲区为空，退出刷出循环");
                    break;
                }

                // 2. 延迟等待，聚合更多数据（提高批量效率）
                await Task.Delay(_flushDelayMs);

                // 3. 再次检查数据（可能在延迟期间被处理完了）
                if (_writeBuffer.IsEmpty)
                {
                    logger.LogTrace("延迟后缓冲区为空，退出刷出循环");
                    break;
                }

                // 4. 检查缓冲区是否溢出
                if (_writeBuffer.Count > _maxBufferSize)
                {
                    logger.LogWarning(
                        "缓冲区超过限制 {Count}/{MaxBufferSize}，紧急刷出",
                        _writeBuffer.Count,
                        _maxBufferSize
                    );
                }

                // 5. 执行刷出
                var count = _writeBuffer.Count;
                logger.LogDebug("执行批量刷出，{Count}条数据", count);

                await FlushAsync();

                // 6. 如果刷出后还有数据，继续循环
                if (_writeBuffer.IsEmpty)
                {
                    logger.LogTrace("刷出完成，缓冲区已清空");
                    break;
                }

                // 注意：这里使用 while 循环而不是递归
                // 即使持续运行1小时，也不会导致栈溢出
                // 因为每次迭代都在同一个栈帧中执行
            }

            // 如果达到最大循环次数，记录警告
            if (iterationCount >= maxLoopIterations)
            {
                logger.LogWarning(
                    "刷出循环达到最大迭代次数 {MaxIterations}，强制退出",
                    maxLoopIterations
                );
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "刷出循环发生异常");
        }
        finally
        {
            // 释放刷出状态
            Interlocked.Exchange(ref _isFlushing, 0);

            // 如果还有数据，重新触发刷出
            if (!_writeBuffer.IsEmpty)
            {
                logger.LogDebug("循环结束后仍有数据 {Count}条，重新触发刷出", _writeBuffer.Count);
                TryTriggerFlush();
            }
            else
            {
                logger.LogTrace("刷出循环正常结束");
            }
        }
    }

    public async Task FlushAsync()
    {
        // ===== 功能：实际刷出逻辑 =====

        if (_writeBuffer.IsEmpty)
            return;

        // 取出当前所有数据（原子操作）
        var bufferToFlush = Interlocked.Exchange(
            ref _writeBuffer,
            new ConcurrentDictionary<string, PendingAnswer>()
        );

        if (bufferToFlush.IsEmpty)
            return;

        _lastFlushTime = DateTimeOffset.UtcNow;
        logger.LogInformation("开始刷出 {Count} 条数据", bufferToFlush.Count);

        try
        {
            await PersistBufferAsync(bufferToFlush);
            logger.LogInformation("刷出成功，{Count}条", bufferToFlush.Count);
        }
        catch (Exception ex)
        {
            // ⚠️ 数据丢失风险：如果持久化失败，数据已从缓冲区移除
            logger.LogError(ex, "刷出失败，丢失{Count}条数据", bufferToFlush.Count);

            // TODO: 可以考虑将失败数据放入重试队列或死信队列
            // 例如：_deadLetterQueue.AddRange(bufferToFlush.Values);
        }
    }

    private async Task PersistBufferAsync(ConcurrentDictionary<string, PendingAnswer> buffer)
    {
        if (buffer.IsEmpty)
            return;

        var items = buffer.Values.ToList();

        using var scope = scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IQuestionRecordRepository>();

        // 批量删除旧答案
        var questionRecordIds = items.Select(x => x.QuestionRecordId).Distinct().ToList();
        await repository.DeleteAnswerRecordByQuestionRecordIdAsync(questionRecordIds);

        // 批量插入新答案
        var answerRecords = items
            .Select(x => new AnswerRecord
            {
                Id = idGenerator.GenerateUlId(),
                QuestionRecordId = x.QuestionRecordId,
                QuizAttemptId = x.QuizAttemptId,
                Value = x.Value,
            })
            .ToList();
        await repository.CreateAnswerRecordAsync(answerRecords);
    }

    public async Task CheckAndFlushAsync()
    {
        // 供后台任务和Timer调用
        TryTriggerFlush();

        // 如果缓冲区超过限制，强制同步刷出
        if (_writeBuffer.Count > _maxBufferSize)
        {
            logger.LogWarning("缓冲区超过限制，强制同步刷出");
            await FlushAsync();
        }
    }
}
