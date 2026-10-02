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
    public object TeddDirectReused() => new global::Tedd.WildcardMatch(Pattern);

    [Benchmark]
    public object TeddRegexReused() => new global::Tedd.WildcardMatchRegex(Pattern);

    [Benchmark]
    public object TeddRegexCompiled() => new global::Tedd.WildcardMatchRegex(Pattern, global::Tedd.WildcardOptions.Compiled);

    [Benchmark]
    public object DotNetGlob() => Glob.Parse(Pattern);
}
