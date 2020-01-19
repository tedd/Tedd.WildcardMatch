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
            Add(ConsoleLogger.Default);

            Add(Job.Default
                .WithLaunchCount(1)
                .WithGcForce(true)
                //.WithId("OutOfProc")
                .With(Platform.X64)
                .With(Jit.RyuJit)
                .With(CoreRuntime.Core31));

            Add(new[] { TargetMethodColumn.Method });
            Add(new[] { new BaselineColumn(), BaselineRatioColumn.RatioMean, BaselineRatioColumn.RatioStdDev });
            Add(new[] { StatisticColumn.StdDev, StatisticColumn.Error, StatisticColumn.Iterations, StatisticColumn.Min, StatisticColumn.Mean, StatisticColumn.Max, StatisticColumn.Median, StatisticColumn.OperationsPerSecond, StatisticColumn.P95, StatisticColumn.P90 });
            Add(new[] { HardwareCounter.BranchMispredictions, HardwareCounter.BranchInstructions, HardwareCounter.TotalIssues });
            //// HardwareCounter.CacheMisses, HardwareCounter.BranchMispredictsRetired, HardwareCounter.TotalCycles, HardwareCounter.UnhaltedCoreCycles, HardwareCounter.UnhaltedReferenceCycles, HardwareCounter.BranchInstructionRetired, 
            //Add(ThreadingDiagnoser.Default);
            ////Add(new ConcurrencyVisualizerProfiler());
            //Add(new TailCallDiagnoser());
            Add(MemoryDiagnoser.Default);
            //            Add(DisassemblyDiagnoser.Create(new DisassemblyDiagnoserConfig(printAsm: true, printIL: true, printSource: false, printPrologAndEpilog: true, recursiveDepth: 2, printDiff: false)));

            Add(EnvironmentAnalyser.Default);
            Add(new[] { RPlotExporter.Default, AsciiDocExporter.Default, CsvExporter.Default, CsvMeasurementsExporter.Default, HtmlExporter.Default, PlainExporter.Default });

        }
    }
}
