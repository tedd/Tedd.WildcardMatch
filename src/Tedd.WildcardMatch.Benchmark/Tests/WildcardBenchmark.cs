using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using FastWildcard;
using TeddWildcardMatchBenchmark;

namespace TeddWildcardMatchBenchmark.Tests
{
    [Config(typeof(TestConfig))]
    public class WildcardBenchmarkComplex
    {
        private const int Iterations = 1000;

        private static readonly string _textLong = "1234567890" + new string('x', 100) + "aabbaabbccddeeffaabbccddeeffgghhiijjkk aabbaabbccddeeffaabbccddeeffgghhiijjkkllmmnnooppqqrrssttuuvvwwxxyyzz" + new string('x', 100) + "1234567890";

        private static readonly string _patternComplex = "*1*a*b*c*d*e*f*g*h*i*j*k*l*m*n*o*p*q*r*s*t*u*v*0*";
        private global::Tedd.WildcardMatch _twmc;

        [GlobalSetup]
        public void Setup()
        {
            _twmc = new global::Tedd.WildcardMatch(_patternComplex, global::Tedd.WildcardOptions.Compiled | global::Tedd.WildcardOptions.IgnoreCase);
        }

        [Benchmark(Description = "T:CS")]
        public void T_D_Complex()
        {
            for (var i = 0; i < Iterations; i++)
            {
                if (!global::Tedd.WildcardMatch.IsMatch(_textLong, _patternComplex, true))
                    throw new Exception("Incapable of matching");
            }
        }

        [Benchmark(Description = "T:CP")]
        public void T_P_Complex()
        {
            for (var i = 0; i < Iterations; i++)
            {
                if (!_twmc.IsMatch(_textLong))
                    throw new Exception("Incapable of matching");
            }
        }

        [Benchmark(Description = "WM")]
        public void WM_Complex()
        {
            for (var i = 0; i < Iterations; i++)
            {
                if (!global::WildcardMatch.StringExtensions.WildcardMatch(_patternComplex, _textLong, true))
                    throw new Exception("Incapable of matching");
            }
        }

        [Benchmark(Description = "FW")]
        public void FW_Complex()
        {
            for (var i = 0; i < Iterations; i++)
            {
                if (!FastWildcard.FastWildcard.IsMatch(_textLong, _patternComplex, new MatchSettings() { StringComparison = StringComparison.CurrentCultureIgnoreCase }))
                    throw new Exception("Incapable of matching");
            }
        }
    }
}
