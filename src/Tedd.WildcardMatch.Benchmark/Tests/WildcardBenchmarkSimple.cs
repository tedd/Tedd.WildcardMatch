using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.Windows.Configs;
using BenchmarkDotNet.Order;
using Tedd;

namespace TeddWildcardMatchBenchmark.Tests
{
    [Config(typeof(TeddWildcardMatchBenchmark.TestConfig))]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [MemoryDiagnoser]
    public class WildcardBenchmarkSimple
    {
        [GlobalSetup]
        public void Setup()
        {
        }

        [Benchmark(Description = "Tedd:Simple,Slow")]
        public void TestTeddSimpleSlow()
        {
            for (int i = 0; i < 1000; i++)
            {
                var isMatch = Tedd.WildcardMatch.IsMatch("abcd", "*a?c*");
            }
        }

        [Benchmark(Description = "Archive:Simple,Slow")]
        public void TestArchiveSimpleSlow()
        {
            for (int i = 0; i < 1000; i++)
            {
                var isMatch = System.Text.RegularExpressions.Regex.IsMatch("abcd", Tedd.Archive.InternalUtilsArchive.StringToWildcard("*a?c*"));
            }
        }

        [Benchmark(Description = "Tedd:Simple,Precompiled")]
        public void TestTeddSimplePrecompiled()
        {
            var match = new Tedd.WildcardMatch("*a?c*", WildcardOptions.Compiled);
            for (int i = 0; i < 1000; i++)
            {
                var isMatch = match.IsMatch("abcd");
            }
        }

        [Benchmark(Description = "FastWildcard:Simple")]
        public void TestFastWildcardSimple()
        {
            for (int i = 0; i < 1000; i++)
            {
                var isMatch = FastWildcard.FastWildcard.IsMatch("abcd", "*a?c*");
            }
        }
    }
}
