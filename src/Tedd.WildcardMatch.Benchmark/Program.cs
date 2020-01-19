using BenchmarkDotNet.Running;
using System;
using TeddWildcardMatchBenchmark.Tests;

namespace TeddWildcardMatchBenchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            var wb = new WildcardBenchmark();
            wb.Setup();
            wb.TS_Slow();
            wb.TS_Precompiled();
            wb.WM_Simple();
            wb.FW_Simple();
            
            wb.TP_Complex();
            wb.TS_Precompiled();
            wb.WM_Complex();
            wb.FW_Complex();

            

            var summary1 = BenchmarkRunner.Run<WildcardBenchmark>();
        }
    }
}
