```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9457/25H2/2025Update/HudsonValley2)
AMD Ryzen 9 5950X 3.40GHz, 1 CPU, 32 logical and 16 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
  Job-ZMHJYT : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3

Affinity=01000000000000000000000000000000  EnvironmentVariables=DOTNET_TieredCompilation=0  IterationCount=10  
IterationTime=100ms  LaunchCount=3  WarmupCount=3  

```
| Method            | Pattern       | Mean         | Error      | StdDev     | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------ |-------------- |-------------:|-----------:|-----------:|-------:|--------:|-------:|----------:|------------:|
| **TeddDirectReused**  | ***a?c*e***       |    **140.03 ns** |   **1.438 ns** |   **2.108 ns** |   **1.00** |    **0.02** | **0.0241** |     **416 B** |        **1.00** |
| TeddRegexReused   | *a?c*e*       |  2,098.36 ns |  24.360 ns |  35.706 ns |  14.99 |    0.33 | 0.1029 |    2000 B |        4.81 |
| TeddRegexCompiled | *a?c*e*       | 16,628.70 ns | 146.285 ns | 218.953 ns | 118.78 |    2.32 | 0.6822 |   12696 B |       30.52 |
| DotNetGlob        | *a?c*e*       |    810.37 ns |   9.842 ns |  13.797 ns |   5.79 |    0.13 | 0.1061 |    1792 B |        4.31 |
|                   |               |              |            |            |        |         |        |           |             |
| **TeddDirectReused**  | **report-??.txt** |     **56.33 ns** |   **2.125 ns** |   **3.115 ns** |   **1.00** |    **0.08** | **0.0124** |     **208 B** |        **1.00** |
| TeddRegexReused   | report-??.txt |  1,973.61 ns |  15.551 ns |  22.795 ns |  35.14 |    1.94 | 0.1176 |    2024 B |        9.73 |
| TeddRegexCompiled | report-??.txt | 10,754.71 ns | 140.177 ns | 196.509 ns | 191.49 |   10.92 | 0.4415 |    8448 B |       40.62 |
| DotNetGlob        | report-??.txt |    670.31 ns |   8.900 ns |  13.046 ns |  11.94 |    0.69 | 0.0525 |     976 B |        4.69 |
