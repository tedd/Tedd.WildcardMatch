r"""Render the LinkedIn figure from the current published benchmark snapshot.

Requires matplotlib, fonttools and Pillow. Install optional local dependencies under
D:\Workspaces\AI\wildcard-linkedin\plotting-dependencies. All renderer caches and
downloaded fonts stay in the --workspace directory; final assets stay beside this
script. Native rows use the lowest mean from the current run, as the website does.
"""
import argparse
import hashlib
import json
import os
import sys
from datetime import date
from pathlib import Path
from urllib.request import Request, urlopen

sys.dont_write_bytecode = True
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument("--workspace", type=Path, default=Path(r"D:\Workspaces\AI\wildcard-linkedin"))
args = parser.parse_args()
workspace = args.workspace.resolve()
workspace.mkdir(parents=True, exist_ok=True)
os.environ["MPLCONFIGDIR"] = str(workspace / "matplotlib")
os.environ["TEMP"] = os.environ["TMP"] = str(workspace)
sys.path.insert(0, str(workspace / "plotting-dependencies"))

import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt
from matplotlib import font_manager
from matplotlib.patches import FancyBboxPatch
from matplotlib.ticker import MaxNLocator
from PIL import Image
from fontTools.ttLib import TTFont
from fontTools.varLib.instancer import instantiateVariableFont

output = Path(__file__).resolve().parent
root = output.parents[1]
source = root / "site" / "assets" / "package-comparison.json"
payload = source.read_bytes()
data = json.loads(payload.decode("utf-8-sig"))
managed_validation = json.loads(source.with_name("validation.json").read_text(encoding="utf-8-sig"))
native_validation = json.loads(source.with_name("native-validation.json").read_text(encoding="utf-8-sig"))
assert managed_validation and native_validation
assert all(r["Failures"] == 0 for r in managed_validation + native_validation)

font_dir = workspace / "fonts"
font_dir.mkdir(exist_ok=True)
font_sources = {
    "DMSans[opsz,wght].ttf": "https://raw.githubusercontent.com/google/fonts/main/ofl/dmsans/DMSans%5Bopsz%2Cwght%5D.ttf",
    "SpaceGrotesk[wght].ttf": "https://raw.githubusercontent.com/google/fonts/main/ofl/spacegrotesk/SpaceGrotesk%5Bwght%5D.ttf",
}
for name, url in font_sources.items():
    original = font_dir / name
    if not original.exists():
        with urlopen(Request(url, headers={"User-Agent": "WildcardMatch-figure"})) as response:
            original.write_bytes(response.read())
    for weight in (400, 600, 700):
        target = font_dir / (original.stem + f"-{weight}.ttf")
        if not target.exists():
            font = TTFont(original)
            axes = {"wght": weight}
            if any(a.axisTag == "opsz" for a in font["fvar"].axes):
                axes["opsz"] = 16
            instantiateVariableFont(font, axes, inplace=False).save(target)
        font_manager.fontManager.addfont(str(target))
for family, weight in (("DM Sans", 400), ("DM Sans", 700), ("Space Grotesk", 700)):
    resolved = Path(font_manager.findfont(font_manager.FontProperties(family=family, weight=weight),
                                        fallback_to_default=False))
    assert resolved.parent == font_dir, (family, weight, resolved)

bg, panel, ink, muted, line = "#f7faf9", "#ffffff", "#13282f", "#61747b", "#d6e3df"
green, gray = "#087f78", "#99a6ab"
plt.rcParams.update({"font.family": "DM Sans", "text.color": ink, "axes.labelcolor": muted,
                     "xtick.color": muted, "ytick.color": ink, "svg.fonttype": "none"})
fig = plt.figure(figsize=(8, 11), dpi=200, facecolor=bg)
fig.text(.05, .985, "Performance with Sol", fontsize=8.1, color="#6b7280", va="top")
heading_x = .05
for piece, color in (("Tedd.", ink), ("WildcardMatch", green), (f" {data['releaseVersion']}", ink)):
    artist = fig.text(heading_x, .953, piece, color=color, fontsize=22, weight="bold",
                      fontfamily="Space Grotesk", va="top")
    heading_x += artist.get_window_extent(fig.canvas.get_renderer()).width / fig.bbox.width
