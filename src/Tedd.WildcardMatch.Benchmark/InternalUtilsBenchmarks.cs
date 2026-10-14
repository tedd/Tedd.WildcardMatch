using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.InProcess.Emit;

namespace Tedd.WildcardMatchBenchmark;

[Config(typeof(Config))]
[MemoryDiagnoser]
public class InternalUtilsBenchmarks
{
    private class Config : ManualConfig
    {
        public Config()
        {
            AddJob(Job.ShortRun.WithToolchain(InProcessEmitToolchain.Instance));
        }
    }

    [Params("hello", "hello*", "*hello", "*hello*", "h?llo", "a*b?c*d", "averylongstringwithnowildcardsbutitsescaped1234567890()[]{}!@#$%", "averylongstringwithnowildcardsbutitsescaped1234567890()[]{}!@#$%averylongstringwithnowildcardsbutitsescaped1234567890()[]{}!@#$%", "*averylongstringwithwildcardsbutitsescaped1234567890()[]{}!@#$%*", "averylongstringwithnowildcardsbutitsescaped1234567890()[]{}!@#$%averylongstringwithnowildcardsbutitsescaped1234567890()[]{}!@#$%averylongstringwithnowildcardsbutitsescaped1234567890()[]{}!@#$%")]
    public string Pattern { get; set; } = null!;

    [Benchmark(Baseline = true)]
    public string Archive() => global::Tedd.Archive.InternalUtils.StringToWildcard(Pattern);

    [Benchmark]
    public string Optimized() => global::Tedd.InternalUtils.StringToWildcard(Pattern);
}
