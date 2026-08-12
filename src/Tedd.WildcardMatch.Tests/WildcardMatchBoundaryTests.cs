using System;
using System.Buffers;
using Xunit;

namespace Tedd.WildcardMatchTests
{
    public class WildcardMatchBoundaryTests
    {
        [Theory]
        [InlineData(null, "*")]
        [InlineData("test", null)]
        [InlineData(null, null)]
        public void IsMatch_NullInputs_ThrowsArgumentNullException(string input, string pattern)
        {
            Assert.Throws<ArgumentNullException>(() => WildcardMatch.IsMatch(input, pattern));
        }

        [Theory]
        [InlineData(null, "*", WildcardOptions.None)]
        [InlineData("test", null, WildcardOptions.IgnoreCase)]
        [InlineData(null, null, WildcardOptions.Compiled)]
        public void IsMatch_Options_NullInputs_ThrowsArgumentNullException(string input, string pattern, WildcardOptions options)
        {
            Assert.Throws<ArgumentNullException>(() => WildcardMatch.IsMatch(input, pattern, options));
        }

        [Theory]
        [InlineData(null)]
        public void Constructor_NullPattern_ThrowsArgumentNullException(string pattern)
        {
            Assert.Throws<ArgumentNullException>(() => new WildcardMatch(pattern));
        }

        [Theory]
        [InlineData(null)]
        public void InstanceIsMatch_NullInput_ThrowsArgumentNullException(string input)
        {
            var wm = new WildcardMatch("*");
            Assert.Throws<ArgumentNullException>(() => wm.IsMatch(input));
        }

        [Theory]
        [InlineData(null, "*", true)]
        [InlineData("test", null, false)]
        [InlineData(null, null, true)]
        public void ExtensionIsWildcardMatch_NullInputs_ThrowsArgumentNullException(string input, string pattern, bool ignoreCase)
        {
            Assert.Throws<ArgumentNullException>(() => input.IsWildcardMatch(pattern, ignoreCase));
        }

        [Theory]
        [InlineData("", "", true)]
        [InlineData("", "*", true)]
        [InlineData("test", "", false)]
        [InlineData("", "?", false)]
        public void IsMatch_EmptyStrings_ReturnsExpectedResult(string input, string pattern, bool expected)
        {
            Assert.Equal(expected, WildcardMatch.IsMatch(input, pattern));
        }

        [Fact]
        public void IsMatch_LargeInput_WithSpan()
        {
            var pool = ArrayPool<char>.Shared;
            var buffer = pool.Rent(1000);
            try
            {
                var span = new Span<char>(buffer, 0, 1000);
                span.Fill('A');
                var input = span.ToString();

                Assert.True(WildcardMatch.IsMatch(input, "*A*"));
                Assert.False(WildcardMatch.IsMatch(input, "*B*"));
            }
            finally
            {
                pool.Return(buffer);
            }
        }
    }
}
