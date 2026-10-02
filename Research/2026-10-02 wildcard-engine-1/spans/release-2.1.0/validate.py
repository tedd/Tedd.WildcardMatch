"""Local-only release validation. Run under run_with_performance_lock.py.

Stages tracked source into the designated scratch directory; no database,
network publication, deployment, application startup or benchmark timing occurs.
Restore may read package feeds. All build products/caches remain in WORK.
"""
import argparse
import datetime
import hashlib
import json
import os
from pathlib import Path
import shutil
import subprocess
import zipfile

HERE = Path(__file__).resolve().parent
REPO = HERE.parents[3]
WORK = Path(r"D:\Workspaces\AI\wildcard-span-release")
parser = argparse.ArgumentParser()
parser.add_argument("--resume-pack", action="store_true", help="Reuse successful suites only if staged source still matches")
args = parser.parse_args()
if args.resume_pack:
    for file in (REPO / "src").rglob("*"):
        if file.suffix in (".cs", ".csproj") and not {"bin", "obj"}.intersection(file.parts):
            assert file.read_bytes() == (WORK / "source" / file.relative_to(REPO)).read_bytes(), file
    assert (REPO / "global.json").read_bytes() == (WORK / "source/global.json").read_bytes()
WORK.mkdir(parents=True, exist_ok=True)
for folder in ("temp", "cli", "packages", "http-cache", "logs"):
    (WORK / folder).mkdir(exist_ok=True)
os.environ.update(TMP=str(WORK / "temp"), TEMP=str(WORK / "temp"),
    DOTNET_CLI_HOME=str(WORK / "cli"), NUGET_PACKAGES=str(WORK / "packages"),
    NUGET_HTTP_CACHE_PATH=str(WORK / "http-cache"),
    DOTNET_SKIP_FIRST_TIME_EXPERIENCE="1", DOTNET_CLI_TELEMETRY_OPTOUT="1")

stage = WORK / "source"
stage.mkdir(exist_ok=True)
shutil.copy2(REPO / "global.json", WORK / "global.json")
for name in ("global.json", "README.md", "LICENSE"):
    shutil.copy2(REPO / name, stage / name)
shutil.copytree(REPO / "src", stage / "src", dirs_exist_ok=True,
    ignore=shutil.ignore_patterns("bin", "obj", ".vs", "TestResults"))

started = datetime.datetime.now(datetime.timezone.utc).isoformat()
commands = []

def run(arguments, name, cwd=stage):
    command = ["dotnet"] + arguments
    print("EXEC", command, flush=True)
    result = subprocess.run(command, cwd=cwd, stdout=subprocess.PIPE,
        stderr=subprocess.STDOUT, text=True, encoding="utf-8", errors="replace")
    (HERE / (name + ".txt")).write_text(result.stdout, encoding="utf-8")
    commands.append(dict(command=command, exitCode=result.returncode))
    print("\n".join(result.stdout.splitlines()[-10:]), flush=True)
    result.check_returncode()

if not args.resume_pack:
    run(["--info"], "environment")
    for configuration in ("Release", "Debug"):
        run(["build", "src/Tedd.WildcardMatch.sln", "-c", configuration], "build-" + configuration)
        for framework in ("net8.0", "net10.0", "net11.0"):
            run(["test", "src/Tedd.WildcardMatch.Tests", "--no-build", "--no-restore",
                "-c", configuration, "-f", framework,
                "--logger", "trx;LogFileName=" + configuration + "-" + framework + ".trx",
                "--results-directory", str(WORK / "logs")], configuration + "-" + framework)
            shutil.copy2(WORK / "logs" / (configuration + "-" + framework + ".trx"), HERE)
    run(["run", "--project", "src/Tedd.WildcardMatch.Benchmark", "--no-build", "-c", "Release",
        "--", "--validate-packages", str(HERE / "corpus.json")], "corpus")
else:
    import xml.etree.ElementTree as ET
    for configuration in ("Release", "Debug"):
        for framework in ("net8.0", "net10.0", "net11.0"):
            counts = ET.parse(HERE / (configuration + "-" + framework + ".trx")).find(
                "{http://microsoft.com/schemas/VisualStudio/TeamTest/2010}ResultSummary/{http://microsoft.com/schemas/VisualStudio/TeamTest/2010}Counters").attrib
            assert counts["total"] == counts["passed"] and int(counts["failed"]) == 0
    assert all(row["Failures"] == 0 for row in json.loads((HERE / "corpus.json").read_text(encoding="utf-8")))
