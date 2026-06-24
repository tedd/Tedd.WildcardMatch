using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using System;
using System.Collections.Generic;
using System.Text;

namespace TeddWildcardMatchBenchmark
{
    public class TestConfig : ManualConfig
    {
        public TestConfig()
        {
            AddLogger(ConsoleLogger.Default);

            AddJob(Job.Default
                .WithLaunchCount(1)
                .WithWarmupCount(3)
                .WithIterationCount(5)
                .WithId("x64 .Net 10.0")
                .WithPlatform(Platform.X64)
                .WithJit(Jit.RyuJit)
                .WithRuntime(CoreRuntime.CreateForNewVersion("net10.0", "net10.0")));

            AddColumn(new[] { TargetMethodColumn.Method });
            AddColumn(new[] { StatisticColumn.Mean, StatisticColumn.StdDev });
            AddDiagnoser(MemoryDiagnoser.Default);

            AddExporter(HtmlExporter.Default, PlainExporter.Default);
        }
    }
}
