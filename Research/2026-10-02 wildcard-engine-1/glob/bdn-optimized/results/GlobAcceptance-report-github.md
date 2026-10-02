```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9457/25H2/2025Update/HudsonValley2)
AMD Ryzen 9 5950X 3.40GHz, 1 CPU, 32 logical and 16 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
  Job-ZMHJYT : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3

Affinity=01000000000000000000000000000000  EnvironmentVariables=DOTNET_TieredCompilation=0  IterationCount=10  
IterationTime=100ms  LaunchCount=3  WarmupCount=3  

```
| Method | Workload  | Library          | Mean         | Error      | StdDev     | Code Size | Allocated |
|------- |---------- |----------------- |-------------:|-----------:|-----------:|----------:|----------:|
| **Match**  | **Literal**   | **DotNetGlob**       |    **15.311 ns** |  **0.0494 ns** |  **0.0692 ns** |      **67 B** |         **-** |
| **Match**  | **Literal**   | **TeddDirectReused** |     **5.920 ns** |  **0.5838 ns** |  **0.8184 ns** |      **67 B** |         **-** |
| **Match**  | **Literal**   | **TeddDirectStatic** |    **14.925 ns** |  **0.0642 ns** |  **0.0921 ns** |      **67 B** |         **-** |
| **Match**  | **LongText**  | **DotNetGlob**       | **7,346.052 ns** | **70.6703 ns** | **99.0699 ns** |      **67 B** |         **-** |
| **Match**  | **LongText**  | **TeddDirectReused** |   **179.378 ns** |  **1.4110 ns** |  **2.0682 ns** |      **67 B** |         **-** |
| **Match**  | **LongText**  | **TeddDirectStatic** |   **212.491 ns** |  **1.7013 ns** |  **2.3849 ns** |      **67 B** |         **-** |
| **Match**  | **MultiStar** | **DotNetGlob**       |    **59.668 ns** |  **0.4871 ns** |  **0.6667 ns** |      **67 B** |         **-** |
| **Match**  | **MultiStar** | **TeddDirectReused** |    **21.735 ns** |  **0.4083 ns** |  **0.5856 ns** |      **67 B** |         **-** |
| **Match**  | **MultiStar** | **TeddDirectStatic** |    **38.161 ns** |  **1.5460 ns** |  **2.1162 ns** |      **67 B** |         **-** |
| **Match**  | **Simple**    | **DotNetGlob**       |    **22.626 ns** |  **0.0253 ns** |  **0.0354 ns** |      **67 B** |         **-** |
| **Match**  | **Simple**    | **TeddDirectReused** |     **5.511 ns** |  **0.0239 ns** |  **0.0343 ns** |      **67 B** |         **-** |
| **Match**  | **Simple**    | **TeddDirectStatic** |    **19.836 ns** |  **0.0455 ns** |  **0.0622 ns** |      **67 B** |         **-** |
