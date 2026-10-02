"""Resolve the span continuation ledger from preserved complete caller measurements."""
import json
from pathlib import Path
HERE = Path(__file__).resolve().parent
summaries = {name:json.loads((HERE / path).read_text(encoding="utf-8")) for name,path in (
    ("initial", "summary.json"), ("H-027", "H-027/summary.json"), ("H-029", "H-029/summary.json"),
    ("H-030", "H-030/summary.json"), ("final", "final/summary.json"))}

def hypothesis(id, claim, mechanism, state, epoch, workload, mode, decision, rationale, scope="local", parent=None):
    row = next(r for r in summaries[epoch] if r["workload"] == workload and r["mode"] == mode)
    c,n = row["control"],row["candidate"]
    entry = dict(id=id, claim=claim, mechanism=mechanism, prediction="Remove caller copying or measured integration overhead while preserving exact matching semantics and the declared ownership/regression constraints.",
        observation="Borrowed API source inspection, complete caller allocation counters, paired timing and optimized native code. Parent-specific evidence and setup corrections are recorded in spans/plan.md.",
        location="WildcardMatch.cs; WildcardEngine.cs; WildcardEngine.Spans.cs", caller="Static, reused, sliced, construction and normalization callers",
        falsification="Oracle mismatch, escaped borrowed memory, unexpected match allocation, or coherent material owned-string regression under the declared 15%/5ns review trigger.",
        change=mechanism, changeScope=scope, maintainability="Public span ownership is explicit; shared validation/Unicode rules and independent string/span oracle tests constrain drift. Per-candidate burden is stated in the rationale.",
        codeComments="Representation specialization, timeout borrowing and scratch bounds are documented adjacent to their implementation; detailed causal record remains in spans/plan.md.",
        result=f"{epoch} {workload}/{mode}: {c['median']:.1f} → {n['median']:.1f} ns ({row['effectPercent']:+.1f}%); {c['bytes']:g} → {n['bytes']:g} B/call. Paired launch effects " + str([round(p['median'],1) for p in row['paired']]) + "%.",
        effectPercent=row["effectPercent"], state=state, decision=decision, decisionRationale=rationale,
        evidence=["spans/plan.md", "spans/" + ("summary.json" if epoch == "initial" else epoch + "/summary.json")])
    if parent: entry["parentId"] = parent
    return entry

h = [
    hypothesis("H-023", "Borrowed inputs eliminate materialization before reusable matching", "Expose ReadOnlySpan<char> instance matching and hold borrowed diagnostic input only until the synchronous call completes.", "retained", "final", "long-literal", "sliced-reused", "Retained", "The requested API removes a proportional input copy/allocation. Empty and short cases remain controls; span matching is not universally faster than already-owned strings.", "moderate"),
    hypothesis("H-024", "Borrowed static patterns eliminate a second caller copy", "Add two-span static overloads and mutable/read-only span extensions; reusable span-pattern constructors copy once into immutable ownership.", "retained", "final", "long-literal", "sliced-static", "Retained", "Both caller conversions disappear from static slice calls. Constructor copies remain required for lifetime and mutation safety, and are measured separately.", "moderate"),
    hypothesis("H-025", "One shared span kernel can preserve the string hot path", "Route existing string APIs through the span core while retaining null validation.", "rejected", "initial", "question-star", "reused", "Superseded", "The initial shared core adds coherent short-string overhead and about 40% question/star regression. Preserve its evidence and refine with H-027/H-029/H-030/H-031; do not retain mandatory string routing.", "refactor"),
    hypothesis("H-026", "VT normalization can use bounded scratch instead of a new string", "Use at most 512 stack bytes, then ArrayPool<char>, and expose only the initialized normalized prefix; return the array in finally.", "retained", "final", "normalize-1024", "normalization", "Retained", "Warm complete .NET 11 normalization calls remove the normalized string allocation. First-use pool costs and retention are recorded separately; all-VT and boundary tests validate initialized ranges.", "local"),
    hypothesis("H-027", "Infinite span matches do not need copied timeout diagnostics", "Keep the already-owned original pattern as a string and capture the input span only for a finite timeout.", "retained", "H-027", "literal", "sliced-reused", "Retained in span kernel", "Borrowed literal overhead declines across launches and the ownership model simplifies. Earliest owned-string static screening results are tier-confounded and not claimed as a universal gain.", "local", "H-025"),
    hypothesis("H-029", "Force the shared entry to inline into public callers", "Apply AggressiveInlining to the common span entry to expose header specialization.", "rejected", "H-029", "dense", "reused", "Refined by H-030", "Monolithic forced inlining expands the owned-string instance body to about 4KiB and regresses reused dense/question-star workloads by over 100%. The child keeps the large kernel separate and later excludes existing strings entirely.", "local", "H-025"),
    hypothesis("H-030", "Separate the large loop from the visible matching header", "Combine visible span entry with NoInlining on the large MatchWindow span kernel.", "retained", "H-030", "dense", "reused", "Retained in span kernel", "Isolated comparison removes the forced-inlining regressions across launches. This combination is retained only for borrowed representations; H-031 restores the original string kernel rather than attributing the child as a universal string gain.", "local", "H-029"),
    hypothesis("H-031", "Representation-specific loops protect existing string calls", "Restore the exact released string method bodies and place tested span traversal overloads in a partial file; share semantic helpers and metadata.", "retained", "final", "question-star", "reused", "Retained", "String compatibility and latency are protected by retaining its measured implementation. Separate span traversal increases maintenance; exhaustive/differential coverage checks both against independent oracles. This burden is justified by the requested borrowed-memory API and observed shared-core regressions.", "refactor", "H-030")
]
h.insert(5, dict(id="H-028", claim="Inline a surviving string-to-span adapter", mechanism="Selective adapter inlining", prediction="Remove a forwarding frame only if one survives optimized compilation.",
    location="WildcardEngine.IsMatch(string,...) and public string callers", observation="Initial optimized callers already bypass the adapter and call the span core directly.",
    falsification="No surviving adapter call in optimized native code.", change="No production delta", changeScope="local", maintainability="No additional compiler annotation", codeComments="Not applicable",
    result="The relevant optimized adapter is already inlined; the surviving call is the larger span core. H-029 tests that separate mechanism.",
    state="not-applicable", decision="Ruled out", decisionRationale="The predicted forwarding frame is absent; do not add an unsupported annotation.", parentId="H-025", evidence=["spans/net10.0-native-code.asm.txt", "spans/plan.md"]))
