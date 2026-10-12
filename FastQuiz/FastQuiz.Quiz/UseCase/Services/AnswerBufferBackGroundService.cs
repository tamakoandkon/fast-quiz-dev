using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FastQuiz.Quiz.UseCase.Services;

public class AnswerBufferBackgroundService(
    IAnswerBufferService _buffer,
    ILogger<AnswerBufferBackgroundService> _logger
) : BackgroundService
{
    private readonly TimeSpan _interval = TimeSpan.FromMilliseconds(1000);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_interval);

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    await _buffer.FlushAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "后台刷出发生异常");
                }
            }
        }
        catch (OperationCanceledException)
        {
            // 正常关闭，忽略
        }

        // 应用关闭时刷出剩余数据
        try
        {
            await _buffer.FlushAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "关闭时刷出剩余数据失败");
        }
    }
}
