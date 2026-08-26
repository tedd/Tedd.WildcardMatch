using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess.Emit;

namespace TeddWildcardMatchBenchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = DefaultConfig.Instance
                .AddJob(Job.Default.WithToolchain(InProcessEmitToolchain.Instance));

            BenchmarkRunner.Run<TranspileBenchmark>(config);
        }
    }
}
