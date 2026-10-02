"""Inspected local-only glob experiment driver. Invoke under fixed performance lock."""
import argparse
import datetime
import hashlib
import json
import os
from pathlib import Path
import shutil
import subprocess
import sys
sys.dont_write_bytecode = True
sys.stdout.reconfigure(encoding="utf-8")
HERE=Path(__file__).resolve().parent
REPO=HERE.parents[2]
WORK=Path(r"D:\Workspaces\AI\wildcard-glob")
parser=argparse.ArgumentParser()
parser.add_argument("epoch")
parser.add_argument("--freeze",action="store_true")
parser.add_argument("--acceptance",action="store_true")
parser.add_argument("--profile",action="store_true")
parser.add_argument("--all-tests",action="store_true")
parser.add_argument("--variant",default="baseline")
parser.add_argument("--control-variant",default="baseline")
parser.add_argument("--cache-contrasts",action="store_true")
args=parser.parse_args()
args.cache_contrasts = args.cache_contrasts or args.variant == "H-044"
EVIDENCE=HERE/args.epoch
EVIDENCE.mkdir(exist_ok=True)
assert not (EVIDENCE/"identity.json").exists(), "Completed evidence epochs are immutable; choose a fresh epoch name"
WORK.mkdir(parents=True,exist_ok=True)
(WORK/"temp").mkdir(exist_ok=True)
os.environ.update(TEMP=str(WORK/"temp"),TMP=str(WORK/"temp"),DOTNET_CLI_HOME=str(WORK/"cli"),
    NUGET_PACKAGES=str(WORK/"packages"),NUGET_HTTP_CACHE_PATH=str(WORK/"http-cache"),
    DOTNET_SKIP_FIRST_TIME_EXPERIENCE="1",DOTNET_CLI_TELEMETRY_OPTOUT="1")
shutil.copy2(REPO/"global.json",WORK/"global.json")
started=datetime.datetime.now(datetime.timezone.utc).isoformat()
def execute(command,destination,env=None):
    print("EXEC",command,flush=True)
    result=subprocess.run(command,cwd=WORK,env=env,stdout=subprocess.PIPE,stderr=subprocess.STDOUT,text=True,encoding="utf-8",errors="replace")
    destination.write_text(result.stdout,encoding="utf-8")
    print("\n".join(result.stdout.splitlines()[-10:]),flush=True)
    result.check_returncode()
baseline=HERE/"baseline-source"
if args.freeze:
    assert not baseline.exists(),"Baseline already frozen"
    baseline.mkdir()
    for file in (REPO/"src/Tedd.WildcardMatch").glob("*"):
        if file.suffix in (".cs",".csproj"): shutil.copy2(file,baseline/(file.name+".txt"))
    shutil.copy2(REPO/"site/assets/package-comparison.json",HERE/"baseline-site.json")
    (HERE/"baseline-dirty.txt").write_text(subprocess.check_output(["git","diff","--stat"],cwd=REPO).decode(),encoding="utf-8")
assert baseline.exists()
for name in ("control","candidate"):
    stage=WORK/name
    (stage/"src/Tedd.WildcardMatch").mkdir(parents=True,exist_ok=True)
    for file in ("README.md","LICENSE"): shutil.copy2(REPO/file,stage/file)
    if name=="control":
        for file in baseline.glob("*.txt"):
            data=file.read_text(encoding="utf-8-sig")
            if file.name.endswith(".csproj.txt"): data=data.replace("<PackageId>Tedd.WildcardMatch</PackageId>","<AssemblyName>Tedd.WildcardMatch.Control</AssemblyName><PackageId>Tedd.WildcardMatch.Control</PackageId>")
            (stage/"src/Tedd.WildcardMatch"/file.name[:-4]).write_text(data,encoding="utf-8")
        from variants import variant
        generated=variant(args.control_variant)
        for file in (stage/"src/Tedd.WildcardMatch").glob("*.cs"):
            if file.name not in generated:
                assert WORK.resolve() in file.resolve().parents
                file.unlink()
        for filename,source in generated.items(): (stage/"src/Tedd.WildcardMatch"/filename).write_text(source,encoding="utf-8")
        (EVIDENCE/"control").mkdir(exist_ok=True)
        for file in (stage/"src/Tedd.WildcardMatch").glob("*.cs"): shutil.copy2(file,EVIDENCE/"control"/(file.name+".txt"))
    else:
        shutil.copytree(REPO/"src/Tedd.WildcardMatch",stage/"src/Tedd.WildcardMatch",dirs_exist_ok=True,ignore=shutil.ignore_patterns("bin","obj"))
        from variants import variant
        generated=variant(args.variant)
        for file in (stage/"src/Tedd.WildcardMatch").glob("*.cs"):
            if file.name not in generated:
                assert WORK.resolve() in file.resolve().parents
                file.unlink()
        for name,source in generated.items(): (stage/"src/Tedd.WildcardMatch"/name).write_text(source,encoding="utf-8")
        for file in (stage/"src/Tedd.WildcardMatch").glob("*.cs"): shutil.copy2(file,EVIDENCE/(file.name+".txt"))
