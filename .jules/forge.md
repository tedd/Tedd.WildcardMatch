## 2024-09-01 - Dependency Drift and Target Framework Modernization

**Observation:** The test projects had outdated references with security vulnerabilities and BenchmarkDotNet.Toolchains.Roslyn was generating warning NU1701 against modern .NET target frameworks. The core library only targeted netstandard2.0. CodeQL CI action was v3 and lacked manual build config for .NET 10.0.

**Strategic Action:** Removed obsolete Roslyn toolchain package, updated test dependencies (coverlet, xunit, etc) and BenchmarkDotNet to modern stable versions. Added multi-targeting to net8.0 and net9.0 to the core library `Tedd.WildcardMatch.csproj` while preserving netstandard2.0 compatibility. Updated CodeQL workflow to v4 with manual build steps. Added include-prerelease to setup-dotnet in main CI.