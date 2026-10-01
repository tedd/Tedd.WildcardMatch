using DotNet.Globbing;
using FastWildcard;

namespace Tedd.WildcardMatchBenchmark;

public sealed record Fixture(string Name, string Pattern, string[] Inputs);
public sealed record ComparisonCase(string Workload, string Library)
{
    public override string ToString() => $"{Workload}/{Library}";
}
public sealed record ValidationResult(string Workload, string Library, int Checked, int Failures,
    string? FirstInput, bool? Expected, bool? Actual);

public static class Comparison
{
    public const int BatchSize = 256;
    public static readonly string[] Libraries =
        ["TeddStatic", "TeddReused", "TeddCompiled", "FastWildcard", "WildcardMatch", "DotNetGlob"];
    public static readonly Fixture[] Fixtures =
    [
        Create("Literal", "report-2026.txt", ["report-2026.txt", "report-2025.txt", "xreport-2026.txt", "report-2026.txtx"]),
        Create("Simple", "report-??.txt", ["report-01.txt", "report-aa.txt", "report-1.txt", "report-001.txt"]),
        Create("MultiStar", "*a?c*e*", ["xabcyef", "abce", "abcd", "ababcd", "aZc-long-e-tail", "acex"]),
        Create("LongText", "*alpha*beta??*omega*", [
            new string('x', 512) + "alpha-middle-beta42-tail-omega" + new string('z', 512),
            new string('x', 512) + "alpha-middle-beta42-tail-delta" + new string('z', 512),
            "alpha-beta12-omega", "alpha-beta1-omega"])
    ];

    private static Fixture Create(string name, string pattern, string[] seeds) =>
        new(name, pattern, Enumerable.Range(0, BatchSize).Select(i => seeds[i % seeds.Length]).ToArray());

    public static Func<string, bool> CreateMatcher(string library, string pattern) => library switch
    {
        "TeddStatic" => input => global::Tedd.WildcardMatch.IsMatch(input, pattern),
        "TeddReused" => new global::Tedd.WildcardMatch(pattern).IsMatch,
        "TeddCompiled" => new global::Tedd.WildcardMatchRegex(pattern, global::Tedd.WildcardOptions.Compiled).IsMatch,
        "FastWildcard" => Fast(pattern),
        "WildcardMatch" => input => global::WildcardMatch.StringExtensions.WildcardMatch(pattern, input, false),
        "DotNetGlob" => Glob.Parse(pattern).IsMatch,
        _ => throw new ArgumentOutOfRangeException(nameof(library))
    };

    private static Func<string, bool> Fast(string pattern)
    {
        var settings = new MatchSettings { StringComparison = StringComparison.Ordinal };
        return input => global::FastWildcard.FastWildcard.IsMatch(input, pattern, settings);
    }

    // Independent dynamic-programming oracle for the common, case-sensitive ASCII syntax.
    // Corpus excludes path separators, line breaks and glob-specific metacharacters.
    public static bool ExpectedMatch(string input, string pattern)
    {
        var previous = new bool[input.Length + 1];
        previous[0] = true;
        foreach (char token in pattern)
        {
            var next = new bool[input.Length + 1];
            next[0] = token == '*' && previous[0];
            for (int i = 1; i <= input.Length; i++)
                next[i] = token == '*' ? previous[i] || next[i - 1]
                    : previous[i - 1] && (token == '?' || token == input[i - 1]);
            previous = next;
        }
        return previous[input.Length];
    }

    public static ValidationResult[] Validate()
    {
        var results = new List<ValidationResult>();
        foreach (var fixture in Fixtures)
        foreach (var library in Libraries)
        {
            var matcher = CreateMatcher(library, fixture.Pattern);
            int failures = 0;
            string? firstInput = null;
            bool? firstExpected = null, firstActual = null;
            foreach (string input in fixture.Inputs)
            {
                bool expected = ExpectedMatch(input, fixture.Pattern), actual = matcher(input);
                if (expected == actual) continue;
                failures++;
                if (firstInput is not null) continue;
                firstInput = input;
                firstExpected = expected;
                firstActual = actual;
            }
            if (library.StartsWith("Tedd", StringComparison.Ordinal) && failures > 0)
                throw new InvalidOperationException($"{library} failed {fixture.Name} correctness validation.");
            results.Add(new(fixture.Name, library, BatchSize, failures, firstInput, firstExpected, firstActual));
        }
        return results.ToArray();
    }

    public static IEnumerable<ComparisonCase> ValidCases() => Validate()
        .Where(row => row.Failures == 0).Select(row => new ComparisonCase(row.Workload, row.Library));
}
