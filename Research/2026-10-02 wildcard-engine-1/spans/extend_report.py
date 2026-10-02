"""Append the borrowed-text epoch to the immutable previously delivered report."""
import datetime
import json
from pathlib import Path
import subprocess
HERE = Path(__file__).resolve().parent
RUN = HERE.parent
REPO = RUN.parent.parent
data = json.loads(subprocess.check_output(["git", "show", "0c39b9d:" + (RUN / "report-data.json").relative_to(REPO).as_posix()], cwd=REPO))
rows = json.loads((HERE / "final/summary.json").read_text(encoding="utf-8"))
decisions = json.loads((HERE / "decisions.json").read_text(encoding="utf-8"))
data["generatedAt"] = datetime.datetime.now(datetime.timezone(datetime.timedelta(hours=2))).isoformat()
data["scope"] += " Borrowed UTF-16 API continuation: preserve existing strings and add static/reusable span matching, owned span-pattern constructors and mutable/read-only span extensions. Allocation removal is a local caller result; no application throughput claim."
data["question"] += " Can borrowed slices remove caller materialization while retaining owned-string performance and all semantic invariants?"
data["status"] = "complete"
data["primaryMetric"] = "ns/call and managed B/call"
for mode in ("static", "reused", "span-static", "span-reused", "sliced-static", "sliced-reused", "construction-span", "normalization"):
    selected = [r for r in rows if r["mode"] == mode]
    chart_rows = []
    for r in selected:
        c, n = r["control"], r["candidate"]
        chart_rows.append(dict(label=r["workload"], detail=f"allocation {c['bytes']:g} → {n['bytes']:g} B/call; runtime {r['framework']}",
            measurements=[{k:c[k] for k in ("median","min","max","n")}, dict(**{k:n[k] for k in ("median","min","max","n")}, changePercent=r["effectPercent"])]))
    data["series"].append(dict(title="Borrowed-text final epoch: " + mode,
        description="Committed 0c39b9d control and retained API integration; three fresh processes, 54 samples/engine/case, all samples retained. Sliced controls materialize text (and static patterns) inside timing. Span-* controls use already-owned strings; construction controls and candidates both copy one pattern. Normalization runs on .NET 11, all others on .NET 10. Whiskers show the complete range.",
        unit="ns/call", lowerIsBetter=True, rounds=["Committed string caller", "Span-capable caller"], rows=chart_rows))
highlight = next(r for r in rows if r["workload"] == "long-literal" and r["mode"] == "sliced-static")
data["summary"] = dict(baseline=highlight["control"]["median"], final=highlight["candidate"]["median"], unit="ns/call", changePercent=highlight["effectPercent"],
    headline=f"Borrowed slices: 1,024-character literal static caller {highlight['control']['median']:.1f} → {highlight['candidate']['median']:.1f} ns; {highlight['control']['bytes']:g} → {highlight['candidate']['bytes']:g} allocated bytes. Existing string callers and earlier epochs remain explicit below.")
for workload, mode in (("long-literal", "sliced-static"), ("long-literal", "sliced-reused"), ("simple", "sliced-static"), ("normalize-1024", "normalization")):
    r = next(r for r in rows if r["workload"] == workload and r["mode"] == mode)
    data["scorecards"].append(dict(title="Borrowed text: " + workload + " / " + mode, baseline=r["control"]["median"], value=r["candidate"]["median"], unit="ns/call",
        wallChangePercent=r["effectPercent"], secondaryMetric="managed allocation", secondaryChangePercent=-100,
        progression=f"{r['control']['bytes']:g} → {r['candidate']['bytes']:g} B/call; final paired epoch"))
data["hypotheses"].extend(decisions["hypotheses"])
data["retainedChanges"].extend(decisions["retainedChanges"])
data["validityThreats"].extend(decisions["limitations"])
data["generatedCode"].extend(decisions["generatedCode"])
for epoch, filename in (("Initial shared span", "summary.json"), ("H-027", "H-027/summary.json"), ("H-029", "H-029/summary.json"), ("H-030", "H-030/summary.json"), ("Final", "final/summary.json")):
    records = json.loads((HERE / filename).read_text(encoding="utf-8"))
    chosen = [r for r in records if r["mode"] == "reused"]
    data["effectGroups"].append(dict(name="Borrowed API owned-string effects: " + epoch,
        control="Each epoch has its own contemporaneous control named in spans/plan.md and identity.json. Final uses 0c39b9d; isolated refinements use their parent. Fractions and ranges are paired block effects; gains across epochs are not additive.",
        items=[dict(hypothesisId=decisions["epochIds"][epoch], label=r["workload"], effect=r["effectPercent"] / 100,
            range=[min(p["min"] for p in r["paired"]) / 100, max(p["max"] for p in r["paired"]) / 100],
            note="launch medians " + ", ".join(f"{p['median']:+.1f}%" for p in r["paired"])) for r in chosen]))
