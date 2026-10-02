"""Inspected local-only validation driver. Execute under the shared performance lock."""
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
HERE = Path(__file__).resolve().parent
REPO = HERE.parents[2]
WORK = Path(r"D:\Workspaces\AI\wildcard-spans")
REVISION = "0c39b9d"
parser = argparse.ArgumentParser()
parser.add_argument("--phase", choices=["validate", "measure", "all"], default="all")
parser.add_argument("--epoch", default=".")
parser.add_argument("--control-epoch")
parser.add_argument("--screening", action="store_true")
args = parser.parse_args()
EVIDENCE = HERE / args.epoch
EVIDENCE.mkdir(exist_ok=True)
WORK.mkdir(parents=True, exist_ok=True)
(WORK / "temp").mkdir(exist_ok=True)
os.environ.update(TMP=str(WORK / "temp"), TEMP=str(WORK / "temp"), DOTNET_CLI_HOME=str(WORK / "cli"),
    DOTNET_SKIP_FIRST_TIME_EXPERIENCE="1", DOTNET_CLI_TELEMETRY_OPTOUT="1")
shutil.copy2(REPO / "global.json", WORK / "global.json")
started = datetime.datetime.now(datetime.timezone.utc).isoformat()

def execute(command, destination, env=None):
    print("EXEC", command, flush=True)
    result = subprocess.run(command, cwd=WORK, env=env, stdout=subprocess.PIPE, stderr=subprocess.STDOUT, text=True, encoding="utf-8", errors="replace")
    if destination.exists():
        archive = EVIDENCE / "archive"
        archive.mkdir(exist_ok=True)
        shutil.copy2(destination, archive / (destination.stem + "-" + str(destination.stat().st_mtime_ns) + destination.suffix))
    destination.write_text(result.stdout, encoding="utf-8")
    print(result.stdout if result.returncode else "\n".join(result.stdout.splitlines()[-8:]), flush=True)
    result.check_returncode()

for name in ("control", "candidate"):
    project = WORK / name
    project.mkdir(exist_ok=True)
    if name == "control":
        files = subprocess.check_output(["git", "ls-tree", "-r", "--name-only", REVISION, "src/Tedd.WildcardMatch"], cwd=REPO).decode().splitlines()
        for path in files:
            if not path.endswith((".cs", ".csproj")): continue
            text = subprocess.check_output(["git", "show", REVISION + ":" + path], cwd=REPO).decode("utf-8-sig")
            if args.control_epoch and path.endswith(".cs"):
                text = (HERE / args.control_epoch / (Path(path).name + ".txt")).read_text(encoding="utf-8")
            if path.endswith(".csproj"):
                text = text.replace("<PackageId>Tedd.WildcardMatch</PackageId>", "<AssemblyName>Tedd.WildcardMatch.Control</AssemblyName><PackageId>Tedd.WildcardMatch.Control</PackageId>")
            (project / Path(path).name).write_text(text, encoding="utf-8")
    else:
        shutil.copytree(REPO / "src/Tedd.WildcardMatch", project, dirs_exist_ok=True, ignore=shutil.ignore_patterns("bin", "obj"))
        for file in project.glob("*.cs"):
            shutil.copy2(file, EVIDENCE / (file.name + ".txt"))

execute(["dotnet", "--info"], EVIDENCE / "environment.txt")
execute(["powershell", "-NoProfile", "-Command", "Get-CimInstance Win32_Processor | Select-Object Name,NumberOfLogicalProcessors; Get-Process dotnet,testhost,VBCSCompiler -ErrorAction SilentlyContinue | Select-Object ProcessName,CPU; powercfg /getactivescheme; exit 0"], HERE / "host.txt")

