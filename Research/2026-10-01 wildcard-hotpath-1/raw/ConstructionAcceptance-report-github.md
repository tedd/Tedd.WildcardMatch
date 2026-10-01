```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9457/25H2/2025Update/HudsonValley2)
AMD Ryzen 9 5950X 3.40GHz, 1 CPU, 32 logical and 16 physical cores
.NET SDK 10.0.401
  [Host]      : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
  net10-cpu30 : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3

Job=net10-cpu30  Affinity=01000000000000000000000000000000  IterationCount=10  
IterationTime=300ms  LaunchCount=2  WarmupCount=5  

```
| Method | Case       | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------- |----------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **Before** | **long-mixed** | **122.421 μs** | **18.014 μs** | **20.744 μs** |  **1.02** |    **0.22** | **2.5803** |  **43.62 KB** |        **1.00** |
| After  | long-mixed |  98.289 μs | 28.604 μs | 30.606 μs |  0.82 |    0.28 | 2.3585 |  39.33 KB |        0.90 |
|        |            |            |           |           |       |         |        |           |             |
| **Before** | **simple**     |   **6.027 μs** |  **1.133 μs** |  **1.212 μs** |  **1.04** |    **0.31** | **0.1912** |   **3.33 KB** |        **1.00** |
| After  | simple     |   4.998 μs |  1.393 μs |  1.604 μs |  0.87 |    0.34 | 0.1974 |   3.23 KB |        0.97 |
