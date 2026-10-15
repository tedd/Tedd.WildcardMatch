```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9711/25H2/2025Update/HudsonValley2)
AMD Ryzen 9 5950X 3.40GHz, 1 CPU, 32 logical and 16 physical cores
.NET SDK 11.0.100-rc.2.26504.105
  [Host]     : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v3
  Job-ZMHJYT : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v3

Affinity=01000000000000000000000000000000  EnvironmentVariables=DOTNET_TieredCompilation=0  IterationCount=10  
IterationTime=100ms  LaunchCount=3  WarmupCount=3  

```
| Method | Case                 | Mean          | Error      | StdDev     | Median        | Allocated |
|------- |--------------------- |--------------:|-----------:|-----------:|--------------:|----------:|
| **Match**  | **Literal/GitWildmatch** |     **29.012 ns** |  **0.2122 ns** |  **0.3177 ns** |     **28.871 ns** |         **-** |
| **Match**  | **Literal/Krauss**       |      **9.466 ns** |  **0.0200 ns** |  **0.0287 ns** |      **9.472 ns** |         **-** |
| **Match**  | **Literal/Pcre2Jit**     |     **16.394 ns** |  **0.0065 ns** |  **0.0093 ns** |     **16.394 ns** |         **-** |
| **Match**  | **Liter(...)cards [21]** |     **75.468 ns** |  **1.0856 ns** |  **1.5570 ns** |     **74.759 ns** |         **-** |
| **Match**  | **LongT(...)match [21]** |    **656.424 ns** |  **2.5905 ns** |  **3.8773 ns** |    **654.865 ns** |         **-** |
| **Match**  | **LongText/Krauss**      |    **231.400 ns** |  **0.7016 ns** |  **1.0284 ns** |    **231.051 ns** |         **-** |
| **Match**  | **LongText/Pcre2Jit**    |  **1,460.878 ns** | **10.3308 ns** | **15.1428 ns** |  **1,458.354 ns** |         **-** |
| **Match**  | **LongT(...)cards [22]** | **21,857.509 ns** | **12.2298 ns** | **17.1444 ns** | **21,853.772 ns** |         **-** |
| **Match**  | **Multi(...)match [22]** |     **29.831 ns** |  **0.2011 ns** |  **0.2948 ns** |     **29.943 ns** |         **-** |
| **Match**  | **MultiStar/Krauss**     |      **8.120 ns** |  **0.0541 ns** |  **0.0759 ns** |      **8.117 ns** |         **-** |
| **Match**  | **MultiStar/Pcre2Jit**   |     **22.946 ns** |  **0.0579 ns** |  **0.0866 ns** |     **22.949 ns** |         **-** |
| **Match**  | **Multi(...)cards [23]** |    **152.819 ns** |  **0.2680 ns** |  **0.3929 ns** |    **153.007 ns** |         **-** |
| **Match**  | **Simple/GitWildmatch**  |     **32.818 ns** |  **0.3295 ns** |  **0.4830 ns** |     **32.530 ns** |         **-** |
| **Match**  | **Simple/Krauss**        |      **9.560 ns** |  **0.0156 ns** |  **0.0218 ns** |      **9.549 ns** |         **-** |
| **Match**  | **Simple/Pcre2Jit**      |     **16.567 ns** |  **0.0221 ns** |  **0.0302 ns** |     **16.561 ns** |         **-** |
| **Match**  | **Simple/ZemaWildcards** |     **79.937 ns** |  **0.1682 ns** |  **0.2357 ns** |     **79.933 ns** |         **-** |
