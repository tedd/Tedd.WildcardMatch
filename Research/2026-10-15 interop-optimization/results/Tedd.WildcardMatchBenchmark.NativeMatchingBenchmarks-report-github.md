```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9711/25H2/2025Update/HudsonValley2)
AMD Ryzen 9 5950X 3.40GHz, 1 CPU, 32 logical and 16 physical cores
.NET SDK 11.0.100-rc.2.26504.105
  [Host]     : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v3
  Job-ZMHJYT : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v3

Affinity=01000000000000000000000000000000  EnvironmentVariables=DOTNET_TieredCompilation=0  IterationCount=10  
IterationTime=100ms  LaunchCount=3  WarmupCount=3  

```
| Method | ShareInputs | Case                 | Mean          | Error      | StdDev     | Median        | Allocated |
|------- |------------ |--------------------- |--------------:|-----------:|-----------:|--------------:|----------:|
| **Match**  | **False**       | **Literal/GitWildmatch** |     **26.262 ns** |  **0.3216 ns** |  **0.4714 ns** |     **25.993 ns** |         **-** |
| **Match**  | **False**       | **Literal/Krauss**       |      **9.420 ns** |  **0.1792 ns** |  **0.2683 ns** |      **9.332 ns** |         **-** |
| **Match**  | **False**       | **Literal/Pcre2Jit**     |     **16.053 ns** |  **0.0766 ns** |  **0.1122 ns** |     **15.996 ns** |         **-** |
| **Match**  | **False**       | **Liter(...)cards [21]** |     **78.486 ns** |  **0.5552 ns** |  **0.8309 ns** |     **78.770 ns** |         **-** |
| **Match**  | **False**       | **LongT(...)match [21]** |    **841.936 ns** |  **0.8062 ns** |  **1.1562 ns** |    **841.741 ns** |         **-** |
| **Match**  | **False**       | **LongText/Krauss**      |    **235.801 ns** |  **0.4174 ns** |  **0.6119 ns** |    **235.884 ns** |         **-** |
| **Match**  | **False**       | **LongText/Pcre2Jit**    |  **1,472.601 ns** | **11.7172 ns** | **17.1749 ns** |  **1,474.479 ns** |         **-** |
| **Match**  | **False**       | **LongT(...)cards [22]** | **22,056.586 ns** | **16.8760 ns** | **24.7367 ns** | **22,054.395 ns** |         **-** |
| **Match**  | **False**       | **Multi(...)match [22]** |     **28.904 ns** |  **0.2640 ns** |  **0.3951 ns** |     **29.109 ns** |         **-** |
| **Match**  | **False**       | **MultiStar/Krauss**     |      **9.602 ns** |  **0.0458 ns** |  **0.0642 ns** |      **9.605 ns** |         **-** |
| **Match**  | **False**       | **MultiStar/Pcre2Jit**   |     **22.515 ns** |  **0.0883 ns** |  **0.1294 ns** |     **22.449 ns** |         **-** |
| **Match**  | **False**       | **Multi(...)cards [23]** |    **152.846 ns** |  **0.3507 ns** |  **0.5029 ns** |    **152.854 ns** |         **-** |
| **Match**  | **False**       | **Simple/GitWildmatch**  |     **28.950 ns** |  **0.0938 ns** |  **0.1404 ns** |     **28.936 ns** |         **-** |
| **Match**  | **False**       | **Simple/Krauss**        |      **9.557 ns** |  **0.0070 ns** |  **0.0104 ns** |      **9.554 ns** |         **-** |
| **Match**  | **False**       | **Simple/Pcre2Jit**      |     **16.183 ns** |  **0.0061 ns** |  **0.0086 ns** |     **16.181 ns** |         **-** |
| **Match**  | **False**       | **Simple/ZemaWildcards** |     **81.219 ns** |  **0.5583 ns** |  **0.8357 ns** |     **81.010 ns** |         **-** |
| **Match**  | **True**        | **Literal/GitWildmatch** |     **26.025 ns** |  **0.0206 ns** |  **0.0268 ns** |     **26.028 ns** |         **-** |
| **Match**  | **True**        | **Literal/Krauss**       |      **9.325 ns** |  **0.0034 ns** |  **0.0049 ns** |      **9.324 ns** |         **-** |
| **Match**  | **True**        | **Literal/Pcre2Jit**     |     **15.810 ns** |  **0.0062 ns** |  **0.0088 ns** |     **15.810 ns** |         **-** |
| **Match**  | **True**        | **Liter(...)cards [21]** |     **79.615 ns** |  **1.4946 ns** |  **2.1908 ns** |     **78.361 ns** |         **-** |
| **Match**  | **True**        | **LongT(...)match [21]** |    **841.494 ns** |  **0.9075 ns** |  **1.2723 ns** |    **841.571 ns** |         **-** |
| **Match**  | **True**        | **LongText/Krauss**      |    **234.287 ns** |  **0.5977 ns** |  **0.8380 ns** |    **234.273 ns** |         **-** |
| **Match**  | **True**        | **LongText/Pcre2Jit**    |  **1,466.610 ns** |  **7.7260 ns** | **11.5640 ns** |  **1,466.000 ns** |         **-** |
| **Match**  | **True**        | **LongT(...)cards [22]** | **22,050.382 ns** | **11.7323 ns** | **16.4470 ns** | **22,048.468 ns** |         **-** |
| **Match**  | **True**        | **Multi(...)match [22]** |     **28.240 ns** |  **0.0187 ns** |  **0.0263 ns** |     **28.239 ns** |         **-** |
| **Match**  | **True**        | **MultiStar/Krauss**     |      **9.582 ns** |  **0.0423 ns** |  **0.0607 ns** |      **9.560 ns** |         **-** |
| **Match**  | **True**        | **MultiStar/Pcre2Jit**   |     **22.487 ns** |  **0.0879 ns** |  **0.1261 ns** |     **22.455 ns** |         **-** |
| **Match**  | **True**        | **Multi(...)cards [23]** |    **152.514 ns** |  **0.4538 ns** |  **0.6652 ns** |    **152.205 ns** |         **-** |
| **Match**  | **True**        | **Simple/GitWildmatch**  |     **28.987 ns** |  **0.0627 ns** |  **0.0939 ns** |     **28.973 ns** |         **-** |
| **Match**  | **True**        | **Simple/Krauss**        |      **9.560 ns** |  **0.0052 ns** |  **0.0074 ns** |      **9.560 ns** |         **-** |
| **Match**  | **True**        | **Simple/Pcre2Jit**      |     **16.191 ns** |  **0.0087 ns** |  **0.0122 ns** |     **16.190 ns** |         **-** |
| **Match**  | **True**        | **Simple/ZemaWildcards** |     **83.047 ns** |  **2.1528 ns** |  **3.2222 ns** |     **81.194 ns** |         **-** |
