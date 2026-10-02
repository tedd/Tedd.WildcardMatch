param(
    [Parameter(Mandatory = $true)][string]$ArtifactsDirectory,
    [Parameter(Mandatory = $true)][string]$ValidationPath,
    [Parameter(Mandatory = $true)][string]$SourceRevision,
    [string]$PackageVersion = '2.2.0',
    [string]$SiteDirectory = (Join-Path $PSScriptRoot '../../site')
)
$ErrorActionPreference = 'Stop'
$invariant = [Globalization.CultureInfo]::InvariantCulture
function Number([double]$Value, [string]$Format = 'N2') { $Value.ToString($Format, $invariant) }
function Html([string]$Value) { [Net.WebUtility]::HtmlEncode($Value) }

$matchingPath = Join-Path $ArtifactsDirectory 'results/Tedd.WildcardMatchBenchmark.MatchingBenchmarks-report-full.json'
$constructionPath = Join-Path $ArtifactsDirectory 'results/Tedd.WildcardMatchBenchmark.ConstructionBenchmarks-report-full.json'
$matchingReport = Get-Content -LiteralPath $matchingPath -Raw | ConvertFrom-Json
$constructionReport = Get-Content -LiteralPath $constructionPath -Raw | ConvertFrom-Json
foreach ($report in @($matchingReport, $constructionReport)) {
    if (@($report.Benchmarks | Where-Object {
        $_.DisplayInfo -notmatch 'LaunchCount=3' -or $_.DisplayInfo -notmatch 'WarmupCount=3' -or
        $_.DisplayInfo -notmatch 'IterationCount=10' -or $_.DisplayInfo -notmatch 'DOTNET_TieredCompilation=0'
    }).Count -gt 0) { throw 'The exporter requires optimized JIT, three launches, three warmups and ten measured iterations.' }
}
$validation = @(Get-Content -LiteralPath $ValidationPath -Raw | ConvertFrom-Json)
$libraries = [ordered]@{
    TeddDirectStatic = @{ label = 'Tedd direct · static'; package = 'Tedd.WildcardMatch'; version = $PackageVersion; mode = 'Direct static call' }
    TeddDirectReused = @{ label = 'Tedd direct · reused'; package = 'Tedd.WildcardMatch'; version = $PackageVersion; mode = 'Reusable direct matcher' }
    TeddRegexStatic = @{ label = 'Tedd Regex · static'; package = 'Tedd.WildcardMatch'; version = $PackageVersion; mode = 'Translation and Regex cache lookup per call' }
    TeddRegexReused = @{ label = 'Tedd Regex · reused'; package = 'Tedd.WildcardMatch'; version = $PackageVersion; mode = 'Reusable interpreted Regex' }
    TeddRegexCompiled = @{ label = 'Tedd Regex · compiled'; package = 'Tedd.WildcardMatch'; version = $PackageVersion; mode = 'Reusable compiled Regex' }
    FastWildcard = @{ label = 'FastWildcard'; package = 'FastWildcard'; version = '3.1.0'; mode = 'Static call, reused settings' }
    WildcardMatch = @{ label = 'WildcardMatch'; package = 'WildcardMatch'; version = '1.0.7'; mode = 'Static extension call' }
    DotNetGlob = @{ label = 'DotNet.Glob'; package = 'DotNet.Glob'; version = '3.1.3'; mode = 'Reusable parsed glob' }
}
$workloads = @(
    @{ name = 'Literal'; label = 'Literal'; pattern = 'report-2026.txt'; inputGroup = 'short' },
    @{ name = 'Simple'; label = 'Question marks'; pattern = 'report-??.txt'; inputGroup = 'short' },
    @{ name = 'MultiStar'; label = 'Multiple stars'; pattern = '*a?c*e*'; inputGroup = 'short' },
    @{ name = 'LongText'; label = 'Long text'; pattern = '*alpha*beta??*omega*'; inputGroup = 'long' }
)
$matching = @($matchingReport.Benchmarks | ForEach-Object {
    # Parameters is truncated by BDN's display width; FullName retains the complete case.
    if ($_.FullName -notmatch '\(Case: ([^/]+)/([^/)]+)\)$') { throw "Unrecognized case: $($_.FullName)" }
    $workload = $Matches[1]
    $library = $Matches[2]
    if ($null -eq $_.Statistics -or $_.Statistics.Mean -le 0) { throw "Missing measurements: $($_.Parameters)" }
    $check = @($validation | Where-Object { $_.Workload -eq $workload -and $_.Library -eq $library })
    if ($check.Count -ne 1 -or $check[0].Failures -ne 0) { throw "Unvalidated measurements: $($_.Parameters)" }
    [pscustomobject][ordered]@{
        workload = $workload; library = $library
        meanNs = $_.Statistics.Mean; errorNs = $_.Statistics.ConfidenceInterval.Margin
        stdDevNs = $_.Statistics.StandardDeviation
        allocatedBytes = $_.Memory.BytesAllocatedPerOperation
        samplesNs = $_.Statistics.OriginalValues
    }
})
$construction = @($constructionReport.Benchmarks | ForEach-Object {
    if ($null -eq $_.Statistics -or $_.Statistics.Mean -le 0) { throw "Missing measurements: $($_.DisplayInfo)" }
    [pscustomobject][ordered]@{
        pattern = $_.Parameters.Substring('Pattern='.Length); library = $_.Method
        meanNs = $_.Statistics.Mean; errorNs = $_.Statistics.ConfidenceInterval.Margin
        stdDevNs = $_.Statistics.StandardDeviation
        allocatedBytes = $_.Memory.BytesAllocatedPerOperation
        samplesNs = $_.Statistics.OriginalValues
    }
})
# Fail closed if a benchmark is absent; never present a partial run as a complete comparison.
if ($matching.Count -ne 32 -or $construction.Count -ne 8 -or $validation.Count -ne 32) {
    throw 'Expected 32 matching cases, 8 construction cases and 32 validation results.'
}
$uniqueCases = @($matching | ForEach-Object { "$($_.workload)/$($_.library)" } | Select-Object -Unique)
if ($uniqueCases.Count -ne 32) { throw 'Duplicate matching cases.' }
foreach ($workload in $workloads) {
    foreach ($library in $libraries.Keys) {
        if ("$($workload.name)/$library" -notin $uniqueCases) { throw "Missing case: $($workload.name)/$library" }
    }
}
$constructionCases = @($construction | ForEach-Object { "$($_.pattern)/$($_.library)" } | Select-Object -Unique)
foreach ($pattern in @('report-??.txt', '*a?c*e*')) {
    foreach ($library in @('TeddDirectReused', 'TeddRegexReused', 'TeddRegexCompiled', 'DotNetGlob')) {
        if ("$pattern/$library" -notin $constructionCases) { throw "Missing construction case: $pattern/$library" }
    }
}
$assets = Join-Path $SiteDirectory 'assets'
$null = New-Item -ItemType Directory -Force -Path $assets
$data = [ordered]@{
    measuredAt = [DateTimeOffset]::Now.ToString('o')
    librarySourceRevision = $SourceRevision
    environment = $matchingReport.HostEnvironmentInfo
    job = @{ name = 'Optimized JIT, three processes'; launchCount = 3; warmupCount = 3; iterationCount = 10; iterationTimeMs = 100; tieredCompilation = $false; affinity = 'CPU 30'; batchSize = 256 }
    libraries = $libraries; workloads = $workloads; matching = $matching; construction = $construction
}
$data | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath (Join-Path $assets 'package-comparison.json') -Encoding utf8
Copy-Item -LiteralPath $ValidationPath -Destination (Join-Path $assets 'validation.json') -Force
foreach ($reportName in @('Matching', 'Construction')) {
    $rawReport = Get-Content -LiteralPath (Join-Path $ArtifactsDirectory "results/Tedd.WildcardMatchBenchmark.$($reportName)Benchmarks-report-github.md") -Raw
    $cleanReport = [regex]::Replace($rawReport, '[ \t]+(?=\r?$)', '', [Text.RegularExpressions.RegexOptions]::Multiline).TrimEnd()
    $cleanReport | Set-Content -LiteralPath (Join-Path $assets "$($reportName.ToLowerInvariant())-report.md") -Encoding utf8
}

