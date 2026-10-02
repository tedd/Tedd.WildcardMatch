"""Inspected local-only experiment driver; invoke under the shared performance lock."""
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
from variants import variant

sys.stdout.reconfigure(encoding="utf-8")
HERE = Path(__file__).resolve().parent
RUN = HERE.parent
REPO = RUN.parent.parent
WORK = Path(r"D:\Workspaces\AI\wildcard-engine-optimization")
REVISION = "4bc779e"
parser = argparse.ArgumentParser()
parser.add_argument("names", nargs="+", help="control, H-004..H-013, integrated, profile, or final")
parser.add_argument("--acceptance", action="store_true")
args = parser.parse_args()
WORK.mkdir(parents=True, exist_ok=True)
(WORK / "temp").mkdir(exist_ok=True)
os.environ.update(TMP=str(WORK / "temp"), TEMP=str(WORK / "temp"), DOTNET_CLI_HOME=str(WORK / "cli"),
    DOTNET_SKIP_FIRST_TIME_EXPERIENCE="1", DOTNET_CLI_TELEMETRY_OPTOUT="1")
(WORK / "global.json").write_text('{"sdk":{"version":"11.0.100-rc.1.26425.128","rollForward":"disable","allowPrerelease":true}}')
started = datetime.datetime.now(datetime.timezone.utc).isoformat()

def git_text(path):
    return subprocess.check_output(["git", "show", REVISION + ":" + path], cwd=REPO).decode("utf-8-sig").replace("\r\n", "\n")

def execute(command, directory, destination, env=None):
    print("EXEC", command, flush=True)
    result = subprocess.run(command, cwd=directory, env=env, stdout=subprocess.PIPE, stderr=subprocess.STDOUT, text=True, encoding="utf-8", errors="replace")
    destination.parent.mkdir(parents=True, exist_ok=True)
    if destination.exists():
        archive = destination.parent / "archive"
        archive.mkdir(exist_ok=True)
        shutil.copy2(destination, archive / (destination.stem + "-" + str(destination.stat().st_mtime_ns) + destination.suffix))
    destination.write_text(result.stdout, encoding="utf-8")
    print(result.stdout if result.returncode else "\n".join(result.stdout.splitlines()[-8:]), flush=True)
    result.check_returncode()

def stage(name):
    project = WORK / name
    project.mkdir(exist_ok=True)
    for filename in ("LICENSE", "README.md"):
        shutil.copy2(REPO / filename, WORK / filename)
    files = subprocess.check_output(["git", "ls-tree", "-r", "--name-only", REVISION, "src/Tedd.WildcardMatch"], cwd=REPO).decode().splitlines()
    for path in files:
        if path.endswith((".cs", ".csproj")):
            text = git_text(path)
            if name == "control":
                if path.endswith(".csproj"):
                    text = text.replace("<PackageId>Tedd.WildcardMatch</PackageId>", "<AssemblyName>Tedd.WildcardMatch.Control</AssemblyName>\n<PackageId>Tedd.WildcardMatch.Control</PackageId>", 1)
            (project / Path(path).name).write_text(text, encoding="utf-8")
    return project

stage("control")
baseline_engine = git_text("src/Tedd.WildcardMatch/WildcardEngine.cs")
baseline_wrapper = git_text("src/Tedd.WildcardMatch/WildcardMatch.cs")

