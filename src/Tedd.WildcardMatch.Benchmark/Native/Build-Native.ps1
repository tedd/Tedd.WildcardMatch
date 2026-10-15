param(
    [string]$Workspace = 'D:\Workspaces\AI\wildcard-native-benchmarks',
    [string]$VisualStudio = 'C:\Program Files\Microsoft Visual Studio\18\Insiders'
)
$ErrorActionPreference = 'Stop'
$Workspace = [IO.Path]::GetFullPath($Workspace)
if (!$Workspace.StartsWith('D:\Workspaces\AI\', [StringComparison]::OrdinalIgnoreCase)) { throw 'Use a dedicated D:\Workspaces\AI subdirectory.' }
$null = New-Item -ItemType Directory -Force -Path $Workspace
$null = New-Item -ItemType Directory -Force -Path (Join-Path $Workspace 'tmp')
$env:TEMP = Join-Path $Workspace 'tmp'
$env:TMP = $env:TEMP
$sources = @(
    @{ name = 'krauss'; url = 'https://github.com/kirkjkrauss/MatchingWildcards.git'; revision = '924b8a5b9d8104492e29448ef56e8fd872fd26b5' },
    @{ name = 'wildcards'; url = 'https://github.com/zemasoft/wildcards.git'; revision = '5662507f01f5d0a4ec22f45749214276ed366d1b' },
    @{ name = 'git-source'; url = 'https://github.com/git/git.git'; revision = '6de20f6092dcf9bdb1c8efe03db4b70c82b423dd' },
    @{ name = 'pcre2'; url = 'https://github.com/PCRE2Project/pcre2.git'; revision = '13e1da98d8d30f54e96afa5829fe335c6115aa45' }
)
foreach ($source in $sources) {
    $path = Join-Path $Workspace $source.name
    if (!(Test-Path -LiteralPath $path)) {
        & git init $path
        if ($LASTEXITCODE) { throw 'git init failed' }
        & git -C $path remote add origin $source.url
        if ($LASTEXITCODE) { throw 'git remote failed' }
        & git -C $path fetch --depth 1 origin $source.revision
        if ($LASTEXITCODE) { throw 'git fetch failed' }
        & git -C $path checkout --detach FETCH_HEAD
        if ($LASTEXITCODE) { throw 'git checkout failed' }
    }
    $actual = & git -C $path rev-parse HEAD
    if ($LASTEXITCODE -or $actual -ne $source.revision) { throw "Unexpected source revision: $path" }
}
& git -C (Join-Path $Workspace 'pcre2') submodule update --init --depth 1
if ($LASTEXITCODE) { throw 'PCRE2 JIT dependency fetch failed' }
$standalone = Join-Path $Workspace 'git-standalone'
$null = New-Item -ItemType Directory -Force -Path $standalone
Copy-Item -LiteralPath (Join-Path $Workspace 'git-source/wildmatch.c'), (Join-Path $Workspace 'git-source/wildmatch.h') -Destination $standalone
# The benchmark accepts ASCII only. Git's application-wide compatibility header is
# replaced with the standard C declarations this translation unit requires.
Set-Content -LiteralPath (Join-Path $standalone 'git-compat-util.h') -Value @'
#include <ctype.h>
#include <string.h>
#define is_glob_special(c) ((c) == '*' || (c) == '?' || (c) == '[' || (c) == '\\')
'@
$msvc = (Get-ChildItem (Join-Path $VisualStudio 'VC/Tools/MSVC') -Directory | Sort-Object Name -Descending | Select-Object -First 1).FullName
$sdk = 'C:\Program Files (x86)\Windows Kits\10'
$sdkVersion = '10.0.22621.0'
$env:PATH = "$(Join-Path $msvc 'bin/Hostx64/x64');$sdk\bin\$sdkVersion\x64;$env:PATH"
$env:INCLUDE = "$msvc\include;$sdk\Include\$sdkVersion\ucrt;$sdk\Include\$sdkVersion\shared;$sdk\Include\$sdkVersion\um"
$env:LIB = "$msvc\lib\x64;$sdk\Lib\$sdkVersion\ucrt\x64;$sdk\Lib\$sdkVersion\um\x64"
$cmake = Join-Path $VisualStudio 'Common7/IDE/CommonExtensions/Microsoft/CMake/CMake/bin/cmake.exe'
$sdkCmake = $sdk.Replace('\', '/')
& $cmake --fresh -S $PSScriptRoot -B (Join-Path $Workspace 'native-build') -G 'NMake Makefiles' '-DCMAKE_BUILD_TYPE=Release' "-DSOURCES=$($Workspace.Replace('\','/'))" "-DCMAKE_RC_COMPILER=$sdkCmake/bin/$sdkVersion/x64/rc.exe" "-DCMAKE_MT=$sdkCmake/bin/$sdkVersion/x64/mt.exe"
if ($LASTEXITCODE) { throw 'Native configure failed' }
& $cmake --build (Join-Path $Workspace 'native-build') --config Release
if ($LASTEXITCODE) { throw 'Native build failed' }
$sources | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $Workspace 'native-sources.json')
Write-Output (Join-Path $Workspace 'native-build/wildcard_native.dll')
