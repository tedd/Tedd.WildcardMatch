# Tedd.WildcardMatch

Wildcard string matching for .NET Standard 2.1, .NET 10, and .NET 11. Patterns use `*` for zero or more characters and `?` for one UTF-16 code unit. Other characters are literal.

[NuGet](https://www.nuget.org/packages/Tedd.WildcardMatch) · [Website](https://tedd.github.io/Tedd.WildcardMatch/) · [Comparative benchmarks](src/Tedd.WildcardMatch.Benchmark/README.md)

[![NuGet](https://img.shields.io/nuget/v/Tedd.WildcardMatch)](https://www.nuget.org/packages/Tedd.WildcardMatch)
[![Build and test](https://github.com/tedd/Tedd.WildcardMatch/actions/workflows/nuget-publish.yml/badge.svg?branch=main)](https://github.com/tedd/Tedd.WildcardMatch/actions/workflows/nuget-publish.yml)

## Installation

```sh
dotnet add package Tedd.WildcardMatch
```

## Compatibility

The 2.x package includes `netstandard2.1`, `net10.0`, and `net11.0` assemblies. .NET 8 and 9 use the .NET Standard 2.1 assembly; .NET 10 and 11 use their native targets. .NET Framework is not supported.

Building requires the SDK in [global.json](global.json), currently [.NET 11 RC1](https://dotnet.microsoft.com/en-us/download/dotnet/11.0). Tests require .NET 8, 10, and 11 runtimes.

## Usage

```csharp
using Tedd;

bool first = "report-01.txt".IsWildcardMatch("report-??.txt"); // true

bool second = WildcardMatch.IsMatch(
    "REPORT-01.TXT", "report-??.txt",
    WildcardOptions.IgnoreCase | WildcardOptions.CultureInvariant); // true
```

Reuse an instance when a pattern is applied to multiple inputs:

```csharp
using System;
using Tedd;

var matcher = new WildcardMatch(
    "report-*.txt",
    WildcardOptions.Compiled | WildcardOptions.CultureInvariant,
    TimeSpan.FromMilliseconds(100));

bool first = matcher.IsMatch("report-01.txt");
bool second = matcher.IsMatch("report-summary.txt");
Console.WriteLine(matcher.WildcardRegex); // ^report-.*\.txt$
```

## Matching behavior

Patterns are escaped and translated to .NET regex expressions: `*` becomes `.*`, `?` becomes `.`, and `^` / `$` anchors are applied. Use stars at both ends for substring matching.

.NET's `$` anchor can match immediately before a final `\n`. Wildcards exclude `\n` by default; `Singleline` permits line feeds. Slashes and bracket expressions have no special glob semantics.

| Option | Behavior |
| --- | --- |
| `None` | Case-sensitive regex matching. |
| `IgnoreCase` | Case-insensitive matching using regex culture rules. |
| `CultureInvariant` | Culture-independent regex casing. |
| `Singleline` | Wildcards can consume line feeds. |
| `Compiled` | Compile regex execution; increases construction cost. |
| `RightToLeft` | Execute regex matching from right to left. |

The default timeout is infinite. For untrusted patterns, use the instance constructor with an explicit timeout and handle `RegexMatchTimeoutException`.

## Performance

Static calls translate the pattern on each invocation and use the runtime regex cache. Reusable instances retain the regex. Compiled instances incur additional construction cost; their matching benefit depends on the workload.

The [published comparison](https://tedd.github.io/Tedd.WildcardMatch/#benchmarks) measures matching and allocations against FastWildcard 3.1.0, WildcardMatch 1.0.7, and DotNet.Glob 3.1.3 on .NET 10. It covers literal, question-mark, multi-star, and long-text fixtures, with both matches and misses. An independent dynamic-programming oracle validates every measured input. Reusable-pattern construction is measured separately.

The website includes the full tables, source revision, runtime and machine details, and confidence intervals. Results describe the selected corpus and API lifetimes; they do not establish a universal library ranking. [Measurement data](site/assets/package-comparison.json) and [benchmark commands](src/Tedd.WildcardMatch.Benchmark/README.md) are included in the repository.

## Build and release

```sh
dotnet build src/Tedd.WildcardMatch.sln --configuration Release
dotnet test src/Tedd.WildcardMatch.Tests --configuration Release
```

The SDK is pinned in `global.json`. [nuget-publish.yml](.github/workflows/nuget-publish.yml) builds, tests .NET 8/10/11, validates the benchmark corpus, and packs on pushes to `main` and `deploy` and on pull requests. NuGet publication requires a push or manual run on `deploy`; see the [release procedure](NuGet%20Documentation.md).

The static website is in `site/`. Its [Pages workflow](.github/workflows/pages.yml) deploys exclusively from `deploy` when GitHub Pages is enabled with **GitHub Actions** as its build source.

## License

[GNU Lesser General Public License 2.1](LICENSE).
