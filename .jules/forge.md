## 2026-10-01 - Dependency and Target Framework Modernization

**Observation:** Target framework `netstandard2.0` is the single target framework while the test framework natively supports `net10.0`. Main package lacks modern target frameworks like `net8.0` and `net9.0` required for baseline modern consumer reach, while dependencies in benchmark and tests projects drifted from latest stables versions.

**Strategic Action:** Added `net8.0` and `net9.0` targets via multi-targeting to `Tedd.WildcardMatch.csproj` to extend modern base line compatibility while retaining `netstandard2.0`. Upgraded dependencies `xunit`, `xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk`, `coverlet.collector`, `BenchmarkDotNet`, and `BenchmarkDotNet.Diagnostics.Windows` to their latest stable compatible versions.
