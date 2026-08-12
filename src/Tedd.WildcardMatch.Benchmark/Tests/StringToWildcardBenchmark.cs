using BenchmarkDotNet.Attributes;
using System;

namespace TeddWildcardMatchBenchmark.Tests
{
    [Config(typeof(TestConfig))]
    public class StringToWildcardBenchmark
    {
        private const string _pattern = "*some?complex*pattern?to*test*";

        [Benchmark(Baseline = true, Description = "Archive: StringToWildcard")]
        public string Archive_StringToWildcard()
        {
            return Tedd.Archive.InternalUtils.StringToWildcard(_pattern);
        }

        [Benchmark(Description = "Optimized: StringToWildcard")]
        public string Optimized_StringToWildcard()
        {
            // Internal class is not visible, but we can benchmark indirectly, or since we made Tedd.Archive public we could make InternalUtils public.
            // Wait, Tedd.Archive.InternalUtils is public, but Tedd.InternalUtils is internal.
            // In the Benchmark project, Tedd internal types are visible because of [assembly: InternalsVisibleTo("Tedd.WildcardMatch.Benchmark")].
            return Tedd.InternalUtils.StringToWildcard(_pattern);
        }
    }
}