for name in args.names:
    evidence = HERE / name
    evidence.mkdir(exist_ok=True)
    control_project = stage("control")
    if name in ("H-020", "H-022"):
        parent = "H-014" if name == "H-020" else "H-017"
        control_engine, control_wrapper = variant(baseline_engine, baseline_wrapper, parent)
        (control_project / "WildcardEngine.cs").write_text(control_engine, encoding="utf-8")
        (control_project / "WildcardMatch.cs").write_text(control_wrapper, encoding="utf-8")
    project = stage("candidate")
    if name in ("integrated", "final", "reprofile"):
        shutil.copytree(REPO / "src/Tedd.WildcardMatch", project, dirs_exist_ok=True, ignore=shutil.ignore_patterns("bin", "obj"))
        engine = (project / "WildcardEngine.cs").read_text(encoding="utf-8")
        wrapper = (project / "WildcardMatch.cs").read_text(encoding="utf-8")
    else:
        engine, wrapper = variant(baseline_engine, baseline_wrapper, name if name != "profile" else "control")
        (project / "WildcardEngine.cs").write_text(engine, encoding="utf-8")
        (project / "WildcardMatch.cs").write_text(wrapper, encoding="utf-8")
    (evidence / "candidate-engine.cs.txt").write_text(engine, encoding="utf-8")
    (evidence / "candidate-wrapper.cs.txt").write_text(wrapper, encoding="utf-8")
    execute(["dotnet", "--info"], WORK, evidence / "environment.txt")
    tests = WORK / "tests"
    shutil.copytree(REPO / "src/Tedd.WildcardMatch.Tests", tests, dirs_exist_ok=True, ignore=shutil.ignore_patterns("bin", "obj"))
    test_project = tests / "Tedd.WildcardMatch.Tests.csproj"
    test_project.write_text(test_project.read_text(encoding="utf-8").replace("..\\Tedd.WildcardMatch\\", "..\\candidate\\"), encoding="utf-8")
    if name == "final":
        for configuration in ("Release", "Debug"):
            execute(["dotnet", "test", str(test_project), "-c", configuration, "-p:GeneratePackageOnBuild=false"], WORK, evidence / (configuration + ".txt"))
        continue
    if name not in ("profile", "reprofile"):
        execute(["dotnet", "test", str(test_project), "-c", "Release", "-f", "net10.0", "-p:GeneratePackageOnBuild=false"], WORK, evidence / "correctness.txt")
    rig = WORK / "rig"
    rig.mkdir(exist_ok=True)
    shutil.copy2(HERE / "Program.cs", rig / "Program.cs")
    shutil.copy2(HERE / "OptimizationBench.csproj", rig / "OptimizationBench.csproj")
    execute(["dotnet", "build", str(rig / "OptimizationBench.csproj"), "-c", "Release", "-p:GeneratePackageOnBuild=false"], WORK, evidence / "build.txt")
    dll = rig / "bin/Release/net10.0/OptimizationBench.dll"
    env = dict(os.environ, DOTNET_JitDisasm="Tedd.WildcardEngine:* Tedd.WildcardMatch:*", DOTNET_JitStdOutFile=str(WORK / (name + ".asm.txt")))
    execute(["dotnet", str(dll), "0", "--asm-only"], WORK, evidence / "native-code.txt", env)
    shutil.copy2(WORK / (name + ".asm.txt"), evidence / "native-code.asm.txt")
    if name in ("profile", "reprofile"):
        trace = WORK / "profile.nettrace"
        execute([r"C:\Users\tedd\.dotnet\tools\dotnet-trace.exe", "collect", "--providers", "Microsoft-DotNETCore-SampleProfiler", "--output", str(trace), "--format", "Speedscope", "--", "dotnet", str(dll), "0", "--profile"], WORK, evidence / "trace.txt")
        speedscope = WORK / "profile.speedscope.json"
        data = json.loads(speedscope.read_text(encoding="utf-8-sig"))
        frames = data["shared"]["frames"]
        from collections import Counter
        inclusive, exclusive = Counter(), Counter()
        for p in data["profiles"]:
            if p["type"] == "sampled":
                for stack, weight in zip(p["samples"], p.get("weights", [1] * len(p["samples"]))):
                    for frame in set(stack): inclusive[frames[frame]["name"]] += weight
                    if stack: exclusive[frames[stack[-1]]["name"]] += weight
            elif p["type"] == "evented":
                stack, previous = [], p["startValue"]
                for event in p["events"]:
                    weight = event["at"] - previous
                    for frame in set(stack): inclusive[frames[frame]["name"]] += weight
                    if stack: exclusive[frames[stack[-1]]["name"]] += weight
                    if event["type"] == "O": stack.append(event["frame"])
                    else: stack.pop()
                    previous = event["at"]
        summary = {"note": "SampleProfiler stack residence weights; evented Speedscope includes time between samples and is not exact retired CPU instruction accounting.", "inclusive": inclusive.most_common(30), "exclusive": exclusive.most_common(30)}
        (evidence / "profile-summary.json").write_text(json.dumps(summary, indent=2), encoding="utf-8")
    else:
        for launch in range(1, 4):
            execute(["dotnet", str(dll), str(launch)] + (["--acceptance"] if args.acceptance else []), WORK, evidence / (f"launch-{launch}.txt"))
            shutil.copy2(WORK / "optimization-results.json", evidence / f"launch-{launch}.json")
    (evidence / "identity.json").write_text(json.dumps(dict(baseline=REVISION + (" + H-014" if name == "H-020" else " + H-017" if name == "H-022" else ""), candidate=name,
        engineSha256=hashlib.sha256(engine.encode()).hexdigest(), wrapperSha256=hashlib.sha256(wrapper.encode()).hexdigest(),
        startedUtc=started, endedUtc=datetime.datetime.now(datetime.timezone.utc).isoformat(),
        lock=r"C:\Users\tedd\.codex\locks\performance-measurement.lock"), indent=2), encoding="utf-8")
