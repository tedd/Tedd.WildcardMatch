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
    -SourceRevision (git rev-parse HEAD) `
    -MeasuredAt 'YYYY-MM-DD' `
    -MeasuredPackageVersion '2.3.0' `
    -PackageVersion '2.3.0' `
    -NativeValidationPath "$results/native-validation.json" `
    -NativeSourcesPath "$results/native-sources.json" `
    -PriorNativeReferencePath "$results/prior-native-reference.json"
```

The exporter writes JSON, managed/native correctness results, full JSON/CSV/Markdown reports, and separate short- and long-string throughput charts. Tables display all 110 cases. Charts show every managed API and the lowest measured native call/storage mode per library; the complete table preserves slower modes. Export requires the complete matrix: 32 managed matching, 32 native batches, 36 per-input interop, two transition controls and eight construction cases, with matching correctness records. A partial run fails instead of producing a misleading snapshot.

Use the actual measurement date, source revision and package metadata from the measured build, rather than the export date or a subsequent release version. `NativeSourcesPath` identifies pinned upstream revisions. `PriorNativeReferencePath` contains separately identified earlier native-build references; these remain distinct from current-run samples and intervals. When the release changes only package metadata, record the measured and released versions separately.

Record the tested library source revision. The short-string selector switches among its three patterns; the long-string comparison remains visible alongside it. Both default charts and links to detailed reports remain available without JavaScript.

## Native C/C++ comparison

`Run-Comparisons.ps1` builds pinned upstream sources, copies the managed sources to the designated temporary workspace, runs tests and correctness checks, and measures all managed, construction and native cases:

```powershell
./src/Tedd.WildcardMatch.Benchmark/Run-Comparisons.ps1
```

The Windows x64 build requires Git, MSVC, a Windows SDK and CMake. `Native/Build-Native.ps1` accepts the Visual Studio installation path and records upstream revisions. Its defaults select Visual Studio 2026 Insiders and Windows SDK 10.0.22621.0. All generated files reside under `D:\Workspaces\AI\wildcard-native-benchmarks`. The affinity parameter defaults to processor 30; select an available processor on another host.

| Native candidate | API and lifetime | Upstream license |
| --- | --- | --- |
| [Krauss MatchingWildcards](https://github.com/kirkjkrauss/MatchingWildcards) | Pointer-based `FastWildCompare`, pattern interpreted per match | Apache-2.0 |
| [Git wildmatch](https://github.com/git/git/blob/master/wildmatch.c) | `wildmatch`, flags 0, pattern interpreted per match | Git COPYING, GPL-2.0 |
| [Zema wildcards](https://github.com/zemasoft/wildcards) | `wildcards::match` with string views, pattern interpreted per match | BSL-1.0 |
| [PCRE2](https://github.com/PCRE2Project/pcre2) | Anchored wildcard translation, retained JIT code and match data | BSD-3-Clause and bundled notices |

The candidate set includes a performance-focused wildcard implementation, a widely deployed C matcher, a general C++ wildcard library and a native regex JIT. The suite identifies the fastest tested candidate per workload; it does not establish an exhaustive ranking of open-source libraries. File-system glob walkers and substring-only matchers do not perform equivalent work.

Native cases use the same 256 inputs and independent dynamic-programming oracle as managed cases. A separate `--validate-native` mode also checks 16,000 deterministic pattern/input pairs per library, including empty strings and adjacent stars. Incorrect native results abort the comparison. The common syntax is case-sensitive, NUL-free ASCII with `*` and `?`; this comparison does not test Unicode or extended glob syntax.

`WILDCARD_NATIVE_LIBRARY` selects the absolute adapter DLL path and enables `NativeMatchingBenchmarks`. Each timed call crosses into native code once, loops over 256 pre-encoded inputs, dispatches through a native function pointer and returns the match count. Times are normalized per match. Encoding, allocation, pattern translation and PCRE2 JIT compilation occur during setup. Managed cases dispatch through a managed delegate once per input. Both paths include loop and dispatch costs; differences in dispatch, encoding width and runtime remain part of this throughput comparison. It is not an end-to-end measurement of calling a C library from a .NET application.

`InteropBenchmarks` measures one native call per input, with ordinary transitions and with `SuppressGCTransition`. Both call sites invoke the identical `match_one_short` export. Suppression is restricted to Krauss, Git and Zema on the three fixed short fixtures (at most 16 input characters). Those paths perform bounded computation without allocation, blocking or managed callbacks. PCRE2 and long inputs retain ordinary transitions. Suppression qualification does not extend to arbitrary patterns or to the entire 256-match batch.

`ShareInputs` selects native buffer storage. `false` uses one owned string per fixture row; `true` resolves repeated values to the same prepared string before timing, matching the managed fixture's repeated seed references. Neither mode retains match results. Both storage modes participate in batched and per-input comparisons, so a native implementation can benefit from the smaller active buffer footprint.

`InteropControlBenchmarks` calls the same trivial native export through ordinary and suppressed call sites. Its measured difference describes that control and cannot be subtracted as a universal transition cost. With native comparisons enabled, the complete suite contains 110 cases: 32 managed matching, 32 native batches, 36 per-input interop, two transition controls and eight construction cases. Every storage and call mode is eligible when identifying the fastest measured competitor.

Git's `wildmatch.c` and `wildmatch.h` remain unchanged. A standalone compatibility header supplies standard C character functions and Git's four-character glob-special predicate. The native adapter uses MSVC `/O2 /Ob2`; PCRE2 uses a Release static build with JIT enabled and its default CPU detection. MemoryDiagnoser reports managed allocations only, and does not account for native setup allocations.
