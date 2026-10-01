using BenchmarkDotNet.Attributes;
using DotNet.Globbing;

namespace Tedd.WildcardMatchBenchmark;

[MemoryDiagnoser]
[JsonExporterAttribute.Full]
public class ConstructionBenchmarks
{
    [Params("report-??.txt", "*a?c*e*")]
    public string Pattern { get; set; } = null!;

    [Benchmark(Baseline = true)]
    public object TeddReused() => new global::Tedd.WildcardMatch(Pattern);

    [Benchmark]
    public object TeddCompiled() => new global::Tedd.WildcardMatch(Pattern, global::Tedd.WildcardOptions.Compiled);

    [Benchmark]
    public object DotNetGlob() => Glob.Parse(Pattern);
}
