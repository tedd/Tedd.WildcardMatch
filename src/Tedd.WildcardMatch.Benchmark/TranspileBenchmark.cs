using BenchmarkDotNet.Attributes;
using Tedd;
using Tedd.Archive;

namespace TeddWildcardMatchBenchmark
{
    [MemoryDiagnoser]
    public class TranspileBenchmark
    {
        private const string ShortWildcard = "*hello? [world] + {test} #123";
        private string _loremAdv = "*!*?*" + "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed aliquet condimentum risus. Maecenas tempus eget diam posuere volutpat. Pellentesque ut dignissim libero." + "\\*1??*\\";

        [Benchmark(Baseline = true)]
        public string Transpile_Archive_Short()
        {
            return InternalUtilsLegacy.StringToWildcard(ShortWildcard);
        }

        [Benchmark]
        public string Transpile_Optimized_Short()
        {
            return InternalUtils.StringToWildcard(ShortWildcard);
        }

        [Benchmark]
        public string Transpile_Archive_Long()
        {
            return InternalUtilsLegacy.StringToWildcard(_loremAdv);
        }

        [Benchmark]
        public string Transpile_Optimized_Long()
        {
            return InternalUtils.StringToWildcard(_loremAdv);
        }
    }
}
