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

`WildcardMatch` uses a direct iterative engine with constant match-state memory. `WildcardMatchRegex` provides string matching through .NET Regex:

```csharp
bool direct = WildcardMatch.IsMatch("report-01.txt", "report-??.txt");
bool regex = WildcardMatchRegex.IsMatch("report-01.txt", "report-??.txt");
```

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
    WildcardOptions.CultureInvariant,
    TimeSpan.FromMilliseconds(100));

bool first = matcher.IsMatch("report-01.txt");
bool second = matcher.IsMatch("report-summary.txt");
Console.WriteLine(matcher.WildcardRegex); // ^report-.*\.txt$
```

Match slices of strings, character arrays or stack buffers with `ReadOnlySpan<char>`:

```csharp
ReadOnlySpan<char> input = "[report-01.txt]".AsSpan(1, 13);
ReadOnlySpan<char> pattern = "report-??.txt".AsSpan();

bool first = WildcardMatch.IsMatch(input, pattern);
bool second = input.IsWildcardMatch(pattern);
bool third = matcher.IsMatch(input);
```

Static span calls borrow both slices; reusable matchers borrow the input for each call. Ordinary matching does not materialize strings. `Span<char>` converts to `ReadOnlySpan<char>` and supports the extension method directly; `ReadOnlyMemory<char>` callers use `.Span`. The caller must keep the underlying buffers stable during a match. Default spans represent empty text.

Constructors accept span patterns and copy them once into owned strings. Subsequent changes to the source buffer do not change the matcher. A timeout creates strings for the exception's input and pattern diagnostics. On .NET 11, numeric `IgnorePatternWhitespace` options with vertical tabs require normalization scratch: bounded stack storage for small patterns and pooled storage for larger ones.

## Matching behavior

The direct engine interprets the wildcard pattern itself. The Regex engine escapes literal characters, translates `*` to `.*` and `?` to `.`, and applies `^` / `$` anchors. Both engines use the same matching semantics. Use stars at both ends for substring matching. Backslashes are literal, and `?` consumes one UTF-16 code unit, including an isolated surrogate.

`WildcardRegex` exposes the equivalent Regex expression on either engine; the direct engine generates it lazily. Ignoring case uses Regex-compatible Unicode lowercase groups and culture rules. Instances capture the construction-time culture; static calls use the current culture.

.NET's `$` anchor can match immediately before a final `\n`. Wildcards exclude `\n` by default; `Singleline` permits line feeds. Slashes and bracket expressions have no special glob semantics.

| Option | Behavior |
| --- | --- |
| `None` | Case-sensitive matching. |
| `IgnoreCase` | Case-insensitive matching using regex culture rules. |
| `CultureInvariant` | Culture-independent regex casing. |
| `Singleline` | Wildcards can consume line feeds. |
| `Compiled` | Compile `WildcardMatchRegex`; the direct engine accepts the flag without extra compilation. |
| `RightToLeft` | Reverse Regex execution; the direct engine preserves the Boolean result using its forward algorithm. |

The default timeout is infinite. For untrusted patterns, use the instance constructor with an explicit timeout and handle `RegexMatchTimeoutException`.

## Performance

Direct static calls avoid pattern translation and Regex cache lookup. Reusable direct instances retain the pattern, classification, and case behavior. Ordinary matching uses constant auxiliary memory; star retries have polynomial worst-case work, so a timeout remains useful for large adversarial inputs.

`WildcardMatchRegex` static calls translate the pattern and use the runtime Regex cache. Its instances retain a Regex; `Compiled` increases construction cost and may improve repeated matching. Choose the engine according to the workload and pattern lifetime.

The [published comparison](https://tedd.github.io/Tedd.WildcardMatch/#benchmarks) measures the Regex-backed implementation at its recorded source revision against FastWildcard 3.1.0, WildcardMatch 1.0.7, and DotNet.Glob 3.1.3 on .NET 10. It covers literal, question-mark, multi-star, and long-text fixtures, with both matches and misses. The current harness compares direct static/reused calls and compiled `WildcardMatchRegex`. An independent dynamic-programming oracle validates every measured input. Reusable-pattern construction is measured separately.

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
