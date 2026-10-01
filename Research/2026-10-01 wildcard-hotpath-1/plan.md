# Baseline epoch E1 and preregistered hypotheses

Date: 2026-10-01 (Europe/Oslo). Source: f3e8565. Initial checkout clean.

Question: which removable mechanism limits the public static wildcard matcher, and can a local change reduce ns/call and managed bytes/call without changing its Regex contract?

Scope: .NET Standard 2.0 library on installed .NET 10 x64. No consuming application is supplied; claims are confined to this library's complete public calls. Preserve exact WildcardRegex text, UTF-16 processing, null exception parameter names, Regex option validation, current-culture casing, newline anchors, and timeout behavior. No new dependencies, shared mutable caches, unsafe access, or public contract changes. Instance matching is a control: only construction can benefit from translation changes.

Equal priority: empty, short literal, repository simple and complex patterns, miss, escaped controls/metacharacters, Unicode, 1024-character literal, and long mixed pattern. Screen medians across nine alternating-order trials. Adoption requires repeatable complete-call improvement above noise for the targeted distributions, no credible >5% regression in other complete-call cases, and proportionate allocation/code burden. No universal gain threshold. Confirm selected implementation with two independent BenchmarkDotNet launches, 5 warmups and 10 measured iterations each. Record uncertainty rather than treat screen ratios as final evidence.

Initial ten hypotheses (independent mechanisms, source InternalUtils.StringToWildcard; callers WildcardMatch.IsMatch and WildcardMatchExtensions.IsWildcardMatch):

| ID | Mechanism / prediction | Falsification |
|---|---|---|
| H-001 | Fuse escape, replacement, anchors into one StringBuilder traversal, eliminating intermediate strings. | Does not reduce mixed-pattern cost or allocation. |
| H-002 | Preallocate builder upper bound to avoid growth/copying (parent H-001). | Growth savings fail to justify extra capacity or long-literal regression. |
| H-003 | Use bounded char array and one final string, removing builder object/chunks. | Extra zeroed buffer/size arithmetic erase benefit. |
| H-004 | Count exact output length before allocation (parent H-003). | Added pass outweighs reduced buffer footprint. |
| H-005 | IndexOfAny literal fast path skips buffer on ordinary text (parent H-003). | Scan overhead/regressions on wildcard cases exceed benefit. |
| H-006 | Append ordinary runs in bulk (parent H-002). | Classification and run tracking outweigh reduced Append calls. |
| H-007 | ASCII classification lookup table replaces switch/range dispatch (parent H-003). | Lookup bounds/load cost outweighs dispatch benefit. |
| H-008 | Preserve Regex.Escape and fuse the two replacements/anchors. | Escape allocation and builder cost still dominate. |
| H-009 | Return constant anchored empty regex (parent H-003). | New branch has material nonempty regression. |
| H-010 | Remove forced inlining when a larger translator inflates callers. | Native code already emits a call/no duplication; premise then inapplicable. |

Each implementation must agree with the immutable Regex.Escape + Replace oracle before timing. Individual variants remain in the durable rig even if rejected. Subsequent evidence, dispositions, and catalogue coverage belong to report-data.json in this same run directory.

## E2 follow-up from E1 screening

The first screen exposed a 7–17x long-literal regression in scalar builders/arrays, while table classification beat switch dispatch on wildcard distributions. H-011 combines table conversion with a two-character IndexOfAny('*', '?') bypass to Regex.Escape + anchors for patterns without wildcards. H-012 competes with H-011 by using two separate character searches, permitting short-circuiting when a star is present. Both predict preservation of baseline literal performance with wildcard conversion gains; a credible complete-call regression above 5% falsifies adoption. E2 changes only the screening calibration to choose repetitions per method (~10ms) rather than inherit the converter's count for an 85us complex match. E1 is retained separately and ratios are compared only within an epoch.

The mixed sampled profile places 92.84% exclusive time in RegexInterpreter.TryMatchAtCurrentPosition. Changing the runtime Regex engine is outside this library's compatible local scope. Short-call profiling is added as a workload contrast, without implying that it represents the time-weighted mixed workload.

## Integrated candidate

E2 supports H-003/H-007/H-011: table-based bounded array construction plus the no-wildcard bypass. H-012 is a competing search strategy with mixed effects; it adds a second search and is not selected without a clear advantage. H-013 combines the independently screened empty constant (H-009) with H-011; prediction: reduce empty-call work while keeping other caller changes within noise. Production integration removes forced inlining (H-010) from the now-large translator, pending Tier1 caller confirmation. The complete integrated implementation remains provisional until production differential checks, library tests and independent BDN launches pass.

## E4 external-validity follow-up

The retained literal bypass protects zero-wildcard inputs but not long, mostly literal patterns containing one to four wildcards. Add long-prefix, long-suffix, long-pair, and clustered-four-wildcard fixtures as adversarial workload contrasts. H-014 counts up to five wildcard occurrences with library IndexOfAny for patterns at least 128 characters; preserve the baseline bulk escape/replace path when at most four occur. H-015 isolates skipping Replace calls for wildcard kinds absent from those sparse inputs. Prediction: restore long-sparse caller performance while preserving dense wildcard gains; >5% credible regression or allocation growth beyond baseline falsifies adoption. Counts/length are explicit experimental routing parameters, not claimed universal thresholds. H-010 compares the actual translator with a generated identical AggressiveInlining clone; the clone is disposable and the generator is versioned.

## E5 acceptance environment

