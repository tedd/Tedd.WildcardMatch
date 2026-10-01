"""Normalize durable results and generate input for the skill's standard report renderer."""
import datetime as dt
import hashlib
import json
from pathlib import Path
import statistics as stats
import sys

sys.stdout.reconfigure(encoding="utf-8")

RUN = Path(__file__).resolve().parent
REPO = RUN.parent.parent
raw = RUN / "raw"
screen = json.loads((raw / "screen-followup.json").read_text(encoding="utf-8"))
inlining = json.loads((raw / "inlining.json").read_text(encoding="utf-8"))
bench = json.loads((raw / "Acceptance-report-full-compressed.json").read_text(encoding="utf-8"))
construction = json.loads((raw / "ConstructionAcceptance-report-full-compressed.json").read_text(encoding="utf-8"))

def screen_values(case, variant):
    return [r["ns"] for r in screen if r["workload"] == case and r["variant"] == variant]

def measurement(values):
    return dict(median=stats.median(values), min=min(values), max=max(values), n=len(values))

def ratio(case, variant, control="control"):
    return stats.median(screen_values(case, variant)) / stats.median(screen_values(case, control)) - 1

def paired_effect(case, variant, control="control"):
    by_id = {(r["variant"], r["trial"]): r for r in screen if r["workload"] == case}
    effects = [by_id[(variant, i)]["ns"] / by_id[(control, i)]["ns"] - 1 for i in range(9)]
    return dict(effect=stats.median(effects), range=[min(effects), max(effects)])

def normalize(document):
    cases = {}
    launch_data = []
    for b in document["Benchmarks"]:
        case = b["Parameters"].split("=", 1)[1].strip('"')
        cases.setdefault(case, {})[b["Method"]] = b
        for launch in sorted({m["LaunchIndex"] for m in b["Measurements"]}):
            values = [m["Nanoseconds"] / m["Operations"] for m in b["Measurements"]
                      if m["LaunchIndex"] == launch and m["IterationMode"] == "Workload" and m["IterationStage"] == "Result"]
            if not values:
                overhead = [m["Nanoseconds"] / m["Operations"] for m in b["Measurements"]
                            if m["LaunchIndex"] == launch and m["IterationMode"] == "Overhead" and m["IterationStage"] == "Actual"]
                correction = stats.median(overhead) if overhead else 0
                values = [max(0, m["Nanoseconds"] / m["Operations"] - correction) for m in b["Measurements"]
                          if m["LaunchIndex"] == launch and m["IterationMode"] == "Workload" and m["IterationStage"] == "Actual"]
            if values:
                launch_data.append(dict(case=case, method=b["Method"], launch=launch, **measurement(values)))
    return cases, launch_data

chronological_cases, chronological_launches = normalize(bench)
chronological_build, chronological_build_launches = normalize(construction)
paired_documents = [json.loads((raw / f"paired-{launch}.json").read_text(encoding="utf-8")) for launch in range(1, 4)]
paired_samples = [sample for document in paired_documents for sample in document["samples"]]
def normalize_paired(kind):
    cases = {}
    launches = []
    for name in dict.fromkeys(s["workload"] for s in paired_samples if s["kind"] == kind):
        cases[name] = {}
        for method in ("Before", "After"):
            samples = [s for s in paired_samples if s["kind"] == kind and s["workload"] == name and s["variant"] == method]
            cases[name][method] = dict(Statistics=dict(OriginalValues=[s["ns"] for s in samples]), Memory=dict(BytesAllocatedPerOperation=stats.median(s["bytes"] for s in samples)))
            for launch in range(1, 4):
                launches.append(dict(case=name, method=method, launch=launch, **measurement([s["ns"] for s in samples if s["launch"] == launch])))
    return cases, launches
cases, launches = normalize_paired("static")
build_cases, build_launches = normalize_paired("construction")
block_effects = []
launch_effects = []
for kind, group in (("static", cases), ("construction", build_cases)):
    for name in group:
        for launch in range(1, 4):
            effects = []
            for block in range(9):
                samples = [s for s in paired_samples if s["kind"] == kind and s["workload"] == name and s["launch"] == launch and s["block"] == block]
                medians = {method: stats.median(s["ns"] for s in samples if s["variant"] == method) for method in ("Before", "After")}
                effect = 100 * (medians["After"] / medians["Before"] - 1)
                effects.append(effect)
                block_effects.append(dict(kind=kind, case=name, launch=launch, block=block, before=medians["Before"], after=medians["After"], effectPercent=effect))
            launch_effects.append(dict(kind=kind, case=name, launch=launch, medianEffectPercent=stats.median(effects), minEffectPercent=min(effects), maxEffectPercent=max(effects)))
