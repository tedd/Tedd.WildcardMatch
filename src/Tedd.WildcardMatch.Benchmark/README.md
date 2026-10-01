# Comparative benchmarks

BenchmarkDotNet 0.15.8 runs on .NET 10. The library project targets .NET Standard 2.1, .NET 10, and .NET 11. Building uses the SDK specified in the repository's `global.json`.

| Library / mode | Version | Timed work |
| --- | --- | --- |
| Tedd.WildcardMatch static | Project source | Direct wildcard matching |
| Tedd.WildcardMatch reused | Project source | Direct matching with retained pattern classification |
| Tedd.WildcardMatchRegex compiled | Project source | Matching with a prepared compiled Regex |
| FastWildcard | 3.1.0 | Static matching with reused ordinal settings |
| WildcardMatch | 1.0.7 | Static extension call |
| DotNet.Glob | 3.1.3 | Matching with a parsed glob |

FastWildcard is deprecated on NuGet and is included for continuity with the repository's earlier comparison. The benchmark dependency does not become a library dependency.

## Workloads and correctness

Four fixtures cover exact literals, question marks, multiple stars, and mixed short/long text. Each fixture has 256 inputs, repeating a deterministic set of positive and negative examples. Long inputs contain over 1,000 characters. Matching is case-sensitive and restricted to common ASCII wildcard syntax without slashes, line breaks, or glob-specific metacharacters.

A dynamic-programming matcher independently determines the expected result for every input. Tedd correctness failures stop execution. Competitor failures are reported with a concrete input and excluded from timed cases for the affected workload. Validation applies to the benchmark corpus, not the entire syntax or behavior of each library.

Matching returns the number of successful matches to retain observable work. Each API is called through a delegate. The reported time and allocations are per match because `OperationsPerInvoke` is 256. Instance creation and first-use setup are excluded from these measurements; static APIs perform their own per-call setup.

Construction benchmarks measure reusable direct Tedd, compiled Tedd Regex, and parsed DotNet.Glob objects. They exclude first-match JIT costs. Static-only competitors have no equivalent reusable-pattern construction API in this suite.

## Run

Build in Release and validate before measuring:

```powershell
dotnet build src/Tedd.WildcardMatch.sln -c Release
dotnet run --project src/Tedd.WildcardMatch.Benchmark -c Release --no-build -- --validate-packages
```

A short run provides an initial comparison:

```powershell
$results = 'D:\Workspaces\AI\wildcard-benchmarks'
New-Item -ItemType Directory -Force -Path $results
dotnet run --project src/Tedd.WildcardMatch.Benchmark -c Release --no-build -- --validate-packages "$results/validation.json"
dotnet run --project src/Tedd.WildcardMatch.Benchmark -c Release --no-build -- --filter '*' --job short --artifacts "$results/artifacts"
```

For more precise measurements, omit `--job short` and inspect the resulting reports. Close unrelated workloads and compare intervals before interpreting small differences.

## Website snapshot

The checked-in snapshot measures the Regex-backed implementation at its recorded source revision, using one launch, three warmups, and three measured iterations. Its static/reused rows describe that revision's Regex API. Error is BenchmarkDotNet's 99.9% confidence-interval half-width. Short runs can produce wide intervals; the snapshot should support workload-specific comparisons, not close rankings.

To export a complete short run to the site:

```powershell
./src/Tedd.WildcardMatch.Benchmark/Export-SiteBenchmarks.ps1 `
    -ArtifactsDirectory "$results/artifacts" `
    -ValidationPath "$results/validation.json" `
    -SourceRevision (git rev-parse HEAD)
```

The exporter writes JSON, correctness results, raw Markdown reports, and static HTML tables with a throughput chart. It requires all 24 matching cases and six construction cases to have valid measurements and matching correctness records. A partial run fails instead of producing a misleading snapshot.

Use a clean checkout and record the tested source revision. The website selector updates the chart; the complete tables remain available without JavaScript.
