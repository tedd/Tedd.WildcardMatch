"""Run under the skill's performance lock. All disposable outputs stay in the task workspace."""
import argparse
import datetime as dt
import json
import os
from pathlib import Path
import shutil
import subprocess

RUN = Path(__file__).resolve().parent
REPO = RUN.parent.parent
WORK = Path(r"D:\Workspaces\AI\wildcard-hotpath")
parser = argparse.ArgumentParser()
parser.add_argument("phase", choices=["before", "after", "accept", "construction", "tests", "followup", "paired", "inlining", "validate", "finish", "measure", "diagnostics-before", "diagnostics-after"])
args = parser.parse_args()
if args.phase in ("finish", "measure", "validate"):
    started = dt.datetime.now(dt.timezone.utc).isoformat()
    phases = ("accept", "construction") if args.phase == "measure" else (("tests", "followup", "paired", "inlining", "diagnostics-after") if args.phase == "validate" else ("tests", "followup", "accept", "construction", "paired", "inlining", "diagnostics-after"))
    for phase in phases:
        subprocess.run(["python", str(RUN / "run.py"), phase], check=True)
    (RUN / "raw" / "measurement-window.json").write_text(json.dumps(dict(startedUtc=started, endedUtc=dt.datetime.now(dt.timezone.utc).isoformat(), phases=phases,
        lock=r"C:\Users\tedd\.codex\locks\performance-measurement.lock", note="Driver execution window fully contained in one skill-wrapper lock acquisition."), indent=2))
    raise SystemExit(0)
