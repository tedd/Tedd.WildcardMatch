"""Append the Glob continuation without mixing historical baseline epochs."""
from pathlib import Path
import datetime,json,statistics
here=Path(__file__).resolve().parent
root=here.parent
path=root/'report-data.json'
d=json.loads(path.read_text(encoding='utf-8-sig'))
assert not any(h['id']=='H-032' for h in d['hypotheses']), 'Continuation already appended; preserve historical report.'
def read(name): return json.loads((here/name).read_text(encoding='utf-8-sig'))
final=read('final/summary.json')
for required in ('final/identity.json','final/profile-summary.json','final/portable-fallback.txt','final/state-bit63-tests.txt','bdn-optimized/results/GlobAcceptance-report-full.json'):
    assert (here/required).is_file(), required
bdn=read('bdn-optimized/results/GlobAcceptance-report-full.json')
assert len(bdn['Benchmarks'])==12 and all(b['Statistics'] is not None for b in bdn['Benchmarks'])
asm=(here/'bdn-optimized/results/GlobAcceptance-asm.md').read_text()
assert 'DELEGATEPROFILE32' not in asm and 'COUNTPROFILE32' not in asm
bdnRows={}
for b in bdn['Benchmarks']:
    params=dict(part.split('=',1) for part in b['Parameters'].split('&'))
    bdnRows[(params['Workload'],params['Library'])]=b
bdnChart=[]
for name in ('Literal','Simple','MultiStar','LongText'):
    values=[]
    for library in ('DotNetGlob','TeddDirectReused','TeddDirectStatic'):
        b=bdnRows[(name,library)]
        actual=[m['Nanoseconds']/m['Operations'] for m in b['Measurements'] if m['IterationMode']=='Workload' and m['IterationStage']=='Actual']
        values.append({'median':b['Statistics']['Mean'],'min':min(actual),'max':max(actual),'n':len(actual)})
    bdnChart.append({'label':name,'detail':'Separate processes, optimized JIT; standard BDN statistical mean','measurements':values})
d['series'].append({'title':'Glob corroboration: optimized BenchmarkDotNet epoch','description':'TieredCompilation=0; three launches, 3 warmups, 10 iterations at 100ms. Bars are BDN means with its default outlier policy; whiskers show all raw actual iteration extrema. Not combined with paired default-PGO epoch.','unit':'ns/call','lowerIsBetter':True,'rounds':['DotNet.Glob','Direct prepared','Direct static'],'rows':bdnChart})
(here/'bdn-optimized/summary.json').write_text(json.dumps([{'fixture':name,'glob':bdnRows[(name,'DotNetGlob')]['Statistics']['Mean'],'prepared':bdnRows[(name,'TeddDirectReused')]['Statistics']['Mean'],'static':bdnRows[(name,'TeddDirectStatic')]['Statistics']['Mean'],'globOverPrepared':bdnRows[(name,'DotNetGlob')]['Statistics']['Mean']/bdnRows[(name,'TeddDirectReused')]['Statistics']['Mean']} for name in ('Literal','Simple','MultiStar','LongText')],indent=2)+'\n',encoding='utf-8')
def row(name,mode='reused'): return next(r for r in final if r['fixture']==name and r['mode']==mode)
simple=row('Simple')
d['generatedAt']=datetime.datetime.now(datetime.timezone.utc).isoformat()
d['scope']+=' Glob continuation: measured prepared and static ordinal callers against DotNet.Glob 3.1.3. Preserve the entire UTF-16/Regex contract and earlier epochs. .NET 10/11 compile-time intrinsics, portable .NET Standard fallback. No application or universal-fastest claim.'
d['question']+=' Can specialized prepared masks exceed twice prepared Glob throughput on the unchanged question-mark fixture without result caching or a narrower Unicode contract?'
oldsummary=d['summary']
d['summary']={'baseline':simple['ns']['glob'],'final':simple['ns']['candidate'],'unit':'ns/call',
    'changePercent':100*(simple['ns']['candidate']/simple['ns']['glob']-1),
    'headline':f"Glob continuation: prepared report-??.txt {simple['ns']['glob']:.2f} → {simple['ns']['candidate']:.2f} ns ({simple['ns']['glob']/simple['ns']['candidate']:.2f}× throughput). Static calls, cold cost and losing contrasts are explicit; this is not a universal 2× claim."}
