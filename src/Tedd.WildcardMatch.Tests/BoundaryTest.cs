using System;
using Xunit;
using Tedd;

namespace Tedd.WildcardMatchTests
{
    public class BoundaryTest
    {
        [Theory]
        [InlineData(null, "*")]
        [InlineData("input", null)]
        [InlineData(null, null)]
        public void StaticIsMatch_NullInputs_ThrowsArgumentNullException(string input, string wildcard)
        {
            Assert.Throws<ArgumentNullException>(() => WildcardMatch.IsMatch(input, wildcard));
        }

        [Theory]
        [InlineData(null, "*")]
        [InlineData("input", null)]
        [InlineData(null, null)]
        public void StaticIsMatchOptions_NullInputs_ThrowsArgumentNullException(string input, string wildcard)
        {
            Assert.Throws<ArgumentNullException>(() => WildcardMatch.IsMatch(input, wildcard, WildcardOptions.None));
        }

        [Theory]
        [InlineData(null)]
        public void InstanceConstructor_NullWildcard_ThrowsArgumentNullException(string wildcard)
        {
            Assert.Throws<ArgumentNullException>(() => new WildcardMatch(wildcard));
        }

        [Theory]
        [InlineData(null)]
        public void InstanceConstructorOptions_NullWildcard_ThrowsArgumentNullException(string wildcard)
        {
            Assert.Throws<ArgumentNullException>(() => new WildcardMatch(wildcard, WildcardOptions.None));
        }

        [Theory]
        [InlineData(null)]
        public void InstanceConstructorOptionsTimeout_NullWildcard_ThrowsArgumentNullException(string wildcard)
        {
            Assert.Throws<ArgumentNullException>(() => new WildcardMatch(wildcard, WildcardOptions.None, TimeSpan.FromSeconds(1)));
        }

        [Theory]
        [InlineData(null)]
        public void InstanceIsMatch_NullInput_ThrowsArgumentNullException(string input)
        {
            var wm = new WildcardMatch("*");
            Assert.Throws<ArgumentNullException>(() => wm.IsMatch(input));
        }

        [Theory]
        [InlineData(null, "*")]
        [InlineData("input", null)]
        [InlineData(null, null)]
        public void ExtensionsIsWildcardMatch_NullInputs_ThrowsArgumentNullException(string input, string wildcard)
        {
            Assert.Throws<ArgumentNullException>(() => input.IsWildcardMatch(wildcard));
        }

        [Theory]
        [InlineData("", "", true)]
        [InlineData("", "*", true)]
        [InlineData("a", "*", true)]
        [InlineData("a", "?", true)]
        [InlineData("", "?", false)]
        public void IsMatch_EmptyStrings(string input, string wildcard, bool expected)
        {
            Assert.Equal(expected, WildcardMatch.IsMatch(input, wildcard));
            Assert.Equal(expected, input.IsWildcardMatch(wildcard));
            var wm = new WildcardMatch(wildcard);
            Assert.Equal(expected, wm.IsMatch(input));
        }
    }
}
