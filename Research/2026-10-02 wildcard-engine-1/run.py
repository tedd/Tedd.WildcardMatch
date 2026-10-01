"""Run under run_with_performance_lock.py; disposable files remain in the task workspace."""
import argparse
import datetime
import json
import os
import re
from pathlib import Path
import shutil
import subprocess

RUN = Path(__file__).resolve().parent
REPO = RUN.parent.parent
WORK = Path(r"D:\Workspaces\AI\wildcard-engine")
parser = argparse.ArgumentParser()
parser.add_argument("phase", choices=["tests", "runtime-matrix", "benchmark", "compiled", "comparison", "diagnostics", "all"])
args = parser.parse_args()
WORK.mkdir(parents=True, exist_ok=True)
(WORK / "temp").mkdir(exist_ok=True)
os.environ.update(TMP=str(WORK / "temp"), TEMP=str(WORK / "temp"), DOTNET_CLI_HOME=str(WORK / "cli"),
                  DOTNET_SKIP_FIRST_TIME_EXPERIENCE="1", DOTNET_CLI_TELEMETRY_OPTOUT="1")
(RUN / "raw").mkdir(exist_ok=True)
(WORK / "global.json").write_text('{"sdk":{"version":"11.0.100-rc.1.26425.128","rollForward":"disable","allowPrerelease":true}}')
started = datetime.datetime.now(datetime.timezone.utc).isoformat()
for name in ("Tedd.WildcardMatch", "Tedd.WildcardMatch.Tests"):
    shutil.copytree(REPO / "src" / name, WORK / name, dirs_exist_ok=True, ignore=shutil.ignore_patterns("bin", "obj"))

def execute(command, name, env=None):
    print("EXEC", command, flush=True)
    result = subprocess.run(command, cwd=WORK, env=env, stdout=subprocess.PIPE, stderr=subprocess.STDOUT, text=True, encoding="utf-8", errors="replace")
    destination = RUN / "raw" / (name + ".txt")
    if destination.exists():
        archive = RUN / "raw" / "archive"
        archive.mkdir(exist_ok=True)
        shutil.copy2(destination, archive / (name + "-" + str(destination.stat().st_mtime_ns) + ".txt"))
    destination.write_text(result.stdout, encoding="utf-8")
    print(result.stdout if result.returncode else "\n".join(result.stdout.splitlines()[-12:]), flush=True)
    result.check_returncode()

execute(["dotnet", "--info"], "environment")
if args.phase in ("comparison", "all"):
    comparison = WORK / "Tedd.WildcardMatch.Benchmark"
    shutil.copytree(REPO / "src" / "Tedd.WildcardMatch.Benchmark", comparison, dirs_exist_ok=True, ignore=shutil.ignore_patterns("bin", "obj"))
    execute(["dotnet", "build", str(comparison / "Tedd.WildcardMatch.Benchmark.csproj"), "-c", "Release", "-p:GeneratePackageOnBuild=false"], "build-comparison")
    execute(["dotnet", str(comparison / "bin" / "Release" / "net10.0" / "Tedd.WildcardMatch.Benchmark.dll"),
             "--validate-packages", str(WORK / "comparison-validation.json")], "comparison-validation")
if args.phase in ("tests", "all"):
    for configuration in ("Release", "Debug"):
        execute(["dotnet", "test", str(WORK / "Tedd.WildcardMatch.Tests" / "Tedd.WildcardMatch.Tests.csproj"),
                 "-c", configuration, "-p:GeneratePackageOnBuild=false"], "tests-" + configuration)
if args.phase in ("runtime-matrix", "all"):
    test_project = WORK / "Tedd.WildcardMatch.Tests" / "Tedd.WildcardMatch.Tests.csproj"
    project_text = test_project.read_text(encoding="utf-8")
    try:
        for framework in ("net8.0", "net9.0"):
            test_project.write_text(re.sub(r"(<TargetFrameworks?>).*?(</TargetFrameworks?>)", lambda m: m[1] + framework + m[2], project_text), encoding="utf-8")
            execute(["dotnet", "test", str(test_project), "-c", "Release", "-p:GeneratePackageOnBuild=false"], "tests-" + framework)
    finally:
        test_project.write_text(project_text, encoding="utf-8")
if args.phase in ("benchmark", "compiled", "all"):
    stage = WORK / "benchmark"
    shutil.copytree(RUN / "rig", stage, dirs_exist_ok=True)
    execute(["dotnet", "build", str(stage / "EngineBench.csproj"), "-c", "Release", "-p:GeneratePackageOnBuild=false"], "build-benchmark")
    for launch in range(1, 4):
        prefix = "compiled" if args.phase == "compiled" else "benchmark"
        execute(["dotnet", str(stage / "bin" / "Release" / "net10.0" / "EngineBench.dll"), str(launch)] +
                (["--compiled-only"] if args.phase == "compiled" else []), prefix + "-" + str(launch))
        destination = RUN / "raw" / (prefix + "-" + str(launch) + ".json")
        if destination.exists():
            archive = RUN / "raw" / "archive"
            archive.mkdir(exist_ok=True)
            shutil.copy2(destination, archive / (destination.stem + "-" + str(destination.stat().st_mtime_ns) + ".json"))
        shutil.copy2(WORK / "results.json", destination)
if args.phase in ("diagnostics", "all"):
    stage = WORK / "benchmark"
    shutil.copytree(RUN / "rig", stage, dirs_exist_ok=True)
    execute(["dotnet", "build", str(stage / "EngineBench.csproj"), "-c", "Release", "-p:GeneratePackageOnBuild=false"], "build-diagnostics")
    native = WORK / "engine.asm.txt"
    env = dict(os.environ, DOTNET_JitDisasm="Tedd.WildcardEngine:* Tedd.WildcardMatch:*", DOTNET_JitStdOutFile=str(native))
    execute(["dotnet", str(stage / "bin" / "Release" / "net10.0" / "EngineBench.dll"), "0", "--asm-only"], "native-code", env)
    (RUN / "disassembly").mkdir(exist_ok=True)
    shutil.copy2(native, RUN / "disassembly" / native.name)
(RUN / "raw" / "execution-window.json").write_text(json.dumps(dict(startedUtc=started,
    endedUtc=datetime.datetime.now(datetime.timezone.utc).isoformat(), phase=args.phase,
    lock=r"C:\Users\tedd\.codex\locks\performance-measurement.lock"), indent=2), encoding="utf-8")