The E4 screen confirms a 66% long-prefix regression in the provisional scalar implementation. H-015 restores the long-sparse cases, preserves dense-pattern improvement against the original control, and avoids an unnecessary Replace scan relative to H-014. Integrate that sparse fallback. Acceptance and final screening use a staged build of the actual netstandard2.0 project (packaging disabled only), rather than source compiled into a net10.0 executable. A generated internal-visibility attribute permits correctness probes without modifying production assembly API. Pin timed worker execution to logical CPU 30 to reduce migration variance on this active desktop. This is a new baseline epoch, with contemporaneous baseline/final measurements; no cross-epoch raw ratio is used. The inlining screen now has matched non-inlined wrapper controls, removing the extra call confound in E4. A single lock acquisition spans tests, final screen, BDN acceptance and final profiles/disassembly.

## E6 API-identical acceptance correction

E5 BDN used a semantically equivalent two-argument Regex control compiled in net10.0. That does not reproduce the original library's actual caller/inlining/overload context, so its timing claim is withdrawn from acceptance. Preserve its full output under raw/E5-control. E6 stages two netstandard2.0 assemblies: the unchanged baseline helper from f3e8565, and the current production implementation. An extern alias distinguishes identical public types. Both Before and After now invoke the actual respective static APIs. Increase iteration target to 300ms after E5 reported several sub-100ms actual iterations and multimodal desktop interference. Constructor-plus-one-match benchmarks additionally charge preparation; no claim is made about an external application.

The first E6 BDN generated build collided because both project filenames mapped to the same ArtifactsPath intermediate directory. It executed zero benchmarks despite returning process exit code zero. Preserve failure logs under raw/E6-build-failure; use a distinct Baseline csproj filename and require nonempty Statistics for every expected exported case before accepting results. This tooling repair does not change fixtures or production code. Record the successful locked measurement window in raw/measurement-window.json.

## E7 sparse-routing boundary audit

H-016: an absolute four-wildcard cutoff misclassifies 1024 literals plus five questions as dense. Add long-five-prefix and long-five-suffix to expose that counterexample, and correctness cases at lengths 127/128/129/256/1024 with zero through six prefix/suffix wildcards. Competing implementation caps counting at length/32 + 1, preserving bulk conversion when density is at most one wildcard per 32 code units. Prediction: avoid five-wildcard sparse regressions without losing baseline-relative dense gains. This is a concrete distribution correction, not an arbitrary threshold sweep. First screen the new mechanism; only integrate if it addresses the counterexample, then remeasure every final API case.

The screen confirms that the absolute cutoff regresses long-five-prefix/suffix by 54%/66%; H-016 restores both to baseline and retains a large baseline-relative dense-pattern gain. Integrate relative density; extend boundary correctness through 34 wildcard tokens. Chronological E6 BDN has severe multimodal/nonstationary host effects (including long-pair medians differing >2x despite an identical regex). Keep all E6 measurements and classify noise-sensitive timing as inconclusive. E7 acceptance uses the same real baseline/final APIs in three fresh processes with ABBA/BAAB blocks, nine blocks per case, common operation counts and >=20ms target blocks. No sample/outlier is removed. Record delegate/loop calibration as a lower bound, and charge constructor-plus-one-match separately. This protocol correction controls temporal drift rather than choosing the favorable BDN results.

Evidence preservation correction: one early E4 screening file was overwritten during an initial followup before timestamped archival was implemented. Its contemporaneously reported median long-prefix result is recorded above; the original unfiltered E4 trials are unavailable. All candidate implementations remain reproducible, E1/E2/E3/E5/E6 evidence is retained, and E7 re-executes every candidate with complete raw trials. Subsequent raw outputs are timestamp-archived automatically; final claims use the complete E7 records.

Execution: scripts and test callbacks contain local computation only; no database or production writes. All compilation, restore, corpus generation, validation, warmup, sampling and disassembly run under the fixed skill lock. Disposable rig copies, package/CLI temp files, traces and build products remain under D:\Workspaces\AI\wildcard-hotpath. Durable report/evidence alone remain here.

## Final disposition (2026-10-02)

E7 complete-call comparison retains the guarded combination: simple median 647.2 to 447.8 ns/call (224 to 128 managed B/call), dense long-mixed median 4203.4 to 1403.1 ns/call. All fifteen static fixtures and two construction fixtures have 54 unfiltered samples per method across three processes. The largest per-launch median paired regression is 4.3%, within the preregistered 5% limit; small overlapping effects remain inconclusive. All 47 xUnit tests and 1,164,576 rig differential assertions pass. Full paired block/launch effects are normalized in raw/paired-effects.json.

Final H-010 isolation uses the same three-argument Regex overload in both wrappers. Timing is mixed: normal versus forced inlining improves dense long-mixed by 34.7%, while long-prefix/suffix favor forcing by 9.9%/5.9% in this one-process screen. Normal optimized caller size is 39 bytes versus 1107 bytes forced. Remove the forced hint to constrain duplication in the integrated implementation; do not infer a universal inlining speedup. Overall integration acceptance is against the actual original API, including this choice.

Stopping condition: sixteen materially distinct hypotheses have dispositions, every optimization catalogue category has an evidence-based coverage entry, and final caller/construction/profile/native-code validation is complete. Within the recorded safe, dependency-free, contract-preserving local policy, further mechanisms would duplicate tested branches or require shared retained-pattern state, unsafe storage, retargeting, or a different matching contract. Those broader opportunities are not claimed exhausted. No arbitrary hypothesis-count cutoff was used.
