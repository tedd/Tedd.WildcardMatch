## 2025-03-10 - Dependency and Framework Drift

**Observation:** Tedd.WildcardMatch targets only netstandard2.0. Benchmark and Tests target net10.0. Benchmark uses an obsolete BenchmarkDotNet.Toolchains.Roslyn (NU1701). Tests use outdated xunit, coverlet.collector, and Microsoft.NET.Test.Sdk packages, leading to a transitive vulnerability warning (NU1903) in Newtonsoft.Json.

**Strategic Action:** Multi-target Tedd.WildcardMatch to netstandard2.0;net8.0;net9.0 to optimize for modern consumers while preserving legacy support. Update BenchmarkDotNet and test dependencies to their latest stable versions, and remove the obsolete Roslyn toolchain package.
