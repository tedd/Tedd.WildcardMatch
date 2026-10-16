param(
    [Parameter(Mandatory = $true)][string]$ArtifactsDirectory,
    [Parameter(Mandatory = $true)][string]$ValidationPath,
    [Parameter(Mandatory = $true)][string]$SourceRevision,
    [Parameter(Mandatory = $true)][string]$MeasuredAt,
    [Parameter(Mandatory = $true)][string]$NativeValidationPath,
    [Parameter(Mandatory = $true)][string]$NativeSourcesPath,
    [Parameter(Mandatory = $true)][string]$PriorNativeReferencePath,
    [string]$PackageVersion = '2.3.0',
    [Parameter(Mandatory = $true)][string]$MeasuredPackageVersion,
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
$nativeReport = Get-Content -LiteralPath (Join-Path $ArtifactsDirectory 'results/Tedd.WildcardMatchBenchmark.NativeMatchingBenchmarks-report-full.json') -Raw | ConvertFrom-Json
$interopReport = Get-Content -LiteralPath (Join-Path $ArtifactsDirectory 'results/Tedd.WildcardMatchBenchmark.InteropBenchmarks-report-full.json') -Raw | ConvertFrom-Json
$controlReport = Get-Content -LiteralPath (Join-Path $ArtifactsDirectory 'results/Tedd.WildcardMatchBenchmark.InteropControlBenchmarks-report-full.json') -Raw | ConvertFrom-Json
foreach ($report in @($matchingReport, $constructionReport, $nativeReport, $interopReport, $controlReport)) {
    if (@($report.Benchmarks | Where-Object {
        $_.DisplayInfo -notmatch 'LaunchCount=3' -or $_.DisplayInfo -notmatch 'WarmupCount=3' -or
        $_.DisplayInfo -notmatch 'IterationCount=10' -or $_.DisplayInfo -notmatch 'DOTNET_TieredCompilation=0'
    }).Count -gt 0) { throw 'The exporter requires optimized JIT, three launches, three warmups and ten measured iterations.' }
}
$validation = @(Get-Content -LiteralPath $ValidationPath -Raw | ConvertFrom-Json)
$nativeValidation = @(Get-Content -LiteralPath $NativeValidationPath -Raw | ConvertFrom-Json)
$nativeSources = @(Get-Content -LiteralPath $NativeSourcesPath -Raw | ConvertFrom-Json)
$priorNativeReference = @(Get-Content -LiteralPath $PriorNativeReferencePath -Raw | ConvertFrom-Json)
$libraries = [ordered]@{
    TeddDirectStatic = @{ label = 'Tedd direct · static'; package = 'Tedd.WildcardMatch'; version = $PackageVersion; mode = 'Direct static call' }
    TeddDirectReused = @{ label = 'Tedd direct · reused'; package = 'Tedd.WildcardMatch'; version = $PackageVersion; mode = 'Reusable direct matcher' }
    TeddRegexStatic = @{ label = 'Tedd Regex · static'; package = 'Tedd.WildcardMatch'; version = $PackageVersion; mode = 'Translation and Regex cache lookup per call' }
    TeddRegexReused = @{ label = 'Tedd Regex · reused'; package = 'Tedd.WildcardMatch'; version = $PackageVersion; mode = 'Reusable interpreted Regex' }
    TeddRegexCompiled = @{ label = 'Tedd Regex · compiled'; package = 'Tedd.WildcardMatch'; version = $PackageVersion; mode = 'Reusable compiled Regex' }
    FastWildcard = @{ label = 'FastWildcard (C#)'; package = 'FastWildcard'; version = '3.1.0'; mode = 'Static call, reused settings' }
    WildcardMatch = @{ label = 'WildcardMatch (C#)'; package = 'WildcardMatch'; version = '1.0.7'; mode = 'Static extension call' }
    DotNetGlob = @{ label = 'DotNet.Glob (C#)'; package = 'DotNet.Glob'; version = '3.1.3'; mode = 'Reusable parsed glob' }
}
foreach ($key in @($libraries.Keys)) { if ($key.StartsWith('Tedd')) { $libraries[$key].version = $MeasuredPackageVersion } }
$nativeNames = [ordered]@{ Krauss = 'Krauss (C++)'; GitWildmatch = 'Git wildmatch (C)'; ZemaWildcards = 'Zema wildcards (C++)'; Pcre2Jit = 'PCRE2 JIT (C)' }
$pinNames = @{ Krauss = 'krauss'; GitWildmatch = 'git-source'; ZemaWildcards = 'wildcards'; Pcre2Jit = 'pcre2' }
foreach ($key in $nativeNames.Keys) {
    $pin = @($nativeSources | Where-Object name -eq $pinNames[$key])
    if ($pin.Count -ne 1) { throw "Missing native source pin: $key" }
    $libraries[$key] = @{ label = $nativeNames[$key]; package = $null; version = $pin[0].revision; source = $pin[0].url; mode = 'Native, prepared ASCII buffers' }
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
        workload = $workload; library = $library; mode = 'managed'
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
    foreach ($library in ($libraries.Keys | Where-Object { $_ -notin $nativeNames.Keys })) {
        if ("$($workload.name)/$library" -notin $uniqueCases) { throw "Missing case: $($workload.name)/$library" }
    }
}
$constructionCases = @($construction | ForEach-Object { "$($_.pattern)/$($_.library)" } | Select-Object -Unique)
foreach ($pattern in @('report-??.txt', '*a?c*e*')) {
    foreach ($library in @('TeddDirectReused', 'TeddRegexReused', 'TeddRegexCompiled', 'DotNetGlob')) {
        if ("$pattern/$library" -notin $constructionCases) { throw "Missing construction case: $pattern/$library" }
    }
}
$nativeMatching = @(foreach ($report in @($nativeReport, $interopReport)) {
    foreach ($row in $report.Benchmarks) {
        if ($row.FullName -notmatch '\(ShareInputs: (False|True), Case: ([^/]+)/([^/)]+)\)$') { throw "Unrecognized native case: $($row.FullName)" }
        $shareInputs = $Matches[1] -eq 'True'; $workload = $Matches[2]; $library = $Matches[3]
        $check = @($nativeValidation | Where-Object { $_.Workload -eq $workload -and $_.Library -eq $library -and $_.ShareInputs -eq $shareInputs })
        if ($check.Count -ne 1 -or $check[0].Failures -ne 0 -or $null -eq $row.Statistics -or $row.Statistics.Mean -le 0) { throw "Unvalidated native measurement: $($row.FullName)" }
        $callMode = switch ($row.Method) { Match { 'batch' } Normal { 'normal' } Suppressed { 'suppressed' } default { throw 'Unknown native call mode.' } }
        $storage = if ($shareInputs) { 'shared' } else { 'cloned' }
        [pscustomobject][ordered]@{
            workload = $workload; library = $library; mode = "${callMode}_$storage"
            meanNs = $row.Statistics.Mean; errorNs = $row.Statistics.ConfidenceInterval.Margin
            stdDevNs = $row.Statistics.StandardDeviation; allocatedBytes = $row.Memory.BytesAllocatedPerOperation
            samplesNs = $row.Statistics.OriginalValues
        }
    }
})
$expectedNativeCases = @(foreach ($library in $nativeNames.Keys) { foreach ($workload in $workloads) {
    foreach ($storage in @('cloned', 'shared')) {
        "$($workload.name)/$library/batch_$storage"
        if ($library -ne 'Pcre2Jit' -and $workload.name -ne 'LongText') {
            "$($workload.name)/$library/normal_$storage"; "$($workload.name)/$library/suppressed_$storage"
        }
    }
} })
$actualNativeCases = @($nativeMatching | ForEach-Object { "$($_.workload)/$($_.library)/$($_.mode)" })
if ($nativeReport.Benchmarks.Count -ne 32 -or $interopReport.Benchmarks.Count -ne 36 -or $actualNativeCases.Count -ne 68 -or
    @($actualNativeCases | Select-Object -Unique).Count -ne 68 -or @(Compare-Object $expectedNativeCases $actualNativeCases).Count -ne 0 -or
    $nativeValidation.Count -ne 40 -or @($nativeValidation | Where-Object Failures -ne 0).Count -gt 0) { throw 'Incomplete native comparison or validation matrix.' }
$interopControl = @($controlReport.Benchmarks | ForEach-Object {
    if ($null -eq $_.Statistics -or $_.Statistics.Mean -le 0) { throw 'Missing transition control measurement.' }
    [pscustomobject]@{ mode = $_.Method; meanNs = $_.Statistics.Mean; errorNs = $_.Statistics.ConfidenceInterval.Margin; samplesNs = $_.Statistics.OriginalValues }
})
if ($interopControl.Count -ne 2 -or @($interopControl.mode | Sort-Object -Unique).Count -ne 2 -or
    'Normal' -notin $interopControl.mode -or 'Suppressed' -notin $interopControl.mode) { throw 'Incomplete transition controls.' }
$matching += $nativeMatching
# Charts choose the lowest measured native call/storage mode per implementation;
# the complete table and downloads preserve every mode, including slower ones.
$chartMatching = @($matching | Where-Object { -not $_.library.StartsWith('Tedd') -or $_.library -eq 'TeddDirectReused' } |
    Group-Object workload,library | ForEach-Object { $_.Group | Sort-Object meanNs | Select-Object -First 1 })
$assets = Join-Path $SiteDirectory 'assets'
$null = New-Item -ItemType Directory -Force -Path $assets
$data = [ordered]@{
    measuredAt = $MeasuredAt
    exportedAt = [DateTimeOffset]::Now.ToString('o')
    releaseVersion = $PackageVersion; measuredPackageVersion = $MeasuredPackageVersion
    librarySourceRevision = $SourceRevision
    environment = $matchingReport.HostEnvironmentInfo
    job = @{ name = 'Optimized JIT, three processes'; launchCount = 3; warmupCount = 3; iterationCount = 10; iterationTimeMs = 100; tieredCompilation = $false; affinity = 'CPU 30'; batchSize = 256 }
    libraries = $libraries; workloads = $workloads; matching = $matching; construction = $construction
    interopControl = $interopControl; nativeSources = $nativeSources; priorNativeReference = $priorNativeReference
    caseCounts = @{ managedMatching = 32; nativeBatch = 32; nativePerInput = 36; construction = 8; transitionControl = 2; total = 110 }
}
$data | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath (Join-Path $assets 'package-comparison.json') -Encoding utf8
Copy-Item -LiteralPath $ValidationPath -Destination (Join-Path $assets 'validation.json') -Force
Copy-Item -LiteralPath $NativeValidationPath -Destination (Join-Path $assets 'native-validation.json') -Force
foreach ($reportName in @('Matching', 'Construction', 'NativeMatching', 'Interop', 'InteropControl')) {
    $rawReport = Get-Content -LiteralPath (Join-Path $ArtifactsDirectory "results/Tedd.WildcardMatchBenchmark.$($reportName)Benchmarks-report-github.md") -Raw
    $cleanReport = [regex]::Replace($rawReport, '[ \t]+(?=\r?$)', '', [Text.RegularExpressions.RegexOptions]::Multiline).TrimEnd()
    $cleanReport | Set-Content -LiteralPath (Join-Path $assets "$($reportName.ToLowerInvariant())-report.md") -Encoding utf8
    foreach ($extension in @('csv', 'json')) {
        $suffix = if ($extension -eq 'json') { '-full.json' } else { '.csv' }
        Copy-Item -LiteralPath (Join-Path $ArtifactsDirectory "results/Tedd.WildcardMatchBenchmark.$($reportName)Benchmarks-report$suffix") -Destination (Join-Path $assets "$($reportName.ToLowerInvariant())-report.$extension") -Force
    }
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
    $rows = @($chartMatching | Where-Object workload -eq $group.initial | Sort-Object meanNs)
    $peak = 1000 / $rows[0].meanNs
    foreach ($row in $rows) {
        $own = $row.library.StartsWith('Tedd')
        $rowClass = if ($own) { 'bar-row comparison-ours' } else { 'bar-row' }
        $barClass = if ($own) { 'ours' } else { 'other' }
        $rate = 1000 / $row.meanNs
        $width = Number ($rate / $peak * 100) 'F2'
        $label = $libraries[$row.library].label + $(if ($row.mode -ne 'managed') { " · $($row.mode)" })
        $null = $builder.AppendLine(('<div class="{0}"><span>{1}</span><div class="bar-track"><i class="bar {2}" style="width:{3}%"></i></div><b>{4}</b></div>' -f $rowClass, (Html $label), $barClass, $width, (Number $rate)))
    }
    $direct = $matching | Where-Object { $_.workload -eq $group.initial -and $_.library -eq 'TeddDirectReused' }
    $competitor = $rows | Where-Object { -not $_.library.StartsWith('Tedd') } | Select-Object -First 1
    $null = $builder.AppendLine(('</div></figure><p class="chart-summary">Reusable direct matcher: <strong data-direct-speedup>{0}×</strong> the throughput of the next-fastest tested library on this workload.</p></div>' -f (Number ($competitor.meanNs / $direct.meanNs) 'F2')))
}
$null = $builder.AppendLine('</div>')
$null = $builder.AppendLine('<div class="results-panel"><div class="results-heading"><h3>All matching comparisons</h3><p>100 cases · lower time is better</p></div><div class="table-scroll"><table><caption>Every managed and native matching call and storage mode</caption><thead><tr><th scope="col">Workload</th><th scope="col">Implementation</th><th scope="col">Call / storage</th><th scope="col">Mean ns</th><th scope="col">Error ± ns</th><th scope="col">Managed B/match</th></tr></thead><tbody>')
foreach ($workload in $workloads) { foreach ($row in ($matching | Where-Object workload -eq $workload.name | Sort-Object meanNs)) {
    $class = if ($row.library.StartsWith('Tedd')) { ' class="comparison-ours"' } else { '' }
    $null = $builder.AppendLine(('<tr{0}><th scope="row">{1}</th><td>{2}</td><td>{3}</td><td>{4}</td><td>{5}</td><td>{6}</td></tr>' -f $class, (Html $workload.label), (Html $libraries[$row.library].label), (Html $row.mode), (Number $row.meanNs 'F3'), (Number $row.errorNs 'F3'), (Number $row.allocatedBytes 'F0')))
} }
$null = $builder.AppendLine('</tbody></table></div></div>')
$null = $builder.AppendLine('<div class="results-panel"><div class="results-heading"><h3>Pattern construction</h3><p>8 cases · excluded from reused matching</p></div><div class="table-scroll"><table><caption>Pattern construction time and managed allocation</caption><thead><tr><th scope="col">Pattern</th><th scope="col">Implementation</th><th scope="col">Mean ns</th><th scope="col">Error ± ns</th><th scope="col">Managed B/construction</th></tr></thead><tbody>')
foreach ($row in $construction) {
    $null = $builder.AppendLine(('<tr><th scope="row"><code>{0}</code></th><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td></tr>' -f (Html $row.pattern), (Html $libraries[$row.library].label), (Number $row.meanNs 'F3'), (Number $row.errorNs 'F3'), (Number $row.allocatedBytes 'F0')))
}
$null = $builder.AppendLine('</tbody></table></div></div>')
$null = $builder.AppendLine('<div class="results-panel"><div class="results-heading"><h3>Managed / native transition control</h3><p>2 cases · trivial native function</p></div><div class="table-scroll"><table><caption>Normal and suppressed transition control time</caption><thead><tr><th scope="col">Call</th><th scope="col">Mean ns</th><th scope="col">Error ± ns</th></tr></thead><tbody>')
foreach ($row in $interopControl) {
    $null = $builder.AppendLine(('<tr><th scope="row">{0}</th><td>{1}</td><td>{2}</td></tr>' -f (Html $row.mode), (Number $row.meanNs 'F3'), (Number $row.errorNs 'F3')))
}
$null = $builder.AppendLine('</tbody></table></div></div>')
$null = $builder.AppendLine('<p class="benchmark-method">Native batch calls cross once per 256 matches; normal and suppressed calls cross once per input into the identical export. Suppression is qualified only for the three short Krauss, Git and Zema fixtures. PCRE2 and long calls use ordinary transitions. The trivial control is not subtracted from matcher timings. Cloned storage owns each input buffer; shared storage reuses buffers for equal input values. Every match executes; results are not cached.</p>')
$hostInfo = $data.environment
$method = '<p class="benchmark-method" id="benchmark-method">' +
    (Html "Measured $MeasuredAt · $($hostInfo.ProcessorName) · $($hostInfo.RuntimeVersion) · BenchmarkDotNet $($hostInfo.BenchmarkDotNetVersion). Three launches, three warmups, ten 100 ms measured iterations, CPU 30 affinity, tiering disabled. Error is the 99.9% confidence-interval half-width over retained iteration measurements. Managed UTF-16 and native NUL-free ASCII buffers, encoding and reusable patterns are prepared before timing. Native allocation is not measured by managed B/match. Results depend on corpus and API lifetime and do not establish a universal ranking. The measured source snapshot has package metadata $MeasuredPackageVersion; release $PackageVersion contains the same matcher source.") +
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
