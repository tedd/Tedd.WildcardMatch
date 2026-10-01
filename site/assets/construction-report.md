```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9457/25H2/2025Update/HudsonValley2)
AMD Ryzen 9 5950X 3.40GHz, 1 CPU, 32 logical and 16 physical cores
.NET SDK 10.0.401
  [Host]   : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
  ShortRun : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3

Job=ShortRun  IterationCount=3  LaunchCount=1
WarmupCount=3

```
| Method       | Pattern       | Mean       | Error       | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------- |-------------- |-----------:|------------:|----------:|------:|--------:|-------:|----------:|------------:|
| **TeddReused**   | ***a?c*e***       | **1,780.0 ns** |   **933.85 ns** |  **51.19 ns** |  **1.00** |    **0.03** | **0.1183** |    **2000 B** |        **1.00** |
| TeddCompiled | *a?c*e*       | 8,872.5 ns | 8,122.74 ns | 445.23 ns |  4.99 |    0.25 | 0.7324 |   12696 B |        6.35 |
| DotNetGlob   | *a?c*e*       |   777.0 ns |   210.60 ns |  11.54 ns |  0.44 |    0.01 | 0.1068 |    1792 B |        0.90 |
|              |               |            |             |           |       |         |        |           |             |
| **TeddReused**   | **report-??.txt** | **1,609.8 ns** |   **119.59 ns** |   **6.56 ns** |  **1.00** |    **0.00** | **0.1202** |    **2024 B** |        **1.00** |
| TeddCompiled | report-??.txt | 5,824.4 ns |   793.45 ns |  43.49 ns |  3.62 |    0.03 | 0.4883 |    8448 B |        4.17 |
| DotNetGlob   | report-??.txt |   524.0 ns |    51.76 ns |   2.84 ns |  0.33 |    0.00 | 0.0582 |     976 B |        0.48 |