worst_launch_effect = max(r["medianEffectPercent"] for r in launch_effects)
assert worst_launch_effect <= 5, "Integrated launch-level regression exceeds the preregistered limit"
(raw / "paired-effects.json").write_text(json.dumps(dict(blocks=block_effects, launches=launch_effects, worstLaunchMedianPercent=worst_launch_effect), indent=2), encoding="utf-8")
(raw / "chronological-launch-summary.json").write_text(json.dumps(dict(static=chronological_launches, construction=chronological_build_launches), indent=2), encoding="utf-8")
(raw / "launch-summary.json").write_text(json.dumps(dict(static=launches, construction=build_launches), indent=2), encoding="utf-8")

def benchmark_row(name, group):
    pair = group[name]
    before = measurement(pair["Before"]["Statistics"]["OriginalValues"])
    after = measurement(pair["After"]["Statistics"]["OriginalValues"])
    after["changePercent"] = 100 * (after["median"] / before["median"] - 1)
    alloc = [pair[side]["Memory"]["BytesAllocatedPerOperation"] for side in ("Before", "After")]
    return dict(label=name, detail=f"managed allocation {alloc[0]:g} → {alloc[1]:g} B/call", measurements=[before, after])

rows = {name: benchmark_row(name, cases) for name in cases}
short = ["empty", "literal", "simple", "miss", "escaped", "unicode"]
long = ["long-literal", "long-mixed", "long-prefix", "long-suffix", "long-pair", "long-cluster", "long-five-prefix", "long-five-suffix", "complex"]

