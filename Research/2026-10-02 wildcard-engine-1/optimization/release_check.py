"""Local compatibility/corpus/package validation only; never publishes or writes a database."""
import json
import argparse
import os
from pathlib import Path
import re
import shutil
import subprocess
import sys
import zipfile
sys.stdout.reconfigure(encoding="utf-8")
parser = argparse.ArgumentParser()
parser.add_argument("--pack-only", action="store_true")
args = parser.parse_args()
HERE = Path(__file__).resolve().parent
REPO = HERE.parent.parent.parent
WORK = Path(r"D:\Workspaces\AI\wildcard-engine-optimization")
EVIDENCE = HERE / "release"
EVIDENCE.mkdir(exist_ok=True)
os.environ.update(TMP=str(WORK / "temp"), TEMP=str(WORK / "temp"), DOTNET_CLI_HOME=str(WORK / "cli"),
    DOTNET_SKIP_FIRST_TIME_EXPERIENCE="1", DOTNET_CLI_TELEMETRY_OPTOUT="1")

def execute(command, name):
    print("EXEC", command, flush=True)
    p = subprocess.run(command, cwd=WORK, stdout=subprocess.PIPE, stderr=subprocess.STDOUT, text=True, encoding="utf-8", errors="replace")
    destination = EVIDENCE / (name + ".txt")
    if destination.exists():
        archive = EVIDENCE / "archive"
        archive.mkdir(exist_ok=True)
        shutil.copy2(destination, archive / (name + "-" + str(destination.stat().st_mtime_ns) + ".txt"))
    destination.write_text(p.stdout, encoding="utf-8")
    print(p.stdout if p.returncode else "\n".join(p.stdout.splitlines()[-8:]), flush=True)
    p.check_returncode()

if not args.pack_only:
    test_project = WORK / "tests/Tedd.WildcardMatch.Tests.csproj"
    text = test_project.read_text(encoding="utf-8")
    try:
        test_project.write_text(re.sub(r"(<TargetFrameworks?>).*?(</TargetFrameworks?>)", lambda m: m[1] + "net9.0" + m[2], text), encoding="utf-8")
        execute(["dotnet", "test", str(test_project), "-c", "Release", "-p:GeneratePackageOnBuild=false"], "net9")
    finally:
        test_project.write_text(text, encoding="utf-8")
    comparison = WORK / "Tedd.WildcardMatch.Benchmark"
    shutil.copytree(REPO / "src/Tedd.WildcardMatch.Benchmark", comparison, dirs_exist_ok=True, ignore=shutil.ignore_patterns("bin", "obj"))
    project = comparison / "Tedd.WildcardMatch.Benchmark.csproj"
    project.write_text(project.read_text(encoding="utf-8").replace("..\\Tedd.WildcardMatch\\", "..\\candidate\\").replace("../Tedd.WildcardMatch/", "../candidate/"), encoding="utf-8")
    execute(["dotnet", "build", str(project), "-c", "Release", "-p:GeneratePackageOnBuild=false"], "comparison-build")
    execute(["dotnet", str(comparison / "bin/Release/net10.0/Tedd.WildcardMatch.Benchmark.dll"), "--validate-packages", str(WORK / "comparison-validation.json")], "comparison-validation")
library_project = WORK / "candidate/Tedd.WildcardMatch.csproj"
# The flat disposable layout has one parent level; repository packaging keeps two.
library_project.write_text(library_project.read_text(encoding="utf-8").replace("..\\..\\LICENSE", "..\\LICENSE").replace("..\\..\\README.md", "..\\README.md"), encoding="utf-8")
execute(["dotnet", "pack", str(WORK / "candidate/Tedd.WildcardMatch.csproj"), "-c", "Release", "--no-build", "--no-restore", "--output", str(WORK / "packages")], "pack")
package = WORK / "packages/Tedd.WildcardMatch.2.0.0.nupkg"
with zipfile.ZipFile(package) as archive:
    names = archive.namelist()
    for target in ("netstandard2.1", "net10.0", "net11.0"):
        assert f"lib/{target}/Tedd.WildcardMatch.dll" in names
    assert "README.md" in names and "LICENSE" in names
    nuspec = archive.read("Tedd.WildcardMatch.nuspec").decode()
    assert "<version>2.0.0</version>" in nuspec
assert (WORK / "packages/Tedd.WildcardMatch.2.0.0.snupkg").exists()
(EVIDENCE / "package-contents.json").write_text(json.dumps(dict(version="2.0.0", packageEntries=names, symbolsPresent=True), indent=2), encoding="utf-8")
print("Verified 2.0.0 package: Standard 2.1, .NET 10, .NET 11, README, license and symbols.")
