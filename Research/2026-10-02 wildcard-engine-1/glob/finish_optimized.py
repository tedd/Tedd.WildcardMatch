"""Optimized BDN corroboration and additional acceptance-bit boundary tests."""
from pathlib import Path
import os,subprocess,sys,shutil
here=Path(__file__).resolve().parent
repo=here.parents[2]
work=Path(r'D:\Workspaces\AI\wildcard-glob')
subprocess.run([sys.executable,'-B',str(here/'finish.py'),'bdn-optimized'],cwd=repo,check=True)
shutil.copy2(repo/'src/Tedd.WildcardMatch.Tests/MaskEngineTest.cs',work/'candidate/src/Tedd.WildcardMatch.Tests/MaskEngineTest.cs')
env=dict(os.environ,TEMP=str(work/'temp'),TMP=str(work/'temp'),DOTNET_CLI_HOME=str(work/'cli'),NUGET_PACKAGES=str(work/'packages'),NUGET_HTTP_CACHE_PATH=str(work/'http-cache'))
with (here/'final/state-bit63-tests.txt').open('w',encoding='utf-8') as output:
    subprocess.run(['dotnet','test',str(work/'candidate/src/Tedd.WildcardMatch.Tests'),'-c','Release','--filter','FullyQualifiedName~BitmapStateBoundsHashCollisionsAndLongFallbackAgreeWithRegex'],cwd=work,env=env,stdout=output,stderr=subprocess.STDOUT,check=True)
