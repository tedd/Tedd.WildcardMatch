using BenchmarkDotNet.Running;

namespace Tedd.WildcardMatchBenchmark;

public class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<InternalUtilsBenchmarks>();
    }
}
