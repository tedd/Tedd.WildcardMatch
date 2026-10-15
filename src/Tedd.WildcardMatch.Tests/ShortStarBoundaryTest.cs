using System;
using Xunit;

namespace Tedd.WildcardMatchTests;

public class ShortStarBoundaryTest
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BitmapPlansPreserveLineFeedContracts(bool singleline)
    {
        var options = singleline ? WildcardOptions.Singleline : WildcardOptions.None;
        foreach (string pattern in new[]
        {
            "*", "**", "?*?", "*a*b*c*", "a*b?", "*??a*b??", "*a\n*b?*",
            "*ab?cd*ef??gh*", "*??*a*??b?", "*\0*\ud800*\udc00*", "*\n*"
        })
        {
            var matcher = new WildcardMatch(pattern, options);
            foreach (string input in new[]
            {
                "", "\n", "\n\n", "abc", "abc\n", "abc\n\n", "a\nbc", "ab\nc",
                "ab\n", "abZ", "abZ\n", "a\nxxbZ", "a\nxxbZ\n", "abZcd-efQRgh",
                "\0\ud800\udc00", "\ud800\0a\udc00", new string('x', 31) + "abc\n",
                new string('x', 63) + "abc", "\n" + new string('x', 63) + "abc"
            })
                Check(matcher, input, pattern, options, singleline);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ShortStarPatternsPreserveSliceBoundsAndNewlines(bool singleline)
    {
        var options = singleline ? WildcardOptions.Singleline : WildcardOptions.None;
        foreach (string pattern in new[]
        {
            "*a?c*e*", "**a?c***e**", "*abcd*e*", "*???*e*", "*a??*e*", "*?bc*e*",
            "*a\0c*e*", "*\ud800?\udc00*\0*", "*a\nc*e*", "*a?c*\n*", "*a?c*ef*",
            "a?c*e*", "*a?c*e", "*ab*e*", "*abcde*e*"
        })
        {
            var matcher = new WildcardMatch(pattern, options);
            string seed = pattern.Replace("*", "").Replace('?', 'Z');
            foreach (int length in new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 15, 16, 31, 32, 33, 63 })
            {
                string input = length < seed.Length ? seed.Substring(0, length)
                    : seed + new string('x', length - seed.Length);
                foreach (string value in new[]
                {
                    input, input + "\n", input + "\n\n", "\n" + input,
                    input.Insert(input.Length / 2, "\n"), "e" + input + "e",
                    "aQcaRce" + input, input.Replace('e', 'x')
                })
                    Check(matcher, value, pattern, options, singleline);
            }
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RandomShortStarPatternsAgreeWithIndependentOracle(bool singleline)
    {
        var options = singleline ? WildcardOptions.Singleline : WildcardOptions.None;
        var random = new Random(731291);
        const string patternAlphabet = "ac?\0\u0100\ud800\udc00";
        const string inputAlphabet = "acexZ\0\n\u0100\ud800\udc00";
        for (int sample = 0; sample < 300; sample++)
        {
            int width = random.Next(2, 6);
            var chars = new char[width];
            for (int i = 0; i < width; i++) chars[i] = patternAlphabet[random.Next(patternAlphabet.Length)];
            string segment = new string(chars);
            char tail = inputAlphabet[random.Next(inputAlphabet.Length)];
            string pattern = "*" + segment + "*" + tail + "*";
            var matcher = new WildcardMatch(pattern, options);
            string positive = segment.Replace('?', 'Z') + tail;
            Check(matcher, positive, pattern, options, singleline);
            Check(matcher, positive + "\n", pattern, options, singleline);
            for (int input = 0; input < 24; input++)
            {
                chars = new char[random.Next(0, 41)];
                for (int i = 0; i < chars.Length; i++) chars[i] = inputAlphabet[random.Next(inputAlphabet.Length)];
                Check(matcher, new string(chars), pattern, options, singleline);
            }
        }
    }

    internal static void Check(WildcardMatch matcher, string input, string pattern, WildcardOptions options, bool singleline)
    {
        bool expected = Oracle(input, pattern, singleline);
        Assert.Equal(expected, matcher.IsMatch(input));
        Assert.Equal(expected, WildcardMatch.IsMatch(input, pattern, options));
        foreach (int offset in new[] { 1, 3 })
        {
            string container = new string('!', offset) + input + "?";
            ReadOnlySpan<char> slice = container.AsSpan(offset, input.Length);
            Assert.Equal(expected, matcher.IsMatch(slice));
            Assert.Equal(expected, WildcardMatch.IsMatch(slice, pattern.AsSpan(), options));
        }
    }

    private static bool Oracle(string input, string pattern, bool singleline)
    {
        bool Full(int end)
        {
            var previous = new bool[end + 1];
            previous[0] = true;
            foreach (char token in pattern)
            {
                var next = new bool[end + 1];
                next[0] = token == '*' && previous[0];
                for (int i = 1; i <= end; i++)
                    next[i] = token == '*' ? previous[i] || (next[i - 1] && (singleline || input[i - 1] != '\n'))
                        : previous[i - 1] && (token == '?' ? singleline || input[i - 1] != '\n' : token == input[i - 1]);
                previous = next;
            }
            return previous[end];
        }
        return Full(input.Length) || (input.EndsWith("\n", StringComparison.Ordinal) && Full(input.Length - 1));
    }
}