$builder = [Text.StringBuilder]::new()
$inputGroups = @(
    @{ name = 'short'; label = 'Short strings'; detail = '4–16 UTF-16 code units'; initial = 'Simple' },
    @{ name = 'long'; label = 'Long strings'; detail = 'Every input exceeds 1,000 UTF-16 code units'; initial = 'LongText' }
)
$null = $builder.AppendLine('<div class="chart-grid">')
foreach ($group in $inputGroups) {
    $workload = $workloads | Where-Object name -eq $group.initial
    $null = $builder.AppendLine(('<div class="benchmark-chart" data-comparison-chart="{0}"><div class="benchmark-controls"><label for="comparison-fixture-{0}">{1}</label><select id="comparison-fixture-{0}" data-fixture-select>' -f $group.name, (Html $group.label)))
    foreach ($item in ($workloads | Where-Object inputGroup -eq $group.name)) {
        $selected = if ($item.name -eq $group.initial) { ' selected' } else { '' }
        $null = $builder.AppendLine(('<option value="{0}"{1}>{2}</option>' -f $item.name, $selected, (Html $item.label)))
    }
    $null = $builder.AppendLine(('</select><span>{0}</span></div><figure class="bar-chart package-chart"><figcaption><span>{1} · matching throughput</span><strong data-chart-workload>{2}</strong><small>Pattern: <code data-chart-pattern>{3}</code> · millions of matches/s</small></figcaption><div data-chart-rows aria-live="polite">' -f (Html $group.detail), (Html $group.label), (Html $workload.label), (Html $workload.pattern)))
    $rows = @($matching | Where-Object workload -eq $group.initial | Sort-Object meanNs)
    $peak = 1000 / $rows[0].meanNs
    foreach ($row in $rows) {
        $own = $row.library.StartsWith('Tedd')
        $rowClass = if ($own) { 'bar-row comparison-ours' } else { 'bar-row' }
        $barClass = if ($own) { 'ours' } else { 'other' }
        $rate = 1000 / $row.meanNs
        $width = Number ($rate / $peak * 100) 'F2'
        $null = $builder.AppendLine(('<div class="{0}"><span>{1}</span><div class="bar-track"><i class="bar {2}" style="width:{3}%"></i></div><b>{4}</b></div>' -f $rowClass, (Html $libraries[$row.library].label), $barClass, $width, (Number $rate)))
    }
    $direct = $matching | Where-Object { $_.workload -eq $group.initial -and $_.library -eq 'TeddDirectReused' }
    $competitor = $rows | Where-Object { -not $_.library.StartsWith('Tedd') } | Select-Object -First 1
    $null = $builder.AppendLine(('</div></figure><p class="chart-summary">Reusable direct matcher: <strong data-direct-speedup>{0}×</strong> the throughput of the next-fastest tested library on this workload.</p></div>' -f (Number ($competitor.meanNs / $direct.meanNs) 'F2')))
}
$null = $builder.AppendLine('</div>')
$hostInfo = $data.environment
$method = '<p class="benchmark-method" id="benchmark-method">' +
    (Html "Measured $($data.measuredAt.Substring(0,10)) · $($hostInfo.ProcessorName) · .NET 10 · three processes with optimized JIT. Reusable patterns are prepared before timing. Results depend on workload and API lifetime; full timings, intervals, allocations and configuration are linked below.") +
    ' Library source: <a href="https://github.com/tedd/Tedd.WildcardMatch/commit/' + (Html $SourceRevision) + '">' + (Html $SourceRevision.Substring(0,7)) + '</a>.</p>'
