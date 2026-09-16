using System;
using Xunit;
using Tedd;

namespace Tedd.WildcardMatchTests
{
    public class BoundaryTest
    {
        [Theory]
        [InlineData(null, "*")]
        [InlineData("test", null)]
        [InlineData(null, null)]
        public void StaticIsMatch_NullInputs_ThrowsArgumentNullException(string input, string pattern)
        {
            Assert.Throws<ArgumentNullException>(() => WildcardMatch.IsMatch(input, pattern));
        }

        [Theory]
        [InlineData("", "", true)]
        [InlineData("", "*", true)]
        [InlineData("", "?", false)]
        [InlineData("a", "", false)]
        public void StaticIsMatch_EmptyStrings(string input, string pattern, bool expected)
        {
            Assert.Equal(expected, WildcardMatch.IsMatch(input, pattern));
        }

        [Theory]
        [InlineData(null)]
        public void Constructor_NullPattern_ThrowsArgumentNullException(string pattern)
        {
            Assert.Throws<ArgumentNullException>(() => new WildcardMatch(pattern));
        }

        [Theory]
        [InlineData("")]
        [InlineData("*")]
        public void InstanceIsMatch_NullInput_ThrowsArgumentNullException(string pattern)
        {
            var wm = new WildcardMatch(pattern);
            Assert.Throws<ArgumentNullException>(() => wm.IsMatch(null));
        }

        [Theory]
        [InlineData("", "", true)]
        [InlineData("*", "", true)]
        [InlineData("?", "", false)]
        public void InstanceIsMatch_EmptyStrings(string pattern, string input, bool expected)
        {
            var wm = new WildcardMatch(pattern);
            Assert.Equal(expected, wm.IsMatch(input));
        }

        [Theory]
        [InlineData(null, "*")]
        [InlineData("test", null)]
        public void Extension_NullInputs_ThrowsArgumentNullException(string input, string pattern)
        {
            Assert.Throws<ArgumentNullException>(() => input.IsWildcardMatch(pattern));
        }

        [Theory]
        [InlineData("", "", true)]
        [InlineData("", "*", true)]
        [InlineData("", "?", false)]
        [InlineData("a", "", false)]
        public void Extension_EmptyStrings(string input, string pattern, bool expected)
        {
            Assert.Equal(expected, input.IsWildcardMatch(pattern));
        }
    }
}
