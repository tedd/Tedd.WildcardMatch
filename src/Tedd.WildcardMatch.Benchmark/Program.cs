using BenchmarkDotNet.Running;
using System;
using TeddWildcardMatchBenchmark.Tests;

namespace TeddWildcardMatchBenchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            var summary1 = BenchmarkRunner.Run<WildcardBenchmarkSimple>();
            var summary2 = BenchmarkRunner.Run<WildcardBenchmark>();
        }
    }
}
