using System;
using BenchmarkDotNet.Attributes;

namespace TeddWildcardMatchBenchmark.Tests
{
    [Config(typeof(TestConfig))]
    public class StringToWildcardBenchmark
    {
        private const string _pattern = "*1*a*b*c*d*e*f*g*h*i*j*k*l*m*n*o*p*q*r*s*t*u*v*0*";

        [Benchmark(Baseline = true, Description = "Original")]
        public string Original()
        {
            return Tedd.Archive.InternalUtils.StringToWildcard(_pattern);
        }

        [Benchmark(Description = "Optimized")]
        public string Optimized()
        {
            return Tedd.InternalUtils.StringToWildcard(_pattern);
        }
    }
}
