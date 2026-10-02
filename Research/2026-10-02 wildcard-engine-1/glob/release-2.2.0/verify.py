"""Local release assurance. No publishing or database-changing execution paths."""
from pathlib import Path
import json,os,shutil,subprocess,zipfile,xml.etree.ElementTree as ET
here=Path(__file__).resolve().parent
repo=here.parents[3]
work=Path(r'D:\Workspaces\AI\wildcard-release-2.2.0')
env=dict(os.environ,TEMP=str(work/'temp'),TMP=str(work/'temp'),DOTNET_CLI_HOME=str(work/'cli'),NUGET_PACKAGES=str(work/'packages'),NUGET_HTTP_CACHE_PATH=str(work/'http-cache'),DOTNET_CLI_TELEMETRY_OPTOUT='1')
for name in ('README.md','LICENSE'): shutil.copy2(repo/name,work/name)
for name in ('Tedd.WildcardMatch','Tedd.WildcardMatch.Benchmark','Tedd.WildcardMatch.Tests'):
    shutil.copytree(repo/'src'/name,work/'src'/name,dirs_exist_ok=True,ignore=shutil.ignore_patterns('bin','obj','BenchmarkDotNet.Artifacts'))
def execute(command,name):
    print('EXEC',command,flush=True)
    with (here/name).open('w',encoding='utf-8') as output:
        subprocess.run(command,cwd=work,env=env,stdout=output,stderr=subprocess.STDOUT,check=True)
    print('COMPLETE',name,flush=True)
execute(['dotnet','test',str(work/'src/Tedd.WildcardMatch.Tests'),'-c','Release'],'release-tests.txt')
execute(['dotnet','build',str(work/'src/Tedd.WildcardMatch.Benchmark'),'-c','Release'],'release-benchmark-build.txt')
execute(['dotnet','pack',str(work/'src/Tedd.WildcardMatch/Tedd.WildcardMatch.csproj'),'-c','Release','--output',str(work/'packages-out')],'pack.txt')
with zipfile.ZipFile(work/'packages-out/Tedd.WildcardMatch.2.2.0.nupkg') as package:
    paths=package.namelist()
    targets=['lib/'+t+'/Tedd.WildcardMatch.dll' for t in ('netstandard2.1','net10.0','net11.0')]
    assert all(t in paths for t in targets)
    assert 'README.md' in paths and 'LICENSE' in paths
    xml=ET.fromstring(package.read('Tedd.WildcardMatch.nuspec'))
    ns={'n':xml.tag.split('}')[0].strip('{')}
    assert xml.find('n:metadata/n:version',ns).text=='2.2.0'
    dependencies=xml.find('n:metadata/n:dependencies',ns)
    assert dependencies is None or not dependencies.findall('.//n:dependency',ns)
site=repo/'site'
data=json.loads((site/'assets/package-comparison.json').read_text(encoding='utf-8-sig'))
assert len(data['matching'])==32 and len(data['construction'])==8
html=(site/'index.html').read_text(encoding='utf-8-sig')
assert '<table' not in html and 'results-panel' not in html
assert html.count('data-direct-speedup')==2 and html.count('class="bar-row')==16
assert 'Version 2.1.0 adds' not in html
ratios={w['name']:next(r['meanNs'] for r in data['matching'] if r['workload']==w['name'] and r['library']=='DotNetGlob')/next(r['meanNs'] for r in data['matching'] if r['workload']==w['name'] and r['library']=='TeddDirectReused') for w in data['workloads']}
(here/'assurance.json').write_text(json.dumps({'packageVersion':'2.2.0','assemblies':targets,'runtimeDependencies':0,'mainPageTables':0,'matchingCases':32,'constructionCases':8,'preparedGlobSpeedups':ratios,'siteUsesSingleMeasuredEpoch':True},indent=2)+'\n')
