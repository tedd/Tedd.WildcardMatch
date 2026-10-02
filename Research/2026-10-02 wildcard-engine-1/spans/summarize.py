"""Summarize complete launch distributions; never filter samples or subtract overhead."""
from collections import defaultdict
import json
from pathlib import Path
import statistics
import sys
HERE = Path(__file__).resolve().parent
if len(sys.argv) > 1: HERE /= sys.argv[1]
result = []
for framework in ("net10.0", "net11.0"):
    if not (HERE / f"{framework}-launch-1.json").exists(): continue
    raw = []
    cold = []
    for launch in range(1, 4):
        data = json.loads((HERE / f"{framework}-launch-{launch}.json").read_text(encoding="utf-8"))
        raw.extend(data["samples"])
        cold.extend(dict(x, launch=launch) for x in data["cold"])
    groups = defaultdict(list)
    for row in raw: groups[(row["workload"], row["mode"])].append(row)
    for (workload, mode), rows in groups.items():
        entry = dict(framework=framework, workload=workload, mode=mode)
        for engine in ("Control", "Candidate"):
            values = [r["ns"] for r in rows if r["engine"] == engine]
            allocated = [r["bytes"] for r in rows if r["engine"] == engine]
            assert len(values) in (24, 54)
            entry[engine.lower()] = dict(median=statistics.median(values), min=min(values), max=max(values), n=len(values),
                bytes=statistics.median(allocated), minBytes=min(allocated), maxBytes=max(allocated))
        entry["effectPercent"] = 100 * (entry["candidate"]["median"] / entry["control"]["median"] - 1)
        paired = []
        for launch in range(1, 4):
            effects = []
            for block in sorted(set(r["block"] for r in rows)):
                c = statistics.median(r["ns"] for r in rows if r["engine"] == "Control" and r["launch"] == launch and r["block"] == block)
                n = statistics.median(r["ns"] for r in rows if r["engine"] == "Candidate" and r["launch"] == launch and r["block"] == block)
                effects.append(100 * (n / c - 1))
            paired.append(dict(launch=launch, median=statistics.median(effects), min=min(effects), max=max(effects)))
        entry["paired"] = paired
        result.append(entry)
    (HERE / (framework + "-cold.json")).write_text(json.dumps(cold, indent=2), encoding="utf-8")
(HERE / "summary.json").write_text(json.dumps(result, indent=2), encoding="utf-8")
for r in result:
    if r["mode"] in ("sliced-reused", "sliced-static", "normalization") or (r["effectPercent"] > 15 and r["candidate"]["median"] - r["control"]["median"] > 5):
        print(r["framework"], r["workload"], r["mode"],
            f"{r['control']['median']:.1f} -> {r['candidate']['median']:.1f} ns ({r['effectPercent']:+.1f}%),",
            f"{r['control']['bytes']:g} -> {r['candidate']['bytes']:g} B,",
            "launch effects", [round(p["median"],1) for p in r["paired"]])