fig.text(.05, .910, "Question marks and multiple stars", fontsize=13, color=muted, va="top")
fig.text(.05, .873, "BAR LENGTH = MILLION MATCHES / SECOND", fontsize=9, weight="bold", color=green)
fig.text(.95, .873, "Higher is better", fontsize=9, color=muted, ha="right")

managed = ["TeddDirectReused", "DotNetGlob", "WildcardMatch", "FastWildcard"]
native = ["Krauss", "GitWildmatch", "ZemaWildcards", "Pcre2Jit"]
names = {k: data["libraries"][k]["label"] for k in managed + native}
names["TeddDirectReused"] = "Tedd reused"
highlighted = {"TeddDirectReused", "WildcardMatch"}
selected = []
summaries = []
for workload, title, y in (("Simple", "QUESTION MARKS", .520), ("MultiStar", "MULTIPLE STARS", .175)):
    rows = []
    for library in managed + native:
        eligible = [r for r in data["matching"] if r["workload"] == workload and r["library"] == library]
        assert eligible, (workload, library)
        if library in managed:
            assert len(eligible) == 1 and eligible[0]["mode"] == "managed", eligible
        row = min(eligible, key=lambda r: r["meanNs"])
        assert row["meanNs"] > row["errorNs"] >= 0
        rows.append(row)
        selected.append({k: v for k, v in row.items() if k != "samplesNs"})
    rows.sort(key=lambda r: r["meanNs"])
    assert rows[0]["library"] == "TeddDirectReused"
    competitor = min((r for r in rows if r["library"] != "TeddDirectReused"), key=lambda r: r["meanNs"])
    ratio = competitor["meanNs"] / rows[0]["meanNs"]
    summaries.append({"workload": workload, "competitor": competitor["library"],
                      "competitorMode": competitor["mode"], "throughputRatio": ratio})
    fig.add_artist(FancyBboxPatch((.05, y), .90, .32, boxstyle="square,pad=0.008",
                                  facecolor=panel, edgecolor=line, linewidth=.6,
                                  transform=fig.transFigure, zorder=0))
    fig.text(.07, y+.291, title, fontsize=12, weight="bold", fontfamily="Space Grotesk")
    pattern = next(w["pattern"] for w in data["workloads"] if w["name"] == workload)
    fig.text(.07, y+.266, pattern, fontsize=10, color=muted, family="Consolas")
    fig.text(.93, y+.291, f"{ratio:.2f}x", fontsize=21, weight="bold",
             fontfamily="Space Grotesk", color=green, ha="right")
    fig.text(.93, y+.267, f"vs {names[competitor['library']]} *", fontsize=9, color=muted, ha="right")
    ax = fig.add_axes([.255, y+.043, .515, .205], facecolor=panel)
    rates = [1000 / r["meanNs"] for r in rows]
    low = [rate - 1000/(r["meanNs"]+r["errorNs"]) for rate, r in zip(rates, rows)]
    high = [1000/(r["meanNs"]-r["errorNs"]) - rate for rate, r in zip(rates, rows)]
    positions = list(range(len(rows)))
    ax.barh(positions, rates, color=[green if r["library"] in highlighted else gray for r in rows],
            height=.50, zorder=3)
    ax.errorbar(rates, positions, xerr=[low, high], fmt="none", ecolor=ink,
                elinewidth=.65, capsize=2, capthick=.65, zorder=4)
    ax.set_ylim(-.6, len(rows)-.4)
    ax.invert_yaxis()
    ax.set_xlim(0, max(v+e for v,e in zip(rates, high))*1.10)
    ax.xaxis.set_major_locator(MaxNLocator(nbins=4, min_n_ticks=3))
    ax.grid(axis="x", color=line, linewidth=.6, zorder=1)
    labels = [names[r["library"]] + (" *" if r["library"] in native else "") for r in rows]
    ax.set_yticks(positions, labels=labels, fontsize=10.5)
    for tick, row in zip(ax.get_yticklabels(), rows):
        tick.set_color(green if row["library"] in highlighted else ink)
        tick.set_fontweight("bold" if row["library"] in highlighted else "normal")
    ax.tick_params(axis="y", length=0, pad=8)
    ax.tick_params(axis="x", length=0, labelsize=9, pad=5)
    for spine in ax.spines.values():
        spine.set_visible(False)
    for i, row in enumerate(rows):
        ax.text(1.05, i, f"{row['meanNs']:.2f} ns", transform=ax.get_yaxis_transform(),
                fontsize=10.5, ha="left", va="center",
                color=green if row["library"] in highlighted else ink,
                weight="bold" if row["library"] in highlighted else "normal", clip_on=False)
    fig.text(.07, y+.009, "256 inputs | 4-16 UTF-16 units | case-sensitive ASCII corpus",
             fontsize=8.1, color=muted)