execute(["dotnet","--info"],EVIDENCE/"environment.txt")
execute(["powershell","-NoProfile","-Command","Get-CimInstance Win32_Processor | Select-Object Name,NumberOfLogicalProcessors; Get-Process dotnet,testhost,VBCSCompiler -ErrorAction SilentlyContinue | Select-Object ProcessName,CPU; powercfg /getactivescheme; exit 0"],EVIDENCE/"host.txt")
tests=WORK/"candidate/src/Tedd.WildcardMatch.Tests"
shutil.copytree(REPO/"src/Tedd.WildcardMatch.Tests",tests,dirs_exist_ok=True,ignore=shutil.ignore_patterns("bin","obj","TestResults"))
execute(["dotnet","test",str(tests),"-c","Release"]+([] if args.all_tests else ["-f","net10.0"]),EVIDENCE/"correctness.txt")
if args.all_tests: execute(["dotnet","test",str(tests),"-c","Debug"],EVIDENCE/"Debug.txt")
rig=WORK/"rig"
rig.mkdir(exist_ok=True)
for file in ("Program.cs","GlobBench.csproj"):
    shutil.copy2(HERE/file,rig/file)
    shutil.copy2(HERE/file,EVIDENCE/(file+".txt"))
execute(["dotnet","build",str(rig/"GlobBench.csproj"),"-c","Release"],EVIDENCE/"build.txt")
dll=rig/"bin/Release/net10.0/GlobBench.dll"
asm=WORK/(args.epoch+".asm.txt")
asm.write_text("",encoding="utf-8")
execute(["dotnet",str(dll),"0","--asm-only"],EVIDENCE/"native-code.txt",dict(os.environ,
    DOTNET_JitDisasm="Tedd.WildcardEngine:* Tedd.WildcardMatch:* Tedd.PreparedPattern:* Tedd.DefaultWildcardEngine:* Tedd.FixedPattern:* Tedd.ShortPattern:* Tedd.CompiledSegments:*",DOTNET_JitStdOutFile=str(asm)))
shutil.copy2(asm,EVIDENCE/"native-code.asm.txt")
for launch in range(1,4):
    execute(["dotnet",str(dll),str(launch)]+(["--acceptance"] if args.acceptance else [])+(["--cache-contrasts"] if args.cache_contrasts else []),EVIDENCE/f"launch-{launch}.txt")
    shutil.copy2(WORK/"results.json",EVIDENCE/f"launch-{launch}.json")
    shutil.copy2(WORK/"validation.json",EVIDENCE/"validation.json")
    shutil.copy2(WORK/"footprint.json",EVIDENCE/"footprint.json")
if args.profile:
    trace=WORK/(args.epoch+".nettrace")
    execute([r"C:\Users\tedd\.dotnet\tools\dotnet-trace.exe","collect","--providers","Microsoft-DotNETCore-SampleProfiler",
        "--output",str(trace),"--format","Speedscope","--","dotnet",str(dll),"0","--profile"],EVIDENCE/"profile.txt")
    data=json.loads(trace.with_suffix(".speedscope.json").read_text(encoding="utf-8-sig"))
    from collections import Counter
    inclusive,exclusive=Counter(),Counter()
    frames=data["shared"]["frames"]
    for p in data["profiles"]:
        if p["type"]!="evented": continue
        stack=[]; previous=p["startValue"]
        for e in p["events"]:
            weight=e["at"]-previous
            for f in set(stack): inclusive[frames[f]["name"]]+=weight
            if stack: exclusive[frames[stack[-1]]["name"]]+=weight
            if e["type"]=="O": stack.append(e["frame"])
            else: stack.pop()
            previous=e["at"]
    (EVIDENCE/"profile-summary.json").write_text(json.dumps(dict(note="Sampled stack residence; not exact retired CPU counters or an application trace",inclusive=inclusive.most_common(35),exclusive=exclusive.most_common(35)),indent=2),encoding="utf-8")
(EVIDENCE/"identity.json").write_text(json.dumps(dict(startedUtc=started,endedUtc=datetime.datetime.now(datetime.timezone.utc).isoformat(),
    hashes={p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in (WORK/"candidate/src/Tedd.WildcardMatch").glob("*.cs")},variant=args.variant,
    controlVariant=args.control_variant,controlHashes={p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in (WORK/"control/src/Tedd.WildcardMatch").glob("*.cs")},
    acceptance=args.acceptance,lock=r"C:\Users\tedd\.codex\locks\performance-measurement.lock"),indent=2),encoding="utf-8")
