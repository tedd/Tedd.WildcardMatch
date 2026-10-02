"""Extend the original report without changing its historical measurement epoch."""
import datetime
import hashlib
import json
from pathlib import Path
import subprocess
import sys
sys.stdout.reconfigure(encoding="utf-8")
HERE = Path(__file__).resolve().parent
RUN = HERE.parent
REPO = RUN.parent.parent
data = json.loads(subprocess.check_output(["git", "show", "4bc779e:" + (RUN / "report-data.json").relative_to(REPO).as_posix()], cwd=REPO))
summary = json.loads((HERE / "summary.json").read_text(encoding="utf-8"))
decisions = json.loads((HERE / "decisions.json").read_text(encoding="utf-8"))
integrated = summary["integrated"]
data["generatedAt"] = datetime.datetime.now(datetime.timezone(datetime.timedelta(hours=2))).isoformat()
data["scope"] += " Continuation: catalogue-driven optimization of committed direct engine 4bc779e; original charts remain historical. New control/candidate charts share a distinct .NET 10 direct-engine epoch. No application throughput claim."
data["question"] += " Which additional measured mechanisms improve the committed direct engine?"
data["status"] = "complete"
for card in data["scorecards"]:
    card["title"] = "Rewrite epoch: " + card["title"]
for chart in data["series"]:
    chart["title"] = "Original rewrite epoch: " + chart["title"]
for environment in data["environment"]:
    if environment["label"] == "Engine SHA-256": environment["label"] = "Original engine SHA-256"
bulk = next(h for h in data["hypotheses"] if h["id"] == "H-002")
bulk["state"] = "retained"
bulk["decision"] = "Confirmed by later ablation"
bulk["result"] += " Continuation H-012 removes only this routing in the same compiler/target epoch and makes long-sparse calls about twenty times slower."
bulk["decisionRationale"] += " The later controlled ablation resolves the former constituent-effect uncertainty; initial cross-epoch data remains historical."
bulk["evidence"].append("optimization/H-012/")
for mode in ("static", "reused", "construction"):
    rows = []
    for r in integrated:
        if r["mode"] != mode: continue
        c, n = r["control"], r["candidate"]
        rows.append(dict(label=r["workload"], detail=f"managed allocation {c['bytes']:g} → {n['bytes']:g} B/call",
            measurements=[{k:c[k] for k in ("median","min","max","n")},
                dict(**{k:n[k] for k in ("median","min","max","n")}, changePercent=r["effectPercent"])]))
    data["series"].append(dict(title="Direct-engine continuation: " + mode,
        description="Committed direct 4bc779e versus retained integration, same build/runtime. Three fresh processes; 54 samples per engine/case, all samples retained. Whiskers span complete sample ranges.",
        unit="ns/call", lowerIsBetter=True, rounds=["Committed direct", "Optimized direct"], rows=rows))

targets = ("dense", "complex", "question-run", "retry-literal", "ascii-literal-ignorecase", "long-sparse")
for name in targets:
    r = next(r for r in integrated if r["mode"] == "reused" and r["workload"] == name)
    data["scorecards"].append(dict(title="New engine: " + name, baseline=r["control"]["median"], value=r["candidate"]["median"], unit="ns/call",
        wallChangePercent=r["effectPercent"], progression="4bc779e direct → integrated; new paired epoch"))
highlight = next(r for r in integrated if r["mode"] == "reused" and r["workload"] == decisions["highlight"])
data["summary"] = dict(baseline=highlight["control"]["median"], final=highlight["candidate"]["median"], unit="ns/call", changePercent=highlight["effectPercent"],
    headline=f"Direct-engine continuation: {highlight['workload']} reused calls {highlight['control']['median']:.1f} → {highlight['candidate']['median']:.1f} ns. Original Regex comparisons remain below in their historical epoch; compiled Regex remains workload dependent.")

