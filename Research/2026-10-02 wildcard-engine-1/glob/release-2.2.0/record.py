"""Record the comparative release epoch without replacing previous measurements."""
from pathlib import Path
import datetime,json,subprocess
here=Path(__file__).resolve().parent
root=here.parent.parent
repo=here.parents[3]
paths=['README.md','site/app.js','site/index.html','site/styles.css','site/assets/package-comparison.json','site/assets/matching-report.md','site/assets/construction-report.md','src/Tedd.WildcardMatch.Benchmark/Export-SiteBenchmarks.ps1','src/Tedd.WildcardMatch.Benchmark/Program.cs','src/Tedd.WildcardMatch.Benchmark/README.md','src/Tedd.WildcardMatch/Tedd.WildcardMatch.csproj']
for name in paths:
    original=subprocess.check_output(['git','show','HEAD:'+name],cwd=repo)
    value=(repo/name).read_text(encoding='utf-8-sig').replace('\r\n','\n')
    if original.count(b'\r\n')*2>original.count(b'\n'): value=value.replace('\n','\r\n')
    (repo/name).write_bytes((b'\xef\xbb\xbf' if original.startswith(b'\xef\xbb\xbf') else b'')+value.encode('utf-8'))
data=json.loads((root/'report-data.json').read_text(encoding='utf-8'))
assert not any(a['path'].startswith('glob/release-2.2.0/') for a in data['artifacts'])
snapshot=json.loads((repo/'site/assets/package-comparison.json').read_text(encoding='utf-8-sig'))
rows=[]
for workload in snapshot['workloads']:
    values=[]
    for library in ('DotNetGlob','TeddDirectReused','TeddDirectStatic'):
        result=next(r for r in snapshot['matching'] if r['workload']==workload['name'] and r['library']==library)
        values.append({'median':result['meanNs'],'min':min(result['samplesNs']),'max':max(result['samplesNs']),'n':len(result['samplesNs'])})
    rows.append({'label':workload['label'],'detail':workload['pattern'],'measurements':values})
data['series'].append({'title':'Release 2.2.0: complete package comparison epoch','description':'All 32 matching and eight constructor cases share one three-process optimized-JIT run. These bars select Glob and direct APIs from that epoch; full competitors remain in the release raw reports and site data. Bars are BDN means; whiskers span statistical samples. This is a distinct epoch from earlier paired investigations.','unit':'ns/call','lowerIsBetter':True,'rounds':['DotNet.Glob','Direct prepared','Direct static'],'rows':rows})
data['correctness'].append('Release 2.2.0: .NET 8/10/11 Release suite (240/240/243 tests), all 32 comparison adapters with zero oracle failures, pack verification for all three assemblies with no runtime package dependencies. Main page contains graphs and report links, with no detailed tables. Browser checks cover all selector values and their displayed ratios; package/site publication is authorized by the current user request.')
data['generatedAt']=datetime.datetime.now(datetime.timezone.utc).isoformat()
data['artifacts'] += [{'label':'Release 2.2.0 comparative raw report','path':'glob/release-2.2.0/results/Tedd.WildcardMatchBenchmark.MatchingBenchmarks-report-full.json'},{'label':'Release assurance','path':'glob/release-2.2.0/assurance.json'},{'label':'Release benchmark identity','path':'glob/release-2.2.0/identity.json'}]
(root/'report-data.json').write_bytes((json.dumps(data,indent=2,ensure_ascii=False)+'\n').replace('\n','\r\n').encode('utf-8'))
