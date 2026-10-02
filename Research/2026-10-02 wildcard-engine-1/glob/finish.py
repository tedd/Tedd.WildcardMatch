"""Final caller validation on an already validated isolated stage, under host lock."""
from pathlib import Path
import os, shutil, subprocess, sys
here=Path(__file__).resolve().parent
repo=here.parents[2]
work=Path(r'D:\Workspaces\AI\wildcard-glob')
env=dict(os.environ,TEMP=str(work/'temp'),TMP=str(work/'temp'),DOTNET_CLI_HOME=str(work/'cli'),
    NUGET_PACKAGES=str(work/'packages'),NUGET_HTTP_CACHE_PATH=str(work/'http-cache'),DOTNET_CLI_TELEMETRY_OPTOUT='1')
evidence=here/sys.argv[1]; evidence.mkdir(exist_ok=True)
assert not (evidence/'results').exists(), 'Completed BDN epochs are immutable'
stage=work/'bdn'; stage.mkdir(exist_ok=True)
for filename in ('BdnBench.cs','BdnBench.csproj'): shutil.copy2(here/filename,stage/filename)
shutil.copy2(repo/'src/Tedd.WildcardMatch.Benchmark/Comparison.cs',stage/'Comparison.cs')
shutil.copy2(stage/'Comparison.cs',evidence/'Comparison.cs.txt')
for filename in ('BdnBench.cs','BdnBench.csproj'): shutil.copy2(here/filename,evidence/(filename+'.txt'))
commands=[['dotnet','build',str(stage/'BdnBench.csproj'),'-c','Release'],
    ['dotnet',str(stage/'bin/Release/net10.0/BdnBench.dll'),'--filter','*','--launchCount','3',
        '--warmupCount','3','--iterationCount','10','--iterationTime','100','--artifacts',str(work/'bdn-artifacts'/sys.argv[1])]]
for index,command in enumerate(commands):
    print('EXEC',command,flush=True)
    with (evidence/f'command-{index}.txt').open('w',encoding='utf-8') as output:
        result=subprocess.run(command,cwd=stage,env=env,stdout=output,stderr=subprocess.STDOUT)
    result.check_returncode()
    print('COMPLETE',index,flush=True)
shutil.copytree(work/'bdn-artifacts'/sys.argv[1]/'results',evidence/'results')
