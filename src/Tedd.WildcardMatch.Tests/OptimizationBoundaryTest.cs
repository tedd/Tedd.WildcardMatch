using System;
using System.Globalization;
using Xunit;

namespace Tedd.WildcardMatchTests;

public class OptimizationBoundaryTest
{
    [Fact]
    public void LiteralSegmentSearchPreservesOffsetsTailsAndNewlineConsumption()
    {
        foreach (int length in new[] { 1, 2, 13, 14, 15, 16, 17, 31, 32, 33, 127, 128, 129 })
        foreach (int offset in new[] { 0, 1, 15, 16, 17, 31, 32, 33, 255 })
        foreach (WildcardOptions options in new[] { WildcardOptions.None, WildcardOptions.Singleline })
        {
            string literal = new string('a', length - 1) + "b";
            foreach (string pattern in new[] { "*" + literal + "*", "head*" + literal + "?*tail", "*" + literal + "*\n*end" })
            foreach (string input in new[]
            {
                new string('x', offset) + literal,
                "head" + new string('x', offset) + literal + "Qtail",
                "head" + new string('x', offset) + "\n" + literal + "Qtail",
                new string('a', offset + length) + "c",
                new string('x', offset) + literal + "\nend",
                new string('x', offset) + literal + "\n",
                new string('x', offset) + literal + "\ud800\udc00"
            })
            {
                bool expected = new WildcardMatchRegex(pattern, options).IsMatch(input);
                Assert.Equal(expected, WildcardMatch.IsMatch(input, pattern, options));
                Assert.Equal(expected, new WildcardMatch(pattern, options).IsMatch(input));
            }
        }
    }

    [Fact]
    public void AsciiLiteralSpecializationRetainsUnicodeAndCultureFallbacks()
    {
        CultureInfo saved = CultureInfo.CurrentCulture;
        try
        {
            foreach (string culture in new[] { "", "en-US", "tr-TR", "az-Latn-AZ" })
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);
                foreach (int length in new[] { 0, 1, 15, 16, 17, 31, 32, 33, 1024 })
                foreach (WildcardOptions options in new[] { WildcardOptions.IgnoreCase, WildcardOptions.IgnoreCase | WildcardOptions.CultureInvariant })
                {
                    string prefix = new string('a', length);
                    foreach (string patternTail in new[] { "I", "K", "S", "[", "\\", "\u0130", "\u0131", "\u212a", "\u017f", "\ud800" })
                    foreach (string inputTail in new[] { "i", "k", "s", "{", "\\", "\u0130", "\u0131", "\u212a", "\u017f", "\ud800" })
                    {
                        string pattern = prefix + patternTail;
                        foreach (string input in new[] { prefix.ToUpperInvariant() + inputTail, prefix.ToUpperInvariant() + inputTail + "\n" })
                        {
                            bool expected = new WildcardMatchRegex(pattern, options).IsMatch(input);
                            Assert.Equal(expected, WildcardMatch.IsMatch(input, pattern, options));
                            Assert.Equal(expected, new WildcardMatch(pattern, options).IsMatch(input));
                        }
                    }
                }
            }
        }
        finally { CultureInfo.CurrentCulture = saved; }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RepeatedMatcherRetainsPreparedPatternAndTimeoutContract(bool finiteTimeout)
    {
        TimeSpan timeout = finiteTimeout ? TimeSpan.FromSeconds(5) : TimeSpan.FromMilliseconds(-1);
        foreach (string pattern in new[] { "*" + new string('a', 1024) + "z", new string('?', 1024), new string('a', 1024) })
        foreach (WildcardOptions options in new[] { WildcardOptions.None, WildcardOptions.IgnoreCase, WildcardOptions.Singleline, (WildcardOptions)2 })
        {
            var direct = new WildcardMatch(pattern, options, timeout);
            var regex = new WildcardMatchRegex(pattern, options, timeout);
            foreach (string input in new[] { new string('a', 1024), new string('A', 1024), new string('a', 1024) + "z", "\n", "" })
                for (int repetition = 0; repetition < 3; repetition++)
                    Assert.Equal(regex.IsMatch(input), direct.IsMatch(input));
        }
    }
}
