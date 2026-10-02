using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Xunit;

namespace Tedd.WildcardMatchTests;

public class SpanTest
{
    private static string Reference(string pattern) =>
        "^" + Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", ".") + "$";

    [Fact]
    public void DefaultAndEmptySpansMatchEmptyText()
    {
        Assert.True(WildcardMatch.IsMatch(default(ReadOnlySpan<char>), default(ReadOnlySpan<char>)));
        Assert.True(new WildcardMatch(ReadOnlySpan<char>.Empty).IsMatch(ReadOnlySpan<char>.Empty));
        Assert.True(WildcardMatch.IsMatch(ReadOnlySpan<char>.Empty, "***"));
        Assert.False(WildcardMatch.IsMatch(ReadOnlySpan<char>.Empty, "?"));
        Assert.False(WildcardMatch.IsMatch("x".AsSpan(), ReadOnlySpan<char>.Empty));
        Assert.Throws<ArgumentNullException>(() => WildcardMatch.IsMatch(null, "*"));
        Assert.Throws<ArgumentNullException>(() => WildcardMatch.IsMatch("x", null));
        Assert.Throws<ArgumentNullException>(() => new WildcardMatch(null));
        Assert.Throws<ArgumentNullException>(() => new WildcardMatch("*").IsMatch(null));
    }

    [Fact]
    public void SliceBoundariesAgreeWithRegex()
    {
        foreach (int length in new[] { 0, 1, 2, 15, 16, 17, 31, 32, 33, 255, 256, 257, 1024 })
            foreach (int offset in new[] { 0, 1, 15, 16, 31, 32 })
                foreach (var options in new[] { WildcardOptions.None, WildcardOptions.Singleline, WildcardOptions.IgnoreCase, (WildcardOptions)2, (WildcardOptions)18 })
                    foreach (string input in new[] { new string('a', length), new string('a', length) + "\n", new string('a', length) + "\0\uD800\uDC00" })
                        foreach (string pattern in new[] { "*", "?", new string('?', length), "*" + new string('a', length) + "*", new string('a', length), "*A*", "*a?*" })
                        {
                            string textBuffer = new string('\n', offset) + input + "BAD\n";
                            string patternBuffer = new string('?', offset) + pattern + "BAD*";
                            ReadOnlySpan<char> text = textBuffer.AsSpan(offset, input.Length);
                            ReadOnlySpan<char> wildcard = patternBuffer.AsSpan(offset, pattern.Length);
                            bool expected = WildcardMatchRegex.IsMatch(input, pattern, options);
                            Assert.Equal(expected, WildcardMatch.IsMatch(text, wildcard, options));
                            Assert.Equal(expected, new WildcardMatch(wildcard, options).IsMatch(text));
                            Assert.Equal(expected, text.IsWildcardMatch(wildcard, options));
                        }
    }

    [Fact]
    public void StackArrayMemoryAndMutableSpanInputsAreBorrowed()
    {
        Span<char> text = stackalloc char[] { '#', 'A', 'b', 'c', '#', '\n' };
        Span<char> pattern = stackalloc char[] { '#', 'a', '?', '*', '#', '?' };
        Assert.True(text.Slice(1, 3).IsWildcardMatch(pattern.Slice(1, 3), true));
        Assert.True(WildcardMatch.IsMatch(text.Slice(1, 3), "A?*"));
        var matcher = new WildcardMatch("A?*");
        Assert.True(matcher.IsMatch(text.Slice(1, 3)));
        text[1] = 'Z';
        Assert.False(matcher.IsMatch(text.Slice(1, 3)));
        Assert.Equal("#Zbc#\n", text.ToString());
        char[] array = "xxAbcyy".ToCharArray();
        ReadOnlyMemory<char> memory = array.AsMemory(2, 3);
        Assert.True(WildcardMatch.IsMatch(memory.Span, "A?*"));
        Assert.True(array.AsSpan(2, 3).IsWildcardMatch("a?*", WildcardOptions.IgnoreCase));
    }