definitions = [
    ("H-001", None, "Fuse escaping, replacements and anchors in a StringBuilder", "M7/C2", "Builder", "rejected", "Superseded", "Removes intermediate strings, but scalar literal scanning and builder/chunk allocation lose to the bounded-array strategy. Standalone long-literal regression is unacceptable."),
    ("H-002", "H-001", "Preallocate builder capacity to remove chunk growth", "M1/M7", "Preallocated", "rejected", "Superseded", "Reduces growth on wildcard inputs but does not fix literal scanning; overprovisioned chunks retain a larger allocation burden than arrays."),
    ("H-003", None, "Use a bounded char array and one final string", "M1/M7", "MaxArray", "retained", "Adopted with guards", "Retained only in dense/short wildcard paths with H-007/H-011/H-015. The unrestricted array variant regresses long literals and sparse wildcards; standalone adoption was rejected."),
    ("H-004", "H-003", "Count exact output length before allocation", "M1/M7", "ExactArray", "rejected", "Rejected", "The second classification traversal lowers bytes modestly but loses time against the upper-bound array; complete caller priorities favor the single pass."),
    ("H-005", "H-003", "Detect all escape characters before a literal bypass", "C3/R1", "LiteralFast", "rejected", "Superseded", "Avoids long-literal scalar emission but scans a broad escape set and still regresses short literals. H-011 reuses Regex.Escape and searches only wildcard tokens."),
    ("H-006", "H-002", "Append ordinary runs in bulk", "M7/C2", "Runs", "rejected", "Rejected", "Run tracking reduces Append calls but still classifies every input character; the long-literal scan remains slower than library bulk scanning."),
    ("H-007", "H-003", "Use an ASCII escape-classification table", "R1/C2", "TableArray", "retained", "Adopted with guards", "The table outperforms the switch-classified array on targeted wildcard cases. Unicode is guarded before indexing; only the tiny immutable classification table is retained."),
    ("H-008", None, "Keep Regex.Escape but fuse replacements and anchors", "M7", "EscapedFusion", "rejected", "Rejected", "An escaped intermediate plus scalar builder traversal retains allocation/scan costs and fails to beat direct emission on the targeted patterns."),
    ("H-009", "H-003", "Return the constant anchored regex for empty patterns", "M1/R2", "EmptyFast", "retained", "Adopted in H-013", "Removes the empty-pattern result allocation. The nonempty array path alone is not adopted; the constant is combined with guarded literal/dense paths."),
    ("H-010", None, "Remove forced inlining from the enlarged translator", "R2", "generated clone", "retained", "Adopted; timing mixed", "Matched wrapper timing has mixed effects: normal dense conversion improves, while some sparse cases favor forcing. Tier1 caller size is 39 bytes normally versus 1107 bytes forced. Retain the compact caller to limit duplication; the full integrated baseline comparison, not this one-process screen, establishes acceptance. No universal inlining or branch-miss claim is made."),
    ("H-011", "H-007", "Bypass wildcard rewriting when no wildcard occurs", "C3/R1/M7", "NoWildcardFast", "retained", "Adopted", "A two-character IndexOfAny plus Regex.Escape preserves bulk literal scanning and the exact escape contract. Wildcard inputs continue to use fused conversion."),
    ("H-012", "H-011", "Use two short-circuited character searches instead", "C3/R1", "SeparateSearch", "inconclusive", "Not adopted", "Search-order benefits vary by fixture and epoch; no stable advantage warrants maintaining a second selection strategy."),
    ("H-013", "H-011", "Combine the empty constant and literal/dense paths", "R1/M1", "Combined", "retained", "Adopted with H-015", "Independent component screens justify the combination, but E4 found a 66% long-prefix regression. The final combination includes H-015; H-013 alone is not claimed acceptable."),
    ("H-014", "H-013", "Use bulk baseline conversion on long sparse wildcards", "C3/R1/M7", "SparseFallback", "rejected", "Superseded by H-015", "Restores sparse-pattern scanning, but still invokes Replace for wildcard kinds that are absent. H-015 retains the fallback mechanism while eliminating those unnecessary scans."),
    ("H-015", "H-014", "Skip replacement scans for absent wildcard kinds", "C3/R1/M7", "SparseSelective", "retained", "Adopted with H-016", "Record star/question presence and invoke only needed replacements. The fixed four-token cutoff is not retained: H-016 fixes its five-token sparse counterexample."),
    ("H-016", "H-015", "Use relative wildcard density for the sparse fallback", "C3/R1/M7", "SparseDensity", "retained", "Adopted", "The fixed cutoff regresses long-five-prefix/suffix by 54%/66% in its falsifying screen. Relative density (at most one wildcard per 32 code units) restores bulk conversion for both and retains large baseline-relative dense gains. The cutoff is measured routing, not a universal optimum."),
]

hypotheses = []
for id_, parent, claim, category, implementation, state, decision, rationale in definitions:
    workloads = ["simple", "long-literal", "long-prefix", "long-mixed", "escaped"]
    result = []
    for name in workloads:
        control = "static-H-010" if id_ == "H-010" else "control"
        candidate = "static-noinline-control" if id_ == "H-010" else id_
        base = measurement([r["ns"] for r in inlining if r["workload"] == name and r["variant"] == "forced"]) if id_ == "H-010" else measurement(screen_values(name, control))
        candidate_measurement = measurement([r["ns"] for r in inlining if r["workload"] == name and r["variant"] == "normal"]) if id_ == "H-010" else measurement(screen_values(name, candidate))
        result.append(dict(workload=name, baseline=base, candidate=candidate_measurement,
                           effectPercent=100 * (candidate_measurement["median"] / base["median"] - 1)))
    hypothesis = dict(id=id_, claim=claim, mechanism=category, source="src/Tedd.WildcardMatch/InternalUtils.cs:StringToWildcard",
        caller="WildcardMatch.IsMatch, WildcardMatchExtensions.IsWildcardMatch, WildcardMatch constructor",
        observation="Escape/Replace sampled costs and allocation; literal and sparse-pattern regressions in earlier screens. See plan.md for chronology.",
        prediction="Reduce ns/call or bytes/call on the affected workloads beyond noise, with no credible >5% regression against original complete calls.",
        falsification="Semantic difference, no repeatable targeted gain, disproportionate change burden, or credible complete-call regression beyond the preregistered limit.",
        change=f"Experiment Variants.{implementation} in rig/Program.cs; production adoption restricted as stated in the verdict.",
        changeScope="moderate" if id_ in ("H-013", "H-015") else "local",
        maintainability="Safe, dependency-free code; added branching/buffer/escape-set invariants require the durable report and exhaustive oracle tests." if state == "retained" else "Candidate remains reproducible in the rig; its extra production implementation is not retained.",
        codeComments="Adjacent production comments reference the run and retained hypotheses; trivial empty return needs no additional commentary." if state == "retained" else "No production candidate retained; experiment is explained here and in plan.md.",
        result="; ".join(f"{r['workload']}: {r['baseline']['median']:.1f} → {r['candidate']['median']:.1f} ns/call ({r['effectPercent']:+.1f}%)" for r in result),
        measurements=result, effectPercent=result[0]["effectPercent"], state=state, decision=decision,
        decisionRationale=rationale, evidence=(["raw/inlining.json", "disassembly/asm-inlining.txt"] if id_ == "H-010" else ["raw/screen-followup.json"]) + ["rig/Program.cs", "plan.md"],
        risks=["Screening timings are kernel results except matched H-010 wrappers; acceptance applies to the integrated complete API, not every individual component."])
    if parent: hypothesis["parentId"] = parent
    hypotheses.append(hypothesis)

