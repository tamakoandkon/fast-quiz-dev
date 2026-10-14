using NLog;
using NLog.Config;
using NLog.Targets;
using Shouldly;

namespace FastQuiz.API.Test.Configuration;

[NonParallelizable] // Environment variables are process-wide.
public sealed class LoggingConfigurationTests
{
    [TestCase(null)]
    [TestCase("")]
    public void MissingOverrideKeepsTheDevelopmentLogDirectory(string? value)
    {
        var previous = Environment.GetEnvironmentVariable("FASTQUIZ_LOG_DIR");
        try
        {
            Environment.SetEnvironmentVariable("FASTQUIZ_LOG_DIR", value);
            using var factory = new LogFactory();
            factory.Configuration = new XmlLoggingConfiguration(Path.Combine(AppContext.BaseDirectory, "NLog.config"), factory);
            var target = factory.Configuration.FindTargetByName("allFile") as FileTarget
                ?? throw new InvalidOperationException("allFile target missing");
            target.FileName.Render(new LogEventInfo()).ShouldStartWith(Path.Combine(AppContext.BaseDirectory, "logs") + Path.DirectorySeparatorChar);
        }
        finally { Environment.SetEnvironmentVariable("FASTQUIZ_LOG_DIR", previous); }
    }

    [Test]
    public void OverrideWritesAllSystemAndApiFilesOutsideTheReleaseDirectory()
    {
        var previous = Environment.GetEnvironmentVariable("FASTQUIZ_LOG_DIR");
        var directory = Path.Combine(Path.GetTempPath(), "fastquiz-logs-" + Guid.NewGuid().ToString("N"));
        try
        {
            Environment.SetEnvironmentVariable("FASTQUIZ_LOG_DIR", directory);
            using (var factory = new LogFactory())
            {
                factory.Configuration = new XmlLoggingConfiguration(Path.Combine(AppContext.BaseDirectory, "NLog.config"), factory);
                factory.GetLogger("FastQuiz.API.LoggingProbe").Info("file-log-probe");
                factory.GetLogger("Microsoft.LoggingProbe").Info("system-log-probe");
                factory.Flush();
            }
            var all = Directory.GetFiles(directory, "all.log", SearchOption.AllDirectories).Single();
            File.ReadAllText(all).ShouldContain("file-log-probe");
            File.ReadAllText(all).ShouldContain("system-log-probe");
            File.ReadAllText(Directory.GetFiles(directory, "api.log", SearchOption.AllDirectories).Single()).ShouldContain("file-log-probe");
            File.ReadAllText(Directory.GetFiles(directory, "system.log", SearchOption.AllDirectories).Single()).ShouldContain("system-log-probe");
        }
        finally
        {
            Environment.SetEnvironmentVariable("FASTQUIZ_LOG_DIR", previous);
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }
    }
}
