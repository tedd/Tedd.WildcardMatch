"""Append release correctness evidence without replacing historical measurements."""
import datetime
from html.parser import HTMLParser
import json
from pathlib import Path
import xml.etree.ElementTree as ET

HERE = Path(__file__).resolve().parent
RUN = HERE.parents[1]
REPO = HERE.parents[3]
identity = json.loads((HERE / "identity.json").read_text(encoding="utf-8"))
identity["hashes"] = {key.replace("\\", "/"): value for key, value in identity["hashes"].items()}
original = json.loads((HERE.parent / "final/identity.json").read_text(encoding="utf-8"))
for file, digest in original["hashes"].items():
    assert identity["hashes"]["src/Tedd.WildcardMatch/" + file] == digest, file

ns = {"t": "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"}
results = []
for configuration in ("Release", "Debug"):
    for framework in ("net8.0", "net10.0", "net11.0"):
        counts = ET.parse(HERE / (configuration + "-" + framework + ".trx")).find("t:ResultSummary/t:Counters", ns).attrib
        assert int(counts["failed"]) == 0 and counts["total"] == counts["passed"]
        results.append(dict(configuration=configuration, framework=framework, **counts))
corpus = json.loads((HERE / "corpus.json").read_text(encoding="utf-8"))
assert len(corpus) == 24 and all(r["Failures"] == 0 and r["Checked"] == 256 for r in corpus)

class SiteParser(HTMLParser):
    def __init__(self):
        super().__init__()
        self.ids, self.fragments, self.files, self.copies = [], [], [], []
    def handle_starttag(self, tag, attributes):
        attributes = dict(attributes)
        if "id" in attributes: self.ids.append(attributes["id"])
        if "data-copy-target" in attributes: self.copies.append(attributes["data-copy-target"])
        for key in ("href", "src"):
            link = attributes.get(key, "")
            if link.startswith("#"): self.fragments.append(link[1:])
            elif link and not link.startswith(("http:", "https:")): self.files.append(link)

parser = SiteParser()
parser.feed((REPO / "site/index.html").read_text(encoding="utf-8"))
assert len(parser.ids) == len(set(parser.ids))
assert all(target in parser.ids for target in parser.fragments + parser.copies)
assert all((REPO / "site" / file).is_file() for file in parser.files)
summary = dict(tests=results, corpus=dict(combinations=24, inputsPerCombination=256, failures=0),
    packageConsumers=["net8.0/netstandard2.1", "net10.0", "net11.0"],
    unchangedProductionHashes=True, site=dict(ids=len(parser.ids), localFiles=parser.files,
        copyTargets=parser.copies, allReferencesExist=True))
(HERE / "summary.json").write_text(json.dumps(summary, indent=2), encoding="utf-8")

data = json.loads((RUN / "report-data.json").read_text(encoding="utf-8"))
marker = "Release 2.1.0 assurance:"
data["correctness"] = [entry for entry in data["correctness"] if not entry.startswith(marker)]
data["correctness"].extend([
    marker + " Full Release AND Debug suites pass: 232 tests each on .NET 8/10 and 235 each on .NET 11. These are test-case counts; exhaustive/fuzz cases run inside individual tests. Historical span/performance test counts above remain the original epoch's evidence.",
    marker + " Static and prepared spans now participate in all 65,534 literal UTF-16 pattern code points with five case variants across four cultures (1,310,680 comparisons per API/runtime), exhaustive .NET 11 line anchors, Unicode newline sequences and numeric option validation. Existing independent DP exhaustive tests cover 945,252 short input/pattern/Singleline combinations per runtime; seeded Unicode fuzz and 10,000 DP long-segment trials remain.",
    marker + " 12,000 additional seeded guarded-array cases across invariant, en-US, tr-TR and az-Latn-AZ verify exact slice extents, all span extensions, owned preparation, finite timeouts, RightToLeft result parity and unchanged caller buffers. Added overlapping input/pattern storage, concurrent captured-culture reuse, concurrent stack/pool normalization, 65,535/65,536/1,000,000-code-unit fixtures and reuse after timeouts.",
    marker + " Initial guarded-fuzz attempts exposed oracle constraints: the non-backtracking Regex 1,000-node limit and its incompatibility with .NET 11 AnyNewLine. Rejections remain in initial-oracle-limit.txt and initial-oracle-options.txt. Final short fuzz uses non-backtracking when compatible and a five-second interpreter oracle for AnyNewLine; separate long-input and normalization fixtures remain. All 24 benchmark competitor combinations pass 256 inputs each. Packed 2.1.0 consumers compile and execute the public span and preserved string APIs against all three package target selections.",
    marker + " Production C# hashes match the final measured span epoch exactly; this release changes tests, version and documentation. No new performance measurements or performance claims. Website fragment, asset and copy-button references validate locally; actual deployment is verified separately."])
data["generatedAt"] = datetime.datetime.now(datetime.timezone.utc).isoformat()
reproduce = 'Release assurance: run spans/release-2.1.0/validate.py under the fixed performance lock, then append_report.py and the standard renderer. All scratch builds/caches remain in D:/Workspaces/AI/wildcard-span-release.'
if reproduce not in data["reproduction"]: data["reproduction"].append(reproduce)
for label, file in (("2.1.0 release validation summary", "summary.json"),
    ("2.1.0 validation driver", "validate.py"), ("2.1.0 source and package hashes", "identity.json"),
    ("Independent oracle limit rejected fixture", "initial-oracle-limit.txt")):
    artifact = dict(label=label, path="spans/release-2.1.0/" + file)
    if artifact not in data["artifacts"]: data["artifacts"].append(artifact)
(RUN / "report-data.json").write_text(json.dumps(data, indent=2, ensure_ascii=False), encoding="utf-8")
print(json.dumps(summary, indent=2))