coverage = [
    ("M1", "Per-call result/buffer allocations", ["H-002", "H-003", "H-004", "H-009"], "Compared builder, upper-bound array, exact array and empty constant. Retained result strings must remain owned; buffer reuse would add ownership/state and pooling dependencies. Native/uninitialized allocation exceeds the safe local change policy."),
    ("M2", "Temporary translation buffer", [], "Stack emission needs unsafe pointer-based string construction or a Span package for netstandard2.0. Outside the preregistered dependency-free safe policy; no stack speedup is claimed."),
    ("M3", "String and output-array indexes", ["H-003", "H-004"], "Safe bounded loops inspected in Tier1; output range-check helper edges remain. No evidence attributes a material caller share to these guards separately from allocation/emission."),
    ("M4", "Remaining bounds checks", [], "No unsafe lifetime/range transformation adopted. Removing guards with raw pointers changes the declared safety/maintenance policy; potential is not claimed exhausted."),
    ("M5", "Library object layout", [], "Three references in a reusable wrapper; no measured object graph traversal or wide-record stream to reorganize."),
    ("M6", "Pattern read and buffer write", [], "Sequential small strings/arrays; no latency-bound indirect-address or software-prefetch evidence."),
    ("M7", "Escape/Replace/Concat intermediates", ["H-001", "H-003", "H-004", "H-006", "H-008", "H-011", "H-015"], "Measured removal/fusion and library bulk scanning; no byte codec, fixed-width serialized record or overlap-copy kernel for packed stores."),
    ("C1", "Sequential emission cursor", [], "Output cursor depends on token width; no independent numerical reductions or independent useful accumulators. No latency/throughput claim from timing alone."),
    ("C2", "Wildcard and escape dispatch", ["H-001", "H-006", "H-007", "H-013", "H-015"], "Literal, wildcard, control-character and Unicode distributions contrast branch/dispatch behavior. No hardware branch counters available; do not infer misses."),
    ("C3", "Bulk literal/wildcard scanning", ["H-005", "H-011", "H-012", "H-014", "H-015", "H-016"], "Reuse runtime vectorized searches/escaping; relative-density route corrects a measured fixed-count counterexample. Explicit AVX2/Vector256 requires retargeting or new dependency for this netstandard2.0 library, contrary to compatibility policy."),
    ("C4", "Token classification", ["H-007"], "No divisions, bit reductions, floating-point math or byte-endian work to strength-reduce. Table dispatch is covered under R1."),
    ("C5", "Translator", [], "No grid, neighborhood or stencil updates."),
    ("S1", "Classifier", ["H-007"], "Tiny ASCII bool table tested; no bulk membership, entity-signature or bitmap-update workload."),
    ("S2", "Regex pattern cache", [], "Lookup is owned by runtime RegexCache; conversion has no dictionary lookup duplication. Additional translation caches add retained patterns and shared mutation excluded by the declared local policy."),
    ("S3", "Read-only classifier", ["H-007"], "Direct 128-entry array replaces hashing. Frozen collections are unavailable in netstandard2.0 and unnecessary for this bounded domain."),
    ("S4", "Repeated conversion and instance preparation", ["H-011", "H-015"], "Measured conversion plus construction. Existing reusable instance already amortizes preparation. No mutable query plan or invalidation protocol; no new pattern cache policy introduced."),
    ("S5", "Consecutive wildcard runs", [], "Collapsing stars or rewriting greedy quantifiers changes observable WildcardRegex text and possibly timeout/backtracking behavior; incompatible with the fixed semantic contract."),
    ("R1", "Stable ASCII token decisions", ["H-007", "H-011", "H-013", "H-015"], "Table and specialized routing tested; no per-character reflection or delegate discovery in production."),
    ("R2", "AggressiveInlining and public callers", ["H-010"], "Identical forced-inline clone versus matched normal wrappers; captured optimized callers. No large value structs, boxing or virtual calls in production matcher."),
    ("R3", "Options", [], "Options are per-call/per-instance API inputs, not startup-frozen feature flags; freezing them would change semantics."),
    ("T1", "Immutable library state", [], "No independently mutated cache-line fields or scaling evidence."),
    ("T2", "Matching ownership", [], "Instance Regex/runtime cache own their concurrency protocol; translator has only call-local storage and immutable tables."),
    ("T3", "Synchronization", [], "No library locks/spin loops; rewriting runtime RegexCache synchronization is outside this library's implementation."),
    ("T4", "One synchronous match", [], "No queues or I/O. Parallelizing one small translation adds scheduling and changes resource use without a supported caller workload."),
]
areas = [dict(name=category, boundary=location, evidence=disposition, catalogue=f"optimize-code {category}",
              hypothesisIds=ids, disposition="Tested" if ids else "Not applicable within declared policy") for category, location, ids, disposition in coverage]

