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
                .WithGcForce(true)
                .WithId("x64 .Net Core 3.1 Ryu")
                .WithPlatform(Platform.X64)
                .WithJit(Jit.RyuJit)
                .WithRuntime(CoreRuntime.Core31));

            AddJob(Job.Default
                .WithLaunchCount(1)
                .WithGcForce(true)
                .WithId("x64 .Net 4.8 Ryu")
                .WithPlatform(Platform.X64)
                .WithJit(Jit.RyuJit)
                .WithRuntime(ClrRuntime.Net48));

            AddJob(Job.Default
                .WithLaunchCount(1)
                .WithGcForce(true)
                .WithId("x64 Mono Llvm")
                .WithPlatform(Platform.X64)
                .WithJit(Jit.Llvm)
                .WithRuntime(MonoRuntime.Default));

            AddColumn(new[] { TargetMethodColumn.Method });
            AddColumn(new[] { new BaselineColumn(), BaselineRatioColumn.RatioMean, BaselineRatioColumn.RatioStdDev });
            AddColumn(new[] { StatisticColumn.StdDev, StatisticColumn.Error, StatisticColumn.Iterations, StatisticColumn.Min, StatisticColumn.Mean, StatisticColumn.Max, StatisticColumn.Median, StatisticColumn.OperationsPerSecond, StatisticColumn.P95, StatisticColumn.P90 });
            AddHardwareCounters(new[] { HardwareCounter.BranchMispredictions, HardwareCounter.BranchInstructions, HardwareCounter.TotalIssues });
            //// HardwareCounter.CacheMisses, HardwareCounter.BranchMispredictsRetired, HardwareCounter.TotalCycles, HardwareCounter.UnhaltedCoreCycles, HardwareCounter.UnhaltedReferenceCycles, HardwareCounter.BranchInstructionRetired, 
            //Add(ThreadingDiagnoser.Default);
            ////Add(new ConcurrencyVisualizerProfiler());
            //Add(new TailCallDiagnoser());
            AddDiagnoser(MemoryDiagnoser.Default);
            //            Add(DisassemblyDiagnoser.Create(new DisassemblyDiagnoserConfig(printAsm: true, printIL: true, printSource: false, printPrologAndEpilog: true, recursiveDepth: 2, printDiff: false)));

            AddAnalyser(EnvironmentAnalyser.Default);
            AddExporter(new[] { RPlotExporter.Default, AsciiDocExporter.Default, CsvExporter.Default, CsvMeasurementsExporter.Default, HtmlExporter.Default, PlainExporter.Default });

        }
    }
}
