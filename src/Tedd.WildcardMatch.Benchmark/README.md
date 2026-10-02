# Comparative benchmarks

BenchmarkDotNet 0.15.8 runs on .NET 10. The library project targets .NET Standard 2.1, .NET 10, and .NET 11. Building uses the SDK specified in the repository's `global.json`.

| Library / mode | Version | Timed work |
| --- | --- | --- |
| Tedd.WildcardMatch static | Project source | Direct wildcard matching |
| Tedd.WildcardMatch reused | Project source | Direct matching with retained pattern classification |
| Tedd.WildcardMatchRegex static | Project source | Pattern translation, Regex cache lookup, and matching |
| Tedd.WildcardMatchRegex reused | Project source | Matching with a prepared interpreted Regex |
| Tedd.WildcardMatchRegex compiled | Project source | Matching with a prepared compiled Regex |
| FastWildcard | 3.1.0 | Static matching with reused ordinal settings |
| WildcardMatch | 1.0.7 | Static extension call |
| DotNet.Glob | 3.1.3 | Matching with a parsed glob |

FastWildcard is deprecated on NuGet and is included for continuity with the repository's earlier comparison. The benchmark dependency does not become a library dependency.

## Workloads and correctness

Three short-string fixtures cover exact literals, question marks, and multiple stars, using inputs of 4–16 UTF-16 code units. The long-string fixture uses only inputs exceeding 1,000 UTF-16 code units with the pattern `*alpha*beta??*omega*`. Each fixture has 256 inputs, repeating a deterministic set of positive and negative examples. Matching is case-sensitive and restricted to common ASCII wildcard syntax without slashes, line breaks, or glob-specific metacharacters.

A dynamic-programming matcher independently determines the expected result for every input. Tedd correctness failures stop execution. Competitor failures are reported with a concrete input and excluded from timed cases for the affected workload. Validation applies to the benchmark corpus, not the entire syntax or behavior of each library.

Matching returns the number of successful matches to retain observable work. Each API is called through a delegate. The reported time and allocations are per match because `OperationsPerInvoke` is 256. Instance creation and first-use setup are excluded from these measurements; static APIs perform their own per-call setup.

Construction benchmarks measure reusable direct Tedd, interpreted and compiled Tedd Regex, and parsed DotNet.Glob objects. They exclude first-match JIT costs. Static-only competitors have no equivalent reusable-pattern construction API in this suite.

## Run

Build in Release and validate before measuring:

```powershell
dotnet build src/Tedd.WildcardMatch.sln -c Release
dotnet run --project src/Tedd.WildcardMatch.Benchmark -c Release --no-build -- --validate-packages
```

A three-process run reproduces the website's configuration. The runner disables tiered compilation so measured calls use optimized JIT code:

```powershell
$results = 'D:\Workspaces\AI\wildcard-benchmarks'
New-Item -ItemType Directory -Force -Path $results
dotnet run --project src/Tedd.WildcardMatch.Benchmark -c Release --no-build -- --validate-packages "$results/validation.json"
dotnet run --project src/Tedd.WildcardMatch.Benchmark -c Release --no-build -- --filter '*' --launchCount 3 --warmupCount 3 --iterationCount 10 --iterationTime 100 --affinity 1073741824 --artifacts "$results/artifacts"
```

The affinity value selects CPU 30 on the measured 32-thread machine; select an available processor on another host. Close unrelated workloads and compare intervals before interpreting small differences. Builds and timed runs share the scientific-method-performance host lock during investigation.

## Website snapshot

The checked-in snapshot compares both direct and Regex engines at its recorded source revision, using three launches, three warmups, and ten measured iterations with tiered compilation disabled. Direct static/reused and Regex static/reused/compiled calls use identical string inputs and default case-sensitive options. Error is BenchmarkDotNet's 99.9% confidence-interval half-width. Compare intervals before interpreting close rankings; results describe the selected corpus and API lifetimes.

To export a complete run with this configuration to the site:

```powershell
./src/Tedd.WildcardMatch.Benchmark/Export-SiteBenchmarks.ps1 `
    -ArtifactsDirectory "$results/artifacts" `
    -ValidationPath "$results/validation.json" `
    -SourceRevision (git rev-parse HEAD)
```

The exporter writes JSON, correctness results, detailed Markdown reports, and separate short- and long-string throughput charts. Detailed tables remain in the linked reports. Each chart includes every tested library and API. It requires all 32 matching cases and eight construction cases to have valid measurements and matching correctness records. A partial run fails instead of producing a misleading snapshot.

Record the tested library source revision. The short-string selector switches among its three patterns; the long-string comparison remains visible alongside it. Both default charts and links to detailed reports remain available without JavaScript.
