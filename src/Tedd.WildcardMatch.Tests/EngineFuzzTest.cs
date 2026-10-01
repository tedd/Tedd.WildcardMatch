using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace Tedd.WildcardMatchTests;

public class EngineFuzzTest
{
    private static string Reference(string pattern) =>
        "^" + Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", ".") + "$";

    // Independent O(inputLength * patternLength) dynamic-programming oracle.
    // No latest-star retries, suffix trimming, Regex, or production helper calls.
    private static bool Oracle(string input, string pattern, bool singleline)
    {
        bool Full(int end)
        {
            var previous = new bool[end + 1];
            previous[0] = true;
            foreach (char token in pattern)
            {
                var next = new bool[end + 1];
                if (token == '*') next[0] = previous[0];
                for (int i = 1; i <= end; i++)
                    next[i] = token == '*' ? previous[i] || (next[i - 1] && (singleline || input[i - 1] != '\n')) :
                        previous[i - 1] && (token == '?' ? singleline || input[i - 1] != '\n' : token == input[i - 1]);
                previous = next;
            }
            return previous[end];
        }
        return Full(input.Length) || (input.EndsWith("\n", StringComparison.Ordinal) && Full(input.Length - 1));
    }

    private static IEnumerable<string> Words(string alphabet, int max)
    {
        yield return "";
        var level = new List<string> { "" };
        for (int length = 1; length <= max; length++)
        {
            level = level.SelectMany(word => alphabet.Select(c => word + c)).ToList();
            foreach (string word in level) yield return word;
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ExhaustiveShortPatternsAgreeWithIndependentOracle(bool singleline)
    {
        string[] inputs = Words("ab\n", 4).ToArray();
        var option = singleline ? WildcardOptions.Singleline : WildcardOptions.None;
        foreach (string pattern in Words("ab*?\n", 5))
        {
            var direct = new WildcardMatch(pattern, option);
            var regex = new WildcardMatchRegex(pattern, option);
            foreach (string input in inputs)
            {
                bool expected = Oracle(input, pattern, singleline);
                Assert.Equal(expected, direct.IsMatch(input));
                Assert.Equal(expected, regex.IsMatch(input));
                Assert.Equal(expected, WildcardMatch.IsMatch(input, pattern, option));
            }
        }
    }

    [Theory]
    [InlineData(19373, "")]
    [InlineData(92741, "en-US")]
    [InlineData(38417, "tr-TR")]
    [InlineData(68129, "az-Latn-AZ")]
    public void SeededUnicodeFuzzAcrossAllEntryPoints(int seed, string culture)
    {
        CultureInfo previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo(culture);
            var random = new Random(seed);
            const string alphabet = "abABiIİı*?\\[](){}.+^$|# \t\r\n\v\f\u0085\u2028\u2029\0ÅßΣσςKſ\uD800\uDC00\uFFFF";
            bool unicodeNewlines = Enum.IsDefined(typeof(RegexOptions), 0x800);
            for (int trial = 0; trial < 15000; trial++)
            {
                char[] letters = new char[random.Next(0, 40)];
                for (int i = 0; i < letters.Length; i++) letters[i] = trial % 7 == 0 ? (char)random.Next(65536) : alphabet[random.Next(alphabet.Length)];
                string input = new string(letters);
                var tokens = new List<char>();
                // Include many successful/near-successful patterns, not just random failures.
                if (trial % 3 != 0)
                {
                    foreach (char letter in letters)
                    {
                        if (random.Next(5) == 0) tokens.Add('*');
                        tokens.Add(random.Next(4) == 0 ? '?' : letter);
                    }
                    if (random.Next(2) == 0) tokens.Add('*');
                    if (trial % 3 == 2 && tokens.Count > 0) tokens[random.Next(tokens.Count)] = alphabet[random.Next(alphabet.Length)];
                }
                else for (int i = random.Next(30); i > 0; i--) tokens.Add(alphabet[random.Next(alphabet.Length)]);
                string pattern = new string(tokens.ToArray());
                int flags = (random.Next(2) != 0 ? 1 : 0) | (random.Next(2) != 0 ? 16 : 0) |
                    (random.Next(2) != 0 ? 64 : 0) | (random.Next(2) != 0 ? 512 : 0) | (random.Next(4) == 0 ? 2 : 0) |
                    (random.Next(4) == 0 ? 32 : 0);
                if (unicodeNewlines && random.Next(2) == 0) flags |= 0x800;
                var options = (WildcardOptions)flags;
                bool expected = new Regex(Reference(pattern), (RegexOptions)flags, TimeSpan.FromSeconds(2)).IsMatch(input);
                bool direct = WildcardMatch.IsMatch(input, pattern, options);
                Assert.True(expected == direct, $"seed={seed}, trial={trial}, flags={flags}, input={Escape(input)}, pattern={Escape(pattern)}");
                Assert.Equal(expected, WildcardMatchRegex.IsMatch(input, pattern, options));
                Assert.Equal(expected, new WildcardMatch(pattern, options).IsMatch(input));
                Assert.Equal(expected, new WildcardMatchRegex(pattern, options).IsMatch(input));
                bool insensitive = (flags & 1) != 0;
                bool basic = Regex.IsMatch(input, Reference(pattern), insensitive ? RegexOptions.IgnoreCase : RegexOptions.None);
                Assert.Equal(basic, WildcardMatch.IsMatch(input, pattern, insensitive));
                Assert.Equal(basic, WildcardMatchRegex.IsMatch(input, pattern, insensitive));
                Assert.Equal(basic, input.IsWildcardMatch(pattern, insensitive));
                Assert.Equal(basic, input.IsWildcardMatchRegex(pattern, insensitive));
                if (flags == 0 || flags == 16) Assert.Equal(expected, Oracle(input, pattern, flags == 16));
            }
        }
        finally { CultureInfo.CurrentCulture = previous; }
    }

    [Theory]
    [InlineData(17293)]
    [InlineData(97131)]
    public void LongSegmentAndRetryFuzz(int seed)
    {
        var random = new Random(seed);
        for (int trial = 0; trial < 5000; trial++)
        {
            string input = new string(Enumerable.Range(0, random.Next(40, 300)).Select(_ => "ab\n"[random.Next(3)]).ToArray());
            string pattern = new string(Enumerable.Range(0, random.Next(20, 80)).Select(_ => "ab*?\n"[random.Next(5)]).ToArray());
            bool singleline = random.Next(2) != 0;
            var options = singleline ? WildcardOptions.Singleline : WildcardOptions.None;
            bool expected = Oracle(input, pattern, singleline);
            Assert.Equal(expected, WildcardMatch.IsMatch(input, pattern, options));
            // Keep the Regex oracle bounded on arbitrary long star runs. Interpreter
            // backtracking can time out independently of matching correctness.
            Assert.Equal(expected, new WildcardMatchRegex(pattern, options | (WildcardOptions)RegexOptions.NonBacktracking, TimeSpan.FromSeconds(2)).IsMatch(input));
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("en-US")]
    [InlineData("tr-TR")]
    [InlineData("az-Latn-AZ")]
    public void EveryUtf16CaseMappingAgreesWithRegex(string culture)
    {
        CultureInfo previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo(culture);
            foreach (int code in Enumerable.Range(0, 65536))
            {
                char c = (char)code;
                if (c == '*' || c == '?') continue;
                string pattern = c.ToString();
                var regex = new WildcardMatchRegex(pattern, WildcardOptions.IgnoreCase);
                foreach (char other in new[] { c, char.ToLowerInvariant(c), char.ToUpperInvariant(c), char.ToLower(c), char.ToUpper(c) })
                {
                    string input = other.ToString();
                    Assert.True(regex.IsMatch(input) == WildcardMatch.IsMatch(input, pattern, true), $"culture={culture}, pattern=U+{code:X4}, input=U+{(int)other:X4}");
                }
            }
            foreach (char c in "IiİıKkKSsſΣσς")
                foreach (char other in "IiİıKkKSsſΣσς")
                    Assert.Equal(WildcardMatchRegex.IsMatch(other.ToString(), c.ToString(), true), WildcardMatch.IsMatch(other.ToString(), c.ToString(), true));
        }
        finally { CultureInfo.CurrentCulture = previous; }
    }

    private static string Escape(string value) => string.Concat(value.Select(c => $"\\u{(int)c:X4}"));

#if NET11_0_OR_GREATER
    [Fact]
    public void UnicodeLineAnchorsExhaustive()
    {
        string[] inputs = Words("a\r\n", 4).ToArray();
        foreach (int flags in new[] { 0x800, 0x802, 0x810, 0x812 })
            foreach (string pattern in Words("a\r\n*?", 4))
            {
                var direct = new WildcardMatch(pattern, (WildcardOptions)flags);
                var regex = new WildcardMatchRegex(pattern, (WildcardOptions)flags);
                foreach (string input in inputs)
                    Assert.True(regex.IsMatch(input) == direct.IsMatch(input), $"flags={flags}, input={Escape(input)}, pattern={Escape(pattern)}");
            }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void EveryUnicodeNewlineSequence(bool regex)
    {
        foreach (string newline in new[] { "\r\n", "\r", "\n", "\v", "\f", "\u0085", "\u2028", "\u2029" })
            foreach (string input in new[] { "", newline, "a" + newline, "a" + newline + "b", newline + "a" + newline, newline + newline })
                foreach (string pattern in new[] { "", "*", "?", "??", "*\r", "*\n", "\r", "a", "*a", "a*", "*a*", "a?b", "a" + newline + "b", "*" + newline + "*" })
                    foreach (int flags in new[] { 0x800, 0x802, 0x810, 0x812, 0x840, 0x842, 0x850, 0x852 })
                    {
                        bool expected = Regex.IsMatch(input, Reference(pattern), (RegexOptions)flags);
                        bool actual = regex ? WildcardMatchRegex.IsMatch(input, pattern, (WildcardOptions)flags) : WildcardMatch.IsMatch(input, pattern, (WildcardOptions)flags);
                        Assert.True(expected == actual, $"flags={flags}, input={Escape(input)}, pattern={Escape(pattern)}");
                    }
    }
#endif
}
