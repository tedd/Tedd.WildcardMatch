# DotNet.Glob comparison continuation

User target: at least twice DotNet.Glob throughput, motivated by the 13-code-unit
`report-??.txt` fixture. Primary acceptance is prepared direct versus prepared
DotNet.Glob on the unchanged four-input Simple corpus and a varied equal-length
corpus. Static calls, construction, literal/multiple-star/long fixtures and losing
contrasts remain explicit; passing one fixture does not establish a universal lead.
Default Unicode code-unit, LF/end-anchor, option, timeout and exception semantics
remain unless a separately measured contract change requires a decision.

Baseline is production C# at fe4fff6, with current dirty project metadata captured.
Existing uncommitted README/website/comparative-benchmark/version changes belong to
another work stream and are preserved. Their snapshot and the user's screenshot
have different measurements and are not a paired experimental epoch.

Same-process control/candidate/Glob caller batches use identical delegate dispatch
and observable counts. ABCCBA/CB AABC orders rotate by block; common calibration,
warmup, CPU 30 and three processes. Screening: four blocks >=5ms per sample;
acceptance: nine blocks >=20ms per sample, all samples retained. Fresh deterministic
inputs supplement repeated website seeds. Independent DP/Regex validation precedes
timing; Glob is excluded on fixtures where its different semantics fail the oracle.
Construction and retained state are charged separately. No application trace exists,
so results are local caller results. Builds/tests/profiles/timing run only under the
fixed OS performance lock. All disposable state stays under
`D:/Workspaces/AI/wildcard-glob`; Research contains durable evidence only.

Initial hypotheses (the existing ledger has H-001 through H-031):

| ID | Mechanism | Falsification / principal risk |
|---|---|---|
| H-032 | Separate infinite default execution from timeout/options state | no complete-call gain; branch changes |
| H-033 | Prepare no-star fixed width and reject length before traversal | equal-width misses do not improve; final LF semantics |
| H-034 | Packed 64-bit literal/question masks | no gain after safe tails; endian and overread |
| H-035 | Two overlapping Vector128 masked comparisons | no caller gain; wildcard LF lanes and exact bounds |
| H-036 | Vector256 masked blocks for longer fixed patterns | loses to 128-bit/tails; register pressure |
| H-037 | Form question masks dynamically for static calls | mask preparation exceeds scalar cost |
| H-038 | Infinite latest-star loop plus intrinsic first-character search | rescans/regression on dense/near-miss inputs |
| H-039 | Prepared star-separated segments and intrinsic run search | setup/segment-object cost or short-pattern regressions |
| H-040 | Compact literal-mask bit-parallel NFA | lookup cost exceeds comparisons; epsilon closure |
| H-041 | Prepared ASCII bitmap lookup table | memory/setup or non-ASCII fallback cost dominates |
| H-042 | Prepared literal bypass of general clock/header | neutral versus current optimized comparison |
| H-043 | Generic timeout-mode specialization | JIT/code-size cost exceeds eliminated checks |
| H-044 | Bounded static pattern-plan cache | lookup/synchronization overhead, cold/churn regressions |
| H-045 | Two-anchor intrinsic candidate-position filtering | poor selectivity and tiny strings lose |
| H-046 | Fuse LF rejection with SIMD wildcard masks | added reduction costs exceed saved scans |
| H-047 | Portable MemoryMarshal packed-mask implementation | portable safety/bounds costs erase improvement |
| H-048 | SIMD position bitmap for short star-separated segments | mask extraction/setup exceeds scalar/IndexOf |
| H-049 | Optional simpler text contract separate from Unicode/Regex modes | no material default-path cost attributable to Unicode |
| H-050 | Krauss-style common prefix loop and lazy retry initialization | no gain over existing latest-star traversal |

Catalogue coverage: CPU C1-C4, memory M3-M5/M7, storage S1/S2/S4/S5,
runtime R1/R2 are candidate sources. M1/M2 are cost constraints (ordinary matches
currently allocate zero); no measured pointer chasing/prefetch, stencil, frozen
collection lookup, dynamic shared mutation, lock contention or scheduling workload
supports M6/C5/S3/R3/T1-T4 changes. Cache candidates must add their own ownership,
publication, churn and retained-memory evidence rather than assume those irrelevant.

Sources: Kirk Krauss' FastWildCompare article uses NUL-terminated input and explicit
retry positions. A length-aware adaptation needs embedded-NUL, LF, slice and timeout
coverage; the article does not establish universal optimality. Runtime Vector128/
Vector256 contracts require complete in-range loads and hardware guards. ISA paths
may be compiled only for net10/net11, with the netstandard2.1 fallback tested.

Protocol v2, declared after H-032 (before its results are used for adoption):
per-engine sample duration calibration replaces common invocation counts. The
RetryNearMiss ratio above 100x made common counts yield dramatically unequal sample
durations. Each engine still receives exactly the same 256-input batch, normalized
per match, observable counts, ABCCBA order and >=5ms/20ms samples. This is a distinct
epoch; only its contemporaneous controls establish effects. It also adds a long
mixed-anchor near-miss contrast to distinguish H-045/H-048 mechanisms. The original
four website fixtures remain unchanged. Program-v1.cs.txt preserves the earlier rig.