for name in ('Literal','Simple','MultiStar','LongText'):
    r=row(name)
    d['scorecards'].append({'title':'Glob continuation: '+name+' / prepared','baseline':r['ns']['glob'],'value':r['ns']['candidate'],'unit':'ns/call','wallChangePercent':100*(r['ns']['candidate']/r['ns']['glob']-1),'progression':'Final contemporaneous Glob → integrated; three processes, 54 unfiltered samples/engine'})
def measurement(r,e):
    launches=list(r['launchMedians'][e].values())
    return {'median':r['ns'][e],'min':min(launches),'max':max(launches),'n':r['samples'][e]}
for mode in ('reused','static','span-reused','span-static','construction'):
    rows=[]
    for r in final:
        if r['mode']!=mode: continue
        engines=['control','candidate']
        if 'glob' in r['ns']: engines.append('glob')
        # Each chart keeps a consistent number of rounds. Semantic mismatches omit Glob.
        if len(engines)!=3: continue
        rows.append({'label':r['fixture'],'detail':mode+'; exact same validated inputs','measurements':[measurement(r,e) for e in engines]})
    lifecycle=mode=='construction'
    d['series'].append({'title':'Glob final epoch: '+('construct + first match' if lifecycle else mode),'description':'Warm-process median '+('ns/lifecycle; all engines construct then match the same first input.' if lifecycle else 'ns/match.')+' Whiskers span process medians. Control is frozen fe4fff6 C#. No samples discarded.','unit':'ns/lifecycle' if lifecycle else 'ns/call','lowerIsBetter':True,'rounds':['Released direct','Integrated','DotNet.Glob'],'rows':rows})
effects=[]
for r in final:
    if r['mode']!='reused': continue
    paired=[r['launchMedians']['candidate'][k]/v-1 for k,v in r['launchMedians']['control'].items()]
    effects.append({'hypothesisId':'H-032–H-057 combination','label':r['fixture'],'effect':r['ns']['candidate']/r['ns']['control']-1,'range':[min(paired),max(paired)],'note':'Combined integration, not isolated H-057 effect; process effects '+', '.join(f'{x:+.1%}' for x in paired)})
