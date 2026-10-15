param(
    [string]$Workspace = 'D:\Workspaces\AI\wildcard-native-benchmarks',
    [long]$Affinity = 1073741824,
    [switch]$SkipNativeBuild
)
$ErrorActionPreference = 'Stop'
$Workspace = [IO.Path]::GetFullPath($Workspace)
if (!$Workspace.StartsWith('D:\Workspaces\AI\', [StringComparison]::OrdinalIgnoreCase)) { throw 'Use a dedicated D:\Workspaces\AI subdirectory.' }
$null = New-Item -ItemType Directory -Force -Path $Workspace, (Join-Path $Workspace 'tmp')
$env:TEMP = Join-Path $Workspace 'tmp'
$env:TMP = $env:TEMP
$repo = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$copy = Join-Path $Workspace 'managed-source'
$null = New-Item -ItemType Directory -Force -Path $copy
& robocopy (Join-Path $repo 'src') (Join-Path $copy 'src') /E /XD bin obj /NFL /NDL /NJH /NJS
if ($LASTEXITCODE -ge 8) { throw 'Source copy failed' }
Copy-Item -LiteralPath (Join-Path $repo 'global.json'), (Join-Path $repo 'LICENSE'), (Join-Path $repo 'README.md') -Destination $copy
if (!$SkipNativeBuild) { & (Join-Path $PSScriptRoot 'Native/Build-Native.ps1') -Workspace $Workspace }
$env:WILDCARD_NATIVE_LIBRARY = Join-Path $Workspace 'native-build/wildcard_native.dll'
if (!(Test-Path -LiteralPath $env:WILDCARD_NATIVE_LIBRARY)) { throw 'Native benchmark DLL missing' }
Push-Location $copy
try {
    & dotnet build src/Tedd.WildcardMatch.sln -c Release *> (Join-Path $Workspace 'managed-build.txt')
    if ($LASTEXITCODE) { throw 'Managed build failed' }
    & dotnet test src/Tedd.WildcardMatch.Tests -c Release --no-build --logger 'console;verbosity=minimal' *> (Join-Path $Workspace 'tests.txt')
    if ($LASTEXITCODE) { throw 'Tests failed' }
    $project = 'src/Tedd.WildcardMatch.Benchmark'
    & dotnet run --project $project -c Release --no-build -- --validate-packages (Join-Path $Workspace 'validation.json') *> (Join-Path $Workspace 'validation.txt')
    if ($LASTEXITCODE) { throw 'Managed validation failed' }
    & dotnet run --project $project -c Release --no-build -- --validate-native (Join-Path $Workspace 'native-validation.json') *> (Join-Path $Workspace 'native-validation.txt')
    if ($LASTEXITCODE) { throw 'Native validation failed' }
    & dotnet run --project $project -c Release --no-build -- --filter '*' --launchCount 3 --warmupCount 3 --iterationCount 10 --iterationTime 100 --affinity $Affinity --artifacts (Join-Path $Workspace 'artifacts') *> (Join-Path $Workspace 'benchmark.txt')
    if ($LASTEXITCODE) { throw 'Benchmarks failed' }
} finally { Pop-Location }