measured_date = date.fromisoformat(data["measuredAt"]).strftime("%d %B %Y").lstrip("0")
footer = [
    (.141, "Tedd and DotNet.Glob reused; other managed APIs static."),
    (.121, "* Native: prepared ASCII buffers, fastest current-run call/storage mode."),
    (.101, "Construction and encoding excluded. Labels: mean ns/match. Whiskers: 99.9% CI."),
    (.081, f".NET 10.0.13 | Ryzen 9 5950X | 3 launches | {measured_date}"),
    (.061, f"Measured assembly {data['measuredPackageVersion']}; release {data['releaseVersion']}. BDN 0.15.8."),
]
for y, text in footer:
    fig.text(.05, y, text, fontsize=8, color=muted)
fig.text(.05, .029, "Benchmark revision " + data["librarySourceRevision"][:7], fontsize=8.1, color=muted)
fig.text(.95, .029, "github.com/tedd/Tedd.WildcardMatch", fontsize=8.1, color="#6b7280", ha="right")
assert len(selected) == 16 and not any(r["library"] == "TeddDirectStatic" for r in selected)
fig.canvas.draw()
renderer = fig.canvas.get_renderer()
fw, fh = fig.canvas.get_width_height()
for artist in fig.findobj(match=matplotlib.text.Text):
    if not artist.get_visible() or not artist.get_text():
        continue
    bbox = artist.get_window_extent(renderer)
    assert bbox.x0 >= -1 and bbox.x1 <= fw+1 and bbox.y0 >= -1 and bbox.y1 <= fh+1, (artist.get_text(), str(bbox))

png, svg = output/"linkedin-comparison.png", output/"linkedin-comparison.svg"
fig.savefig(png, dpi=200, facecolor=bg, metadata={"Title": "Wildcard matcher comparison",
              "Description": "Current-run results; lowest measured native mode per library."})
fig.savefig(svg, facecolor=bg)
svg.write_text("\n".join(s.rstrip() for s in svg.read_text(encoding="utf-8").splitlines()).rstrip()+"\n", encoding="utf-8")
Image.open(png).resize((800,1100), Image.Resampling.LANCZOS).save(workspace/"comparison-preview.png")
provenance = {
    "source": "site/assets/package-comparison.json", "sourceSha256": hashlib.sha256(payload).hexdigest(),
    "measuredAt": data["measuredAt"], "exportedAt": data["exportedAt"],
    "sourceRevision": data["librarySourceRevision"], "releaseVersion": data["releaseVersion"],
    "measuredPackageVersion": data["measuredPackageVersion"], "environment": data["environment"],
    "job": data["job"], "selection": "Two short fixtures; reusable Tedd, three managed competitors, best current native mode per library.",
    "rows": selected, "summaries": summaries, "libraries": data["libraries"],
    "nativeSources": data["nativeSources"], "priorNativeReference": data["priorNativeReference"],
    "priorNativeReferenceUsage": "Separate historical context; not used in bars, intervals, or current-run ratios.",
    "fontSources": font_sources,
}
(output/"linkedin-comparison-data.json").write_text(json.dumps(provenance, indent=2)+"\n", encoding="utf-8")
print(json.dumps({"png":str(png),"svg":str(svg),"pixels":Image.open(png).size,"rows":len(selected),"summaries":summaries}))