identity = json.loads((HERE / "final/identity.json").read_text(encoding="utf-8"))
data["environment"].extend([
    dict(label="Borrowed API baseline", value="0c39b9d, published 2.0.0 string engine. Distinct control AssemblyName/PackageId; identical production code otherwise. Every source epoch and failed/inconclusive path remains in spans/."),
    dict(label="Borrowed API source identity", value=json.dumps(identity["hashes"])),
    dict(label="Borrowed API timing", value="SDK pinned by global.json; Release .NET 10 (caller/string/span) and .NET 11 (VT normalization), x64, CPU 30, default tiering/PGO, workstation GC. Joint public-path primer and 500 ms JIT settling interval; 100 ms joint warmup per case, common control calibration ≥20 ms, nine ABBA/BAAB blocks and three launches. All 54 samples/engine/case retained; no overhead subtraction. Isolated screening uses ≥5 ms and four blocks; its earliest static cases are not compared across epochs."),
    dict(label="Borrowed API host exclusion", value="Builds, tests, measurement and profiles serialized through the fixed performance-measurement.lock. High performance power plan recorded. Cooperative lock cannot exclude arbitrary external processes; hardware counter data is unavailable.")])
profile = json.loads((HERE / "final/profile-summary.json").read_text(encoding="utf-8"))
data["hotspots"].append(dict(name="Borrowed-input caller", location="WildcardMatch.IsMatch(ReadOnlySpan<char>)",
    evidence="spans/final/profile-summary.json: equal-duration nine-fixture borrowed-input sweep; sampled inclusive stack residence only. Allocation counters directly quantify eliminated caller ToString conversions.",
    limit="Caller copying/allocation and remaining wildcard work; this fixture sweep is different from the historical profile and is not an application trace."))
data["areas"].append(dict(name="Borrowed text ownership and compiler visibility", boundary="String/span API boundary, diagnostic state, normalization scratch and shared/specialized kernels",
    catalogue="M1/M2/M3/M7 and R2; previous CPU/storage/concurrency dispositions remain applicable.", hypothesisIds=[h["id"] for h in decisions["hypotheses"]],
    evidence="Complete caller allocations, before/after latency and optimized native bodies; large owned-string regressions triggered isolated refinements.", disposition=decisions["stop"]))
data["correctness"].extend([
    "Borrowed API: Release 221 tests pass on .NET 8 and .NET 10; 224 on .NET 11. Eight span-focused Debug tests pass on each runtime. Independent exhaustive DP and seeded Unicode/long-segment differential fuzz suites exercise both string and span static/instance entry points.",
    "Span tests cover default/empty spans, guarded nonzero slices, exact tails and length boundaries, stack buffers, mutable arrays, ReadOnlyMemory.Span, source mutation after copied construction, culture capture, flags/invalid timeout/null-string overloads, finite successful allocation-free matches and exact timeout diagnostics limited to borrowed slices.",
    "All retained default matching samples allocate zero managed bytes. Reusable span constructors copy the pattern once; benchmark setup includes that copy in construction-plus-match. Normalization first-use/pool costs are recorded separately in final/net11.0-cold.json, so warm zero allocation is not a cold-pool guarantee."])
data["reproduction"].extend([
    "From the repository: python C:/Users/tedd/.codex/skills/scientific-method-performance/scripts/run_with_performance_lock.py --cwd D:/SourceCode/Tedd.WildcardMatch -- python -B \"Research/2026-10-02 wildcard-engine-1/spans/run.py\" --phase all --epoch final",
    "Run spans/summarize.py final, spans/extend_report.py, then the standard skill renderer for report-data.json/report.html. The extension reads the immutable report at 0c39b9d; do not run the older optimization extension afterwards, because it reconstructs an earlier historical deliverable.",
    "Isolated span-parent experiments: add --control-epoch <parent> --screening --epoch <new-name>. Initial parent source snapshots are in spans/; H-027/H-029/H-030 snapshots in their own subdirectories. Current driver uses the documented priming epoch and clean disassembly files."])
data["artifacts"].extend([dict(label=label, path="spans/" + path) for label,path in (
    ("Borrowed API plan and epoch corrections", "plan.md"), ("Local validation and measurement driver", "run.py"),
    ("Initial borrowed API acceptance", "summary.json"), ("Final borrowed API acceptance", "final/summary.json"),
    ("Span ownership and decisions", "decisions.json"), ("Caller benchmark", "Program.cs"),
    ("Initial benchmark source", "initial-program.cs.txt"), ("Final source hashes and runtime", "final/identity.json"),
    ("Normalized string IL and compiled caller targets", "final/compiled-calls.txt"),
    ("Final Release tests", "final/Release.txt"), ("Final native code", "final/net10.0-native-code.asm.txt"),
    ("First-use normalization allocation", "final/net11.0-cold.json"), ("Span caller profile", "final/profile-summary.json"))])
(RUN / "report-data.json").write_text(json.dumps(data, indent=2, ensure_ascii=False), encoding="utf-8")
print("Extended report:", len(data["hypotheses"]), "hypotheses;", len(data["series"]), "epoch-separated charts")
