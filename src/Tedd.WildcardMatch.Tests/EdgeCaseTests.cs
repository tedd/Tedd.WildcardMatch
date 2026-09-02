using System;
using Xunit;

namespace Tedd.WildcardMatchTests
{
    public class EdgeCaseTests
    {
        [Theory]
        [InlineData(null, "*")]
        [InlineData("text", null)]
        [InlineData(null, null)]
        public void IsMatch_NullArguments_ThrowsArgumentNullException(string input, string pattern)
        {
            Assert.Throws<ArgumentNullException>(() => WildcardMatch.IsMatch(input, pattern));
        }

        [Theory]
        [InlineData("", "")]
        [InlineData("", "*")]
        [InlineData("a", "*")]
        [InlineData("", "?")]
        public void IsMatch_EmptyArguments(string input, string pattern)
        {
            // Empty string pattern shouldn't throw exception, just evaluates.
            // But if pattern is "?", empty input should be false.
            // If pattern is "*", empty input should be true.
            // If pattern is "", empty input should be true.

            bool expected = false;
            if (pattern == "") expected = (input == "");
            if (pattern == "*") expected = true;

            Assert.Equal(expected, WildcardMatch.IsMatch(input, pattern));
        }
    }
}
