using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.Windows.Configs;
using BenchmarkDotNet.Order;
using System.Text.RegularExpressions;
using Tedd;

namespace TeddWildcardMatchBenchmark.Tests
{

    [Config(typeof(TeddWildcardMatchBenchmark.TestConfig))]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [MemoryDiagnoser]
    //[NativeMemoryProfiler]
    //[EtwProfiler]
    public class WildcardBenchmark
    {
        private string _lorem = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";


        [GlobalSetup]
        public void Setup()
        {
        }


        [Benchmark(Description = "Tedd:Complex,Slow")]
        public void TestTeddComplexSlow()
        {
            for (int i = 0; i < 1000; i++)
            {
                var isMatch = Tedd.WildcardMatch.IsMatch(_lorem, "*ipsum*ad??in*sit*");
            }
        }

        [Benchmark(Description = "Archive:Complex,Slow")]
        public void TestArchiveComplexSlow()
        {
            for (int i = 0; i < 1000; i++)
            {
                var isMatch = Regex.IsMatch(_lorem, Tedd.Archive.InternalUtilsArchive.StringToWildcard("*ipsum*ad??in*sit*"));
            }
        }

        [Benchmark(Description = "Tedd:Complex,Precompiled")]
        public void TestTeddComplexPrecompiled()
        {
            var match = new Tedd.WildcardMatch("*ipsum*ad??in*sit*", WildcardOptions.Compiled);
            for (int i = 0; i < 1000; i++)
            {
                var isMatch = match.IsMatch(_lorem);
            }
        }

        [Benchmark(Description = "FastWildcard:Complex")]
        public void TestFastWildcardComplex()
        {
            for (int i = 0; i < 1000; i++)
            {
                var isMatch = FastWildcard.FastWildcard.IsMatch(_lorem, "*ipsum*ad??in*sit*");
            }
        }
    }
}
