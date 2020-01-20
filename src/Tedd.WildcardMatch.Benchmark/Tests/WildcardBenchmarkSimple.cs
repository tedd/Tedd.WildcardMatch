using System;
using BenchmarkDotNet.Attributes;
using FastWildcard;

namespace TeddWildcardMatchBenchmark.Tests
{
    [Config(typeof(TestConfig))]
    public class WildcardBenchmarkSimple
    {
        private const int Iterations = 1000;

        private static readonly string _textShort = "a*abbaabbccddee";

        private static readonly string _patternSimple = "*aa??cc*ee*";
        private Tedd.WildcardMatch _twm;

        [GlobalSetup]
        public void Setup()
        {
            _twm = new Tedd.WildcardMatch(_patternSimple, Tedd.WildcardOptions.Compiled | Tedd.WildcardOptions.IgnoreCase);
        }

        [Benchmark(Description = "T:SS")]
        public void TS_Slow()
        {
            for (var i = 0; i < Iterations; i++)
            {
                if (!Tedd.WildcardMatch.IsMatch(_textShort, _patternSimple, true))
                    throw new Exception("Incapable of matching");
            }
        }

        [Benchmark(Baseline = true, Description = "T:SP")]
        public void TS_Precompiled()
        {
            for (var i = 0; i < Iterations; i++)
            {
                if (!_twm.IsMatch(_textShort))
                    throw new Exception("Incapable of matching");
            }
        }

        [Benchmark(Description = "WM:S")]
        public void WM_Simple()
        {
            for (var i = 0; i < Iterations; i++)
            {
                if (!WildcardMatch.StringExtensions.WildcardMatch(_patternSimple, _textShort, true))
                    throw new Exception("Incapable of matching");
            }
        }

        [Benchmark(Description = "FW:S")]
        public void FW_Simple()
        {
            for (var i = 0; i < Iterations; i++)
            {
                if (!FastWildcard.FastWildcard.IsMatch(_textShort, _patternSimple, new MatchSettings() { StringComparison = StringComparison.CurrentCultureIgnoreCase }))
                    throw new Exception("Incapable of matching");
            }
        }
    }
}