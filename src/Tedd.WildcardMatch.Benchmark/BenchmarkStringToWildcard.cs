using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.InProcess.Emit;
using Tedd.Archive;

namespace TeddWildcardMatchBenchmark
{
    [MemoryDiagnoser]
    [Config(typeof(Config))]
    public class BenchmarkStringToWildcard
    {
        private class Config : ManualConfig
        {
            public Config()
            {
                AddJob(Job.Default.WithToolchain(InProcessEmitToolchain.Instance));
            }
        }

        private const string _pattern = "a*abba?bbcc*dd?ee";

        [Benchmark(Baseline = true, Description = "Legacy")]
        public string Legacy()
        {
            return LegacyInternalUtils.StringToWildcard(_pattern);
        }

        [Benchmark(Description = "Optimized")]
        public string Optimized()
        {
            return Tedd.InternalUtils.StringToWildcard(_pattern);
        }
    }
}