package_dir = WORK / "nuget"
package_dir.mkdir(exist_ok=True)
run(["pack", "src/Tedd.WildcardMatch/Tedd.WildcardMatch.csproj", "--no-build", "--no-restore",
    "-c", "Release", "-o", str(package_dir)], "pack")
package = package_dir / "Tedd.WildcardMatch.2.1.0.nupkg"
symbols = package_dir / "Tedd.WildcardMatch.2.1.0.snupkg"
assert package.exists() and symbols.exists()
with zipfile.ZipFile(package) as archive:
    entries = archive.namelist()
    for target in ("netstandard2.1", "net10.0", "net11.0"):
        assert "lib/" + target + "/Tedd.WildcardMatch.dll" in entries
    assert "README.md" in entries and "LICENSE" in entries
    (HERE / "package.nuspec.txt").write_bytes(archive.read("Tedd.WildcardMatch.nuspec"))

# Compile against the packed assets rather than a ProjectReference.
consumer = WORK / "consumer"
consumer.mkdir(exist_ok=True)
(consumer / "Consumer.csproj").write_text('''<Project Sdk="Microsoft.NET.Sdk">
<PropertyGroup><OutputType>Exe</OutputType><TargetFrameworks>net8.0;net10.0;net11.0</TargetFrameworks></PropertyGroup>
<ItemGroup><PackageReference Include="Tedd.WildcardMatch" Version="2.1.0" /></ItemGroup>
</Project>''', encoding="utf-8")
(consumer / "Program.cs").write_text('''using System;
using Tedd;
Span<char> input = stackalloc char[] { '#', 'A', 'b', 'c', '#' };
Span<char> pattern = stackalloc char[] { '#', 'a', '?', '*', '#' };
ReadOnlySpan<char> text = input.Slice(1, 3);
ReadOnlySpan<char> wildcard = pattern.Slice(1, 3);
var options = WildcardOptions.IgnoreCase | WildcardOptions.CultureInvariant;
var matcher = new WildcardMatch(wildcard, options, TimeSpan.FromSeconds(1));
if (!WildcardMatch.IsMatch(text, wildcard, true) || !WildcardMatch.IsMatch(text, wildcard, options)
    || !text.IsWildcardMatch(wildcard, options) || !text.IsWildcardMatch(wildcard, true)
    || !input.Slice(1, 3).IsWildcardMatch(wildcard, options)
    || !input.Slice(1, 3).IsWildcardMatch(wildcard, true) || !matcher.IsMatch(text)
    || !new WildcardMatch("A?*".AsSpan()).IsMatch(text)
    || !new WildcardMatch(wildcard, options).IsMatch(text)
    || !new WildcardMatchRegex("a?*", options).IsMatch(text.ToString())) throw new Exception("Packed API failure");
pattern[1] = 'z';
if (!matcher.IsMatch(text) || matcher.Wildcard != "a?*") throw new Exception("Pattern ownership failure");
input[1] = 'z';
if (matcher.IsMatch(text)) throw new Exception("Borrowed input failure");
Console.WriteLine("Packed span and string APIs passed on " + Environment.Version);
''', encoding="utf-8")
run(["restore", "--source", str(package_dir)], "consumer-restore", consumer)
for framework in ("net8.0", "net10.0", "net11.0"):
    run(["run", "--no-restore", "-c", "Release", "-f", framework], "consumer-" + framework, consumer)

(HERE / "identity.json").write_text(json.dumps(dict(startedUtc=started,
    finishedUtc=datetime.datetime.now(datetime.timezone.utc).isoformat(), commands=commands,
    version="2.1.0", work=str(WORK), resumedPack=args.resume_pack,
    hashes={str(p.relative_to(REPO)): hashlib.sha256(p.read_bytes()).hexdigest()
        for folder in ("Tedd.WildcardMatch", "Tedd.WildcardMatch.Tests")
        for p in (REPO / "src" / folder).glob("*.cs")},
    packages={p.name: hashlib.sha256(p.read_bytes()).hexdigest() for p in (package, symbols)},
    lock=r"C:\Users\tedd\.codex\locks\performance-measurement.lock"), indent=2), encoding="utf-8")