final = summaries["final"]
own = [r for r in final if r["mode"] in ("static","reused")]
flagged = [r for r in own if r["effectPercent"] > 15 and r["candidate"]["median"]-r["control"]["median"] > 5 and all(p["median"] > 15 for p in r["paired"])]
limits = [
    "Borrowed API results are local complete-call measurements on one Windows x64 host. Already-owned strings, spans over strings, stack buffers and .NET runtimes can have different execution costs; removing allocation does not establish a universal latency improvement.",
    "Retaining representation-specific traversal increases maintenance. Validation, culture and newline definitions are shared; every algorithmic change must keep both independent-oracle entry points covered.",
    "Short H-027 screening static results show tier maturation sensitivity; later epochs add a documented joint primer. No raw latency is compared across those epochs. Runtime-default tiering/PGO remains enabled; background JIT and unrelated processes can still contribute variation.",
    "Initial/H-027/H-029 disassembly files include earlier appended bodies; final and H-030 captures are clean. Evidence attribution uses source epochs and explicit matching signatures. No hardware branch-miss, ISA or exact exclusive CPU claims are made.",
    "VT normalization fallback can allocate on first pool rent or exhaustion and retains pool capacity. Warm per-thread allocation counters omit retained/native memory and cold initialization; first calls are recorded separately."]
limits.append("Owned-string regression review triggers across all three launches: " + (", ".join(r["workload"]+"/"+r["mode"] for r in flagged) if flagged else "none") + ". Complete sample ranges and paired effects remain visible, including smaller regressions.")
limits.append("Restored string API/engine methods have identical normalized IL and implementation flags, verified in final/compiled-calls.txt. Nevertheless, static question-run medians differ by +77.1% and question-run-LF by +59.6%, with per-launch paired effects reversing direction (approximately +86/-26/+88% and +77/-30/+85%). These comparisons are inconclusive; code layout/tiering are plausible confounders, not measured causes. No universal preservation of latency is claimed. Conversely, the large dense reused gain between identical string bodies is not credited as an algorithmic improvement.")
decision = dict(hypotheses=h, retainedChanges=[
    "Static two-span matching, reusable span input matching, span pattern constructors and mutable/read-only span extension APIs accept UTF-16 views of stack, string and array buffers.",
    "Successful ordinary span matches allocate no input/pattern strings. Reusable construction owns a pattern copy; finite timeout exceptions materialize only borrowed input diagnostics.",
    "H-031 preserves the released string method bodies; H-027/H-030 are retained only in the span-specialized traversal. Shared option validation, culture folding and newline definitions constrain semantic drift.",
    "H-026 bounds VT normalization stack scratch and safely returns larger pooled storage; first-use allocation remains documented."],
    limitations=limits, epochIds={"Initial shared span":"H-025", "H-027":"H-027", "H-029":"H-029", "H-030":"H-030", "Final":"H-031"},
    generatedCode=[dict(hypothesisId="H-031", summary="Separate representation-specific kernels; released string source bodies restored exactly.",
        before="Initial/forced shared-core routing retained an extra entry or expanded the public caller into a large body, depending on annotation.",
        after="Final clean .NET 10 dump contains string and span signatures with independent diagnostic state; source comparison verifies the released string bodies. Span MatchWindow remains separate from its visible header.",
        artifact="spans/final/net10.0-native-code.asm.txt")],
    stop="Borrowed APIs and lifetime invariants pass; allocation removal and string/slice counterexamples measured; all nine new hypotheses resolved. Retain string specialization to avoid the evidenced shared-core burden. No further material hypothesis within this API/ownership scope.")
(HERE / "decisions.json").write_text(json.dumps(decision, indent=2, ensure_ascii=False), encoding="utf-8")
print("Decision ledger:", len(h), "hypotheses; owned-string review triggers:", len(flagged))