effect_groups = []
for name in ("simple", "long-literal", "long-prefix", "long-mixed"):
    effect_groups.append(dict(name=f"E7 conversion screen: {name}", control=f"Baseline converter {stats.median(screen_values(name, 'control')):.1f} ns/call; nine paired-order trials, pinned CPU 30.",
        items=[dict(hypothesisId=h[0], label=h[0] + " " + h[4], **paired_effect(name, h[0]), note="kernel only")
               for h in definitions if h[0] != "H-010"]))

summary = rows["simple"]["measurements"]
scorecards = []
for name in ("simple", "escaped", "long-mixed", "empty"):
    pair = cases[name]
    before, after = rows[name]["measurements"]
    before_bytes = pair["Before"]["Memory"]["BytesAllocatedPerOperation"]
    after_bytes = pair["After"]["Memory"]["BytesAllocatedPerOperation"]
    scorecards.append(dict(title=name, value=after["median"], baseline=before["median"], unit="ns/call", wallChangePercent=after["changePercent"],
                          secondaryMetric="managed allocation", secondaryChangePercent=100 * (after_bytes / before_bytes - 1),
                          progression=f"{before_bytes:g} → {after_bytes:g} B/call"))

source = REPO / "src" / "Tedd.WildcardMatch" / "InternalUtils.cs"
data = dict(title="Wildcard translation hot-path investigation", generatedAt=dt.datetime.now(dt.timezone(dt.timedelta(hours=2))).isoformat(),
    status="complete", scope="Safe dependency-free local optimization of the netstandard2.0 library, executed on .NET 10 x64. Complete static calls and construction-plus-one-match; no external consuming application supplied.",
    question="Can fused wildcard translation remove measured Escape/Replace work while preserving exact regex text, options, null exceptions, culture and newline behavior, without credible >5% caller regressions?",
    primaryMetric="ns/call", summary=dict(baseline=summary[0]["median"], final=summary[1]["median"], unit="ns/call", changePercent=summary[1]["changePercent"],
    headline=f"Simple complete static call: {summary[0]['median']:.1f} → {summary[1]['median']:.1f} ns, {summary[1]['changePercent']:+.1f}%; allocation 224 → 128 B. Long/sparse and noise-sensitive results are shown individually; no aggregate application speedup is claimed."),
    scorecards=scorecards,
    environment=[dict(label="Baseline revision", value="f3e8565; initial checkout clean. Only helper implementation changes; baseline helper snapshot versioned."),
        dict(label="Final helper SHA-256", value=hashlib.sha256(source.read_bytes()).hexdigest()),
        dict(label="Acceptance epoch", value="E7: actual original/candidate netstandard2.0 assemblies, distinct assembly names/extern alias; SDK 10.0.401. Investigation allocated 2026-10-01; completed 2026-10-02."),
        dict(label="Runtime", value=".NET 10.0.12, x64 RyuJIT; tiered compilation/PGO defaults enabled; Concurrent Workstation GC. No diagnostic tiering override used for acceptance."),
        dict(label="CPU/ISA", value="AMD Ryzen 9 5950X, 16 cores/32 logical processors; AVX2/BMI1/BMI2/FMA/SSE/POPCNT available; runtime reports VectorSize=256."),
        dict(label="OS/power/affinity", value="Windows 11 26200.9457, High performance power scheme; E6/E7 workers pinned to logical CPU 30."),
        dict(label="Primary comparison", value="Three fresh processes; nine ABBA/BAAB blocks per case, common count, >=20ms calibration target, 500ms joint warmup per case. 54 samples per method/case. No sample removed; delegate/loop lower-bound calibration " + ", ".join(f"{d['overheadNs']:.2f} ns" for d in paired_documents) + ". Includes observable checksum and managed allocation accounting."),
        dict(label="BDN diagnostic comparison", value="E6: two independent BDN launches, five warmups/ten target measurements, 300ms target. Severe multimodal/nonstationary host effects; this chronological comparison is not the adoption basis. Full unfiltered measurements remain linked."),
        dict(label="Host isolation", value="Fixed OS-backed C:\\Users\\tedd\\.codex\\locks\\performance-measurement.lock covers builds/tests/warmup/profiling/measurement. Active desktop apps remain uncoordinated; CPU-delta snapshots retained."),
        dict(label="Fixture/epoch evolution", value="E1 common-count screen; E2 per-variant counts; E3 integrated candidate; E4 sparse fixtures; E5 target/pinning/surrogate control; E6 actual API and 300ms BDN; E7 density boundary and interleaved drift control. E5 BDN withdrawn; E6 timing noise-sensitive."),
        dict(label="Build/storage", value="Disposable builds/traces under D:\\Workspaces\\AI\\wildcard-hotpath; versioned evidence here. Packaging disabled in staged csproj; internal visibility enabled only in experimental copy.")],
    hotspots=[dict(name="RegexInterpreter.TryMatchAtCurrentPosition", location=".NET Regex runtime", inclusivePercent=94.3, exclusivePercent=93.67,
                   evidence="E2 mixed six-fixture CPU sampling, raw/profile-top-before.txt", limit="Backtracking dominates the time-weighted complex workload; runtime engine changes are outside the fixed contract/local policy."),
              dict(name="RegexParser.EscapeImpl", location="InternalUtils.StringToWildcard → Regex.Escape", inclusivePercent=18.35, exclusivePercent=13.72,
                   evidence="E2 short-workload CPU sampling, raw/profile-short-top-before.txt", limit="Repeated escaping and intermediate strings."),
              dict(name="String.Replace", location="InternalUtils.StringToWildcard", inclusivePercent=14.32, exclusivePercent=6.48,
                   evidence="E2 short-workload CPU sampling; ReplaceHelper listed separately; inclusive/exclusive shares are not summed.", limit="Two replacement traversals and intermediate strings.")],
    areas=areas,
    series=[dict(title="Complete static API — short cases (E7)", description="Medians across three independent interleaved processes; whiskers span all 54 raw samples per method. No filtering; includes equal delegate/loop overhead. Lower is better.", unit="ns/call", lowerIsBetter=True, rounds=["Before", "After"], rows=[rows[n] for n in short]),
            dict(title="Complete static API — long/adversarial cases (E7)", description="Same epoch and iteration policy. Interpret small/overlapping effects cautiously on this active desktop.", unit="ns/call", lowerIsBetter=True, rounds=["Before", "After"], rows=[rows[n] for n in long]),
            dict(title="Construction plus one match (E7)", description="Interleaved actual original/final instance APIs; includes translation, Regex construction, wrapper allocation and matching, plus identical delegate/loop overhead.", unit="ns/call", lowerIsBetter=True, rounds=["Before", "After"], rows=[benchmark_row(n, build_cases) for n in build_cases])],
    effectGroups=effect_groups, hypotheses=hypotheses,
    retainedChanges=["Fused bounded char-buffer emission and ASCII escape lookup for short/dense wildcard patterns; exact UTF-16 output and initialized-prefix proof documented adjacent to code.",
        "Keep bulk Regex.Escape for patterns without wildcards, and selective bulk Replace for long sparse patterns at wildcard density at most 1/32. Empty pattern returns a constant anchored regex.",
        "Remove forced inlining from the enlarged translator; preserve small public wrapper hints. No public API, Regex options, dependency, target framework or matching engine changed.",
        "New independent oracle tests preserve every UTF-16 code unit, randomized cultures/options, escapes, long inputs and null exception parameter names."],
    decisions=[f"Largest per-launch median paired effect: {worst_launch_effect:+.2f}%; below the preregistered +5% regression limit. Every block and launch is retained in raw/paired-effects.json. Small/overlapping effects, including complex matching, remain inconclusive.",
        "No adoption decision requested within the declared safe local policy. Pattern caches, unsafe emission, retargeting and a custom matching engine remain broader opportunities, not exhausted potential.",
        "Stopping condition: sixteen distinct local hypotheses have tested dispositions, every catalogue category is covered, and integrated caller/constructor checks completed. Fewer than twenty because this library contains one translation helper; additional candidates would require a different state/safety/dependency/compatibility policy or duplicate an already falsified mechanism."],
    correctness=["47 xUnit tests pass in Release against the actual netstandard2.0 project, including prefix/suffix routing boundaries for lengths 127/128/129/256/1024 and zero through 34 wildcards.",
        "Research rig reports 1,164,576 candidate/reference assertions per execution, plus production translation comparisons for all 65,536 UTF-16 code units for each variant.",
        "Independent oracle uses original Regex.Escape + two literal Replace calls, not the optimized classification/emission logic. Random seed 19373; en-US and tr-TR; IgnoreCase, Singleline, RightToLeft, CultureInvariant and IgnorePatternWhitespace.",
        "Result copy uses only [0,position); allocation capacity is checked 2*inputLength+2; every input unit emits at most two chars. Escaping excludes literal closing bracket/brace exactly as Regex.Escape does.",
        "Null wildcard preserves Regex.Escape parameter name str. Existing timeout/options validation and matching continue in unchanged Regex code."],
    generatedCode=[dict(hypothesisId="H-007", summary="ASCII classification is a guarded table load; control remapping and wildcard dispatch remain scalar. Array bounds guards remain; no elimination claim.", before="Switch/range branches in Variants.Array's non-table path.", after="Guarded escape table lookup, initialized output stores, then string constructor; full Tier0/Instrumented Tier0/Tier1 retained.", artifact="disassembly/asm-after.txt"),
        dict(hypothesisId="H-010", summary="Normal translator remains a call in optimized ProductionCaller; forced clone is expanded into ForcedCaller. Both wrappers use the same three-argument Regex overload.", before="ForcedCaller integrates the translator body into a 1107-byte caller.", after="ProductionCaller is 39 bytes, with a call to StringToWildcard followed by Regex.IsMatch.", artifact="disassembly/asm-inlining.txt"),
        dict(hypothesisId="H-011", summary="Keep library scanning rather than manually scalarizing long literal input.", before="Baseline generated code exposes runtime AVX2 search paths and two String.Replace calls.", after="Literal route retains Regex.Escape and eliminates both replacements; dense route avoids Escape/Replace intermediates.", artifact="disassembly/asm-before.txt")],
    validityThreats=["Active desktop interference, CPU frequency/thermal variation and fixed before/after BDN order can confound small differences. Pinning and the cooperative lock cannot silence unrelated processes; raw host snapshots and all BDN diagnostics remain visible.",
        "Fixed repeated fixtures favor cache/predictor training. Static cache cardinality is below Regex.CacheSize; constructor benchmarks cover preparation, but no external workload or high-cardinality cache churn is supplied.",
        "Screening boxes bool results uniformly (+24 B) and invokes delegates; final allocation claims use the interleaved harness returning bool directly and thread allocation counters. BDN is supporting diagnostic evidence. Screen comparisons are within epoch; no cross-epoch raw timing ratio is used.",
        "E6 BDN default outlier removal and multimodal warnings must be considered; chronological method ordering produced implausibly large swings on identical regexes. E7 uses interleaving and no filtering; launch-summary.json exposes independent processes. Neither method establishes a universal application gain.",
        "Mixed CPU sampling is dominated by the complex fixture; short profiling is a workload contrast, not a sum of exclusive shares or evidence of a universal application gain.",
        "Only the installed .NET 10 x64 configuration is timed. netstandard2.0 API compatibility is preserved, but earlier runtime/other-ISA performance is unmeasured.",
        "Length 128 and density 1/32 are measured routing choices, not universal optima. Near-maximum .NET string sizes/OutOfMemory timing are not exercised; checked output capacity rejects integer overflow safely.",
        "An early E4 unfiltered screening file was overwritten before archival was fixed. Its reported 66% long-prefix median regression is preserved in plan.md; E7 repeats every candidate with full raw trials. Final claims use complete E7 evidence.",
        "Source lookup: dotnet/runtime v10.0.0 RegexParser.Escape and its escape set; semantic confidence additionally comes from local differential tests against executing .NET 10.0.12. https://github.com/dotnet/runtime/blob/v10.0.0/src/libraries/System.Text.RegularExpressions/src/System/Text/RegularExpressions/RegexParser.cs"],
    reproduction=["From an inspected checkout, run python C:\\Users\\tedd\\.codex\\skills\\scientific-method-performance\\scripts\\run_with_performance_lock.py --cwd D:\\Workspaces\\AI\\wildcard-hotpath -- python \"<this-run>\\run.py\" finish.",
        "Use run.py validate for primary acceptance: tests, full candidate screen, three interleaved API/constructor processes, matched inlining screen and final profiles/native code. run.py measure additionally reproduces the BDN diagnostic sequence. Windows x64 with SDK 10.0.401 and dotnet-trace is assumed.",
        "Baseline profile reproduction: same wrapper and run.py diagnostics-before; use run.py before for a fresh baseline screen. Do not compare epochs if compiler/runtime/workloads change.",
        "After results, run python build_report.py, then the skill's scripts/render_report.py report-data.json report.html. Renderer owns the standard HTML template; do not hand-edit generated report."],
    artifacts=[dict(label="Preregistered method and epoch chronology", path="plan.md"), dict(label="Reproducible rig and all candidate implementations", path="rig/Program.cs"),
        dict(label="Execution/staging driver", path="run.py"), dict(label="Baseline source snapshot", path="baseline/InternalUtils.cs.txt"),
        dict(label="Primary interleaved comparison — launch 1", path="raw/paired-1.json"), dict(label="Primary interleaved comparison — launch 2", path="raw/paired-2.json"), dict(label="Primary interleaved comparison — launch 3", path="raw/paired-3.json"),
        dict(label="Noisy API-identical E6 static BDN JSON", path="raw/Acceptance-report-full-compressed.json"), dict(label="Noisy E6 construction BDN JSON", path="raw/ConstructionAcceptance-report-full-compressed.json"),
        dict(label="Independent launch summary", path="raw/launch-summary.json"), dict(label="Paired block effects and launch acceptance", path="raw/paired-effects.json"), dict(label="Complete API screening and allocations", path="raw/screen-followup.json"),
        dict(label="Earlier epochs and withdrawn surrogate-control results", path="raw/"), dict(label="Correctness log", path="raw/tests.txt"),
        dict(label="Final short-workload profile", path="raw/profile-short-top-after.txt"), dict(label="Final mixed-workload profile", path="raw/profile-top-after.txt"),
        dict(label="Optimized native code and diagnostic tiers", path="disassembly/"), dict(label="Report data/generation", path="build_report.py")])
(RUN / "report-data.json").write_text(json.dumps(data, ensure_ascii=False, indent=2), encoding="utf-8")
print(data["summary"]["headline"])
for name, row in rows.items():
    before, after = row["measurements"]
    print(f"{name}: {before['median']:.2f} → {after['median']:.2f} ns ({after['changePercent']:+.2f}%)")
