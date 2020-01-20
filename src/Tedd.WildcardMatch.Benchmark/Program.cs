using BenchmarkDotNet.Running;
using System;
using TeddWildcardMatchBenchmark.Tests;

namespace TeddWildcardMatchBenchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            var wbs = new WildcardBenchmarkSimple();
            wbs.Setup();
            wbs.TS_Slow();
            wbs.TS_Precompiled();
            wbs.WM_Simple();
            wbs.FW_Simple();

            var wbc = new WildcardBenchmarkComplex();
            wbc.Setup();
            wbc.T_D_Complex();
            wbc.T_P_Complex();
            wbc.WM_Complex();
            wbc.FW_Complex();

            

            //var summary1 = BenchmarkRunner.Run<WildcardBenchmarkSimple>();
            var summary2 = BenchmarkRunner.Run<WildcardBenchmarkComplex>();
        }
    }
}
