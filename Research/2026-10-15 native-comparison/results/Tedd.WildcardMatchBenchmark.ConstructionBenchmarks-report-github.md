```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9711/25H2/2025Update/HudsonValley2)
AMD Ryzen 9 5950X 3.40GHz, 1 CPU, 32 logical and 16 physical cores
.NET SDK 11.0.100-rc.2.26504.105
  [Host]     : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v3
  Job-ZMHJYT : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v3

Affinity=01000000000000000000000000000000  EnvironmentVariables=DOTNET_TieredCompilation=0  IterationCount=10  
IterationTime=100ms  LaunchCount=3  WarmupCount=3  

```
| Method            | Pattern       | Mean         | Error     | StdDev    | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------ |-------------- |-------------:|----------:|----------:|-------:|--------:|-------:|----------:|------------:|
| **TeddDirectReused**  | ***a?c*e***       |    **131.34 ns** |  **1.491 ns** |  **2.232 ns** |   **1.00** |    **0.02** | **0.0246** |     **416 B** |        **1.00** |
| TeddRegexReused   | *a?c*e*       |  2,084.37 ns |  9.182 ns | 13.169 ns |  15.87 |    0.28 | 0.1084 |    2000 B |        4.81 |
| TeddRegexCompiled | *a?c*e*       | 15,911.59 ns | 64.092 ns | 89.848 ns | 121.18 |    2.13 | 0.6382 |   12696 B |       30.52 |
| DotNetGlob        | *a?c*e*       |    806.48 ns | 17.973 ns | 26.345 ns |   6.14 |    0.22 | 0.1010 |    1792 B |        4.31 |
|                   |               |              |           |           |        |         |        |           |             |
| **TeddDirectReused**  | **report-??.txt** |     **52.37 ns** |  **0.317 ns** |  **0.475 ns** |   **1.00** |    **0.01** | **0.0121** |     **208 B** |        **1.00** |
| TeddRegexReused   | report-??.txt |  1,903.47 ns | 15.679 ns | 22.981 ns |  36.35 |    0.54 | 0.1152 |    2024 B |        9.73 |
| TeddRegexCompiled | report-??.txt | 10,004.94 ns | 65.777 ns | 92.210 ns | 191.05 |    2.41 | 0.4980 |    8448 B |       40.62 |
| DotNetGlob        | report-??.txt |    666.19 ns | 15.495 ns | 22.712 ns |  12.72 |    0.44 | 0.0581 |     976 B |        4.69 |
