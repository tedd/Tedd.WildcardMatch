"""Preserve every sample; report paired effects by launch and complete-call medians."""
import json
import math
from pathlib import Path
import statistics as stats
import sys
sys.stdout.reconfigure(encoding="utf-8")
HERE = Path(__file__).resolve().parent
all_results = {}
for directory in sorted(HERE.iterdir()):
    paths = sorted(directory.glob("launch-*.json")) if directory.is_dir() else []
    if len(paths) != 3:
        continue
    documents = [json.loads(path.read_text(encoding="utf-8")) for path in paths]
    identity = directory / "identity.json"
    control_source = json.loads(identity.read_text(encoding="utf-8"))["baseline"] if identity.exists() else "4bc779e"
    samples = [s for d in documents for s in d["samples"]]
    rows = []
    for mode in ("static", "reused", "construction"):
        for name in dict.fromkeys(s["workload"] for s in samples):
            group = [s for s in samples if s["mode"] == mode and s["workload"] == name]
            metrics = {}
            for engine in ("Control", "Candidate"):
                v = [s["ns"] for s in group if s["engine"] == engine]
                metrics[engine] = dict(median=stats.median(v), min=min(v), max=max(v), n=len(v),
                    bytes=stats.median(s["bytes"] for s in group if s["engine"] == engine))
            effects = []
            for launch in range(1, 4):
                block_effects = []
                for block in sorted(set(s["block"] for s in group)):
                    pair = [s for s in group if s["launch"] == launch and s["block"] == block]
                    m = {engine: stats.median(s["ns"] for s in pair if s["engine"] == engine) for engine in metrics}
                    block_effects.append(100 * (m["Candidate"] / m["Control"] - 1))
                effects.append(dict(launch=launch, median=stats.median(block_effects), min=min(block_effects), max=max(block_effects), blocks=block_effects))
            rows.append(dict(mode=mode, workload=name, controlSource=control_source, control=metrics["Control"], candidate=metrics["Candidate"],
                effectPercent=100 * (metrics["Candidate"]["median"] / metrics["Control"]["median"] - 1), paired=effects))
    all_results[directory.name] = rows
    print(directory.name)
    for mode in ("static", "reused", "construction"):
        original = [r for r in rows if r["mode"] == mode][:13]
        geo = math.exp(stats.mean(math.log(r["candidate"]["median"] / r["control"]["median"]) for r in original))
        print(" ", mode, "original equal-weight geometric effect", round(100 * (geo - 1), 1))
    for r in rows:
        if r["mode"] == "reused" and (abs(r["effectPercent"]) > 12 or r["workload"] in ("dense", "complex", "unicode", "simple-ignorecase", "question-run", "retry-literal", "ascii-literal-ignorecase")):
            print(" ", r["workload"], round(r["control"]["median"],1), "->", round(r["candidate"]["median"],1), round(r["effectPercent"],1), "paired", [round(p["median"],1) for p in r["paired"]])
(HERE / "summary.json").write_text(json.dumps(all_results, indent=2), encoding="utf-8")
