```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9457/25H2/2025Update/HudsonValley2)
AMD Ryzen 9 5950X 3.40GHz, 1 CPU, 32 logical and 16 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
  Job-UWLSOM : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3

IterationCount=10  LaunchCount=1  WarmupCount=3

```
| Method            | Pattern       | Mean         | Error        | StdDev       | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------ |-------------- |-------------:|-------------:|-------------:|-------:|--------:|-------:|----------:|------------:|
| **TeddDirectReused**  | ***a?c*e***       |     **20.40 ns** |     **0.929 ns** |     **0.553 ns** |   **1.00** |    **0.04** | **0.0043** |      **72 B** |        **1.00** |
| TeddRegexReused   | *a?c*e*       |  1,405.92 ns |    88.479 ns |    46.276 ns |  68.96 |    2.79 | 0.1183 |    2000 B |       27.78 |
| TeddRegexCompiled | *a?c*e*       | 10,612.86 ns | 2,007.944 ns | 1,050.194 ns | 520.53 |   50.39 | 0.7324 |   12696 B |      176.33 |
| DotNetGlob        | *a?c*e*       |    712.46 ns |    92.166 ns |    60.962 ns |  34.94 |    2.99 | 0.1068 |    1792 B |       24.89 |
|                   |               |              |              |              |        |         |        |           |             |
| **TeddDirectReused**  | **report-??.txt** |     **21.53 ns** |     **1.852 ns** |     **0.969 ns** |   **1.00** |    **0.06** | **0.0043** |      **72 B** |        **1.00** |
| TeddRegexReused   | report-??.txt |  1,362.78 ns |    87.870 ns |    58.121 ns |  63.42 |    3.89 | 0.1202 |    2024 B |       28.11 |
| TeddRegexCompiled | report-??.txt |  6,978.17 ns | 1,704.063 ns | 1,127.132 ns | 324.75 |   52.28 | 0.4883 |    8448 B |      117.33 |
| DotNetGlob        | report-??.txt |    410.05 ns |    22.007 ns |    14.556 ns |  19.08 |    1.09 | 0.0582 |     976 B |       13.56 |
