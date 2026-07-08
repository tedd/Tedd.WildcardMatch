using BenchmarkDotNet.Attributes;
using System;
using System.Text.RegularExpressions;

namespace TeddWildcardMatchBenchmark.Tests
{
    [Config(typeof(TestConfig))]
    [MemoryDiagnoser]
    public class StringToWildcardBenchmark
    {
        private const string WildcardShort = "*.txt";
        private const string WildcardComplex = "*hello?world*and?more*stuff?here*";
        private const string WildcardLong = "this*is?a*very?long*wildcard?string*with?many*different?characters*and?symbols*in?it*1*2*3*4*5*6*7*8*9*0*";

        [Benchmark(Baseline = true)]
        public string Archive_Short() => Tedd.Archive.InternalUtils.StringToWildcard(WildcardShort);

        [Benchmark]
        public string Optimized_Short() => Tedd.InternalUtils.StringToWildcard(WildcardShort);

        [Benchmark]
        public string Archive_Complex() => Tedd.Archive.InternalUtils.StringToWildcard(WildcardComplex);

        [Benchmark]
        public string Optimized_Complex() => Tedd.InternalUtils.StringToWildcard(WildcardComplex);

        [Benchmark]
        public string Archive_Long() => Tedd.Archive.InternalUtils.StringToWildcard(WildcardLong);

        [Benchmark]
        public string Optimized_Long() => Tedd.InternalUtils.StringToWildcard(WildcardLong);
    }
}