Adaptive H-052: H-037's dynamic SIMD removes most fixed traversal, exposing repeated
static option/normalization work. A known None route can avoid those checks without
altering validation for other values; predict reduced static caller cost, falsified
by neutral/noisy measurements or any exception/normalization mismatch.
Adaptive H-053: Fixed is a separate generated-code boundary in H-037. Force inline
only that helper, predict elimination of fixed-kernel forwarding cost; reject if
caller size/layout regresses other fixtures or the gain remains within variance.

Adaptive H-054: H-036 makes dense fixed-width matching roughly 7x faster, but uses
parallel Pattern/Mask arrays. One immutable Block array with ref readonly iteration
can remove one allocation and one array-length/ownership relation. Predict lower
setup footprint and neutral/faster complete dense calls. This is a simplification
candidate; inspect actual Tier1 copies and reject regressions beyond run variation.

H-044 additionally enables a predeclared cold/churn contrast: 256 distinct 13-unit
patterns alternate continuously through a two-slot thread-local cache. Validation
precedes measurement; per-call allocation is recorded. This contrast has only
contemporaneous control/candidate static string/span methods. Prepared Glob is
not a lifetime-equivalent comparison for per-call pattern changes.

Adaptive H-051: fixed SIMD still dereferences the pattern string merely to obtain
its immutable width. Cache that integer in the prepared plan to remove one dependent
load. Predict a small short-mask caller gain without extra allocated bytes; scalar
fallbacks must not regress materially. Source/layout inspection plus paired timing
will distinguish this from unsupported generic prefetch speculation.

Adaptive H-056: H-048's two-anchor SIMD cuts mixed near misses about 15x, but its
constructor uses List growth and ToArray. Count nonempty segments once and allocate
one exact immutable array. Predict fewer constructor bytes and no matching change;
inspect caller timing because value-array layout/PGO can still move unrelated costs.

Adaptive H-055: H-041 achieves >2x Glob on MultiStar but uses a 128-entry (1KiB)
ASCII table. Build a bounded collision-free low-bit table over pattern literals,
verify the full UTF-16 code unit on every lookup, and retain no Dictionary or ASCII
restriction. Predict lower preparation/retention cost with comparable short calls.
Reject latency regressions or hash-collision false positives. Unsupported literal
sets fall back to the established engine.
Adaptive H-057: H-041/H-055 traverse every input unit, severely regressing long
inputs. Bound NFA dispatch to <=32 units and use the proven clock-free engine for
larger inputs. Predict preservation of short MultiStar gains and long intrinsic
search performance. This explicit hybrid is a refactor candidate, not a claim that
NFA is universally superior; complete-caller and setup evidence govern adoption.

Final integration retains the supported mechanisms, with default ordinal/timeout
dispatch, fixed masks, bounded short-input bitmap state and long compiled anchors.
Plan footprint is capped at 32 KiB vector payload and 64 segment descriptors for
patterns <=8192 units. Preserve H-020 bulk leading questions: long leading-?? star
patterns use DefaultWindow; all-question segments bulk-test LF. Unicode code-unit,
NUL, final-LF, null/option precedence and borrowed-input semantics are unchanged.
The final rig additionally declares QuestionLong (65536 units) and constructor-only
allocation counters; the four website fixtures and all earlier epochs stay intact.

H-055 failed its standalone strict latency criterion: 21.61 ->23.96 ns on MultiStar
versus H-041, despite 1272 ->288 constructor bytes. Its compact encoding is carried
into H-057, whose bounded dispatcher is adopted with an explicit setup/footprint
tradeoff and independently validated full integration. It is not a retained
unbounded replacement. H-046 executes separated literal/LF reductions (initial
table label said fusion); H-045 is scalar second-anchor rejection. H-047/H-050 are
duplicates, H-043/H-049 not applicable. H-051/H-052/H-053 remain inconclusive and
are omitted. Other classifications and raw parent controls appear in report data.

Final paired acceptance uses default tiering/PGO, three launches and 54 unfiltered
samples per engine/caller. First BDN corroboration exposed instrumented Tier0
profiling helpers and is retained as diagnostic evidence. Second BDN epoch sets
DOTNET_TieredCompilation=0, confirms the optimized caller, and retains every raw
measurement plus BDN's standard filtered statistics separately. It does not share
a baseline with default-PGO timing. Additional acceptance-bit63 tests run after
the complete framework matrix; no production C# changes follow final timing.

Stopping condition: the primary prepared question-mark target is exceeded in both
independent epochs; all evidence-supported bounded catalogue branches have a
disposition. No remaining material hypothesis is justified within this caller
scope. Small layout/inline/header effects remain inconclusive under observed
variance. Wider ISA/ARM64 and application throughput require additional hardware
or a representative application trace. Static calls and earliest-literal misses
do not establish a universal 2x lead. Construction costs remain explicit.

Raw launch JSON is losslessly gzip archived with original SHA256 and byte counts
in raw-archive.json; summarize.py reads both formats. No samples are discarded.
All production publication remains outside this local commit under the user's
current specific-production-write authorization rule.