for name, decision in decisions["candidates"].items():
    rows = summary[name]
    r = next(r for r in rows if r["mode"] == "reused" and r["workload"] == decision["target"])
    effects = [round(p["median"], 1) for p in r["paired"]]
    entry = dict(id=name, claim=decision["claim"], mechanism=decision["mechanism"], prediction=decision["prediction"],
        location="src/Tedd.WildcardMatch/WildcardEngine.cs and WildcardMatch.cs", caller="Public static/instance IsMatch and construction-plus-match",
        observation="Committed engine sampling, scalar Tier1 loops and contemporaneous candidate/control effects; parent-specific evidence is recorded in optimization/plan.md.",
        falsification="Independent-oracle mismatch, no repeated benefit beyond the identical-code control variation, or material credible regression under original equal-weight workload priorities.",
        change=decision["change"], changeScope=decision.get("scope", "local"),
        maintainability=decision["burden"], codeComments=decision.get("comments", "Discarded candidates do not alter production comments; candidate source is retained as evidence."),
        result=f"Control {r['controlSource']}; {decision['target']} reused: {r['control']['median']:.1f} → {r['candidate']['median']:.1f} ns ({r['effectPercent']:+.1f}%). Paired per-launch median effects {effects}%. " + decision["finding"],
        effectPercent=r["effectPercent"], state=decision["state"], decision=decision["decision"], decisionRationale=decision["rationale"],
        evidence=[f"optimization/{name}/", "optimization/summary.json", "optimization/variants.py", "optimization/plan.md"], risks=decision.get("risks", []))
    if "parent" in decision: entry["parentId"] = decision["parent"]
    data["hypotheses"].append(entry)

for target in ("dense", "complex", "question-run", "retry-literal", "ascii-literal-ignorecase", "long-sparse"):
    items = []
    for name, decision in decisions["candidates"].items():
        r = next(r for r in summary[name] if r["mode"] == "reused" and r["workload"] == target)
        items.append(dict(hypothesisId=name, label=name, effect=r["effectPercent"] / 100,
            range=[min(p["min"] for p in r["paired"]) / 100, max(p["max"] for p in r["paired"]) / 100], note=decision["state"] + "; " + r["controlSource"]))
    data["effectGroups"].append(dict(name="Isolated variants: " + target + " reused",
        control="Each candidate's contemporaneous control is named per item: usually 4bc779e, H-020 uses H-014 and H-022 uses H-017. 24 screening samples per engine/case; ranges span paired block effects. Separate candidate runs are not a shared raw-time epoch; gains are not additive.", items=items))

data["environment"].extend([
    dict(label="Continuation baseline", value="4bc779e committed direct engine. Control uses a distinct AssemblyName/PackageId and external alias; otherwise identical committed production source. Candidate transformations and source hashes are preserved. Old charts/hashes refer to the original report epoch."),
    dict(label="Continuation final source SHA-256", value=hashlib.sha256((REPO / "src/Tedd.WildcardMatch/WildcardEngine.cs").read_bytes()).hexdigest()),
    dict(label="Continuation protocol", value="Same SDK 11 preview/.NET 10.0.12 x64 and CPU 30; en-GB; runtime-default tiering/PGO and Workstation GC. 23 fixtures, static/reused/construction. Screening four ABBA/BAAB blocks with >=5ms control calibration; acceptance nine blocks with >=20ms. Three fresh processes, 100ms joint timed warmup; 24/54 samples per engine/case."),
    dict(label="Profile", value="dotnet-trace SampleProfiler, equal 0.5s work per fixture. Speedscope stack-residence weights support inclusive attribution; synthetic CPU_TIME leaves prevent accurate exclusive method attribution. This is a local workload, not an application traffic profile.")])
data["hotspots"].append(dict(name="MatchWindow", location="src/Tedd.WildcardMatch/WildcardEngine.cs",
    inclusivePercent=67.6, evidence="optimization/profile/profile-summary.json: 7760.8 / 11485.4 sampled CPU residence weights in equal-duration fixture sweep; inclusive only.",
    limit="Scalar wildcard/retry branches, character folding, and repeated fixed-run searches; hardware miss/IPC counters unavailable."))
