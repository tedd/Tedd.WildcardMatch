param(
    [Parameter(Mandatory = $true)][string]$ArtifactsDirectory,
    [Parameter(Mandatory = $true)][string]$ValidationPath,
    [Parameter(Mandatory = $true)][string]$SourceRevision,
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
    if (@($report.Benchmarks | Where-Object DisplayInfo -NotMatch 'ShortRun\(IterationCount=3, LaunchCount=1, WarmupCount=3\)').Count -gt 0) {
        throw 'The website exporter requires a ShortRun with one launch and three warmup/measurement iterations.'
    }
}
$validation = @(Get-Content -LiteralPath $ValidationPath -Raw | ConvertFrom-Json)
$libraries = [ordered]@{
    TeddStatic = @{ label = 'Tedd · static'; package = 'Tedd.WildcardMatch'; version = 'source'; mode = 'Static call' }
    TeddReused = @{ label = 'Tedd · reused'; package = 'Tedd.WildcardMatch'; version = 'source'; mode = 'Reusable regex' }
    TeddCompiled = @{ label = 'Tedd · compiled'; package = 'Tedd.WildcardMatch'; version = 'source'; mode = 'Reusable compiled regex' }
    FastWildcard = @{ label = 'FastWildcard'; package = 'FastWildcard'; version = '3.1.0'; mode = 'Static call, reused settings' }
    WildcardMatch = @{ label = 'WildcardMatch'; package = 'WildcardMatch'; version = '1.0.7'; mode = 'Static extension call' }
    DotNetGlob = @{ label = 'DotNet.Glob'; package = 'DotNet.Glob'; version = '3.1.3'; mode = 'Reusable parsed glob' }
}
$workloads = @(
    @{ name = 'Literal'; label = 'Literal'; pattern = 'report-2026.txt' },
    @{ name = 'Simple'; label = 'Question marks'; pattern = 'report-??.txt' },
    @{ name = 'MultiStar'; label = 'Multiple stars'; pattern = '*a?c*e*' },
    @{ name = 'LongText'; label = 'Long text'; pattern = '*alpha*beta??*omega*' }
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
if ($matching.Count -ne 24 -or $construction.Count -ne 6 -or $validation.Count -ne 24) {
    throw 'Expected 24 matching cases, 6 construction cases and 24 validation results.'
}
$uniqueCases = @($matching | ForEach-Object { "$($_.workload)/$($_.library)" } | Select-Object -Unique)
if ($uniqueCases.Count -ne 24) { throw 'Duplicate matching cases.' }
$assets = Join-Path $SiteDirectory 'assets'
$null = New-Item -ItemType Directory -Force -Path $assets
$data = [ordered]@{
    measuredAt = [DateTimeOffset]::Now.ToString('o')
    librarySourceRevision = $SourceRevision
    environment = $matchingReport.HostEnvironmentInfo
    job = @{ name = 'ShortRun'; launchCount = 1; warmupCount = 3; iterationCount = 3; batchSize = 256 }
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
$null = $builder.AppendLine('<figure class="bar-chart package-chart"><figcaption><span>Matching throughput</span><strong id="chart-workload">Question marks</strong><small>Pattern: <code id="chart-pattern">report-??.txt</code> · millions of matches/s</small></figcaption><div id="chart-rows">')
$rows = @($matching | Where-Object workload -eq 'Simple' | Sort-Object meanNs)
$peak = 1000 / $rows[0].meanNs
foreach ($row in $rows) {
    $own = $row.library.StartsWith('Tedd')
    $rowClass = if ($own) { 'bar-row comparison-ours' } else { 'bar-row' }
    $barClass = if ($own) { 'ours' } else { 'other' }
    $rate = 1000 / $row.meanNs
    $width = Number ($rate / $peak * 100) 'F2'
    $null = $builder.AppendLine(('<div class="{0}"><span>{1}</span><div class="bar-track"><i class="bar {2}" style="width:{3}%"></i></div><b>{4}</b></div>' -f $rowClass, (Html $libraries[$row.library].label), $barClass, $width, (Number $rate)))
}
$null = $builder.AppendLine('</div></figure><div class="results-panel"><div class="results-heading"><h3>Matching · all workloads</h3><p>Time and allocations per match · lower is better</p></div><div class="table-scroll"><table><caption>Matching performance across all workloads and APIs</caption><thead><tr><th scope="col">Workload / pattern</th><th scope="col">Library / API</th><th scope="col">Mean (ns)</th><th scope="col">Error (ns)</th><th scope="col">Std. dev. (ns)</th><th scope="col">Allocated (B)</th></tr></thead><tbody>')
foreach ($workload in $workloads) {
    foreach ($row in ($matching | Where-Object workload -eq $workload.name | Sort-Object meanNs)) {
        $rowClass = if ($row.library.StartsWith('Tedd')) { ' class="comparison-ours"' } else { '' }
        $lib = $libraries[$row.library]
        $null = $builder.AppendLine(('<tr{0}><th scope="row">{1}<br><code>{2}</code></th><td><a href="https://www.nuget.org/packages/{3}">{4}</a></td><td>{5}</td><td>{6}</td><td>{7}</td><td>{8}</td></tr>' -f $rowClass, (Html $workload.label), (Html $workload.pattern), (Html $lib.package), (Html $lib.label), (Number $row.meanNs), (Number $row.errorNs), (Number $row.stdDevNs), (Number $row.allocatedBytes 'N0')))
    }
}
$null = $builder.AppendLine('</tbody></table></div></div><div class="results-panel"><div class="results-heading"><h3>Pattern construction</h3><p>One reusable object · first-match JIT excluded</p></div><div class="table-scroll"><table><caption>Construction cost for reusable matchers</caption><thead><tr><th scope="col">Pattern</th><th scope="col">Library / API</th><th scope="col">Mean (ns)</th><th scope="col">Error (ns)</th><th scope="col">Std. dev. (ns)</th><th scope="col">Allocated (B)</th></tr></thead><tbody>')
foreach ($row in $construction) {
    $null = $builder.AppendLine(('<tr><th scope="row"><code>{0}</code></th><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td><td>{5}</td></tr>' -f (Html $row.pattern), (Html $libraries[$row.library].label), (Number $row.meanNs), (Number $row.errorNs), (Number $row.stdDevNs), (Number $row.allocatedBytes 'N0')))
}
$null = $builder.AppendLine('</tbody></table></div></div>')
$hostInfo = $data.environment
$method = '<p class="benchmark-method" id="benchmark-method">' +
    (Html "Measured $($data.measuredAt.Substring(0,10)) · $($hostInfo.ProcessorName) · $($hostInfo.OsVersion) · $($hostInfo.RuntimeVersion) · SDK $($hostInfo.DotNetCliVersion) · BenchmarkDotNet 0.15.8. ShortRun: one launch, three warmups, three measured iterations. Error is the 99.9% confidence-interval half-width; short runs can have wide intervals. Compare broad differences, not close rankings.") +
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
$index.TrimEnd() | Set-Content -LiteralPath $indexPath -Encoding utf8
Write-Host "Exported $($matching.Count) matching and $($construction.Count) construction measurements."
