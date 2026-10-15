using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;

namespace Tedd.WildcardMatchBenchmark;

[MemoryDiagnoser]
[JsonExporterAttribute.Full]
public class NativeMatchingBenchmarks
{
    public IEnumerable<ComparisonCase> Cases => string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WILDCARD_NATIVE_LIBRARY"))
        ? [] : Comparison.Fixtures.SelectMany(f => NativeMatcher.Libraries.Select(l => new ComparisonCase(f.Name, l)));
    [ParamsSource(nameof(Cases))]
    public ComparisonCase Case { get; set; } = null!;
    private NativeMatcher _matcher = null!;

    [GlobalSetup]
    public void Setup()
    {
        var fixture = Comparison.Fixtures.Single(f => f.Name == Case.Workload);
        _matcher = new NativeMatcher(Case.Library, fixture.Pattern, fixture.Inputs);
        _matcher.Validate(fixture);
    }

    [GlobalCleanup]
    public void Cleanup() => _matcher.Dispose();

    [Benchmark(OperationsPerInvoke = Comparison.BatchSize)]
    public int Match() => _matcher.Match();
}

// All encoding, allocation and pattern preparation happens outside timed work.
public sealed unsafe class NativeMatcher : IDisposable
{
    public static readonly string[] Libraries = ["Krauss", "GitWildmatch", "ZemaWildcards", "Pcre2Jit"];
    private readonly nint _library;
    private readonly nint _context;
    private readonly delegate* unmanaged[Cdecl]<nint, int, int> _match;
    private readonly delegate* unmanaged[Cdecl]<nint, int, int> _single;
    private readonly delegate* unmanaged[Cdecl]<nint, void> _destroy;

    public NativeMatcher(string library, string pattern, string[] inputs)
    {
        int index = Array.IndexOf(Libraries, library);
        if (index < 0) throw new ArgumentOutOfRangeException(nameof(library));
        _library = NativeLibrary.Load(Environment.GetEnvironmentVariable("WILDCARD_NATIVE_LIBRARY")
            ?? throw new InvalidOperationException("Set WILDCARD_NATIVE_LIBRARY to the absolute benchmark DLL path."));
        _match = (delegate* unmanaged[Cdecl]<nint, int, int>)NativeLibrary.GetExport(_library, "match_batch");
        _single = (delegate* unmanaged[Cdecl]<nint, int, int>)NativeLibrary.GetExport(_library, "match_one");
        _destroy = (delegate* unmanaged[Cdecl]<nint, void>)NativeLibrary.GetExport(_library, "destroy_context");
        var create = (delegate* unmanaged[Cdecl]<int, byte*, byte**, int, nint>)NativeLibrary.GetExport(_library, "create_context");
        nint p = 0;
        var strings = new nint[inputs.Length];
        try
        {
            if (pattern.Any(c => c > 127 || c == 0) || inputs.Any(s => s.Any(c => c > 127 || c == 0)))
                throw new ArgumentException("Native comparison requires NUL-free ASCII.");
            p = Marshal.StringToCoTaskMemUTF8(pattern);
            for (int i = 0; i < inputs.Length; i++) strings[i] = Marshal.StringToCoTaskMemUTF8(inputs[i]);
            fixed (nint* values = strings) _context = create(index, (byte*)p, (byte**)values, inputs.Length);
            if (_context == 0) throw new InvalidOperationException($"Native setup failed: {library}");
        }
        catch { NativeLibrary.Free(_library); throw; }
        finally
        {
            Marshal.FreeCoTaskMem(p);
            foreach (nint s in strings) Marshal.FreeCoTaskMem(s);
        }
    }

    public int Match() => _match(_context, Comparison.BatchSize);

    public void Validate(Fixture fixture)
    {
        for (int i = 0; i < fixture.Inputs.Length; i++)
            if ((_single(_context, i) != 0) != Comparison.ExpectedMatch(fixture.Inputs[i], fixture.Pattern))
                throw new InvalidOperationException($"Native correctness failure: {fixture.Name}, input {i}");
        int expected = fixture.Inputs.Count(s => Comparison.ExpectedMatch(s, fixture.Pattern));
        if (_match(_context, fixture.Inputs.Length) != expected)
            throw new InvalidOperationException("Native batch correctness failure.");
    }

    public void Dispose()
    {
        _destroy(_context);
        NativeLibrary.Free(_library);
    }

    public static object ValidateAll()
    {
        var results = new List<object>();
        foreach (string library in Libraries)
        {
            foreach (var fixture in Comparison.Fixtures)
            {
                using var matcher = new NativeMatcher(library, fixture.Pattern, fixture.Inputs);
                matcher.Validate(fixture);
                results.Add(new { Library = library, Workload = fixture.Name, Checked = fixture.Inputs.Length, Failures = 0 });
            }
            var random = new Random(73129);
            for (int i = 0; i < 1000; i++)
            {
                string pattern = new(Enumerable.Range(0, random.Next(12)).Select(_ => "abc*?"[random.Next(5)]).ToArray());
                string[] inputs = Enumerable.Range(0, 16).Select(_ => new string(Enumerable.Range(0, random.Next(20)).Select(_ => "abc"[random.Next(3)]).ToArray())).ToArray();
                var fixture = new Fixture("SeededAscii", pattern, inputs);
                using var matcher = new NativeMatcher(library, pattern, inputs);
                matcher.Validate(fixture);
            }
            results.Add(new { Library = library, Workload = "SeededAscii", Checked = 16000, Failures = 0 });
        }
        return results;
    }
}