profile = json.loads((HERE / "reprofile/profile-summary.json").read_text(encoding="utf-8"))["inclusive"]
total = next(weight for name, weight in profile if name == "CPU_TIME")
match = next(weight for name, weight in profile if "WildcardEngine.MatchWindow" in name)
data["hotspots"].append(dict(name="Integrated MatchWindow", location="src/Tedd.WildcardMatch/WildcardEngine.cs",
    inclusivePercent=100 * match / total, evidence="optimization/reprofile/profile-summary.json, same equal-duration fixture sweep; inclusive sampled residence only.",
    limit="Remaining scalar/general-case matching plus runtime search. Relative profile shares change with fixture execution frequency; they are not before/after absolute operation-time estimates."))
data["areas"].extend(decisions["areas"])
data["retainedChanges"].extend(decisions["retained"])
data["decisions"] = [decisions["stopping"], "No product decision required. Package version 2.0.0 releases the direct engine with the preserved Regex alternative; publication uses the repository's deploy-only CI workflow."]
data["correctness"].extend(decisions["correctness"])
data["generatedCode"].extend(decisions["native"])
data["validityThreats"].extend([
    "Identical-code controls show appreciable per-fixture/per-launch variation. Small compiler/branch effects remain inconclusive. Screening is not an acceptance result; retained integration uses longer blocks and all launch/block distributions.",
    "Control and candidate have distinct assembly identities; JIT address/layout and tiering can differ even for identical source. No hardware branch-miss or cache-miss inference is made from timing alone.",
    "The original thirteen cases have equal importance; ten additional adversarial distributions are explicit contrasts, not an application frequency model. A targeted improvement does not establish improvement on every pattern.",
    "Desktop/uncoordinated background processes are not controlled by the advisory lock. The full raw distributions are retained; no outlier exclusions, overhead subtraction or hidden baseline-epoch comparison.",
    "Historical Regex charts describe the initial delivered engine. New direct-engine charts quantify only the continuation; constituent gains and cross-epoch ratios must not be combined."])
data["reproduction"].extend([
    "Continuation: python -B <skill>/scripts/run_with_performance_lock.py --cwd <repo> -- python -B <run>/optimization/run.py profile control H-004 H-005 H-006 H-007 H-008 H-009 H-010 H-011 H-012 H-013 H-014 H-015 H-016.",
    "Candidates always derive from Git revision 4bc779e. Run integrated --acceptance for the current production integration, then final and reprofile. Additional compatibility/corpus/pack validation uses release_check.py under the same host lock.",
    "Run optimization/summarize.py, optimization/extend_report.py, then the skill renderer for report-data.json/report.html. The extension reads the immutable original report at 4bc779e and preserves its charts; generated HTML is never edited by hand."])
data["artifacts"].extend([dict(label="Continuation plan and catalogue coverage", path="optimization/plan.md"),
    dict(label="Candidate transformations", path="optimization/variants.py"), dict(label="Continuation driver and rig", path="optimization/run.py"),
    dict(label="All continuation metrics and paired effects", path="optimization/summary.json"), dict(label="Candidate dispositions", path="optimization/decisions.json"),
    dict(label="Initial sampled profile", path="optimization/profile/profile-summary.json"), dict(label="Integrated acceptance", path="optimization/integrated/"),
    dict(label="Final sampled profile", path="optimization/reprofile/profile-summary.json"), dict(label="Final configured tests", path="optimization/final/"),
    dict(label="Release validation", path="optimization/release/"), dict(label="Report extension", path="optimization/extend_report.py")])
(RUN / "report-data.json").write_text(json.dumps(data, ensure_ascii=False, indent=2), encoding="utf-8")
print(data["summary"]["headline"])
