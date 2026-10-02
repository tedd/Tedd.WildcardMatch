"""Serialized local final sequence; no packaging, deployment, or external writes."""
from pathlib import Path
import os,subprocess,sys
here=Path(__file__).resolve().parent
repo=here.parents[2]
subprocess.run([sys.executable,'-B',str(here/'run.py'),'final','--variant','current','--acceptance','--all-tests','--profile'],cwd=repo,check=True)
subprocess.run([sys.executable,'-B',str(here/'summarize.py'),'final'],cwd=repo,check=True)
work=Path(r'D:\Workspaces\AI\wildcard-glob')
env=dict(os.environ,TEMP=str(work/'temp'),TMP=str(work/'temp'),DOTNET_CLI_HOME=str(work/'cli'),NUGET_PACKAGES=str(work/'packages'),NUGET_HTTP_CACHE_PATH=str(work/'http-cache'),DOTNET_EnableHWIntrinsic='0')
with (here/'final/portable-fallback.txt').open('w',encoding='utf-8') as output:
    subprocess.run(['dotnet','test',str(work/'candidate/src/Tedd.WildcardMatch.Tests'),'-c','Release','-f','net10.0','--no-build'],cwd=work,env=env,stdout=output,stderr=subprocess.STDOUT,check=True)
subprocess.run([sys.executable,'-B',str(here/'finish.py'),'bdn-final'],cwd=repo,check=True)
import shutil
shutil.copy2(repo/'src/Tedd.WildcardMatch.Tests/MaskEngineTest.cs',work/'candidate/src/Tedd.WildcardMatch.Tests/MaskEngineTest.cs')
env.pop('DOTNET_EnableHWIntrinsic',None)
with (here/'final/state-bit63-tests.txt').open('w',encoding='utf-8') as output:
    subprocess.run(['dotnet','test',str(work/'candidate/src/Tedd.WildcardMatch.Tests'),'-c','Release','--filter','FullyQualifiedName~BitmapStateBoundsHashCollisionsAndLongFallbackAgreeWithRegex'],cwd=work,env=env,stdout=output,stderr=subprocess.STDOUT,check=True)
