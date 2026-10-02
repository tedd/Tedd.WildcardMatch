using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;

namespace Tedd.WildcardMatchTests;

public class SpanContractTest
{
    private static string Reference(string pattern) =>
        "^" + Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", ".") + "$";

    [Theory]
    [InlineData(145721, "")]
    [InlineData(912743, "en-US")]
    [InlineData(318721, "tr-TR")]
    [InlineData(782113, "az-Latn-AZ")]
    public void SeededGuardedBufferFuzz(int seed, string culture)
    {
        CultureInfo original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo(culture);
            var random = new Random(seed);
            const string alphabet = "abcABCiIİıΣσςKſ\\[](){}.+^$|# \t\r\n\v\f\u0085\u2028\u2029\0\uD800\uDC00\uFFFF";
            for (int trial = 0; trial < 3000; trial++)
            {
                string input = new string(Enumerable.Range(0, random.Next(65))
                    .Select(_ => alphabet[random.Next(alphabet.Length)]).ToArray());
                string pattern = string.Concat(input.Select(c => random.Next(7) == 0 ? "*" : random.Next(4) == 0 ? "?" : c.ToString()));
                if (trial % 3 == 0) pattern += "Z"; // Near misses as well as guaranteed matches.
                if (trial % 5 == 0) pattern = "*" + pattern + "*";
                if (trial % 7 == 0) pattern = new string('\v', 17) + pattern;
                int flags = (random.Next(2) * 1) | (random.Next(2) * 16) |
                    (random.Next(2) * 2) | (random.Next(2) * 32) | (random.Next(2) * 512);
#if NET11_0_OR_GREATER
                flags |= random.Next(2) * 0x800;
#endif
                var options = (WildcardOptions)flags;
                // AnyNewLine in .NET 11 cannot be combined with NonBacktracking.
                // Its short, high-entropy corpus uses the bounded interpreter oracle.
                RegexOptions oracleOptions = (flags & 0x800) != 0 ? (RegexOptions)flags : (RegexOptions)flags | RegexOptions.NonBacktracking;
                bool expected = new Regex(Reference(pattern), oracleOptions,
                    TimeSpan.FromSeconds(5)).IsMatch(input);
                int textOffset = random.Next(1, 65), patternOffset = random.Next(1, 65);
                char[] textBuffer = (new string('\n', textOffset) + input + "Z\0\uD800\n").ToCharArray();
                char[] patternBuffer = (new string('?', patternOffset) + pattern + "*Z?").ToCharArray();
                char[] textBefore = (char[])textBuffer.Clone(), patternBefore = (char[])patternBuffer.Clone();
                Span<char> text = textBuffer.AsSpan(textOffset, input.Length);
                Span<char> wildcard = patternBuffer.AsSpan(patternOffset, pattern.Length);
                ReadOnlySpan<char> readOnly = text;
                var prepared = new WildcardMatch(wildcard, options, TimeSpan.FromSeconds(5));
                Assert.True(expected == WildcardMatch.IsMatch(text, wildcard, options), $"seed={seed}, trial={trial}, flags={flags}");
                Assert.Equal(expected, prepared.IsMatch(text));
                Assert.Equal(expected, prepared.IsMatch(input));
                Assert.Equal(expected, text.IsWildcardMatch(wildcard, options));
                Assert.Equal(expected, readOnly.IsWildcardMatch(wildcard, options));
                Assert.Equal(expected, WildcardMatch.IsMatch(text, wildcard, options | WildcardOptions.RightToLeft));
                Assert.Equal(textBefore, textBuffer);
                Assert.Equal(patternBefore, patternBuffer);
            }
        }
        finally { CultureInfo.CurrentCulture = original; }
    }

    [Fact]
    public void OverlappingSlicesNeverMutateTheirSharedBuffer()
    {
        foreach (string contents in new[] { "a*a", "a?a", "***", "\n*\n", "I?ı", "\0?\0", "\uD800?\uDC00" })
            for (int inputStart = 0; inputStart <= contents.Length; inputStart++)
                for (int patternStart = 0; patternStart <= contents.Length; patternStart++)
                    for (int inputLength = 0; inputLength <= contents.Length - inputStart; inputLength++)
                        for (int patternLength = 0; patternLength <= contents.Length - patternStart; patternLength++)
                            foreach (var options in new[] { WildcardOptions.None, WildcardOptions.IgnoreCase, WildcardOptions.Singleline })
                            {
                                char[] buffer = contents.ToCharArray();
                                Span<char> input = buffer.AsSpan(inputStart, inputLength);
                                Span<char> pattern = buffer.AsSpan(patternStart, patternLength);
                                bool expected = WildcardMatchRegex.IsMatch(input.ToString(), pattern.ToString(), options);
                                Assert.Equal(expected, WildcardMatch.IsMatch(input, pattern, options));
                                Assert.Equal(expected, new WildcardMatch(pattern, options).IsMatch(input));
                                Assert.Equal(contents, new string(buffer));
                            }
    }

    [Fact]
    public void ConcurrentReuseCapturesCultureAndKeepsMatchStateLocal()
    {
        CultureInfo original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("tr-TR");
            var matcher = new WildcardMatch("I*?".AsSpan(), WildcardOptions.IgnoreCase, TimeSpan.FromSeconds(5));
            var regex = new WildcardMatchRegex("I*?", WildcardOptions.IgnoreCase, TimeSpan.FromSeconds(5));
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            Parallel.For(0, 4000, new ParallelOptions { MaxDegreeOfParallelism = 4 }, i =>
            {
                string input = i % 2 == 0 ? "ıxyz" : "ixyz";
                char[] buffer = ("BAD" + input + "BAD\n").ToCharArray();
                bool expected = i % 2 == 0;
                Assert.Equal(expected, matcher.IsMatch(buffer.AsSpan(3, input.Length)));
                Assert.Equal(expected, matcher.IsMatch(input));
                Assert.Equal(expected, regex.IsMatch(input));
                Assert.Equal(Reference("I*?"), matcher.WildcardRegex);
            });
        }
        finally { CultureInfo.CurrentCulture = original; }
    }

    [Fact]
    public void ConcurrentNormalizationDoesNotLeakOtherPooledPatterns()
    {
        Parallel.For(0, 3000, new ParallelOptions { MaxDegreeOfParallelism = 4 }, i =>
        {
            string marker = i.ToString(CultureInfo.InvariantCulture);
            string pattern = "\v" + marker + new string('\v', 257 + i % 1024) + "?*";
            string input = i % 2 == 0 ? marker + "Ztail" : "BAD" + marker + "Ztail";
            var options = (WildcardOptions)(32 | 1 | (i % 3 == 0 ? 16 : 0));
            bool expected = WildcardMatchRegex.IsMatch(input, pattern, options);
            Assert.Equal(expected, WildcardMatch.IsMatch(input.AsSpan(), pattern.AsSpan(), options));
            Assert.Equal(expected, new WildcardMatch(pattern.AsSpan(), options).IsMatch(input.AsSpan()));
        });
    }

    [Theory]
    [InlineData(65535)]
    [InlineData(65536)]
    [InlineData(1000000)]
    public void LargeSlicesCoverLiteralQuestionStarAndAnchorPaths(int length)
    {
        string literal = new string('a', length);
        string[] patterns = { literal, new string('?', length), "*", new string('*', 100000),
            "*" + literal, literal + "*", "*a*a*a*a*a*", new string('?', length + 1),
            "*" + new string('a', length / 2) + "b" };
        // Explicit outcomes avoid constructing a million-state Regex automaton.
        bool[][] outcomes = {
            new[] { true, true, true, true, true, true, true, false, false },
            new[] { true, true, true, true, true, true, true, false, false },
            new[] { false, false, true, true, false, true, true, false, false }
        };
        string[] inputs = { literal, literal + "\n", literal + "\0\uD800" };
        for (int inputIndex = 0; inputIndex < inputs.Length; inputIndex++)
            for (int patternIndex = 0; patternIndex < patterns.Length; patternIndex++)
            {
                string input = inputs[inputIndex], pattern = patterns[patternIndex];
                bool expected = outcomes[inputIndex][patternIndex];
                string textBuffer = "BAD\n" + input + "BAD\n";
                string patternBuffer = "BAD?" + pattern + "BAD*";
                var text = textBuffer.AsSpan(4, input.Length);
                var wildcard = patternBuffer.AsSpan(4, pattern.Length);
                Assert.Equal(expected, WildcardMatch.IsMatch(text, wildcard));
                Assert.Equal(expected, new WildcardMatch(wildcard).IsMatch(text));
            }
    }

    [Fact]
    public void TimeoutRangePrecedenceAndPostTimeoutReuse()
    {
        foreach (var timeout in new[] { TimeSpan.Zero, TimeSpan.FromTicks(-1), TimeSpan.MaxValue,
            TimeSpan.FromMilliseconds(-2), TimeSpan.FromMilliseconds(int.MaxValue - 1) + TimeSpan.FromTicks(1) })
            Assert.Equal("matchTimeout", Assert.Throws<ArgumentOutOfRangeException>(() =>
                new WildcardMatch("*".AsSpan(), WildcardOptions.None, timeout)).ParamName);
        Assert.Equal("options", Assert.Throws<ArgumentOutOfRangeException>(() =>
            new WildcardMatch("*".AsSpan(), (WildcardOptions)(-1), TimeSpan.Zero)).ParamName);
        foreach (var timeout in new[] { Regex.InfiniteMatchTimeout, TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(int.MaxValue - 1) })
            Assert.True(new WildcardMatch("*".AsSpan(), WildcardOptions.None, timeout).IsMatch("abc".AsSpan()));
        string pattern = "*" + new string('a', 10000) + "b*";
        var matcher = new WildcardMatch(pattern.AsSpan(), WildcardOptions.IgnoreCase, TimeSpan.FromMilliseconds(20));
        string adversarial = new string('a', 200000);
        Assert.Throws<RegexMatchTimeoutException>(() => matcher.IsMatch(adversarial.AsSpan()));
        Assert.False(matcher.IsMatch(ReadOnlySpan<char>.Empty));
        Assert.True(matcher.IsMatch((new string('a', 10000) + "b").AsSpan()));
        Assert.Throws<RegexMatchTimeoutException>(() => matcher.IsMatch(adversarial.AsSpan()));
    }
}