if args.phase in ("validate", "all"):
    tests = WORK / "tests"
    shutil.copytree(REPO / "src/Tedd.WildcardMatch.Tests", tests, dirs_exist_ok=True, ignore=shutil.ignore_patterns("bin", "obj"))
    test_project = tests / "Tedd.WildcardMatch.Tests.csproj"
    test_project.write_text(test_project.read_text(encoding="utf-8").replace("..\\Tedd.WildcardMatch\\", "..\\candidate\\"), encoding="utf-8")
    execute(["dotnet", "test", str(test_project), "-c", "Release", "-p:GeneratePackageOnBuild=false"], EVIDENCE / "Release.txt")
    execute(["dotnet", "test", str(test_project), "-c", "Debug", "--filter", "FullyQualifiedName~SpanTest", "-p:GeneratePackageOnBuild=false"], EVIDENCE / "Debug.txt")

if args.phase in ("measure", "all"):
    rig = WORK / "rig"
    rig.mkdir(exist_ok=True)
    shutil.copy2(HERE / "SpanBench.csproj", rig / "SpanBench.csproj")
    shutil.copy2(HERE / "Program.cs", rig / "Program.cs")
    execute(["dotnet", "build", str(rig / "SpanBench.csproj"), "-c", "Release", "-p:GeneratePackageOnBuild=false", "-p:ControlHasSpans=" + str(bool(args.control_epoch)).lower()], EVIDENCE / "build.txt")
    for framework in (("net10.0",) if args.screening else ("net10.0", "net11.0")):
        dll = rig / f"bin/Release/{framework}/SpanBench.dll"
        for launch in range(1, 4):
            execute(["dotnet", str(dll), str(launch)] + (["--screening"] if args.screening else []), EVIDENCE / f"{framework}-launch-{launch}.txt")
            shutil.copy2(WORK / "span-results.json", EVIDENCE / f"{framework}-launch-{launch}.json")
        asm = WORK / (framework + ".asm.txt")
        # CoreCLR appends to this output. Start a new file per isolated epoch.
        asm.write_text("", encoding="utf-8")
        env = dict(os.environ, DOTNET_JitDisasm="Tedd.WildcardEngine:* Tedd.WildcardMatch:*", DOTNET_JitStdOutFile=str(asm))
        execute(["dotnet", str(dll), "0", "--asm-only"], EVIDENCE / (framework + "-native-code.txt"), env)
        shutil.copy2(asm, EVIDENCE / (framework + "-native-code.asm.txt"))
    trace = WORK / "span-profile.nettrace"
    execute([r"C:\Users\tedd\.dotnet\tools\dotnet-trace.exe", "collect", "--providers", "Microsoft-DOTNETCore-SampleProfiler", "--output", str(trace), "--format", "Speedscope", "--", "dotnet", str(rig / "bin/Release/net10.0/SpanBench.dll"), "0", "--profile"], EVIDENCE / "trace.txt")
    data = json.loads((WORK / "span-profile.speedscope.json").read_text(encoding="utf-8-sig"))
    from collections import Counter
    inclusive = Counter()
    frames = data["shared"]["frames"]
    for profile in data["profiles"]:
        if profile["type"] != "evented": continue
        stack, previous = [], profile["startValue"]
        for event in profile["events"]:
            weight = event["at"] - previous
            for frame in set(stack): inclusive[frames[frame]["name"]] += weight
            if event["type"] == "O": stack.append(event["frame"])
            else: stack.pop()
            previous = event["at"]
    (EVIDENCE / "profile-summary.json").write_text(json.dumps({"note": "Inclusive sampled stack residence for equal-duration slice calls; not exact exclusive CPU or throughput.", "inclusive": inclusive.most_common(35)}, indent=2), encoding="utf-8")

(EVIDENCE / "identity.json").write_text(json.dumps(dict(baseline=REVISION if not args.control_epoch else args.control_epoch, startedUtc=started,
    endedUtc=datetime.datetime.now(datetime.timezone.utc).isoformat(),
    hashes={p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in (REPO / "src/Tedd.WildcardMatch").glob("*.cs")},
    lock=r"C:\Users\tedd\.codex\locks\performance-measurement.lock", phase=args.phase, screening=args.screening), indent=2), encoding="utf-8")
