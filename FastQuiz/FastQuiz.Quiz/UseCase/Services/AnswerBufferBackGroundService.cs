using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FastQuiz.Quiz.UseCase.Services;

public class AnswerBufferBackgroundService(
    IServiceProvider _serviceProvider,
    ILogger<AnswerBufferBackgroundService> _logger
) : BackgroundService
{
    // ===== 功能 =====
    // 定时触发刷出，保证数据最终一致性

    // ===== 为什么这样做 =====
    // 1. BackgroundService：ASP.NET Core标准后台任务
    // 2. 定时检查：每秒检查一次
    // 3. 优雅关闭：应用停止时刷出剩余数据


    private readonly int _checkIntervalSeconds = 1;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(_checkIntervalSeconds * 1000, stoppingToken);

            using var s = _serviceProvider.CreateScope();
            var b = s.ServiceProvider.GetRequiredService<IAnswerBufferService>();

            await b.CheckAndFlushAsync();
        }

        // 应用关闭时强制刷出
        using var scope = _serviceProvider.CreateScope();
        var bufferService = scope.ServiceProvider.GetRequiredService<IAnswerBufferService>();
        await bufferService.FlushAsync();
    }
}
