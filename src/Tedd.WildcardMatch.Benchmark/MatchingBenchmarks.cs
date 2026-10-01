using BenchmarkDotNet.Attributes;

namespace Tedd.WildcardMatchBenchmark;

[MemoryDiagnoser]
[JsonExporterAttribute.Full]
public class MatchingBenchmarks
{
    public IEnumerable<ComparisonCase> Cases => Comparison.ValidCases();
    [ParamsSource(nameof(Cases))]
    public ComparisonCase Case { get; set; } = null!;
    private string[] _inputs = null!;
    private Func<string, bool> _match = null!;

    [GlobalSetup]
    public void Setup()
    {
        var fixture = Comparison.Fixtures.Single(f => f.Name == Case.Workload);
        _inputs = fixture.Inputs;
        _match = Comparison.CreateMatcher(Case.Library, fixture.Pattern);
        foreach (string input in _inputs)
            if (_match(input) != Comparison.ExpectedMatch(input, fixture.Pattern))
                throw new InvalidOperationException($"Invalid benchmark case: {Case}");
    }

    // Every adapter incurs delegate dispatch. BDN reports time and allocations per match.
    [Benchmark(OperationsPerInvoke = Comparison.BatchSize)]
    public int Match()
    {
        int matches = 0;
        foreach (string input in _inputs)
            if (_match(input)) matches++;
        return matches;
    }
}
