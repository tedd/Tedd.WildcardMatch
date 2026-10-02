"""Read-only IL inspection. Local build executes under the shared host lock."""
from pathlib import Path
import os
import shutil
import subprocess
HERE = Path(__file__).resolve().parent
WORK = Path(r"D:\Workspaces\AI\wildcard-spans")
INSPECT = WORK / "inspect"
INSPECT.mkdir(exist_ok=True)
shutil.copy2(HERE / "InspectCalls.cs", INSPECT / "Program.cs")
(INSPECT / "InspectCalls.csproj").write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable></PropertyGroup></Project>')
os.environ.update(TMP=str(WORK / "temp"), TEMP=str(WORK / "temp"), DOTNET_CLI_HOME=str(WORK / "cli"))
command = ["dotnet", "run", "--project", str(INSPECT / "InspectCalls.csproj"), "-c", "Release", "--",
    str(WORK / "control/bin/Release/net10.0/Tedd.WildcardMatch.Control.dll"),
    str(WORK / "candidate/bin/Release/net10.0/Tedd.WildcardMatch.dll"),
    str(WORK / "rig/bin/Release/net10.0/SpanBench.dll")]
result = subprocess.run(command, cwd=WORK, stdout=subprocess.PIPE, stderr=subprocess.STDOUT, text=True, encoding="utf-8")
(HERE / "final/compiled-calls.txt").write_text(result.stdout, encoding="utf-8")
print(result.stdout)
result.check_returncode()
