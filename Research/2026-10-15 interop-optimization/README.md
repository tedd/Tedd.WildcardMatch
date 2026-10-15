# Wildcard optimization with optimized native interop — 2026-10-15

Final production source epoch 4; representative caller confirmation epoch 5. The first timing epoch is confounded and excluded. After the user stopped competing benchmarks, epoch 2 reran all 76 original cases. Its unresolved dense-input guard led to a substantive anchor-verification change; the final run contains 110 cases, including both native buffer-storage modes. No observations are pooled across epochs.

Reused Tedd matching has the lowest measured mean on every original fixture.
The final 110-case run meets the 20% mean-cost target on all four fixtures. Against the stronger retained native build, MultiStar is 18.33% lower, so that stricter reference does not meet the 20% target uniformly. The other three fixtures remain more than 20% lower against that reference.

| Workload | Tedd reused, ns ± error | Fastest competitor and mode | Competitor, ns ± error | Lower mean cost |
| --- | ---: | --- | ---: | ---: |
| Literal | 5.233 ± 0.002 | Krauss / batch_shared | 9.325 ± 0.003 | 43.88% |
| Simple | 4.562 ± 0.001 | Krauss / batch_cloned | 9.557 ± 0.007 | 52.26% |
| MultiStar | 6.622 ± 0.062 | Krauss / batch_shared | 9.582 ± 0.042 | 30.89% |
| LongText | 183.406 ± 1.898 | Krauss / batch_shared | 234.287 ± 0.598 | 21.72% |

Error is BenchmarkDotNet’s 99.9% interval half-width. The 20% criterion concerns means, not a confidence guarantee of a 20% effect. The [complete matrix](results-table.md) includes all libraries and call/storage modes; [unrounded values and retained iteration samples](matching-summary.json) accompany the [raw output](benchmark.txt).

The primary API retains a prepared matcher. Static Tedd calls still classify the pattern on each call and remain slower than Krauss on the short fixtures. Construction costs are reported separately. This is a ranking of the tested implementations, corpus, lifetimes and machine; it does not establish a universally fastest wildcard library.

## Source and mechanism

- Prepared 3–4-unit segments followed by a literal tail use masked UTF-16 words and a bounded vector tail search on eligible short inputs. Other shapes use the existing bitmap/segment engines; the scalar fallback remains supported. Every four-unit read is within the borrowed span, including unaligned slices.
- Constructor-selected managed strategy pointers keep the fallback route compact, while literal, fixed and prepared short paths remain directly visible to the caller.
- Default non-Singleline bitmap plans without literal LF defer interior-LF rejection until acceptance, outside the recurrence.
- A successful literal-anchor search proves those characters. Candidate verification checks the surrounding positions, including question-mark newline rules. Prefix and suffix checks without that search proof still compare their complete segments.

The final code is in the source freeze and delivery hashes. [Generated matcher code](final-jit.asm) and [interop call-site code](final-interop-jit.asm) bind observations to the actual optimized runtime. These listings support code-path review; they do not establish cache-miss, branch-misprediction or per-instruction timing claims. The [post-restart ledger](post-restart-candidates.md) records every further candidate, including rejected ordinal-search, scalar-mask, dispatch-order, minimum-length and subsequence alternatives. Earlier [candidate ratios](candidates.md) are explicitly confounded and are not adoption evidence.

## Interop and storage controls

