using System;
using Xunit;

namespace Tedd.WildcardMatchTests
{
    public class MethodTest
    {
        [Fact]
        public void IsMatch()
        {
            Assert.True(WildcardMatch.IsMatch(TestData._lorem, "*L??em*"));
        }
        [Fact]
        public void IsMatch_case()
        {
            Assert.False(WildcardMatch.IsMatch(TestData._lorem, "*mauris*"));
            Assert.True(WildcardMatch.IsMatch(TestData._lorem, "*mauris*",true));
        }
        [Fact]
        public void IsMatch_options()
        {
            Assert.True(WildcardMatch.IsMatch(TestData._lorem, "*mauris*", WildcardOptions.IgnoreCase));
        }

        [Theory]
        [InlineData(null, "*")]
        [InlineData("test", null)]
        public void IsMatch_Null_ThrowsArgumentNullException(string input, string pattern)
        {
            Assert.Throws<ArgumentNullException>(() => WildcardMatch.IsMatch(input, pattern));
        }

        [Theory]
        [InlineData("", "*", true)]
        [InlineData("", "?", false)]
        [InlineData("", "", true)]
        [InlineData("a", "", false)]
        [InlineData("a", "*", true)]
        [InlineData("a", "?", true)]
        public void IsMatch_EmptyString_BoundaryChecks(string input, string pattern, bool expected)
        {
            Assert.Equal(expected, WildcardMatch.IsMatch(input, pattern));
        }
    }
}
