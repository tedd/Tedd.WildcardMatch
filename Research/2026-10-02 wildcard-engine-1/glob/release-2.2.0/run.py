"""Full comparative release snapshot, local-only. Invoke under performance lock."""
from pathlib import Path
import datetime,hashlib,json,os,shutil,subprocess,sys
here=Path(__file__).resolve().parent
repo=here.parents[3]
work=Path(r'D:\Workspaces\AI\wildcard-release-2.2.0')
work.mkdir(parents=True,exist_ok=True)
(work/'temp').mkdir(exist_ok=True)
env=dict(os.environ,TEMP=str(work/'temp'),TMP=str(work/'temp'),DOTNET_CLI_HOME=str(work/'cli'),NUGET_PACKAGES=str(work/'packages'),NUGET_HTTP_CACHE_PATH=str(work/'http-cache'),DOTNET_CLI_TELEMETRY_OPTOUT='1')
assert not (here/'identity.json').exists(),'Completed release measurements are immutable'
shutil.copy2(repo/'global.json',work/'global.json')
for name in ('README.md','LICENSE'): shutil.copy2(repo/name,work/name)
for name in ('Tedd.WildcardMatch','Tedd.WildcardMatch.Benchmark'):
    shutil.copytree(repo/'src'/name,work/'src'/name,dirs_exist_ok=True,ignore=shutil.ignore_patterns('bin','obj','BenchmarkDotNet.Artifacts'))
benchmark=work/'src/Tedd.WildcardMatch.Benchmark'
program=(benchmark/'Program.cs').read_text()
program=program.replace('using BenchmarkDotNet.Running;', 'using BenchmarkDotNet.Running;\nusing BenchmarkDotNet.Configs;\nusing BenchmarkDotNet.Jobs;')
program=program.replace('BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);', '''var config = DefaultConfig.Instance.AddJob(Job.Default.WithAffinity(new IntPtr(1L << 30))
    .WithEnvironmentVariable("DOTNET_TieredCompilation", "0"));
BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);''')
(benchmark/'Program.cs').write_text(program)
matching=(benchmark/'MatchingBenchmarks.cs').read_text().replace('[MemoryDiagnoser]', '[MemoryDiagnoser]\n[DisassemblyDiagnoser(maxDepth: 1, exportCombinedDisassemblyReport: true)]')
(benchmark/'MatchingBenchmarks.cs').write_text(matching)
started=datetime.datetime.now(datetime.timezone.utc).isoformat()
def execute(command,name):
    print('EXEC',command,flush=True)
    with (here/name).open('w',encoding='utf-8') as output:
        subprocess.run(command,cwd=work,env=env,stdout=output,stderr=subprocess.STDOUT,check=True)
    print('COMPLETE',name,flush=True)
execute(['dotnet','build',str(benchmark),'-c','Release'],'build.txt')
dll=benchmark/'bin/Release/net10.0/Tedd.WildcardMatch.Benchmark.dll'
execute(['dotnet',str(dll),'--validate-packages',str(work/'validation.json')],'validation.txt')
shutil.copy2(work/'validation.json',here/'validation.json')
execute(['dotnet',str(dll),'--filter','*','--launchCount','3','--warmupCount','3','--iterationCount','10','--iterationTime','100','--artifacts',str(work/'artifacts')],'benchmark.txt')
shutil.copytree(work/'artifacts/results',here/'results')
assert 'DELEGATEPROFILE32' not in (here/'results/Tedd.WildcardMatchBenchmark.MatchingBenchmarks-asm.md').read_text()
for filename in ('Program.cs','Comparison.cs','MatchingBenchmarks.cs','ConstructionBenchmarks.cs'):
    shutil.copy2(benchmark/filename,here/(filename+'.txt'))
(here/'identity.json').write_text(json.dumps({'startedUtc':started,'endedUtc':datetime.datetime.now(datetime.timezone.utc).isoformat(),'engineRevision':subprocess.check_output(['git','rev-parse','HEAD'],cwd=repo,text=True).strip(),'version':'2.2.0','runtimePolicy':'DOTNET_TieredCompilation=0; CPU30; 3 launches,3 warmups,10 iterations,100ms; no database or external writes','productionSourceHashes':{p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in (repo/'src/Tedd.WildcardMatch').glob('*.cs')}},indent=2)+'\n')
