using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.InProcess.Emit;

namespace TeddWildcardMatchBenchmark.Tests
{
    [Config(typeof(Config))]
    [MemoryDiagnoser]
    public class StringToWildcardBenchmark
    {
        private class Config : ManualConfig
        {
            public Config()
            {
                AddJob(Job.Default
                    .WithToolchain(InProcessEmitToolchain.Instance));
            }
        }

        private const string WildcardShort = "*aa??cc*ee*";
        private const string WildcardLong = "a*b?c*d?e*f?g*h?i*j?k*l?m*n?o*p?q*r?s*t?u*v?w*x?y*z?";
        private const string WildcardNoSpecial = "abcdefghijklmnopqrstuvwxyz";
        private const string WildcardManySpecial = @"C:\Users\*\Documents\*\*.txt";

        [Benchmark(Baseline = true)]
        public string Archive_Short() => Tedd.Archive.InternalUtilsArchive.StringToWildcard(WildcardShort);

        [Benchmark]
        public string New_Short() => Tedd.InternalUtils.StringToWildcard(WildcardShort);

        [Benchmark]
        public string Archive_Long() => Tedd.Archive.InternalUtilsArchive.StringToWildcard(WildcardLong);

        [Benchmark]
        public string New_Long() => Tedd.InternalUtils.StringToWildcard(WildcardLong);

        [Benchmark]
        public string Archive_NoSpecial() => Tedd.Archive.InternalUtilsArchive.StringToWildcard(WildcardNoSpecial);

        [Benchmark]
        public string New_NoSpecial() => Tedd.InternalUtils.StringToWildcard(WildcardNoSpecial);

        [Benchmark]
        public string Archive_ManySpecial() => Tedd.Archive.InternalUtilsArchive.StringToWildcard(WildcardManySpecial);

        [Benchmark]
        public string New_ManySpecial() => Tedd.InternalUtils.StringToWildcard(WildcardManySpecial);
    }
}
