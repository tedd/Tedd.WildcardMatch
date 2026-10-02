"""Update the existing graph exporter and product copy; source-only edits."""
from pathlib import Path
import re
here=Path(__file__).resolve().parent
repo=here.parents[3]
export=repo/'src/Tedd.WildcardMatch.Benchmark/Export-SiteBenchmarks.ps1'
s=export.read_text(encoding='utf-8-sig')
s=s.replace("    [string]$SiteDirectory", "    [string]$PackageVersion = '2.2.0',\n    [string]$SiteDirectory",1)
s=s.replace("$_.FullName", "$_.FullName")
start=s.index('foreach ($report in @($matchingReport, $constructionReport)) {')
end=s.index('$validation =',start)
s=s[:start]+'''foreach ($report in @($matchingReport, $constructionReport)) {
    if (@($report.Benchmarks | Where-Object {
        $_.DisplayInfo -notmatch 'LaunchCount=3' -or $_.DisplayInfo -notmatch 'WarmupCount=3' -or
        $_.DisplayInfo -notmatch 'IterationCount=10' -or $_.DisplayInfo -notmatch 'DOTNET_TieredCompilation=0'
    }).Count -gt 0) { throw 'The exporter requires optimized JIT, three launches, three warmups and ten measured iterations.' }
}
'''+s[end:]
s=s.replace("version = 'source'", 'version = $PackageVersion')
s=s.replace("job = @{ name = 'ShortRun with ten measured iterations'; launchCount = 1; warmupCount = 3; iterationCount = 10; batchSize = 256 }", "job = @{ name = 'Optimized JIT, three processes'; launchCount = 3; warmupCount = 3; iterationCount = 10; iterationTimeMs = 100; tieredCompilation = $false; affinity = 'CPU 30'; batchSize = 256 }")
start=s.index('foreach ($group in $inputGroups) {\n    $null = $builder.AppendLine((\'<div class="results-panel"')
end=s.index('$hostInfo = $data.environment',start)
s=s[:start]+s[end:]
s=s.replace("    $null = $builder.AppendLine('</div></figure></div>')", '''    $direct = $matching | Where-Object { $_.workload -eq $group.initial -and $_.library -eq 'TeddDirectReused' }
    $glob = $matching | Where-Object { $_.workload -eq $group.initial -and $_.library -eq 'DotNetGlob' }
    $null = $builder.AppendLine(('</div></figure><p class="chart-summary">Reusable direct matcher: <strong data-direct-speedup>{0}×</strong> DotNet.Glob throughput on this workload.</p></div>' -f (Number ($glob.meanNs / $direct.meanNs) 'F2')))''')
s=s.replace('One launch, three warmups, ten measured iterations. Error is the 99.9% confidence-interval half-width. Compare intervals before interpreting close rankings; results describe these fixtures and API lifetimes.', 'Three launches, three warmups, ten measured iterations; optimized JIT, tiering disabled. Prepared patterns are outside matching time. Results depend on workload and API lifetime; full timings, intervals, allocations and construction costs are linked below.')
marker='$index.TrimEnd() | Set-Content'
s=s.replace(marker,'''$simpleDirect = $matching | Where-Object { $_.workload -eq 'Simple' -and $_.library -eq 'TeddDirectReused' }
$simpleGlob = $matching | Where-Object { $_.workload -eq 'Simple' -and $_.library -eq 'DotNetGlob' }
$hero = '<p class="hero-evidence"><strong>' + (Number ($simpleGlob.meanNs / $simpleDirect.meanNs) 'F2') + '× DotNet.Glob throughput</strong> for the reusable direct matcher in our question-mark benchmark. <a href="#benchmarks">Compare the measured workloads</a>.</p>'
$index = [regex]::Replace($index, '(?s)<p class="hero-evidence">.*?</p>', [Text.RegularExpressions.MatchEvaluator]{ param($match) $hero })
'''+marker)
export.write_text(s,encoding='utf-8')
index=repo/'site/index.html'
s=index.read_text(encoding='utf-8-sig')
s=s.replace('Highly optimized wildcard matching for .NET. Fastest in our literal-matching benchmark against FastWildcard, WildcardMatch and DotNet.Glob, with complete short- and long-text comparisons.', 'High-performance wildcard matching for .NET strings and spans. Compare matching throughput across literal, question-mark, multi-star and long-text workloads.')
s=s.replace('Highly optimized wildcard matching. Fastest in our literal-matching benchmark, with complete comparisons across short and long inputs.', 'Wildcard matching for strings and spans, with measured throughput comparisons across short and long inputs.')
s=s.replace('href="styles.css"','href="styles.css?v=2.2.0"').replace('src="app.js"','src="app.js?v=2.2.0"')
s=s.replace('Version 2.1.0 adds span matching.', 'Match strings and character spans.')
s=s.replace('Compare all four packages and five Tedd API modes on short inputs (4–16 UTF-16 code units) and long inputs (over 1,000). Both charts show every tested API; complete tables include time, confidence intervals, and allocations.', 'Compare matching throughput across short inputs and strings over 1,000 characters. Select a short-string pattern to see literal, question-mark, and multi-star workloads. Higher bars mean faster matching.')
s=s.replace('Chart throughput: higher is faster. Table time and allocations: lower is better. Select a short-string pattern to compare literal, question-mark, and multi-star workloads.', 'Each graph includes every tested library and API. Full results, confidence intervals, and construction costs are available in the linked reports.')
start=s.index('      <p class="benchmark-method">Each invocation checks 256 inputs')
end=s.index('      <div class="evidence-links">',start)
s=s[:start]+s[end:]
index.write_text(s,encoding='utf-8')
app=repo/'site/app.js'
s=app.read_text().replace('fetch("assets/package-comparison.json")','fetch("assets/package-comparison.json?v=2.2.0")')
s=s.replace('        const peak = 1000 / rows[0].meanNs;', '''        const peak = 1000 / rows[0].meanNs;
        const direct = rows.find((row) => row.library === "TeddDirectReused");
        const glob = rows.find((row) => row.library === "DotNetGlob");
        chart.querySelector("[data-direct-speedup]").textContent = `${(glob.meanNs / direct.meanNs).toFixed(2)}×`;''')
app.write_text(s,encoding='utf-8')
style=repo/'site/styles.css'
s=style.read_text()+'''\n.chart-summary { margin: 0; padding: 1rem 1.3rem; color: var(--muted); background: #fff; border: 1px solid var(--line); border-top: 0; font-size: .8rem; }
.chart-summary strong { color: var(--aqua-dark); font-variant-numeric: tabular-nums; }
'''
style.write_text(s,encoding='utf-8')
readme=repo/'README.md'
s=readme.read_text().replace('It includes the full tables, source revision, runtime and machine details, and confidence intervals.', 'Full timing tables, construction costs, and confidence intervals are available in linked reports; the graphs identify the measured source, runtime, and machine.')
s=s.replace('https://tedd.github.io/Tedd.WildcardMatch/', 'https://tedd.no/Tedd.WildcardMatch/')
readme.write_text(s,encoding='utf-8')
