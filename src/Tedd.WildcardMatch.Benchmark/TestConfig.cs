using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Toolchains.InProcess.Emit;
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
                .WithToolchain(InProcessEmitToolchain.Instance)
                .WithLaunchCount(1)
                .WithGcForce(true)
                .WithId("x64 .Net 10.0")
                .WithPlatform(Platform.X64));

            AddColumn(TargetMethodColumn.Method);
            AddColumn(StatisticColumn.StdDev, StatisticColumn.Error, StatisticColumn.Mean, StatisticColumn.Median);

            AddDiagnoser(MemoryDiagnoser.Default);

            AddAnalyser(EnvironmentAnalyser.Default);
            AddExporter(HtmlExporter.Default, PlainExporter.Default);
        }
    }
}