d['effectGroups'].append({'name':'Glob continuation: integrated versus frozen control','control':'Final epoch only; run-range effects use contemporaneous process medians. Constituents were isolated before combination.','items':effects})
entries=[
 (32,'Clock-free infinite ordinal kernel','retained','Removes timeout/options dispatch while preserving UTF-16 and final LF; literal bypass and long intrinsic literal search materially improve complete callers.'),
 (33,'Prepared no-star width and question plan','retained','Parent mechanism retained within fixed masks; scalar prototype is superseded by H-035, with portable scalar fallback.'),
 (34,'Portable packed 64-bit masks','rejected','Correct but slower than modern SIMD, and first-literal misses regress versus scalar; retain portable scalar fallback.'),
 (35,'Two overlapping Vector128 masked comparisons','retained','Large repeatable fixed-width gain beyond control variance; all lanes, LF and exact-end slices tested. Fused single error reduction.'),
 (36,'Vector256 long fixed-pattern blocks','retained','Dense 512-unit matching improves roughly sevenfold against prepared scalar; bounded immutable state and hardware fallback constrain maintenance.'),
 (37,'Dynamic masks for allocation-free static calls','retained','Complete static question-mark calls improve without cache/state allocation; still not universally twice prepared Glob.'),
 (38,'Short-star intrinsic literal search threshold','rejected','MultiStar and mixed near misses regress; existing threshold remains.'),
 (39,'Prepared star-separated segments','retained','Retained only for long inputs after bounded dispatch; short matching and setup lose in isolation. Prefix/suffix/literal anchors avoid long retries.'),
 (40,'Dictionary literal-mask bitmap NFA','rejected','Lookup and linear long-input traversal regress. State closure correctness survives in descendants; dictionary representation rejected.'),
 (41,'ASCII bitmap lookup table','rejected','Short MultiStar exceeded twice Glob, but 1 KiB table, ASCII-only plan eligibility and severe long-text regression justify compact bounded descendant instead. Unicode patterns preserve fallback semantics.'),
 (42,'Separate prepared literal plan','rejected','Matching gain is available through H-032 without the extra allocation; standalone plan is redundant.'),
 (43,'Generic finite-timeout specialization','not-applicable','Measured calls have infinite timeout; H-032 already removes clock checks. Finite timeout behavior remains original and tested.'),
 (44,'Two-slot thread-local pattern cache','rejected','Churn cost 149.56 versus 80.86 ns and allocated 224 B/call. Warm repeated gains do not justify hidden retained state and cold regressions.'),
 (45,'Scalar second-anchor rejection','retained','Descriptor’s second literal enables cheap rejection and H-048 vector filter; isolated mixed near-miss gain supports parent mechanism.'),
 (46,'Unfused two-stage literal then LF SIMD checks','inconclusive','Direction reverses across processes; H-035 fused reduction is retained. Initial plan label described fusion, but executed transform separates reductions.'),
 (47,'MemoryMarshal packed-mask variant','not-applicable','Duplicate of H-034, not an independent experiment.'),
 (48,'SIMD bitmap over eight candidate positions','retained','Mixed near misses improved about 15× versus second-anchor control; exact two-anchor lanes and safe tail retain portable IndexOf fallback.'),
 (49,'Narrower Unicode opt-out contract','not-applicable','Default ordinal path already compares UTF-16 units without decoding/folding. No measured removable Unicode cost supports an API contract change.'),
 (50,'Krauss prefix loop and lazy latest-star retry','not-applicable','Already implemented in the frozen engine and H-032; linked NUL-terminated routines do not establish universal optimality.'),
 (51,'Cached immutable fixed width','inconclusive','Small layout-dependent gain with other process shifts; extra field is omitted pending evidence stronger than host/code-layout variance.'),
 (52,'Known None option shortcut','inconclusive','Simple static improves, but other caller directions and large host shifts confound universal adoption. Keep general validation order.'),
 (53,'Force-inline dynamic fixed helper','inconclusive','Approximately 7% static gain accompanies reused regression and code-layout variation; omit attribute.'),
 (54,'One readonly vector Block array','retained','Matching is neutral within noise; removes a parallel array/allocation and ownership relation. ref readonly prevents plan copies.'),
 (55,'Compact exact UTF-16 literal-mask table','rejected','Standalone strict latency criterion failed: MultiStar 21.61 → 23.96 ns versus H-041 (+10.8%), despite constructor allocation 1272 → 288 B and contemporaneous Glob 56.77 ns (2.37×). The compact representation is carried into bounded H-057; standalone unbounded engine is rejected. Exact Code checks preserve UTF-16 collisions.'),
 (56,'Exact two-pass segment allocation','retained','Simplification removes List growth and ToArray with unchanged matching logic; lower constructor bytes justify neutral noisy matching.'),
 (57,'Bound bitmap matching to short input','retained','Avoids linear long-text regression while preserving short NFA; final combination adds fixed SIMD and bounded compiled anchors. Preparation cost is charged explicitly.')]
parents={33:32,34:33,35:33,36:35,37:32,38:32,39:32,41:40,45:39,46:35,48:45,51:35,52:37,53:37,54:36,55:41,56:48,57:55}
for number,claim,state,rationale in entries:
    ident=f'H-{number:03}'
    candidates=sorted(p for p in here.glob(ident+'*') if p.is_dir() and (p/'summary.json').exists())
    result=[]
    for epoch in candidates:
        values=json.loads((epoch/'summary.json').read_text())
        controlVariant=json.loads((epoch/'identity.json').read_text()).get('controlVariant','baseline (original driver)')
        focus='Dense' if number in (36,54) else 'MixedAnchorNearMiss' if number in (45,48,56) else 'MultiStar' if number in (40,41,55,57) else 'Simple'
        for r in values:
            if r['fixture']==focus and r['mode']==('static' if number in (37,44,52,53) else 'reused'):
                ratios=[r['launchMedians']['control'][k]/v for k,v in r['launchMedians']['candidate'].items()]
                result.append(f"{epoch.name} {focus}/{r['mode']}: control {controlVariant} {r['ns']['control']:.2f}, candidate {r['ns']['candidate']:.2f} ns; process speedup range {min(ratios):.2f}–{max(ratios):.2f}×; {r['bytes']['candidate']:.0f} B/match.")
    item={'id':ident,'claim':claim,'mechanism':rationale,'prediction':'Reduce repeated traversal, dispatch, lookup or preparation cost in the predeclared affected caller; plan.md records the falsifiable mechanism.','observation':'Frozen caller, native code, independent oracle and contemporaneous control/candidate packets; adaptive branches declared before execution in glob/plan.md.','location':'src/Tedd.WildcardMatch/*.cs; glob isolated source snapshots','caller':'Default ordinal string/span static, reused and construction calls','falsification':'Oracle mismatch, overread, escaped span, unexpected match allocation, or material regression beyond process variation and setup burden.','change':claim,'changeScope':'simplification' if number in (54,56) else 'refactor' if number in (32,39,57) else 'local','maintainability':'Bounded private immutable state; portable and ISA fallbacks. Code comments document fragile mechanisms and Research IDs.','codeComments':'Adjacent production comments reference retained mechanism IDs and range/state invariants.','result':' '.join(result) or rationale,'state':state,'decision':state,'decisionRationale':rationale,'evidence':['glob/plan.md']+[f'glob/{p.name}/summary.json' for p in candidates]}
    if number in parents: item['parentId']=f'H-{parents[number]:03}'
    d['hypotheses'].append(item)
