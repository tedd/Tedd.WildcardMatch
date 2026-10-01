"""Normalize complete-engine measurements for the standard performance report."""
import datetime
import hashlib
import json
from pathlib import Path
import statistics as stats
import sys

sys.stdout.reconfigure(encoding="utf-8")
RUN = Path(__file__).resolve().parent
REPO = RUN.parent.parent
documents = [json.loads((RUN / "raw" / f"{prefix}-{launch}.json").read_text(encoding="utf-8"))
             for prefix in ("benchmark", "compiled") for launch in range(1, 4)]
samples = [sample for document in documents for sample in document["samples"]]
names = list(dict.fromkeys(s["workload"] for s in samples))

def values(name, mode, engine):
    return [s["ns"] for s in samples if s["workload"] == name and s["mode"] == mode and s["engine"] == engine]

def metric(name, mode, engine):
    v = values(name, mode, engine)
    assert len(v) == 54
    return dict(median=stats.median(v), min=min(v), max=max(v), n=len(v))

def row(name, mode):
    before, after = [metric(name, mode, engine) for engine in ("Regex", "Direct")]
    after["changePercent"] = 100 * (after["median"] / before["median"] - 1)
    allocation = [stats.median(s["bytes"] for s in samples if s["workload"] == name and s["mode"] == mode and s["engine"] == engine) for engine in ("Regex", "Direct")]
    return dict(label=name, detail=f"allocation {allocation[0]:g} → {allocation[1]:g} B/call", measurements=[before, after])

summary_rows = {mode: [row(name, mode) for name in names] for mode in ("static", "reused", "construction", "reused-compiled")}
paired = []
for mode in summary_rows:
    for name in names:
        for launch in range(1, 4):
            effects = []
            for block in range(9):
                group = [s for s in samples if s["mode"] == mode and s["workload"] == name and s["launch"] == launch and s["block"] == block]
                medians = {engine: stats.median(s["ns"] for s in group if s["engine"] == engine) for engine in ("Regex", "Direct")}
                effects.append(100 * (medians["Direct"] / medians["Regex"] - 1))
            paired.append(dict(mode=mode, workload=name, launch=launch, medianEffectPercent=stats.median(effects), minEffectPercent=min(effects), maxEffectPercent=max(effects), blocks=effects))
(RUN / "raw" / "paired-effects.json").write_text(json.dumps(paired, indent=2), encoding="utf-8")
(RUN / "raw" / "summary.json").write_text(json.dumps(summary_rows, indent=2), encoding="utf-8")
simple = row("simple", "static")["measurements"]
scorecards = []
for name in ("simple", "long-sparse", "complex", "unicode"):
    before, after = row(name, "static")["measurements"]
    scorecards.append(dict(title=name + " static", value=after["median"], baseline=before["median"], unit="ns/call", wallChangePercent=after["changePercent"], progression="Regex → direct; 54 unfiltered samples per engine"))
series = [dict(title=title, description="Medians across three fresh processes; whiskers span all 54 samples per engine. Same build and runtime within each chart, no filtering or overhead subtraction.", unit="ns/call", lowerIsBetter=True, rounds=["Regex", "Direct"], rows=summary_rows[mode])
          for mode, title in (("static", "Complete static calls"), ("reused", "Reused ordinary instances"), ("construction", "Construction plus one match"), ("reused-compiled", "Reused compiled Regex control"))]
