```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9711/25H2/2025Update/HudsonValley2)
AMD Ryzen 9 5950X 3.40GHz, 1 CPU, 32 logical and 16 physical cores
.NET SDK 11.0.100-rc.2.26504.105
  [Host]     : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v3
  Job-ZMHJYT : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v3

Affinity=01000000000000000000000000000000  EnvironmentVariables=DOTNET_TieredCompilation=0  IterationCount=10  
IterationTime=100ms  LaunchCount=3  WarmupCount=3  

```
| Method     | ShareInputs | Case                 | Mean      | Error    | StdDev   | Median    | Allocated |
|----------- |------------ |--------------------- |----------:|---------:|---------:|----------:|----------:|
| **Normal**     | **False**       | **Literal/GitWildmatch** |  **27.81 ns** | **0.013 ns** | **0.020 ns** |  **27.81 ns** |         **-** |
| Suppressed | False       | Literal/GitWildmatch |  27.24 ns | 0.021 ns | 0.031 ns |  27.23 ns |         - |
| **Normal**     | **False**       | **Literal/Krauss**       |  **11.12 ns** | **0.010 ns** | **0.014 ns** |  **11.11 ns** |         **-** |
| Suppressed | False       | Literal/Krauss       |  10.52 ns | 0.007 ns | 0.010 ns |  10.51 ns |         - |
| **Normal**     | **False**       | **Liter(...)cards [21]** |  **79.64 ns** | **0.459 ns** | **0.687 ns** |  **79.81 ns** |         **-** |
| Suppressed | False       | Liter(...)cards [21] |  78.92 ns | 0.899 ns | 1.345 ns |  79.49 ns |         - |
| **Normal**     | **False**       | **Multi(...)match [22]** |  **30.01 ns** | **0.020 ns** | **0.029 ns** |  **30.01 ns** |         **-** |
| Suppressed | False       | Multi(...)match [22] |  29.70 ns | 0.021 ns | 0.030 ns |  29.70 ns |         - |
| **Normal**     | **False**       | **MultiStar/Krauss**     |  **11.30 ns** | **0.026 ns** | **0.036 ns** |  **11.29 ns** |         **-** |
| Suppressed | False       | MultiStar/Krauss     |  10.98 ns | 0.019 ns | 0.027 ns |  10.97 ns |         - |
| **Normal**     | **False**       | **Multi(...)cards [23]** | **157.23 ns** | **3.112 ns** | **4.464 ns** | **154.29 ns** |         **-** |
| Suppressed | False       | Multi(...)cards [23] | 153.15 ns | 0.300 ns | 0.449 ns | 152.93 ns |         - |
| **Normal**     | **False**       | **Simple/GitWildmatch**  |  **31.79 ns** | **0.074 ns** | **0.109 ns** |  **31.78 ns** |         **-** |
| Suppressed | False       | Simple/GitWildmatch  |  30.27 ns | 0.059 ns | 0.088 ns |  30.28 ns |         - |
| **Normal**     | **False**       | **Simple/Krauss**        |  **11.34 ns** | **0.005 ns** | **0.007 ns** |  **11.34 ns** |         **-** |
| Suppressed | False       | Simple/Krauss        |  11.03 ns | 0.004 ns | 0.005 ns |  11.03 ns |         - |
| **Normal**     | **False**       | **Simple/ZemaWildcards** |  **81.73 ns** | **0.403 ns** | **0.590 ns** |  **81.52 ns** |         **-** |
| Suppressed | False       | Simple/ZemaWildcards |  81.36 ns | 0.625 ns | 0.916 ns |  81.79 ns |         - |
| **Normal**     | **True**        | **Literal/GitWildmatch** |  **28.17 ns** | **0.011 ns** | **0.016 ns** |  **28.18 ns** |         **-** |
| Suppressed | True        | Literal/GitWildmatch |  27.48 ns | 0.009 ns | 0.013 ns |  27.48 ns |         - |
| **Normal**     | **True**        | **Literal/Krauss**       |  **11.41 ns** | **0.005 ns** | **0.008 ns** |  **11.41 ns** |         **-** |
| Suppressed | True        | Literal/Krauss       |  10.81 ns | 0.003 ns | 0.005 ns |  10.81 ns |         - |
| **Normal**     | **True**        | **Liter(...)cards [21]** |  **79.77 ns** | **0.591 ns** | **0.884 ns** |  **80.15 ns** |         **-** |
| Suppressed | True        | Liter(...)cards [21] |  80.35 ns | 1.980 ns | 2.964 ns |  79.47 ns |         - |
| **Normal**     | **True**        | **Multi(...)match [22]** |  **31.92 ns** | **0.057 ns** | **0.085 ns** |  **31.91 ns** |         **-** |
| Suppressed | True        | Multi(...)match [22] |  31.60 ns | 0.058 ns | 0.085 ns |  31.60 ns |         - |
| **Normal**     | **True**        | **MultiStar/Krauss**     |  **11.60 ns** | **0.041 ns** | **0.056 ns** |  **11.58 ns** |         **-** |
| Suppressed | True        | MultiStar/Krauss     |  11.30 ns | 0.029 ns | 0.043 ns |  11.28 ns |         - |
| **Normal**     | **True**        | **Multi(...)cards [23]** | **154.92 ns** | **0.452 ns** | **0.634 ns** | **155.27 ns** |         **-** |
| Suppressed | True        | Multi(...)cards [23] | 154.53 ns | 1.191 ns | 1.783 ns | 153.68 ns |         - |
| **Normal**     | **True**        | **Simple/GitWildmatch**  |  **32.08 ns** | **0.088 ns** | **0.129 ns** |  **32.08 ns** |         **-** |
| Suppressed | True        | Simple/GitWildmatch  |  30.65 ns | 0.056 ns | 0.077 ns |  30.64 ns |         - |
| **Normal**     | **True**        | **Simple/Krauss**        |  **11.63 ns** | **0.003 ns** | **0.005 ns** |  **11.64 ns** |         **-** |
| Suppressed | True        | Simple/Krauss        |  11.33 ns | 0.006 ns | 0.008 ns |  11.33 ns |         - |
| **Normal**     | **True**        | **Simple/ZemaWildcards** |  **82.45 ns** | **0.613 ns** | **0.918 ns** |  **82.19 ns** |         **-** |
| Suppressed | True        | Simple/ZemaWildcards |  81.92 ns | 0.590 ns | 0.884 ns |  82.12 ns |         - |
