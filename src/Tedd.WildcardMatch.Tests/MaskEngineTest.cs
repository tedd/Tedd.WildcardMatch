using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;

namespace Tedd.WildcardMatchTests;

public class MaskEngineTest
{
    private static Regex Reference(string pattern, WildcardOptions options) => new Regex(
        "^" + Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", ".") + "$",
        (RegexOptions)options, TimeSpan.FromSeconds(5));

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FixedMasksCoverEveryWidthLaneAndExactArrayEnd(bool singleline)
    {
        var options = singleline ? WildcardOptions.Singleline : WildcardOptions.None;
        foreach (int length in Enumerable.Range(0, 65).Concat(new[] { 127, 128, 129, 255, 256, 257, 511, 512, 513 }))
        foreach (int offset in new[] { 0, 1, 7, 8, 15, 16, 31 })
        {
            string pattern = new string(Enumerable.Range(0, length).Select(i => i % 3 == 0 ? '?' : (char)(0x410 + i % 32)).ToArray());
            char[] original = pattern.Select(c => c == '?' ? '\ud800' : c).ToArray();
            var reference = Reference(pattern, options);
            var prepared = new WildcardMatch(pattern, options);
            void Check(string input)
            {
                bool expected = reference.IsMatch(input);
                char[] buffer = (new string('\n', offset) + input).ToCharArray();
                ReadOnlySpan<char> slice = buffer.AsSpan(offset, input.Length);
                Assert.Equal(expected, prepared.IsMatch(slice));
                Assert.Equal(expected, prepared.IsMatch(input));
                Assert.Equal(expected, WildcardMatch.IsMatch(slice, pattern.AsSpan(), options));
                Assert.Equal(expected, WildcardMatch.IsMatch(input, pattern, options));
                Assert.Equal(expected, new WildcardMatchRegex(pattern, options).IsMatch(input));
                Assert.Equal(new string('\n', offset) + input, new string(buffer));
            }
            Check(new string(original));
            Check(new string(original) + "\n");
            Check(new string(original) + "\n\n");
            if (length != 0) Check(new string(original, 0, length - 1));
            for (int lane = 0; lane < length; lane++)
            {
                char[] changed = (char[])original.Clone();
                changed[lane] = '\n'; Check(new string(changed));
                changed[lane] = '\0'; Check(new string(changed));
                changed[lane] = '\uffff'; Check(new string(changed));
            }
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void LiteralNewlinesNeverContaminateQuestionLaneMasks(bool singleline)
    {
        var options = singleline ? WildcardOptions.Singleline : WildcardOptions.None;
        foreach (int length in Enumerable.Range(2, 33))
        for (int lane = 0; lane < length - 1; lane++)
        foreach (string pair in new[] { "\n?", "?\n", "\n\n", "??" })
        {
            char[] tokens = Enumerable.Repeat('a', length).ToArray();
            pair.CopyTo(0, tokens, lane, 2);
            string pattern = new string(tokens);
            var reference = Reference(pattern, options);
            var prepared = new WildcardMatch(pattern, options);
            foreach (char value in new[] { '\n', '\v', '\0', '\uffff', '\ud800', '\udc00' })
            {
                string input = new string(tokens.Select(c => c == '?' ? value : c).ToArray());
                foreach (string text in new[] { input, input + "\n" })
                {
                    bool expected = reference.IsMatch(text);
                    Assert.Equal(expected, prepared.IsMatch(text));
                    Assert.Equal(expected, prepared.IsMatch(text.AsSpan()));
                    Assert.Equal(expected, WildcardMatch.IsMatch(text, pattern, options));
                    Assert.Equal(expected, WildcardMatch.IsMatch(text.AsSpan(), pattern.AsSpan(), options));
                }
            }
        }
    }

    [Theory]
    [InlineData(179391)]
    [InlineData(819731)]
    public void FixedWidthUnicodeFuzzIncludesNearMatchesAndAllQuestionMasks(int seed)
    {
        var random = new Random(seed);
        for (int trial = 0; trial < 8000; trial++)
        {
            int length = random.Next(257);
            char[] input = Enumerable.Range(0, length).Select(_ => (char)random.Next(65536)).ToArray();
            char[] tokens = input.Select(c => c is '*' or '?' ? '?' : c).ToArray();
            for (int lane = 0; lane < length; lane++) if (trial % 7 == 0 || random.Next(3) == 0) tokens[lane] = '?';
            if (length > 0 && trial % 3 == 0) input[random.Next(length)] = trial % 6 == 0 ? '\n' : (char)random.Next(65536);
            string pattern = new string(tokens), text = new string(input);
            if (trial % 5 == 0) text += "\n";
            var options = trial % 2 == 0 ? WildcardOptions.Singleline : WildcardOptions.None;
            bool expected = Reference(pattern, options).IsMatch(text);
            var prepared = new WildcardMatch(pattern, options);
            Assert.True(expected == prepared.IsMatch(text), $"seed={seed}, trial={trial}, length={length}");
            Assert.Equal(expected, prepared.IsMatch(text.AsSpan()));
            Assert.Equal(expected, WildcardMatch.IsMatch(text, pattern, options));
            Assert.Equal(expected, WildcardMatch.IsMatch(text.AsSpan(), pattern.AsSpan(), options));
            Assert.Equal(expected, new WildcardMatchRegex(pattern, options).IsMatch(text));
        }
    }

    [Fact]
    public void PreparedPlansReadCurrentMutableInputUnderConcurrentReuse()
    {
        const string pattern = "report-??.txt";
        var prepared = new WildcardMatch(pattern);
        Parallel.For(0, 128, i =>
        {
            char[] buffer = "report-01.txt".ToCharArray();
            Assert.True(prepared.IsMatch(buffer.AsSpan()));
            buffer[7] = '\n'; Assert.False(prepared.IsMatch(buffer.AsSpan()));
            buffer[7] = '\0'; Assert.True(prepared.IsMatch(buffer.AsSpan()));
            buffer[11] = 'X'; Assert.False(prepared.IsMatch(buffer.AsSpan()));
            buffer[11] = 'x'; Assert.True(prepared.IsMatch(buffer.AsSpan()));
        });
    }

    [Fact]
    public void BitmapStateBoundsHashCollisionsAndLongFallbackAgreeWithRegex()
    {
        foreach (string pattern in new[] { "*", "***", "*\0?*", "*\0\u0040*", "*\ud800?\udc00*", "*a?c*e*",
            "*" + new string('?', 62), "*" + new string('?', 63), new string('*', 64),
            string.Concat(Enumerable.Repeat("*a", 31)) + "*", string.Concat(Enumerable.Repeat("*a", 32)),
            string.Concat(Enumerable.Repeat("a*", 65)), "*" + new string('a', 8193) + "*", new string('?', 65536) })
        foreach (var options in new[] { WildcardOptions.None, WildcardOptions.Singleline })
        {
            // NonBacktracking is an independent oracle for deliberate pathological stars.
            var referenceOptions = pattern.Length < 1000 ? options | (WildcardOptions)RegexOptions.NonBacktracking : options;
            var reference = Reference(pattern, referenceOptions);
            var prepared = new WildcardMatch(pattern, options);
            string positive = pattern.Replace("*", "ab").Replace("?", "\ud800");
            foreach (string text in new[] { "", "\0", "\u0040", "xabcyef", "aZce", "a\nce", "\0x", positive,
                positive + "\n", positive + "\n\n", new string('a', 30), new string('a', 31), new string('a', 32),
                new string('a', 31) + "\n", new string('x', 32) + positive, new string('x', 33) + positive })
            {
                bool expected = reference.IsMatch(text);
                Assert.Equal(expected, prepared.IsMatch(text));
                Assert.Equal(expected, prepared.IsMatch(text.AsSpan()));
                Assert.Equal(expected, WildcardMatch.IsMatch(text, pattern, options));
                Assert.Equal(expected, WildcardMatch.IsMatch(text.AsSpan(), pattern.AsSpan(), options));
                Assert.Equal(expected, new WildcardMatchRegex(pattern, referenceOptions).IsMatch(text));
            }
        }
    }
}
