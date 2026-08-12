using BenchmarkDotNet.Running;
using System;
using TeddWildcardMatchBenchmark.Tests;

namespace TeddWildcardMatchBenchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<StringToWildcardBenchmark>();
        }
    }
}