headline = f"Simple static calls: {simple[0]['median']:.1f} → {simple[1]['median']:.1f} ns ({simple[1]['changePercent']:+.1f}%). Default direct matching allocates no buffers. Compiled Regex remains faster for some reused workloads."
data = dict(title="Direct wildcard engine: correctness and performance", generatedAt=datetime.datetime.now(datetime.timezone(datetime.timedelta(hours=2))).isoformat(),
    status="complete", scope="Requested engine rewrite and validation, rather than an exhaustive search for all optimization opportunities. Direct WildcardMatch and preserved WildcardMatchRegex; .NET Standard 2.1/.NET 10/.NET 11 source targets, timed on .NET 10 x64.",
    question="Can a direct iterative wildcard engine preserve the Regex contract and improve common complete calls while avoiding per-match buffers?",
    primaryMetric="ns/call", summary=dict(baseline=simple[0]["median"], final=simple[1]["median"], unit="ns/call", changePercent=simple[1]["changePercent"], headline=headline),
    scorecards=scorecards, series=series, effectGroups=[],
    hypotheses=[dict(id="H-001", claim="A direct iterative engine improves common complete calls over ordinary Regex", mechanism="Remove translation, Regex lookup/interpreter and exponential star backtracking", prediction="Repeatable lower static time and zero default match buffers", falsification="Differential mismatch or no repeatable caller benefit", change="Replace WildcardMatch with a direct engine; preserve the original class as WildcardMatchRegex and expose both extensions.", changeScope="refactor", maintainability="Additional independently implemented engine and runtime semantic paths require differential validation; no package dependency, unsafe storage, mutable cache, recursion or match buffer.", codeComments="WildcardEngine documents latest-star invariants, newline anchors, culture groups, bulk scans and timeout checkpoints.", result=headline, effectPercent=simple[1]["changePercent"], state="retained", decision="Adopted", decisionRationale="Requested rewrite passes independent/exhaustive/fuzz checks on four runtimes and improves all measured static and ordinary reused cases. Compiled Regex remains available for workloads where specialization wins.", evidence=["raw/summary.json", "raw/paired-effects.json", "plan.md"], risks=["Polynomial worst-case retries remain; the direct engine is not universally faster than compiled Regex."]),
        dict(id="H-002", parentId="H-001", claim="Bulk fixed literal runs address scalar long-prefix/suffix costs", mechanism="Library search and ordinal bulk comparison", prediction="Lower long-sparse work while preserving '?' and newline semantics", falsification="Boundary mismatch or complete-call regression", change="For case-sensitive patterns of length at least 32, compare literal prefix/suffix runs in bulk before star retries.", changeScope="local", maintainability="Safe indexed string operations and one routing guard; no new storage.", codeComments="Adjacent bulk-path comment links this run and explains keeping short patterns scalar.", result="Final long-sparse comparisons and Tier1 library calls support the integrated path. Initial/final build epochs differ, so an isolated numeric effect is not established.", state="inconclusive", decision="Integrated; isolated effect inconclusive", decisionRationale="Keep as part of the validated H-001 implementation, not as a separately quantified improvement. Initial source/results remain archived.", evidence=["raw/initial-engine/", "disassembly/engine.asm.txt", "raw/summary.json"], risks=["Length 32 is a routing choice, not a universal optimum; constituent effects are not isolated."]),
        dict(id="H-003", claim="The direct engine beats reused compiled Regex across all fixtures", mechanism="Compare generic iterative matching with compiled specialization", prediction="Lower reused time for every fixture", falsification="Credible compiled Regex advantage on any fixture", change="Benchmark the same public APIs with Compiled on the preserved Regex engine and direct engine.", changeScope="local", maintainability="Preserved Regex engine provides an explicit alternative; Compiled does not add compilation to the direct engine.", codeComments="Constructor remarks and option XML describe engine-specific flags.", result="Compiled Regex is faster on some repeated-pattern cases; full numbers and variability are shown above.", state="rejected", decision="Universal superiority rejected", decisionRationale="Report workload-dependent tradeoffs; do not hide the specialized Regex control.", evidence=["raw/compiled-1.json", "raw/compiled-2.json", "raw/compiled-3.json"], risks=[])],
    retainedChanges=["Direct, iterative prefix/suffix and latest-star matching with constant auxiliary state. No recursive Regex or wildcard engine fallback.",
        "Public API names preserved; Regex implementation exposed as WildcardMatchRegex. WildcardRegex remains diagnostic and lazy on the direct engine.",
        "Capture constructor-time case culture, preserve Unicode lowercase groups, LF/dollar anchors, numeric option validation, and .NET 11 AnyNewLine/CRLF semantics. Normalize .NET 11 whitespace-mode VT once for instances.",
        "Cooperative finite timeout with amortized checkpoints and original RegexMatchTimeoutException payload; thread-safe shared instances and lazy diagnostic publication.",
        "Unit tests, independent DP/Regex oracles, generated matches/near misses, exhaustive small inputs, culture/Unicode/numeric flag/boundary/timeout/concurrency/large-input/allocation tests.",
        "Documentation describes both engines; comparative compiled modes explicitly use WildcardMatchRegex. Published external-library snapshot remains labeled with its original Regex source revision."],
    decisions=["No decision required. This completes the requested rewrite; it does not claim exhausted optimization potential.", "Use the explicit Regex engine when compiled per-pattern specialization is beneficial. Benchmark construction and reuse for the actual workload."],
    environment=[dict(label="Baseline/build epoch", value="Initial source 0f0784d; concurrent documentation/packaging commits a1cd860/fd4c9e5 preserved. Final comparison uses contemporaneous direct/Regex types in the current net10.0 build. Initial data used netstandard2.0/SDK 10 and is not cross-compared numerically."),
        dict(label="Engine SHA-256", value=hashlib.sha256((REPO / "src/Tedd.WildcardMatch/WildcardEngine.cs").read_bytes()).hexdigest()),
        dict(label="Compiler/runtime", value="SDK 11.0.100-rc.1.26425.128; timed workers .NET 10.0.12 x64, runtime-default tiering/PGO, Concurrent Workstation GC."),
        dict(label="Host", value="AMD Ryzen 9 5950X, Windows 11; logical CPU 30 pinned. Active desktop interference remains possible."),
        dict(label="Protocol", value="13 deterministic fixtures, static/reused/construction plus separate reused compiled control. Three fresh processes, 200ms joint warmup, common count calibrated on Regex to >=20ms, nine ABBA/BAAB blocks. 54 samples per engine/case/mode; all retained."),
        dict(label="Culture", value="Compiled-control records capture current culture; earlier ordinary records omitted this field. Both engines run in the same process/culture. Tests explicitly cover invariant/en-US/tr-TR/az-Latn-AZ."),
        dict(label="Lock/storage", value="All builds/tests/timed/native-code work serialized by the skill's fixed OS lock. Disposable files only under D:\\Workspaces\\AI\\wildcard-engine; durable logs/data here.")],
    hotspots=[], areas=[dict(name="Execution and preparation", boundary="WildcardMatch/Regex public calls", evidence="Complete-call and constructor measurements above; no external consuming application supplied.", catalogue="Engine rewrite; constant-memory state, allocation removal and bulk scans", hypothesisIds=["H-001", "H-002", "H-003"], disposition="Requested engine delivered; caller-specific control retained")],
    correctness=["Release and Debug: 209 tests each on .NET 8 and .NET 10, 212 tests on .NET 11; additional .NET 9 Release run: 209 tests. Actual package references use Standard 2.1 for older runtimes and the matching modern target for .NET 10/11.",
        "945,252 exhaustive LF-mode pattern/input comparisons per test run, checked against an independent dynamic-programming oracle and both engines; additional 378,004 Unicode-newline comparisons on .NET 11.",
        "60,000 seeded Unicode fuzz trials across four cultures, including generated matches/near misses and all entry points; 10,000 long segment trials; every BMP casing candidate and isolated surrogate/combining/supplementary inputs.",
        "All declared options and numeric Regex values through 4096, null/error precedence, timeout validation/payload, construction culture capture, parallel reuse, bulk thresholds and long adversarial inputs are checked.",
        "Default static/reused matching measured at zero managed bytes/call; dedicated allocation test also passes. Optional VT normalization, construction, diagnostic property access and exception paths may allocate.",
        "Package comparative corpus validation passes; current runtime Regex probes remain independent of the direct engine."],
    generatedCode=[dict(hypothesisId="H-002", summary="Optimized MatchWindow calls runtime bulk search/CompareOrdinal for fixed runs; scalar wildcard retries remain.", before="Initial direct engine had scalar fixed-run checks (source archived); build epochs differ.", after="Tier1 output includes LastIndexOfAnyValueType and String.CompareOrdinal. No explicit ISA or range-check-elimination claim.", artifact="disassembly/engine.asm.txt")],
    validityThreats=["Generic direct matching does not beat every compiled specialization. Reused compiled Regex is faster on some dense, case-insensitive and complex cases; the compiled control is separately displayed.",
        "Common counts are chosen using the slower ordinary Regex engine, so direct blocks can be shorter than 20ms. Equal delegate/loop overhead is included; no empty-loop subtraction or outlier filtering. Interpret small differences cautiously.",
        "Fixed fixtures favor caches and branch prediction; static pattern cardinality is below Regex.CacheSize. No application traffic, cache-churn, contention throughput or cold-process startup claim.",
        "Only .NET 10 x64 is timed. Correctness on .NET 8/9/10/11 does not establish equivalent speed on other runtimes/architectures or future preview releases.",
        "Initial/final compiler and target epochs differ because concurrent authorized work changed package targets. Final ratios are within epoch; the bulk path's isolated numeric effect is inconclusive.",
        "Constant auxiliary memory does not imply linear time: ordinary retries are O(n*m), and Multiline line-start searches can add an input-length factor. Timeout is cooperative and may overshoot within bulk library calls.",
        "Unicode and Regex parser/option rules are runtime-version contracts. .NET 11 AnyNewLine and whitespace-mode vertical tabs were discovered by differential testing; future changes need the same oracle checks.",
        "Failed/withdrawn fixture and build results remain archived. Existing analyzer/style and transitive test-package warnings do not indicate test failures; successful logs remain available."],
    reproduction=["Run the inspected driver under the skill's run_with_performance_lock.py: python <skill>/scripts/run_with_performance_lock.py --cwd <repo> -- python <this-directory>/run.py all.",
        "Run phase compiled separately for the compiled-Regex comparison; phases tests/runtime-matrix/comparison/benchmark/diagnostics allow bounded reproduction. All uses the installed pinned SDK 11 preview and actual current targets.",
        "Run python build_report.py, then the skill's render_report.py report-data.json report.html. The standard renderer owns HTML formatting."],
    artifacts=[dict(label="Method, corrections and epochs", path="plan.md"), dict(label="Reproduction driver", path="run.py"), dict(label="Timed fixtures and observable loop", path="rig/Program.cs"),
        dict(label="Normalized comparisons", path="raw/summary.json"), dict(label="Per-launch paired block effects", path="raw/paired-effects.json"), dict(label="All raw measurements, test/build logs and initial engine", path="raw/"),
        dict(label="Tier0/Tier1 native code", path="disassembly/engine.asm.txt"), dict(label="Report generator", path="build_report.py")])
(RUN / "report-data.json").write_text(json.dumps(data, ensure_ascii=False, indent=2), encoding="utf-8")
print(headline)
for mode, rows in summary_rows.items():
    print(mode, [(r["label"], round(r["measurements"][1]["changePercent"], 1)) for r in rows])
