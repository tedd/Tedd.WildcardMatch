using System;
using Xunit;
using Tedd;

namespace Tedd.WildcardMatchTests
{
    public class WildcardMatchTests
    {
        [Theory]
        [InlineData("test", "test", true, false)]
        [InlineData("test", "Test", false, false)]
        [InlineData("test", "Test", true, true)]
        [InlineData("test", "t*t", true, false)]
        [InlineData("test", "t?st", true, false)]
        [InlineData("test", "t??t", true, false)]
        [InlineData("test", "t*s", false, false)]
        [InlineData("abc", "a*", true, false)]
        [InlineData("abc", "*c", true, false)]
        [InlineData("abc", "*b*", true, false)]
        [InlineData("abc.def", "*.def", true, false)]
        [InlineData("abc.def", "abc.*", true, false)]
        [InlineData("a.b.c", "a.*.c", true, false)]
        [InlineData("abcdef", "a*d?f", true, false)]
        [InlineData("", "", true, false)]
        [InlineData("", "*", true, false)]
        [InlineData("", "?", false, false)]
        [InlineData("a", "?", true, false)]
        [InlineData("ab", "?", false, false)]
        [InlineData("ab", "??", true, false)]
        [InlineData("ab", "*?", true, false)]
        [InlineData("ab", "?*", true, false)]
        public void Extension_IsWildcardMatch_String_Theory(string input, string pattern, bool expected, bool ignoreCase)
        {
            Assert.Equal(expected, input.IsWildcardMatch(pattern, ignoreCase));
        }

        [Fact]
        public void Extension_IsWildcardMatch_DefaultIgnoreCase()
        {
            Assert.True("test".IsWildcardMatch("test"));
            Assert.False("test".IsWildcardMatch("Test"));
        }

        [Fact]
        public void Extension_IsWildcardMatch_NullInput_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => ((string)null).IsWildcardMatch("*"));
        }

        [Fact]
        public void Extension_IsWildcardMatch_NullPattern_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => "test".IsWildcardMatch(null));
        }

        [Theory]
        [InlineData("test", "test", true, false)]
        [InlineData("test", "Test", false, false)]
        [InlineData("test", "Test", true, true)]
        [InlineData("abcdef", "a*d?f", true, false)]
        public void Static_IsMatch_BoolIgnoreCase_Theory(string input, string pattern, bool expected, bool ignoreCase)
        {
            Assert.Equal(expected, WildcardMatch.IsMatch(input, pattern, ignoreCase));
        }

        [Fact]
        public void Static_IsMatch_BoolIgnoreCase_Default()
        {
            Assert.True(WildcardMatch.IsMatch("test", "test"));
            Assert.False(WildcardMatch.IsMatch("test", "Test"));
        }

        [Fact]
        public void Static_IsMatch_Nulls_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => WildcardMatch.IsMatch(null, "*", false));
            Assert.Throws<ArgumentNullException>(() => WildcardMatch.IsMatch("test", null, false));
        }

        [Theory]
        [InlineData("test", "test", WildcardOptions.None, true)]
        [InlineData("test", "Test", WildcardOptions.None, false)]
        [InlineData("test", "Test", WildcardOptions.IgnoreCase, true)]
        [InlineData("test\ntest", "test?test", WildcardOptions.None, false)]
        [InlineData("test\ntest", "test?test", WildcardOptions.Singleline, true)]
        [InlineData("test\ntest", "test*test", WildcardOptions.Singleline, true)]
        public void Static_IsMatch_WildcardOptions_Theory(string input, string pattern, WildcardOptions options, bool expected)
        {
            Assert.Equal(expected, WildcardMatch.IsMatch(input, pattern, options));
        }

        [Fact]
        public void Static_IsMatch_Options_Nulls_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => WildcardMatch.IsMatch(null, "*", WildcardOptions.None));
            Assert.Throws<ArgumentNullException>(() => WildcardMatch.IsMatch("test", null, WildcardOptions.None));
        }

        [Theory]
        [InlineData("test", "t*st", true)]
        [InlineData("abc", "a*", true)]
        [InlineData("test", "Test", false)]
        public void Instance_Match_DefaultOptions(string input, string pattern, bool expected)
        {
            var match = new WildcardMatch(pattern);
            Assert.Equal(pattern, match.Wildcard);
            Assert.Equal(expected, match.IsMatch(input));
        }

        [Theory]
        [InlineData("test", "Test", WildcardOptions.IgnoreCase, true)]
        [InlineData("test\ntest", "test*test", WildcardOptions.Singleline, true)]
        public void Instance_Match_WithOptions(string input, string pattern, WildcardOptions options, bool expected)
        {
            var match = new WildcardMatch(pattern, options);
            Assert.Equal(pattern, match.Wildcard);
            Assert.Equal(expected, match.IsMatch(input));
        }

        [Fact]
        public void Instance_Match_WithTimeout()
        {
            var match = new WildcardMatch("a*b", WildcardOptions.None, TimeSpan.FromMilliseconds(100));
            Assert.Equal("a*b", match.Wildcard);
            Assert.True(match.IsMatch("ab"));
        }

        [Fact]
        public void Instance_Constructor_NullPattern_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new WildcardMatch(null));
        }

        [Fact]
        public void Instance_IsMatch_NullInput_Throws()
        {
            var match = new WildcardMatch("*");
            Assert.Throws<ArgumentNullException>(() => match.IsMatch(null));
        }

        [Theory]
        [InlineData("test", "^test$")]
        [InlineData("test*", "^test.*$")]
        [InlineData("*test", "^.*test$")]
        [InlineData("t?st", "^t.st$")]
        [InlineData("t*s?t", "^t.*s.t$")]
        [InlineData("a.b", @"^a\.b$")]
        [InlineData("a\\b", @"^a\\b$")]
        [InlineData(@"a\*b", @"^a\\.*b$")]
        [InlineData("?", "^.$")]
        [InlineData("*", "^.*$")]
        public void WildcardRegex_Property_IsCorrect(string pattern, string expectedRegex)
        {
            var match = new WildcardMatch(pattern);
            Assert.Equal(expectedRegex, match.WildcardRegex);
        }
    }
}
