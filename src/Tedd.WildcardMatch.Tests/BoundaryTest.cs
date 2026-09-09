using System;
using Xunit;
using Tedd;

namespace Tedd.WildcardMatchTests
{
    public class BoundaryTest
    {
        [Theory]
        [InlineData("text", null)]
        [InlineData(null, "pattern")]
        [InlineData(null, null)]
        public void StaticIsMatch_NullInputs_ThrowsArgumentNullException(string input, string pattern)
        {
            Assert.Throws<ArgumentNullException>(() => WildcardMatch.IsMatch(input, pattern));
        }

        [Theory]
        [InlineData("text", null, false)]
        [InlineData(null, "pattern", true)]
        [InlineData(null, null, false)]
        public void StaticIsMatch_WithIgnoreCase_NullInputs_ThrowsArgumentNullException(string input, string pattern, bool ignoreCase)
        {
            Assert.Throws<ArgumentNullException>(() => WildcardMatch.IsMatch(input, pattern, ignoreCase));
        }

        [Theory]
        [InlineData("text", null, WildcardOptions.None)]
        [InlineData(null, "pattern", WildcardOptions.IgnoreCase)]
        [InlineData(null, null, WildcardOptions.None)]
        public void StaticIsMatch_WithOptions_NullInputs_ThrowsArgumentNullException(string input, string pattern, WildcardOptions options)
        {
            Assert.Throws<ArgumentNullException>(() => WildcardMatch.IsMatch(input, pattern, options));
        }

        [Theory]
        [InlineData(null)]
        public void Constructor_NullPattern_ThrowsArgumentNullException(string pattern)
        {
            Assert.Throws<ArgumentNullException>(() => new WildcardMatch(pattern));
            Assert.Throws<ArgumentNullException>(() => new WildcardMatch(pattern, WildcardOptions.None));
            Assert.Throws<ArgumentNullException>(() => new WildcardMatch(pattern, WildcardOptions.None, TimeSpan.FromSeconds(1)));
        }

        [Theory]
        [InlineData(null)]
        public void InstanceIsMatch_NullInput_ThrowsArgumentNullException(string input)
        {
            var wm = new WildcardMatch("pattern");
            Assert.Throws<ArgumentNullException>(() => wm.IsMatch(input));
        }

        [Theory]
        [InlineData("text", null)]
        [InlineData(null, "pattern")]
        [InlineData(null, null)]
        public void ExtensionIsWildcardMatch_NullInputs_ThrowsArgumentNullException(string input, string pattern)
        {
            Assert.Throws<ArgumentNullException>(() => input.IsWildcardMatch(pattern));
        }

        [Theory]
        [InlineData("text", null, false)]
        [InlineData(null, "pattern", true)]
        [InlineData(null, null, false)]
        public void ExtensionIsWildcardMatch_WithIgnoreCase_NullInputs_ThrowsArgumentNullException(string input, string pattern, bool ignoreCase)
        {
            Assert.Throws<ArgumentNullException>(() => input.IsWildcardMatch(pattern, ignoreCase));
        }

        [Theory]
        [InlineData("", "", true)]
        [InlineData("", "*", true)]
        [InlineData("a", "", false)]
        [InlineData("", "?", false)]
        public void StaticIsMatch_EmptyStrings(string input, string pattern, bool expected)
        {
            Assert.Equal(expected, WildcardMatch.IsMatch(input, pattern));
        }

        [Theory]
        [InlineData("", "", true)]
        [InlineData("", "*", true)]
        [InlineData("a", "", false)]
        [InlineData("", "?", false)]
        public void InstanceIsMatch_EmptyStrings(string input, string pattern, bool expected)
        {
            var wm = new WildcardMatch(pattern);
            Assert.Equal(expected, wm.IsMatch(input));
        }
    }
}
