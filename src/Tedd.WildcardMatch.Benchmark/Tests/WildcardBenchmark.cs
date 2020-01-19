using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using FastWildcard;
using TeddWildcardMatchBenchmark;

namespace TeddWildcardMatchBenchmark.Tests
{
    [Config(typeof(TestConfig))]
    public class WildcardBenchmark
    {
        private const int Iterations = 1000;

        private static readonly string _textLong = "1234567890" + new string('x', 100) + "aabbaabbccddeeffaabbccddeeffgghhiijjkkllmmnnooppqqrrssttuuvvwwxxyyzz" + new string('x', 100) + "1234567890";
        private static readonly string _textShort = "a*abbaabbccddee";

        private static readonly string _patternSimple = "*aa??cc*ee*";
        private static readonly string _patternComplex = "*1*a*b*c*d*e*f*g*h*i*j*k*l*m*n*o*p*q*r*s*t*u*v*0*";
        private Tedd.WildcardMatch _twm;
        private Tedd.WildcardMatch _twmc;

        [GlobalSetup]
        public void Setup()
        {
            _twm = new Tedd.WildcardMatch(_patternSimple, Tedd.WildcardOptions.Compiled | Tedd.WildcardOptions.CultureInvariant | Tedd.WildcardOptions.IgnoreCase);
            _twmc = new Tedd.WildcardMatch(_patternSimple, Tedd.WildcardOptions.Compiled | Tedd.WildcardOptions.CultureInvariant | Tedd.WildcardOptions.IgnoreCase);
        }

        [Benchmark(Description = "Tedd:Simple,Slow")]
        public void TS_Slow()
        {
            for (var i = 0; i < Iterations; i++)
            {
                if (!Tedd.WildcardMatch.IsMatch(_textShort, _patternSimple, true))
                    throw new Exception("Incapable of matching");
            }
        }

        [Benchmark(Baseline = true, Description = "Tedd:Simple,Precompiled")]
        public void TS_Precompiled()
        {
            for (var i = 0; i < Iterations; i++)
            {
                if (!_twm.IsMatch(_textShort))
                    throw new Exception("Incapable of matching");
            }
        }

        [Benchmark(Description = "WildcardMatcher:Simple")]
        public void WM_Simple()
        {
            for (var i = 0; i < Iterations; i++)
            {
                if (!WildcardMatch.StringExtensions.WildcardMatch(_patternSimple, _textShort, true))
                    throw new Exception("Incapable of matching");
            }
        }

        [Benchmark(Description = "FastWildcard:Simple")]
        public void FW_Simple()
        {
            for (var i = 0; i < Iterations; i++)
            {
                if (!FastWildcard.FastWildcard.IsMatch(_textShort, _patternSimple, new MatchSettings() { StringComparison = StringComparison.OrdinalIgnoreCase }))
                    throw new Exception("Incapable of matching");
            }
        }




        [Benchmark(Description = "Tedd:Complex,Slow")]
        public void TS_Complex()
        {
            for (var i = 0; i < Iterations; i++)
            {
                if (!Tedd.WildcardMatch.IsMatch(_textLong, _patternComplex, true))
                    throw new Exception("Incapable of matching");
            }
        }

        [Benchmark(Description = "Tedd:Complex,Precompiled")]
        public void TP_Complex()
        {
            for (var i = 0; i < Iterations; i++)
            {
                if (!_twmc.IsMatch(_textLong))
                    throw new Exception("Incapable of matching");
            }
        }

        [Benchmark(Description = "WildcardMatch:Complex")]
        public void WM_Complex()
        {
            for (var i = 0; i < Iterations; i++)
            {
                if (!WildcardMatch.StringExtensions.WildcardMatch(_patternComplex, _textLong, true))
                    throw new Exception("Incapable of matching");
            }
        }

        [Benchmark(Description = "FastWildcard:Complex")]
        public void FW_Complex()
        {
            for (var i = 0; i < Iterations; i++)
            {
                if (!FastWildcard.FastWildcard.IsMatch(_textLong, _patternComplex, new MatchSettings() { StringComparison = StringComparison.OrdinalIgnoreCase }))
                    throw new Exception("Incapable of matching");
            }
        }
    }
}
