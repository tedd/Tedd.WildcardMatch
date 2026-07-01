using System;
using Xunit;
using Tedd;

namespace Tedd.WildcardMatchTests
{
    public class AegisBoundaryTests
    {
        [Theory]
        [InlineData(null, "*")]
        [InlineData("input", null)]
        [InlineData(null, null)]
        public void StaticIsMatch_NullInputs_ThrowsArgumentNullException(string input, string wildcard)
        {
            Assert.Throws<ArgumentNullException>(() => WildcardMatch.IsMatch(input, wildcard));
            Assert.Throws<ArgumentNullException>(() => WildcardMatch.IsMatch(input, wildcard, true));
            Assert.Throws<ArgumentNullException>(() => WildcardMatch.IsMatch(input, wildcard, WildcardOptions.None));
        }

        [Theory]
        [InlineData(null, "*")]
        [InlineData("input", null)]
        [InlineData(null, null)]
        public void ExtensionIsWildcardMatch_NullInputs_ThrowsArgumentNullException(string input, string wildcard)
        {
            if (input == null)
            {
                // IsWildcardMatch is an extension method, it will just pass null to Regex.IsMatch which throws ArgumentNullException
                Assert.Throws<ArgumentNullException>(() => WildcardMatchExtensions.IsWildcardMatch(input, wildcard));
            }
            else
            {
                Assert.Throws<ArgumentNullException>(() => input.IsWildcardMatch(wildcard));
            }
        }

        [Fact]
        public void Constructor_NullWildcard_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new WildcardMatch(null));
            Assert.Throws<ArgumentNullException>(() => new WildcardMatch(null, WildcardOptions.None));
            Assert.Throws<ArgumentNullException>(() => new WildcardMatch(null, WildcardOptions.None, TimeSpan.FromSeconds(1)));
        }

        [Theory]
        [InlineData(null)]
        public void InstanceIsMatch_NullInput_ThrowsArgumentNullException(string input)
        {
            var wm = new WildcardMatch("*");
            Assert.Throws<ArgumentNullException>(() => wm.IsMatch(input));
        }

        [Theory]
        [InlineData("", "", true)]
        [InlineData("", "*", true)]
        [InlineData("", "?", false)]
        [InlineData("a", "", false)]
        public void StaticIsMatch_EmptyStringBoundaries(string input, string wildcard, bool expected)
        {
            Assert.Equal(expected, WildcardMatch.IsMatch(input, wildcard));
            Assert.Equal(expected, WildcardMatch.IsMatch(input, wildcard, true));
            Assert.Equal(expected, WildcardMatch.IsMatch(input, wildcard, WildcardOptions.None));
        }

        [Theory]
        [InlineData("", "", true)]
        [InlineData("", "*", true)]
        [InlineData("", "?", false)]
        [InlineData("a", "", false)]
        public void ExtensionIsWildcardMatch_EmptyStringBoundaries(string input, string wildcard, bool expected)
        {
            Assert.Equal(expected, input.IsWildcardMatch(wildcard));
            Assert.Equal(expected, input.IsWildcardMatch(wildcard, true));
        }

        [Theory]
        [InlineData("", true)]
        [InlineData("a", true)]
        public void InstanceIsMatch_EmptyStringBoundaries_Star(string input, bool expected)
        {
            var wm = new WildcardMatch("*");
            Assert.Equal(expected, wm.IsMatch(input));
        }

        [Theory]
        [InlineData("", false)]
        [InlineData("a", true)]
        public void InstanceIsMatch_EmptyStringBoundaries_Question(string input, bool expected)
        {
            var wm = new WildcardMatch("?");
            Assert.Equal(expected, wm.IsMatch(input));
        }
    }
}
