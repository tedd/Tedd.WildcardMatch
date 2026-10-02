```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9457/25H2/2025Update/HudsonValley2)
AMD Ryzen 9 5950X 3.40GHz, 1 CPU, 32 logical and 16 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
  Job-GPHEUC : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3

Affinity=01000000000000000000000000000000  IterationCount=10  IterationTime=100ms  
LaunchCount=3  WarmupCount=3  

```
| Method | Workload  | Library          | Mean         | Error        | StdDev       | Median       | Code Size | Allocated |
|------- |---------- |----------------- |-------------:|-------------:|-------------:|-------------:|----------:|----------:|
| **Match**  | **Literal**   | **DotNetGlob**       |    **130.75 ns** |     **2.212 ns** |     **2.953 ns** |    **130.61 ns** |     **289 B** |         **-** |
| **Match**  | **Literal**   | **TeddDirectReused** |     **43.39 ns** |     **0.759 ns** |     **1.088 ns** |     **43.00 ns** |     **289 B** |         **-** |
| **Match**  | **Literal**   | **TeddDirectStatic** |     **55.66 ns** |     **1.466 ns** |     **2.055 ns** |     **54.34 ns** |     **289 B** |         **-** |
| **Match**  | **LongText**  | **DotNetGlob**       | **58,537.64 ns** | **2,094.977 ns** | **3,004.554 ns** | **59,797.40 ns** |     **289 B** |         **-** |
| **Match**  | **LongText**  | **TeddDirectReused** |    **420.27 ns** |     **4.917 ns** |     **6.893 ns** |    **416.08 ns** |     **289 B** |         **-** |
| **Match**  | **LongText**  | **TeddDirectStatic** |    **623.55 ns** |    **12.387 ns** |    **16.536 ns** |    **612.79 ns** |     **289 B** |         **-** |
| **Match**  | **MultiStar** | **DotNetGlob**       |    **482.81 ns** |     **8.747 ns** |    **12.821 ns** |    **483.79 ns** |     **289 B** |         **-** |
| **Match**  | **MultiStar** | **TeddDirectReused** |    **188.37 ns** |     **1.836 ns** |     **2.633 ns** |    **189.12 ns** |     **289 B** |         **-** |
| **Match**  | **MultiStar** | **TeddDirectStatic** |    **200.62 ns** |     **1.296 ns** |     **1.899 ns** |    **200.77 ns** |     **289 B** |         **-** |
| **Match**  | **Simple**    | **DotNetGlob**       |    **169.13 ns** |     **1.549 ns** |     **2.319 ns** |    **169.57 ns** |     **289 B** |         **-** |
| **Match**  | **Simple**    | **TeddDirectReused** |     **55.78 ns** |     **2.569 ns** |     **3.765 ns** |     **54.03 ns** |     **289 B** |         **-** |
| **Match**  | **Simple**    | **TeddDirectStatic** |     **79.74 ns** |     **1.498 ns** |     **2.148 ns** |     **80.05 ns** |     **289 B** |         **-** |
