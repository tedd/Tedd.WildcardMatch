```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9711/25H2/2025Update/HudsonValley2)
AMD Ryzen 9 5950X 3.40GHz, 1 CPU, 32 logical and 16 physical cores
.NET SDK 11.0.100-rc.2.26504.105
  [Host]     : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v3
  Job-ZMHJYT : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v3

Affinity=01000000000000000000000000000000  EnvironmentVariables=DOTNET_TieredCompilation=0  IterationCount=10
IterationTime=100ms  LaunchCount=3  WarmupCount=3

```
| Method            | Pattern       | Mean         | Error     | StdDev     | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------ |-------------- |-------------:|----------:|-----------:|-------:|--------:|-------:|----------:|------------:|
| **TeddDirectReused**  | ***a?c*e***       |    **110.76 ns** |  **1.167 ns** |   **1.673 ns** |   **1.00** |    **0.02** | **0.0176** |     **296 B** |        **1.00** |
| TeddRegexReused   | *a?c*e*       |  2,100.76 ns | 21.004 ns |  30.788 ns |  18.97 |    0.39 | 0.1032 |    2000 B |        6.76 |
| TeddRegexCompiled | *a?c*e*       | 15,969.08 ns | 96.616 ns | 132.248 ns | 144.20 |    2.40 | 0.6396 |   12696 B |       42.89 |
| DotNetGlob        | *a?c*e*       |    802.04 ns |  6.371 ns |   9.536 ns |   7.24 |    0.14 | 0.1017 |    1792 B |        6.05 |
|                   |               |              |           |            |        |         |        |           |             |
| **TeddDirectReused**  | **report-??.txt** |     **55.10 ns** |  **0.232 ns** |   **0.333 ns** |   **1.00** |    **0.01** | **0.0132** |     **224 B** |        **1.00** |
| TeddRegexReused   | report-??.txt |  1,905.11 ns | 16.509 ns |  24.198 ns |  34.58 |    0.48 | 0.1118 |    2024 B |        9.04 |
| TeddRegexCompiled | report-??.txt |  9,969.46 ns | 35.249 ns |  51.667 ns | 180.94 |    1.41 | 0.4940 |    8448 B |       37.71 |
| DotNetGlob        | report-??.txt |    666.70 ns |  9.938 ns |  14.252 ns |  12.10 |    0.26 | 0.0523 |     976 B |        4.36 |