$indexPath = Join-Path $SiteDirectory 'index.html'
$index = Get-Content -LiteralPath $indexPath -Raw
$contentPattern = '(?s)<!-- BENCHMARK_CONTENT_START -->.*?<!-- BENCHMARK_CONTENT_END -->'
$methodPattern = '(?s)<!-- BENCHMARK_METHOD_START -->.*?<!-- BENCHMARK_METHOD_END -->'
if ($index -notmatch $contentPattern -or $index -notmatch $methodPattern) { throw 'Site benchmark markers are missing.' }
$content = "<!-- BENCHMARK_CONTENT_START -->`n$builder<!-- BENCHMARK_CONTENT_END -->"
$methodContent = "<!-- BENCHMARK_METHOD_START -->`n$method`n<!-- BENCHMARK_METHOD_END -->"
$index = [regex]::Replace($index, $contentPattern, [Text.RegularExpressions.MatchEvaluator]{ param($match) $content })
$index = [regex]::Replace($index, $methodPattern, [Text.RegularExpressions.MatchEvaluator]{ param($match) $methodContent })
$simpleDirect = $matching | Where-Object { $_.workload -eq 'Simple' -and $_.library -eq 'TeddDirectReused' }
$simpleCompetitor = $matching | Where-Object { $_.workload -eq 'Simple' -and -not $_.library.StartsWith('Tedd') } | Sort-Object meanNs | Select-Object -First 1
$hero = '<p class="hero-evidence"><strong>' + (Number ($simpleCompetitor.meanNs / $simpleDirect.meanNs) 'F2') + '× the throughput of the next-fastest tested library</strong> for the reusable direct matcher in our question-mark benchmark. <a href="#benchmarks">Compare the measured workloads</a>.</p>'
$index = [regex]::Replace($index, '(?s)<p class="hero-evidence">.*?</p>', [Text.RegularExpressions.MatchEvaluator]{ param($match) $hero })
$index.TrimEnd() | Set-Content -LiteralPath $indexPath -Encoding utf8
Write-Host "Exported $($matching.Count) matching and $($construction.Count) construction measurements."
