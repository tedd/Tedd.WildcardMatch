using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;

namespace Tedd.WildcardMatchBenchmark;

[MemoryDiagnoser]
[JsonExporterAttribute.Full]
public class InteropBenchmarks
{
    [Params(false, true)]
    public bool ShareInputs { get; set; }
    public IEnumerable<ComparisonCase> Cases => string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WILDCARD_NATIVE_LIBRARY"))
        ? [] : Comparison.Fixtures.Where(f => f.Name != "LongText")
            .SelectMany(f => NativeMatcher.Libraries.Where(l => l != "Pcre2Jit").Select(l => new ComparisonCase(f.Name, l)));
    [ParamsSource(nameof(Cases))]
    public ComparisonCase Case { get; set; } = null!;
    private NativeMatcher _matcher = null!;
    [GlobalSetup]
    public void Setup()
    {
        var fixture = Comparison.Fixtures.Single(f => f.Name == Case.Workload);
        _matcher = new NativeMatcher(Case.Library, fixture.Pattern, fixture.Inputs, ShareInputs);
        _matcher.Validate(fixture);
    }
    [GlobalCleanup]
    public void Cleanup() => _matcher.Dispose();
    [Benchmark(OperationsPerInvoke = Comparison.BatchSize)]
    public int Normal() => _matcher.MatchEach();
    [Benchmark(OperationsPerInvoke = Comparison.BatchSize)]
    public int Suppressed() => _matcher.MatchEachSuppressed();
}

[JsonExporterAttribute.Full]
public unsafe class InteropControlBenchmarks
{
    private nint _library;
    private delegate* unmanaged[Cdecl]<int, int> _normal;
    private delegate* unmanaged[Cdecl, SuppressGCTransition]<int, int> _suppressed;
    [GlobalSetup]
    public void Setup()
    {
        _library = NativeLibrary.Load(Environment.GetEnvironmentVariable("WILDCARD_NATIVE_LIBRARY")
            ?? throw new InvalidOperationException("Set WILDCARD_NATIVE_LIBRARY."));
        nint function = NativeLibrary.GetExport(_library, "interop_control");
        _normal = (delegate* unmanaged[Cdecl]<int, int>)function;
        _suppressed = (delegate* unmanaged[Cdecl, SuppressGCTransition]<int, int>)function;
        if (_normal(17) != 17 || _suppressed(17) != 17) throw new InvalidOperationException("Interop control failed.");
    }
    [GlobalCleanup]
    public void Cleanup() => NativeLibrary.Free(_library);
    [Benchmark(OperationsPerInvoke = Comparison.BatchSize)]
    public int Normal()
    {
        int sum = 0;
        for (int i = 0; i < Comparison.BatchSize; i++) sum += _normal(i);
        return sum;
    }
    [Benchmark(OperationsPerInvoke = Comparison.BatchSize)]
    public int Suppressed()
    {
        int sum = 0;
        for (int i = 0; i < Comparison.BatchSize; i++) sum += _suppressed(i);
        return sum;
    }
}