    [Fact]
    public void ConstructorsCopyPatternAndCaptureCulture()
    {
        CultureInfo previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("tr-TR");
            Span<char> pattern = stackalloc char[] { '#', 'I', '*', '#' };
            var simple = new WildcardMatch(pattern.Slice(1, 2));
            var cultured = new WildcardMatch(pattern.Slice(1, 2), WildcardOptions.IgnoreCase);
            var timed = new WildcardMatch(pattern.Slice(1, 2), WildcardOptions.IgnoreCase, TimeSpan.FromSeconds(1));
            pattern[1] = 'Z';
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            Assert.Equal("I*", simple.Wildcard);
            Assert.Equal("^I.*$", simple.WildcardRegex);
            Assert.True(simple.IsMatch("Input".AsSpan()));
            Assert.True(cultured.IsMatch("ıx".AsSpan()));
            Assert.False(cultured.IsMatch("ix".AsSpan()));
            Assert.True(timed.IsMatch("ıx".AsSpan()));
            Assert.False(timed.IsMatch("ix".AsSpan()));
        }
        finally { CultureInfo.CurrentCulture = previous; }
    }

    [Fact]
    public void InvalidOptionsAndTimeoutsRetainValidation()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => WildcardMatch.IsMatch("a".AsSpan(), "*".AsSpan(), (WildcardOptions)(-1)));
        Assert.Throws<ArgumentOutOfRangeException>(() => new WildcardMatch("*".AsSpan(), (WildcardOptions)(-1)));
        Assert.Throws<ArgumentOutOfRangeException>(() => new WildcardMatch("*".AsSpan(), WildcardOptions.None, TimeSpan.Zero));
        Assert.Throws<ArgumentOutOfRangeException>(() => new WildcardMatch("*".AsSpan(), WildcardOptions.None, TimeSpan.FromMilliseconds(-2)));
    }

    [Fact]
    public void SpanTimeoutDiagnosticsContainOnlyBorrowedSlices()
    {
        string pattern = "*" + new string('a', 10000) + "b*";
        string input = new string('a', 200000);
        string textBuffer = "GUARD" + input + "GUARD";
        string patternBuffer = "GUARD" + pattern + "GUARD";
        var limit = TimeSpan.FromMilliseconds(1);
        var matcher = new WildcardMatch(patternBuffer.AsSpan(5, pattern.Length), WildcardOptions.IgnoreCase, limit);
        var error = Assert.Throws<RegexMatchTimeoutException>(() => matcher.IsMatch(textBuffer.AsSpan(5, input.Length)));
        Assert.Equal(input, error.Input);
        Assert.Equal(Reference(pattern), error.Pattern);
        Assert.Equal(limit, error.MatchTimeout);
    }

    [Fact]
    public void SuccessfulSpanCallsAllocateNoMatchBuffers()
    {
        var matcher = new WildcardMatch("*aa??cc*ee*", WildcardOptions.None, TimeSpan.FromSeconds(1));
        Span<char> text = stackalloc char[] { '#', 'a', 'a', 'b', 'b', 'c', 'c', 'd', 'd', 'e', 'e', '#' };
        Span<char> pattern = stackalloc char[] { '#', '*', 'a', 'a', '?', '?', 'c', 'c', '*', 'e', 'e', '*', '#' };
        for (int i = 0; i < 10000; i++)
        {
            matcher.IsMatch(text.Slice(1, 10));
            WildcardMatch.IsMatch(text.Slice(1, 10), pattern.Slice(1, 11));
            text.Slice(1, 10).IsWildcardMatch(pattern.Slice(1, 11));
        }
        long before = GC.GetAllocatedBytesForCurrentThread();
        bool result = false;
        for (int i = 0; i < 10000; i++)
            result ^= matcher.IsMatch(text.Slice(1, 10)) ^ WildcardMatch.IsMatch(text.Slice(1, 10), pattern.Slice(1, 11)) ^ text.Slice(1, 10).IsWildcardMatch(pattern.Slice(1, 11));
        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        Assert.False(result);
        Assert.Equal(0, allocated);
    }

    [Fact]
    public void VerticalTabNormalizationUsesOnlyInitializedScratch()
    {
        foreach (int length in new[] { 0, 1, 255, 256, 257, 511, 512, 513, 4096 })
            foreach (string pattern in new[] { new string('\v', length), "a" + new string('\v', length) + "?*", new string('a', length) + "\v*", "\v" + new string('?', length) + "\v" })
                foreach (string input in new[] { "", "a", "aZ", new string('a', length), pattern, "a\vb" })
                    foreach (int flags in new[] { 32, 33, 48, 50 })
                    {
                        var options = (WildcardOptions)flags;
                        bool expected = WildcardMatchRegex.IsMatch(input, pattern, options);
                        Assert.Equal(expected, WildcardMatch.IsMatch(input.AsSpan(), pattern.AsSpan(), options));
                        Assert.Equal(expected, new WildcardMatch(pattern.AsSpan(), options).IsMatch(input.AsSpan()));
                    }
    }
}
