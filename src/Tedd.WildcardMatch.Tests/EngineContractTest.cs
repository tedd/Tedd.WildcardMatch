using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;

namespace Tedd.WildcardMatchTests;

public class EngineContractTest
{
    private static string Reference(string pattern) =>
        "^" + Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", ".") + "$";

    public static IEnumerable<object[]> EdgeCases()
    {
        var cases = new (string Text, string Pattern, bool Expected)[]
        {
            ("", "", true), ("", "*", true), ("", "**", true), ("", "?", false),
            ("", "*?*", false), ("a", "", false), ("a", "?", true), ("ab", "?", false),
            ("abc", "abc", true), ("xabc", "abc", false), ("abcx", "abc", false),
            ("abc", "a*c", true), ("abc", "*b*", true), ("abc", "*d*", false),
            ("ab", "a*b", true), ("ab", "a**b", true), ("abc", "***?***?***?***", true),
            ("ab", "???*", false), ("aaab", "*aab", true), ("ababab", "*ab*ab", true),
            ("ababcd", "*a?c*e*", false), ("abcd", "*a?c*e*", false),
            ("abac", "*ab*ac", true), ("abaac", "*a?ac", false), ("mississippi", "m*iss*?pi", true),
            ("abefcdgiescdfimde", "ab*cd?i*de", true), ("abc", "*?*?*?*?*", false),
            ("[a].+$^{b}|()#", "[a].+$^{b}|()#", true), ("abc", "[abc]", false),
            ("\\x", "\\?", true), ("x", "\\*", false), ("\\abc", "\\*", true),
            ("\\", "\\", true), ("*", "?", true), ("?", "*", true),
            ("a\0b", "a?b", true), ("a\0b", "*\0*", true), ("\t\r\f #", "\t\r\f #", true),
            ("\n", "", true), ("a\n", "a", true), ("a\n", "?", true),
            ("\n", "?", false), ("\n", "*", true), ("\n\n", "*", false),
            ("a\nb", "*", false), ("a\r\nb", "*", false), ("a\r", "a?", true),
            ("a\r\n", "a?", true), ("a\n", "a?", false), ("a\n", "a\n", true),
            ("a\nb", "a\nb", true), ("a\nb\n", "*\n*", true),
            ("a\nb\nc", "*\n*", false), ("a\nb\nc", "*\n*\n*", true),
            ("\n\n", "\n", true), ("\n\n", "*\n", true),
            ("😀", "?", false), ("😀", "??", true), ("😀", "*", true),
            ("\uD800", "?", true), ("\uDC00", "?", true), ("\uD800x", "\uD800?", true),
            ("e\u0301", "?", false), ("e\u0301", "??", true), ("é", "e\u0301", false),
            ("a\u2028b", "*", true), ("a\u0085b", "*", true), ("a\u2029b", "*", true)
        };
        int caseId = 0;
        foreach (var c in cases)
        {
            foreach (bool regex in new[] { false, true })
                yield return new object[] { caseId, regex, c.Text, c.Pattern, c.Expected };
            caseId++;
        }
    }

