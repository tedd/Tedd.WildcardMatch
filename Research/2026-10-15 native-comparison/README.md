# Managed and native wildcard comparison — 2026-10-15

Krauss FastWildCompare had the lowest mean among the four native candidates on every workload. Tedd's reused direct matcher had the lowest mean overall on three workloads. Krauss was 2.61× faster than Tedd reused on the multiple-star fixture. These conclusions concern the measured corpus and API lifetimes, not all possible wildcard workloads.

Mean nanoseconds per match; lower is better:

| Workload | Tedd static | Tedd reused | Fastest native: Krauss |
| --- | ---: | ---: | ---: |
| Literal | 14.24 | 5.24 | 9.47 |
| Question marks | 18.86 | 5.31 | 9.56 |
| Multiple stars | 35.96 | 21.22 | 8.12 |
| Long text | 225.36 | 188.20 | 231.40 |

Krauss took 1.81×, 1.80× and 1.23× as long as Tedd reused on literals, question marks and long text respectively. Tedd static took longer than Krauss on all three short-string workloads; its long-text mean was slightly lower. Each reported comparison has non-overlapping BenchmarkDotNet confidence intervals in this run. The [complete table](results-table.md) includes all 12 implementations and their 99.9% confidence-interval half-widths. The [machine-readable summary](matching-summary.json) retains unrounded values and post-outlier iteration samples.

## Candidates and discovery

The search focused on in-memory, full-string matching with the common `*`/`?` syntax. File-system traversal benchmarks and substring-only matching were excluded. The selection combines an implementation with published wildcard performance comparisons, a production C matcher, a generic C++ library and a native regex JIT. It is a targeted comparison, not an exhaustive survey or proof of the fastest library universally.

| Candidate | Source revision | Selection evidence and API |
| --- | --- | --- |
| Krauss MatchingWildcards | `924b8a5b9d8104492e29448ef56e8fd872fd26b5` | [Author's implementation and comparative-performance discussion](https://github.com/kirkjkrauss/MatchingWildcards); pointer-based `FastWildCompare`, Apache-2.0 |
| Git wildmatch | `6de20f6092dcf9bdb1c8efe03db4b70c82b423dd` | [Git's C implementation](https://github.com/git/git/blob/6de20f6092dcf9bdb1c8efe03db4b70c82b423dd/wildmatch.c); `wildmatch(pattern, text, 0)`, Git GPL-2.0 COPYING |
| Zema wildcards | `5662507f01f5d0a4ec22f45749214276ed366d1b` | [Header-only C++ library](https://github.com/zemasoft/wildcards); `wildcards::match` with `cx::string_view`, BSL-1.0 |
| PCRE2 JIT | `13e1da98d8d30f54e96afa5829fe335c6115aa45` | [C regex library with JIT](https://github.com/PCRE2Project/pcre2); 10.50-DEV snapshot, BSD-3-Clause WITH PCRE2-exception and bundled notices |

PCRE2 uses its pinned SLJIT submodule `39c508d1b8fbae332d60c7450db20236913089da`. Its pattern is translated to an anchored `\A...\z` regex, compiled with DOTALL, and JIT-compiled before measurement. Code and match data are reused. Krauss, Git and Zema interpret the supplied wildcard pattern on each match. Git's matcher files are unchanged; the standalone compatibility shim supplies standard C declarations and the four-character glob-special predicate. Upstream license texts are retained in [licenses](licenses/).

Managed competitors remain FastWildcard 3.1.0, WildcardMatch 1.0.7 and DotNet.Glob 3.1.3, alongside the five Tedd direct/Regex API modes. Tedd library source is commit `5de155b6a832cec5438d0d6711ad294e4b0a32dd`, version 2.2.0. No matcher implementation was changed for this comparison. Benchmark harness changes accompany this report; [source hashes](source-hashes.json) identify the measured files.

## Measurement and correctness

- Ryzen 9 5950X, Windows 11 build 26200.9711; CPU 30 affinity, mask 1073741824.
- BenchmarkDotNet 0.15.8; .NET 10.0.13 x64 RyuJIT; SDK 11.0.100-rc.2.26504.105; tiered compilation disabled.
- MSVC 19.52.37025.0, C++17, Release `/O2 /Ob2 /DNDEBUG /MD`; Windows SDK 10.0.22621.0; CMake 4.3.1-msvc1. PCRE2 uses a Release static build with JIT enabled. See [compiler flags](native-compiler-flags.txt) and [environment](environment.json).
- Three process launches, three warmups and ten measured iterations per launch, each targeting 100 ms. OperationsPerInvoke is 256. BenchmarkDotNet applies its usual overhead adjustment and outlier handling. Confidence intervals describe its iteration samples; they are not a guarantee of variation across machines or independent sessions.
- The original four fixtures are unchanged: `report-2026.txt`, `report-??.txt`, `*a?c*e*`, and `*alpha*beta??*omega*`. Each repeats a small deterministic seed set to 256 positive/negative inputs. Short inputs contain 4–16 UTF-16 code units; long inputs exceed 1,000. The corpus is case-sensitive, NUL-free ASCII.
- Managed calls include delegate dispatch per match. Native timing includes one interop transition per batch, the native loop and function-pointer dispatch per match. Native encoding, buffer allocation, regex translation and JIT compilation are excluded. Native buffers use byte strings while managed APIs use UTF-16 strings; native copies and managed seed-reference reuse also have different memory layouts. These measurements compare prepared throughput, not end-to-end native interop cost.
- 32 managed correctness records cover 8,192 corpus checks. Native validation passes 68,096 checks: 1,024 corpus checks plus 16,000 seeded pattern/input pairs per native candidate. The oracle is independent dynamic programming. Native validation also verifies batch match counts.
- All 729 repository tests pass: 242 on .NET 8, 242 on .NET 10 and 245 on .NET 11. The Release build has zero errors and 59 existing analyzer warnings. No tests were modified.
- All 56 benchmark cases completed: 32 managed matching, 16 native matching and eight construction cases. BenchmarkDotNet elapsed time was 422.94 seconds. Raw [execution output](benchmark.txt), [managed validation](validation.json), [native validation](native-validation.json), [test output](tests.txt), and complete [JSON/CSV/Markdown/HTML reports](results/) are retained.

The four patterns and repeated inputs do not characterize Unicode, case folding, path semantics, extended globs, high-diversity pattern streams or adversarial complexity. MemoryDiagnoser observes managed allocations, not native allocations. Native pattern-construction latency is not measured. Rankings should not be generalized beyond these limits. The managed website snapshot remains a separate historical run.

## Reproduction

Run [Run-Comparisons.ps1](../../src/Tedd.WildcardMatch.Benchmark/Run-Comparisons.ps1) from the repository. The [benchmark guide](../../src/Tedd.WildcardMatch.Benchmark/README.md#native-cc-comparison) documents prerequisites and API lifetimes. The runner copies sources, builds native and managed code, validates results, and runs the suite under `D:\Workspaces\AI\wildcard-native-benchmarks`. Select another available CPU through `-Affinity` when necessary. Upstream revisions and hashes are retained in [native-sources.json](native-sources.json) and [native-source-hashes.json](native-source-hashes.json).

Results are committed on the original `main` branch. The task-owned temporary workspace is removed after evidence retention; reproduction recreates it.
