using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace TeddWildcardMatchBenchmark.Tests
{
    [Config(typeof(TestConfig))]
    public class StringToWildcardBenchmark
    {
        private string _simpleString = "*a?b";
        private string _complexString = "*a?b\\*c[d]e+f(g)*a?b\\*c[d]e+f(g)*a?b\\*c[d]e+f(g)";

        [Benchmark(Baseline = true)]
        public string Archive_Simple() => Tedd.Archive.InternalUtils.StringToWildcard(_simpleString);

        [Benchmark]
        public string Optimized_Simple() => Tedd.InternalUtils.StringToWildcard(_simpleString);

        [Benchmark]
        public string Archive_Complex() => Tedd.Archive.InternalUtils.StringToWildcard(_complexString);

        [Benchmark]
        public string Optimized_Complex() => Tedd.InternalUtils.StringToWildcard(_complexString);
    }
}