Native batch calls make one ordinary transition per 256 matches. Per-input normal and suppressed sites call the identical native export for each storage mode. Suppression applies only to the fixed short Krauss/Git/Zema fixtures; PCRE2 and long calls retain normal transitions. Qualification follows [Microsoft’s requirements](https://learn.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.suppressgctransitionattribute?view=net-10.0): bounded sub-microsecond work, without blocking, callbacks, exceptions or locks. The whole batch is not suppressed.

The trivial transition control measured 3.271 ns normally and 1.505 ns with suppression. This difference is not subtracted from other measurements. Real matcher costs and differences are in the complete table.

`ShareInputs=false` retains one owned native string per row. `true` resolves repeated values to the same prepared string, matching the managed seed-reference reuse. Both modes execute every match; no results are cached. Both storage modes are measured for native batching and per-input calls, and every mode is eligible for the fastest-competitor baseline.

Encoding, native allocation, regex translation/JIT compilation and pattern preparation occur outside matching timing. Managed inputs use UTF-16; native inputs use NUL-free ASCII bytes. Loop, dispatch and interop costs remain included. This prepared-throughput comparison does not measure end-to-end encoding or one-shot construction.

## Confirmation and correctness

The fresh normal-build portfolio ratio is 0.8953; the targeted multi-star ratio is 0.3632, candidate/original control. All 17 predefined noninferiority guards pass. All sessions are correct, joined and quiescent, with zero measured per-match managed allocation.

The [confirmation table](confirmation.md) and [exact samples/IDs](confirmation-results.json) retain individual primary classifications. Equivalent guards are not labeled speedups. Source adoption requires useful portfolio and multi-star benefit and every mandatory 5% noninferiority guard; the [assessment](adoption-assessment.json) reports those gates explicitly.

- 753 tests pass: 250 on .NET 8, 250 on .NET 10, 253 on .NET 11. Eight boundary/anchor checks also pass with hardware intrinsics disabled. New cases use an independent dynamic-programming oracle and exercise LF/final-LF, embedded NUL, unpaired surrogates, span offsets and positions outside searched anchors.
- Managed validation covers 8,192 corpus checks. Native validation covers 136,192 logical oracle pairs across both storage modes, plus batch/per-input count consistency, including the qualified suppressed paths. See [tests](tests.txt), [scalar checks](scalar-tests.txt), [managed validation](validation.json), [native validation](native-validation.json) and [build output](build.txt).

## Method and limits

- Ryzen 9 5950X, Windows 11 build 26200.9711, CPU 30 affinity; BenchmarkDotNet 0.15.8, .NET 10.0.13 x64 RyuJIT, tiering disabled. Three launches, three warmups, ten 100 ms measured iterations per launch. All 110 cases complete; raw output preserves BenchmarkDotNet’s outlier handling and prefilter measurements.
- DotNetLab confirmation uses .NET 10.0.12 x64 managed IL, sealed normal bundles, identical scenario callers and dependency/runtime configuration. Twenty randomized paired fresh-process blocks per scenario, two warmups, seed 20261015, family error 0.05 across 17 comparisons. Short/portfolio batches use 3,000,000 operations; long batches 300,000; dense anchors 512; newline 1,000; GeneralShort 30,000,000. Both normal-build arms use High process priority.
- A named host lock serializes this task’s builds, tests and measurements. The user stopped known competing benchmarks before the restart. Operating-system load, frequency and thermal drift remain uncontrolled; distinct processes and repeated inner operations do not prove stochastic independence. Confirmation intervals assume the declared paired log-ratio model.
- Four small seed sets repeated to 256 rows do not characterize arbitrary wildcard syntax, Unicode performance, case folding, pattern churn or adversarial native complexity. Managed allocation measurements do not quantify native allocation. No individual-call p99 or application-level latency claim is made.

## Provenance and reproduction

Native revisions and licenses remain those in the [native comparison discovery record](../2026-10-15%20native-comparison/README.md#candidates-and-discovery); [current pins](native-sources.json) and [source freeze](source-freeze.json) identify this run. MSVC 19.52.37025.0 uses C++17 Release `/O2 /Ob2 /DNDEBUG /MD`, Windows SDK 10.0.22621.0 and PCRE2’s pinned SLJIT with default CPU detection. The final source and benchmark hashes are in [delivery-source-hashes.json](delivery-source-hashes.json).

Run `src/Tedd.WildcardMatch.Benchmark/Run-Comparisons.ps1` with a dedicated `D:\Workspaces\AI\<name>` directory and an available CPU affinity. The [benchmark guide](../../src/Tedd.WildcardMatch.Benchmark/README.md) documents setup and modes. [Post-restart evidence](post-restart.zip) contains source snapshots, drivers, full controller transcripts, exact plans/grants/operations, sealed artifacts and a verified catalog backup. [Confounded epoch 1](confounded-epoch1.zip) is retained separately for audit. [Archive hashes](evidence-archives.json) verify the retained files. Re-execution requires fresh plans and grants; frozen accepted operations are not extra sampling slots.

Integration branch: `main`. Task-owned scratch workspaces are removed after evidence retention and integration.

## Earlier native-build reference

The adapter rebuild changed observed native costs, particularly on MultiStar. The following conservative reference retains the lowest earlier unconfounded native mean. These are separately identified cross-build/epoch comparisons; their samples are not pooled with the final experiment and they do not supply a combined confidence interval. No hardware cause is inferred.

| Workload | Earlier native mean ns | Final Tedd mean ns | Lower mean cost |
| --- | ---: | ---: | ---: |
| Literal | 9.326 (Krauss, epoch3) | 5.233 | 43.89% |
| Simple | 9.552 (Krauss, epoch3) | 4.562 | 52.24% |
| MultiStar | 8.108 (Krauss, epoch2) | 6.622 | 18.33% |
| LongText | 233.062 (Krauss, epoch2) | 183.406 | 21.31% |

Unrounded source identities are in [prior-native-reference.json](prior-native-reference.json). Earlier shared-caller families retain their unresolved dense, broad-pattern, or static-long guard. In epoch 4, static LongText was 1.0497 [1.0356, 1.0640]; in the representative delegate caller it is 1.0120 [1.0034, 1.0206]. The production code is identical across these last two regimes. This comparison does not establish a hardware cause or convert the earlier result into a pass.

Epoch 5 uses the delegate dispatch of the actual comparative benchmark, with scenario version 2 and a separately frozen twenty-block, High-priority budget. Its [caller source](normal-caller.cs), [baseline code](delegate-caller-baseline.asm), and [candidate code](delegate-caller-candidate.asm) are retained. ExecuteBatch emits 161 bytes in each normal bundle and contains one selected delegate call, without the prior instance/static loop branch. No earlier samples were pooled or replaced.