    [Theory]
    [MemberData(nameof(EdgeCases))]
    public void EdgeCase_AllEntryPoints(int caseId, bool regex, string input, string pattern, bool expected)
    {
        Assert.True(expected == Regex.IsMatch(input, Reference(pattern)), $"Independent Regex oracle for edge case {caseId}");
        Assert.Equal(expected, regex ? WildcardMatchRegex.IsMatch(input, pattern) : WildcardMatch.IsMatch(input, pattern));
        Assert.Equal(expected, regex ? new WildcardMatchRegex(pattern).IsMatch(input) : new WildcardMatch(pattern).IsMatch(input));
        Assert.Equal(expected, regex ? input.IsWildcardMatchRegex(pattern) : input.IsWildcardMatch(pattern));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DeclaredOptionCombinations(bool regex)
    {
        var flags = new[] { WildcardOptions.IgnoreCase, WildcardOptions.Singleline, WildcardOptions.Compiled,
            WildcardOptions.RightToLeft, WildcardOptions.CultureInvariant };
        foreach (string culture in new[] { "", "en-US", "tr-TR", "az-Latn-AZ" })
        {
            CultureInfo original = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo(culture);
                for (int mask = 0; mask < 32; mask++)
                {
                    WildcardOptions options = WildcardOptions.None;
                    for (int bit = 0; bit < flags.Length; bit++) if ((mask & (1 << bit)) != 0) options |= flags[bit];
                    foreach (var pair in new[] { ("Iİıi", "*i?*"), ("x\ny\n", "*?*"), ("\n", "?"),
                        ("a\nb", "a?b"), ("ΣσςKk", "*σ*k"), ("ab", "a**?"), ("a\n", "a") })
                    {
                        bool expected = new Regex(Reference(pair.Item2), (RegexOptions)options).IsMatch(pair.Item1);
                        Assert.Equal(expected, regex ? WildcardMatchRegex.IsMatch(pair.Item1, pair.Item2, options) : WildcardMatch.IsMatch(pair.Item1, pair.Item2, options));
                        Assert.Equal(expected, regex ? new WildcardMatchRegex(pair.Item2, options).IsMatch(pair.Item1) : new WildcardMatch(pair.Item2, options).IsMatch(pair.Item1));
                    }
                }
            }
            finally { CultureInfo.CurrentCulture = original; }
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NumericRegexFlagsAndValidation(bool regex)
    {
        for (int bits = -1; bits <= 4096; bits++)
        {
            var options = (WildcardOptions)bits;
            Exception expected = Record.Exception(() => new Regex("^a.*$", (RegexOptions)bits));
            Exception actual = Record.Exception(() => { if (regex) new WildcardMatchRegex("a*", options); else new WildcardMatch("a*", options); });
            Assert.True(expected?.GetType() == actual?.GetType(), $"Regex option bits {bits}: expected {expected?.GetType().Name ?? "valid"}, actual {actual?.GetType().Name ?? "valid"}");
            if (expected is ArgumentException e) Assert.Equal(e.ParamName, ((ArgumentException)actual).ParamName);
            if (expected != null) continue;
            foreach (var pair in new[] { ("a\nb", "*b"), ("a\nb\nc", "b"), ("x\n\n", ""),
                ("a\nb", "a*b"), ("a\nb", "a\nb"), (" a#\t", " a#\t"), ("\r\n", "?"), ("\n\n", "*\n*"), ("ab", "a\vb"), ("a\vb", "a\vb") })
            {
                bool result = new Regex(Reference(pair.Item2), (RegexOptions)bits).IsMatch(pair.Item1);
                Assert.Equal(result, regex ? WildcardMatchRegex.IsMatch(pair.Item1, pair.Item2, options) : WildcardMatch.IsMatch(pair.Item1, pair.Item2, options));
            }
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NullValidationAndPrecedence(bool regex)
    {
        foreach (var pair in new[] { ((string)null, "*"), ("input", (string)null), ((string)null, (string)null) })
        {
            var e = Assert.Throws<ArgumentNullException>(() => Regex.IsMatch(pair.Item1, Reference(pair.Item2)));
            var actual = Assert.Throws<ArgumentNullException>(() => { if (regex) WildcardMatchRegex.IsMatch(pair.Item1, pair.Item2); else WildcardMatch.IsMatch(pair.Item1, pair.Item2); });
            Assert.Equal(e.ParamName, actual.ParamName);
        }
        Assert.Equal("str", Assert.Throws<ArgumentNullException>(() => { if (regex) new WildcardMatchRegex(null, (WildcardOptions)(-1), TimeSpan.Zero); else new WildcardMatch(null, (WildcardOptions)(-1), TimeSpan.Zero); }).ParamName);
        Assert.Equal("options", Assert.Throws<ArgumentOutOfRangeException>(() => { if (regex) new WildcardMatchRegex("*", (WildcardOptions)(-1), TimeSpan.Zero); else new WildcardMatch("*", (WildcardOptions)(-1), TimeSpan.Zero); }).ParamName);
        Assert.Throws<ArgumentNullException>(() => { if (regex) new WildcardMatchRegex("*").IsMatch(null); else new WildcardMatch("*").IsMatch(null); });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void TimeoutRangeAndExceptionPayload(bool regex)
    {
        foreach (TimeSpan timeout in new[] { TimeSpan.Zero, TimeSpan.FromTicks(-1), TimeSpan.FromMilliseconds(-2), TimeSpan.MaxValue,
            TimeSpan.FromMilliseconds(int.MaxValue - 1) + TimeSpan.FromTicks(1) })
            Assert.Equal("matchTimeout", Assert.Throws<ArgumentOutOfRangeException>(() => { if (regex) new WildcardMatchRegex("*", WildcardOptions.None, timeout); else new WildcardMatch("*", WildcardOptions.None, timeout); }).ParamName);
        foreach (TimeSpan timeout in new[] { Regex.InfiniteMatchTimeout, TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(int.MaxValue - 1) })
            Assert.True(regex ? new WildcardMatchRegex("*", WildcardOptions.None, timeout).IsMatch("abc") : new WildcardMatch("*", WildcardOptions.None, timeout).IsMatch("abc"));
        // Both engines face expensive failures, with different polynomial/exponential costs.
        string pattern = regex ? "*a*a*a*a*a*a*a*a*a*a*b*a*" : "*" + new string('a', 10000) + "b*";
        string input = new string('a', 200000) + (regex ? "b" : "");
        var limit = TimeSpan.FromMilliseconds(1);
        var error = Assert.Throws<RegexMatchTimeoutException>(() => { if (regex) new WildcardMatchRegex(pattern, WildcardOptions.None, limit).IsMatch(input); else new WildcardMatch(pattern, WildcardOptions.None, limit).IsMatch(input); });
        Assert.Equal(input, error.Input);
        Assert.Equal(Reference(pattern), error.Pattern);
        Assert.Equal(limit, error.MatchTimeout);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void InstanceCapturesCultureAndSupportsConcurrentReuse(bool regex)
    {
        CultureInfo original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("tr-TR");
            var direct = new WildcardMatch("I*?", WildcardOptions.IgnoreCase);
            var reference = new WildcardMatchRegex("I*?", WildcardOptions.IgnoreCase);
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            Parallel.For(0, 2000, i =>
            {
                string text = i % 2 == 0 ? "ıxyz" : "ixyz";
                bool expected = i % 2 == 0;
                Assert.Equal(expected, regex ? reference.IsMatch(text) : direct.IsMatch(text));
                Assert.Equal(Reference("I*?"), regex ? reference.WildcardRegex : direct.WildcardRegex);
            });
            Assert.True(WildcardMatch.IsMatch("ixyz", "I*?", true));
            Assert.False(WildcardMatch.IsMatch("ıxyz", "I*?", true));
        }
        finally { CultureInfo.CurrentCulture = original; }
    }

    [Fact]
    public void DirectEngineLargeAndAdversarialPatterns()
    {
        string text = new string('a', 1_000_000);
        Assert.True(WildcardMatch.IsMatch(text, "*"));
        Assert.True(WildcardMatch.IsMatch(text, text));
        Assert.True(WildcardMatch.IsMatch(text, new string('?', text.Length)));
        Assert.True(WildcardMatch.IsMatch(text, new string('*', 100000)));
        Assert.False(WildcardMatch.IsMatch(text, "*" + new string('a', 50000) + "b"));
        Assert.False(WildcardMatch.IsMatch(text, new string('?', text.Length + 1)));
        Assert.True(WildcardMatch.IsMatch(text, "*a*a*a*a*a*a*a*a*a*"));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BulkLiteralRunBoundaries(bool regex)
    {
        foreach (int length in new[] { 31, 32, 33, 127, 128, 129, 256, 1024 })
        {
            string literal = new string('a', length);
            string half = new string('a', length / 2);
            foreach (string pattern in new[] { literal, "*" + literal, literal + "*", half + "*" + half,
                "*" + literal + "*", "*" + new string('?', length / 2) + half, literal + "?",
                "a\n*" + literal, "*" + literal + "\n", literal + "\uD800*" })
                foreach (string input in new[] { literal, literal + "\n", literal + "b", "b" + literal,
                    literal + "\nb", "a\n" + literal, literal.ToUpperInvariant(), new string('a', length - 1), literal + "\uD800" })
                    foreach (WildcardOptions option in new[] { WildcardOptions.None, WildcardOptions.Singleline,
                        WildcardOptions.IgnoreCase, WildcardOptions.RightToLeft })
                    {
                        bool expected = Regex.IsMatch(input, Reference(pattern), (RegexOptions)option);
                        Assert.Equal(expected, regex ? WildcardMatchRegex.IsMatch(input, pattern, option) : WildcardMatch.IsMatch(input, pattern, option));
                        Assert.Equal(expected, regex ? new WildcardMatchRegex(pattern, option).IsMatch(input) : new WildcardMatch(pattern, option).IsMatch(input));
                    }
        }
    }

    [Fact]
    public void DirectDefaultCallsAllocateNoMatchBuffers()
    {
        var instance = new WildcardMatch("*aa??cc*ee*");
        for (int i = 0; i < 10000; i++) { instance.IsMatch("a*abbaabbccddee"); WildcardMatch.IsMatch("a*abbaabbccddee", "*aa??cc*ee*"); }
        long before = GC.GetAllocatedBytesForCurrentThread();
        bool result = false;
        for (int i = 0; i < 10000; i++) result ^= instance.IsMatch("a*abbaabbccddee") ^ WildcardMatch.IsMatch("a*abbaabbccddee", "*aa??cc*ee*");
        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        Assert.False(result);
        Assert.Equal(0, allocated);
    }
}
