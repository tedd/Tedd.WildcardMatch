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

namespace TeddWildcardMatchBenchmark
{
    public class TestConfig : ManualConfig
    {
        public TestConfig()
        {
            AddLogger(ConsoleLogger.Default);
            AddJob(Job.Default.WithToolchain(InProcessEmitToolchain.Instance));
            AddColumn(new[] { TargetMethodColumn.Method });
            AddColumn(new[] { new BaselineColumn(), BaselineRatioColumn.RatioMean, BaselineRatioColumn.RatioStdDev });
            AddColumn(new[] { StatisticColumn.StdDev, StatisticColumn.Error, StatisticColumn.Iterations, StatisticColumn.Min, StatisticColumn.Mean, StatisticColumn.Max, StatisticColumn.Median, StatisticColumn.OperationsPerSecond, StatisticColumn.P95, StatisticColumn.P90 });
            AddDiagnoser(MemoryDiagnoser.Default);
            AddAnalyser(EnvironmentAnalyser.Default);
            AddExporter(new[] { RPlotExporter.Default, AsciiDocExporter.Default, CsvExporter.Default, CsvMeasurementsExporter.Default, HtmlExporter.Default, PlainExporter.Default });
        }
    }
}