WORK.mkdir(parents=True, exist_ok=True)
os.environ["TMP"] = os.environ["TEMP"] = str(WORK / "temp")
Path(os.environ["TMP"]).mkdir(exist_ok=True)
os.environ["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1"
os.environ["DOTNET_SKIP_FIRST_TIME_EXPERIENCE"] = "1"
os.environ["DOTNET_CLI_HOME"] = str(WORK / "cli")
stage = WORK / "rig"
stage.mkdir(exist_ok=True)
for source in (RUN / "rig").iterdir():
    if source.is_file(): shutil.copy2(source, stage / source.name)
library = stage / "Library"
library.mkdir(exist_ok=True)
for source in (REPO / "src" / "Tedd.WildcardMatch").glob("*.cs"):
    shutil.copy2(source, library / source.name)
if args.phase in ("before", "diagnostics-before"):
    shutil.copy2(RUN / "baseline" / "InternalUtils.cs.txt", library / "InternalUtils.cs")
project = (REPO / "src" / "Tedd.WildcardMatch" / "Tedd.WildcardMatch.csproj").read_text(encoding="utf-8")
(library / "Tedd.WildcardMatch.csproj").write_text(project.replace("<GeneratePackageOnBuild>true", "<GeneratePackageOnBuild>false"), encoding="utf-8")
(library / "AssemblyInfo.cs").write_text('[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Investigation")]')
baseline = stage / "Baseline"
baseline.mkdir(exist_ok=True)
for source in (REPO / "src" / "Tedd.WildcardMatch").glob("*.cs"):
    shutil.copy2(source, baseline / source.name)
shutil.copy2(RUN / "baseline" / "InternalUtils.cs.txt", baseline / "InternalUtils.cs")
baseline_project = project.replace("<GeneratePackageOnBuild>true", "<GeneratePackageOnBuild>false")
baseline_project = baseline_project.replace("</PropertyGroup>", "<AssemblyName>Tedd.WildcardMatch.Baseline</AssemblyName></PropertyGroup>", 1)
(baseline / "Tedd.WildcardMatch.Baseline.csproj").write_text(baseline_project, encoding="utf-8")
# A generated experimental clone isolates the inlining hint without editing production.
translator = (library / "InternalUtils.cs").read_text(encoding="utf-8")
translator = translator.replace("class InternalUtils", "class ForcedInternalUtils")
translator = translator.replace("[MethodImpl(MethodImplOptions.AggressiveInlining)]", "")
translator = translator.replace("public static string StringToWildcard", "[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]\n    public static string StringToWildcard")
(stage / "ForcedInternalUtils.cs").write_text(translator, encoding="utf-8")
(stage / "global.json").write_text('{"sdk":{"version":"10.0.401","rollForward":"disable"}}')
(RUN / "raw").mkdir(exist_ok=True)

def execute(command, name, env=None):
    print("EXEC", command, flush=True)
    result = subprocess.run(command, cwd=stage, text=True, stdout=subprocess.PIPE, stderr=subprocess.STDOUT, env=env)
    destination = RUN / "raw" / (name + ".txt")
    if destination.exists(): archive(destination)
    destination.write_text(result.stdout, encoding="utf-8")
    print(result.stdout[-5000:], flush=True)
    result.check_returncode()

def archive(source):
    archive_dir = RUN / "raw" / "archive"
    archive_dir.mkdir(exist_ok=True)
    shutil.copy2(source, archive_dir / (source.stem + "-" + str(source.stat().st_mtime_ns) + source.suffix))

# Capture runtime/hardware/power policy and coarsely inspect nonparticipating load before execution.
execute(["powershell", "-NoProfile", "-Command", "Get-CimInstance Win32_Processor | Format-List Name,NumberOfCores,NumberOfLogicalProcessors; powercfg /getactivescheme; $sample=@{}; Get-Process | ForEach-Object { $sample[$_.Id]=$_.CPU }; Start-Sleep -Milliseconds 500; Get-Process | Where-Object { $sample.ContainsKey($_.Id) } | Select-Object Name,Id,@{n='CpuSecondsInHalfSecond';e={$_.CPU-$sample[$_.Id]}} | Sort-Object CpuSecondsInHalfSecond -Descending | Select-Object -First 12 | Format-Table -AutoSize; dotnet --info"], "environment-" + args.phase)

if args.phase != "tests":
    execute(["dotnet", "build", "-c", "Release"], "build-" + args.phase)
    dll = str(stage / "bin" / "Release" / "net10.0" / "Investigation.dll")
    execute(["dotnet", dll, "--check"], "correctness-" + args.phase)
    if args.phase == "inlining":
        execute(["dotnet", dll, "--inlining"], "inlining")
        shutil.copy2(stage / "inlining.json", RUN / "raw" / "inlining.json")
        env = dict(os.environ, DOTNET_JitDisasm="Program:ForcedCaller Program:ProductionCaller Tedd.InternalUtils:*", DOTNET_JitStdOutFile=str(WORK / "asm-inlining.txt"))
        execute(["dotnet", dll, "--asm"], "asm-run-inlining", env)
        shutil.copy2(WORK / "asm-inlining.txt", RUN / "disassembly")
    elif args.phase == "paired":
        for launch in range(1, 4):
            execute(["dotnet", dll, "--paired", str(launch)], "paired-" + str(launch))
            destination = RUN / "raw" / ("paired-" + str(launch) + ".json")
            if destination.exists(): archive(destination)
            shutil.copy2(stage / "paired.json", destination)
    elif args.phase == "followup":
        execute(["dotnet", dll, "--screen", "--final"], "screen-followup")
        if (RUN / "raw" / "screen-followup.json").exists():
            archive(RUN / "raw" / "screen-followup.json")
        shutil.copy2(stage / "screen-final.json", RUN / "raw" / "screen-followup.json")
        env = dict(os.environ, DOTNET_JitDisasm="Program:ForcedCaller Program:ProductionCaller Tedd.InternalUtils:*", DOTNET_JitStdOutFile=str(WORK / "asm-followup.txt"))
        execute(["dotnet", dll, "--asm"], "asm-run-followup", env)
        shutil.copy2(WORK / "asm-followup.txt", RUN / "disassembly")
    elif args.phase in ("before", "after", "diagnostics-before", "diagnostics-after"):
        epoch = args.phase.removeprefix("diagnostics-")
        if not args.phase.startswith("diagnostics-"):
            execute(["dotnet", dll, "--screen"] + (["--final"] if epoch == "after" else []), "screen-" + epoch)
            if epoch == "before" and (RUN / "raw" / "screen-before.json").exists():
                shutil.copy2(RUN / "raw" / "screen-before.json", RUN / "raw" / "screen-E1.json")
            shutil.copy2(stage / ("screen-final.json" if epoch == "after" else "screen-before.json"), RUN / "raw")
        trace = str(WORK / (epoch + ".nettrace"))
        execute([r"C:\Users\tedd\.dotnet\tools\dotnet-trace.exe", "collect", "--providers", "Microsoft-DotNETCore-SampleProfiler", "--output", trace, "--", "dotnet", dll, "--profile"], "profile-" + epoch)
        execute([r"C:\Users\tedd\.dotnet\tools\dotnet-trace.exe", "report", trace, "topN", "--number", "30"], "profile-top-" + epoch)
        short_trace = str(WORK / (epoch + "-short.nettrace"))
        execute([r"C:\Users\tedd\.dotnet\tools\dotnet-trace.exe", "collect", "--providers", "Microsoft-DotNETCore-SampleProfiler", "--output", short_trace, "--", "dotnet", dll, "--profile-short"], "profile-short-" + epoch)
        execute([r"C:\Users\tedd\.dotnet\tools\dotnet-trace.exe", "report", short_trace, "topN", "--number", "30"], "profile-short-top-" + epoch)
        env = dict(os.environ, DOTNET_JitDisasm="Variants:* Tedd.InternalUtils:* Program:ProductionCaller", DOTNET_JitStdOutFile=str(WORK / ("asm-" + epoch + ".txt")))
        execute(["dotnet", dll, "--asm"], "asm-run-" + epoch, env)
        (RUN / "disassembly").mkdir(exist_ok=True)
        shutil.copy2(WORK / ("asm-" + epoch + ".txt"), RUN / "disassembly")
    else:
        artifact = WORK / ("construction-bdn" if args.phase == "construction" else "bdn-actual")
        benchmark_filter = "*ConstructionAcceptance*" if args.phase == "construction" else "Acceptance.*"
        execute(["dotnet", dll, "--filter", benchmark_filter, "--artifacts", str(artifact), "--exporters", "json"], "construction" if args.phase == "construction" else "acceptance")
        result_file = artifact / "results" / (("ConstructionAcceptance" if args.phase == "construction" else "Acceptance") + "-report-full-compressed.json")
        benchmarks = json.loads(result_file.read_text(encoding="utf-8"))["Benchmarks"]
        expected = 4 if args.phase == "construction" else 30
        if len(benchmarks) != expected or any(not b.get("Statistics") or not b["Statistics"].get("N") for b in benchmarks):
            raise RuntimeError("BenchmarkDotNet did not execute every expected case; exit code alone does not verify success")
        for source in (artifact / "results").iterdir():
            if source.is_file():
                destination = RUN / "raw" / source.name
                if destination.exists(): archive(destination)
                shutil.copy2(source, destination)
elif args.phase == "tests":
    checkout = WORK / "validation"
    shutil.copytree(REPO / "src", checkout, dirs_exist_ok=True, ignore=shutil.ignore_patterns("bin", "obj"))
    execute(["dotnet", "test", str(checkout / "Tedd.WildcardMatch.Tests" / "Tedd.WildcardMatch.Tests.csproj"), "-c", "Release", "-p:GeneratePackageOnBuild=false"], "tests")
