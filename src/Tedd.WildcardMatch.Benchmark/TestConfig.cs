using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Toolchains.InProcess.Emit;

namespace TeddWildcardMatchBenchmark
{
    public class TestConfig : ManualConfig
    {
        public TestConfig()
        {
            AddLogger(ConsoleLogger.Default);

            AddJob(Job.Default
                .WithLaunchCount(1)
                .WithGcForce(true)
                .WithToolchain(InProcessEmitToolchain.Instance));

            AddDiagnoser(MemoryDiagnoser.Default);
        }
    }
}
