using System.Reflection;
using BenchmarkDotNet.Running;

namespace FastQuiz.Question.BenckMark;

internal class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            // 没有参数：全量跑
            Console.WriteLine("🚀 运行所有 Benchmark...");
            BenchmarkRunner.Run(Assembly.GetExecutingAssembly());
        }
        else if (args[0] == "--list")
        {
            // 列出所有 Benchmark
            Console.WriteLine("📋 可用的 Benchmark 过滤关键字：");
            Console.WriteLine("  Random   - 随机抽题相关");
        }
        else
        {
            // 有参数：只跑匹配的
            var filter = args[0];
            Console.WriteLine($"🔍 只跑包含 '{filter}' 的 Benchmark...");
            BenchmarkRunner.Run(
                Assembly.GetExecutingAssembly(),
                args: new[] { "--filter", $"*{filter}*" }
            );
        }
    }
}
