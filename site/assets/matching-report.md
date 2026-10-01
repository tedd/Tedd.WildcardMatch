```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9457/25H2/2025Update/HudsonValley2)
AMD Ryzen 9 5950X 3.40GHz, 1 CPU, 32 logical and 16 physical cores
.NET SDK 10.0.401
  [Host]   : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
  ShortRun : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3

Job=ShortRun  IterationCount=3  LaunchCount=1
WarmupCount=3

```
| Method | Case                 | Mean         | Error         | StdDev     | Gen0   | Allocated |
|------- |--------------------- |-------------:|--------------:|-----------:|-------:|----------:|
| **Match**  | **Literal/DotNetGlob**   |     **17.40 ns** |      **2.407 ns** |   **0.132 ns** |      **-** |         **-** |
| **Match**  | **Literal/FastWildcard** |     **77.94 ns** |     **14.956 ns** |   **0.820 ns** |      **-** |         **-** |
| **Match**  | **Literal/TeddCompiled** |     **40.78 ns** |      **1.942 ns** |   **0.106 ns** |      **-** |         **-** |
| **Match**  | **Literal/TeddReused**   |    **102.81 ns** |     **15.840 ns** |   **0.868 ns** |      **-** |         **-** |
| **Match**  | **Literal/TeddStatic**   |    **204.92 ns** |    **108.940 ns** |   **5.971 ns** | **0.0072** |     **120 B** |
| **Match**  | **Liter(...)Match [21]** |     **40.81 ns** |     **10.118 ns** |   **0.555 ns** | **0.0029** |      **48 B** |
| **Match**  | **LongText/DotNetGlob**  |  **3,311.45 ns** |  **2,292.430 ns** | **125.656 ns** |      **-** |         **-** |
| **Match**  | **LongT(...)dcard [21]** |    **196.51 ns** |    **279.800 ns** |  **15.337 ns** |      **-** |         **-** |
| **Match**  | **LongT(...)piled [21]** |    **204.02 ns** |    **334.086 ns** |  **18.312 ns** |      **-** |         **-** |
| **Match**  | **LongText/TeddReused**  | **12,929.58 ns** |  **7,944.150 ns** | **435.446 ns** |      **-** |         **-** |
| **Match**  | **LongText/TeddStatic**  | **13,649.36 ns** | **17,130.414 ns** | **938.976 ns** |      **-** |     **192 B** |
| **Match**  | **LongT(...)Match [22]** |  **9,259.87 ns** |  **7,634.806 ns** | **418.489 ns** | **1.9226** |   **32184 B** |
| **Match**  | **MultiStar/DotNetGlob** |     **58.26 ns** |     **50.780 ns** |   **2.783 ns** |      **-** |         **-** |
| **Match**  | **Multi(...)dcard [22]** |     **60.13 ns** |     **49.729 ns** |   **2.726 ns** |      **-** |         **-** |
| **Match**  | **Multi(...)piled [22]** |     **60.65 ns** |     **37.318 ns** |   **2.046 ns** |      **-** |         **-** |
| **Match**  | **MultiStar/TeddReused** |    **234.20 ns** |    **182.956 ns** |  **10.028 ns** |      **-** |         **-** |
| **Match**  | **MultiStar/TeddStatic** |    **283.66 ns** |    **290.310 ns** |  **15.913 ns** | **0.0062** |     **104 B** |
| **Match**  | **Multi(...)Match [23]** |    **144.71 ns** |     **12.255 ns** |   **0.672 ns** | **0.0284** |     **479 B** |
| **Match**  | **Simple/DotNetGlob**    |     **20.68 ns** |      **8.882 ns** |   **0.487 ns** |      **-** |         **-** |
| **Match**  | **Simple/FastWildcard**  |     **75.64 ns** |     **75.763 ns** |   **4.153 ns** |      **-** |         **-** |
| **Match**  | **Simple/TeddCompiled**  |     **35.66 ns** |     **11.292 ns** |   **0.619 ns** |      **-** |         **-** |
| **Match**  | **Simple/TeddReused**    |     **87.08 ns** |     **24.013 ns** |   **1.316 ns** |      **-** |         **-** |
| **Match**  | **Simple/TeddStatic**    |    **155.27 ns** |     **85.858 ns** |   **4.706 ns** | **0.0081** |     **136 B** |
| **Match**  | **Simple/WildcardMatch** |     **38.26 ns** |      **4.275 ns** |   **0.234 ns** | **0.0029** |      **48 B** |