d['retainedChanges'] += [
 'Glob continuation H-032/H-037: allocation-free static and literal default kernel, while finite timeout/case/newline modes retain the released engines.',
 'H-035/H-036/H-054: compile-time NET10_0_OR_GREATER intrinsics with runtime hardware checks; exact overlapping loads, immutable vector Blocks, maximum 32 KiB vector payload.',
 'H-055/H-057: bounded 63-token bitmap plan, exact UTF-16 literal verification, collapsed stars, input length <=32 dispatch; larger text uses compiled anchors or scalar fallback.',
 'H-039/H-045/H-048/H-056: long-text anchor search and SIMD candidate-position masks; at most 64 descriptors and 8192 pattern units. Preserve H-020 bulk leading question handling.']
d['environment'] += [{'label':'Glob continuation epoch','value':'fe4fff6037442e5fe6da9128e8d689e4f19a59ab plus captured dirty project metadata. Existing README/site/Comparison edits preserved; see glob/baseline-dirty.txt.'},{'label':'Final runtime/controls','value':'net10.0 x64; SDK 11.0.100-rc.1.26425.128; runtime 10.0.12; CPU30; High performance; default tiering/PGO; workstation GC. Environment and ISA flags captured in raw launch JSON.'},{'label':'Host exclusivity','value':'All builds, tests, diagnostics and timing under fixed OS lock; scratch D:/Workspaces/AI/wildcard-glob.'}]
d['hotspots'].append({'name':'Ordinal fixed-width and star traversal','location':'WildcardEngine; fixed and retry loops','evidence':'Paired complete caller baseline and sampled local profile; H-032 DefaultWindow dominates sampled candidate stack residence. Final profile includes changed kernels. No exact exclusive CPU or application trace.','limit':'Scalar token dispatch, repeated mismatch work and general timeout header. Intrinsics and prepared state reduce these locally.'})
d['areas'] += [{'name':'Glob continuation catalogue coverage','boundary':'Ordinal matcher and plan construction','evidence':'glob/plan.md; isolated source/native-code/raw packets; final complete calls','catalogue':'CPU C1–C4; Memory M3–M5/M7; Storage S1/S2/S4/S5; Runtime R1/R2','hypothesisIds':[f'H-{i:03}' for i in range(32,58)],'disposition':'All concrete branches classified; M1/M2 constrain setup and no-allocation matching. M6/C5/S3/R3/T1–T4 unsupported by measured workload; no contention, prefetch or scheduling trace.'}]
d['correctness'] += ['Glob continuation: full Release and Debug matrix on net8.0/net10.0/net11.0; counts and logs in glob/final. net8 exercises netstandard2.1 portable code. DOTNET_EnableHWIntrinsic=0 full net10 suite checks runtime fallback.', 'New deterministic width/lane/tail/LF/Unicode mask fuzz, state63/64, lookup collisions, long-plan caps and concurrent mutable-span reuse complement existing exhaustive DP oracle, seeded Unicode and Regex tests. Inputs are never cached or retained.', 'Failed first integration validation was a five-second backtracking Regex-oracle timeout on a pathological fixture; preserved in glob/integrated-screen. Oracle then uses NonBacktracking for bounded patterns, ordinary Regex for large literal/question patterns to respect its symbolic-state limit.']
d['generatedCode'] += [{'hypothesisId':'H-035/H-048/H-054','summary':'Tier1 vector logical/compare operations and fused reduction; candidate-position extraction plus guarded trailing-zero scan. Long blocks use Vector256 and readonly paired plans.','before':'Scalar per-token dispatch and retry.','after':'In-range masked loads and candidate bitmap rejection, with portable fallback.','artifact':'glob/final/native-code.asm.txt'}, {'hypothesisId':'H-057','summary':'Compile-time NET10_0_OR_GREATER covers net10/net11; netstandard compiles scalar spans. Runtime hardware guards preserve portable execution. ARM64 code and throughput were not measured.','before':'General header and scalar traversal.','after':'Bounded dispatcher and prepared kernels.','artifact':'glob/final/native-code.asm.txt'}]
d['validityThreats'] += ['Glob continuation: host process shifts occasionally exceed 2× absolute timing despite affinity/lock. Same-IL alias screening showed about 7% variation. Small directional changes remain inconclusive; all process ranges are exposed. Separate BDN process evidence corroborates the primary caller.', 'Screening v1 used common counts; v2 uses per-engine equal-duration calibration after >100× ratios made slow samples disproportionate. Preserve both epochs; do not add effects across epochs.', 'A 2× success on report-??.txt is workload-specific. First-literal misses and static calls may remain below 2×; Glob LF/empty/case semantic mismatches are excluded rather than timed as equivalent. The Krauss articles support hypotheses, not a fastest-algorithm theorem.', 'Reuse trades extra constructor storage/time for matching throughput. Final footprint and construction rows charge both short and long prepared plans; no hidden result or thread-local cache. Assembly/runtime/ISA portability remains guarded, with unsupported hardware performance unknown.', 'First baseline failed on Glob empty-pattern construction; first profile used the wrong provider casing and yielded no useful samples. Both failed packets are retained; valid replacements are identified.']
d['validityThreats'].append('Construction timings are warm-process construction plus first match, not process startup. Separate footprint.json measures constructor allocation only; tiny fractional bookkeeping overhead is exposed without rounding to an exact retained-object size.')
d['validityThreats'].append('First default-tiered BDN epoch, glob/bdn-final, exposed instrumented Tier0 with delegate/count profiling helpers and inflated absolute timings. It is diagnostic only. glob/bdn-optimized explicitly disables tiered compilation and confirms optimized caller code; its different runtime policy and standard outlier handling remain a separate corroboration epoch.')
d['reproduction'] += ['python C:/Users/tedd/.codex/skills/scientific-method-performance/scripts/run_with_performance_lock.py --cwd D:/SourceCode/Tedd.WildcardMatch -- python -B "Research/2026-10-02 wildcard-engine-1/glob/validate_final.py" (completed epoch names are immutable; use fresh names to reproduce).', 'Isolated branches: glob/run.py <fresh-epoch> --variant H-035 --control-variant baseline; summarize.py <epoch>. variants.py always uses immutable baseline snapshots except explicitly named current integration.', 'Append glob/append_report.py once to the historical report; render with the skill render_report.py. Full commands, source hashes, fixture values, raw results, profiling and disassembly remain versioned.']
d['artifacts'] += [{'label':'Glob experiment plan','path':'glob/plan.md'},{'label':'Final raw identity and hashes','path':'glob/final/identity.json'},{'label':'Final complete-caller results','path':'glob/final/summary.json'},{'label':'Constructor allocation footprint','path':'glob/final/footprint.json'},{'label':'Final sampled profile','path':'glob/final/profile-summary.json'},{'label':'Native Tier1 kernels','path':'glob/final/native-code.asm.txt'},{'label':'Independent optimized BDN caller validation','path':'glob/bdn-optimized/results/GlobAcceptance-report-github.md'},{'label':'Rejected instrumented BDN epoch','path':'glob/bdn-final/results/GlobAcceptance-asm.md'}]
d['status']='complete'
path.write_text(json.dumps(d,indent=2,ensure_ascii=False)+'\n',encoding='utf-8')
