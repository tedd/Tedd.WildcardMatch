```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9711/25H2/2025Update/HudsonValley2)
AMD Ryzen 9 5950X 3.40GHz, 1 CPU, 32 logical and 16 physical cores
.NET SDK 11.0.100-rc.2.26504.105
  [Host]     : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v3
  Job-ZMHJYT : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v3

Affinity=01000000000000000000000000000000  EnvironmentVariables=DOTNET_TieredCompilation=0  IterationCount=10
IterationTime=100ms  LaunchCount=3  WarmupCount=3

```
| Method     | Mean     | Error     | StdDev    |
|----------- |---------:|----------:|----------:|
| Normal     | 3.271 ns | 0.0010 ns | 0.0015 ns |
| Suppressed | 1.505 ns | 0.0004 ns | 0.0006 ns |
