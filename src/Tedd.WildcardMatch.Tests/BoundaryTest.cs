using System;
using System.Buffers;
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
        [InlineData(null, "*", false)]
        [InlineData("test", null, false)]
        [InlineData(null, null, false)]
        public void StaticIsMatch_WithIgnoreCase_NullInputs_ThrowsArgumentNullException(string input, string pattern, bool ignoreCase)
        {
            Assert.Throws<ArgumentNullException>(() => WildcardMatch.IsMatch(input, pattern, ignoreCase));
        }

        [Theory]
        [InlineData(null, "*", WildcardOptions.None)]
        [InlineData("test", null, WildcardOptions.None)]
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
        }

        [Theory]
        [InlineData(null)]
        public void InstanceIsMatch_NullInput_ThrowsArgumentNullException(string input)
        {
            var wm = new WildcardMatch("*");
            Assert.Throws<ArgumentNullException>(() => wm.IsMatch(input));
        }

        [Theory]
        [InlineData(null, "*", false)]
        [InlineData("test", null, false)]
        [InlineData(null, null, false)]
        public void ExtensionsIsWildcardMatch_NullInputs_ThrowsArgumentNullException(string input, string pattern, bool ignoreCase)
        {
            Assert.Throws<ArgumentNullException>(() => input.IsWildcardMatch(pattern, ignoreCase));
        }

        [Theory]
        [InlineData("", "", true)]
        [InlineData("", "*", true)]
        [InlineData("a", "*", true)]
        [InlineData("", "?", false)]
        [InlineData("a", "?", true)]
        [InlineData("ab", "?", false)]
        public void EmptyAndSmallStrings_MatchCorrectly(string input, string pattern, bool expectedResult)
        {
            Assert.Equal(expectedResult, WildcardMatch.IsMatch(input, pattern));
            Assert.Equal(expectedResult, input.IsWildcardMatch(pattern));
            var wm = new WildcardMatch(pattern);
            Assert.Equal(expectedResult, wm.IsMatch(input));
        }

        [Theory]
        [InlineData(1024 * 1024)] // 1MB string
        public void LargeBuffer_Match(int size)
        {
            var pool = ArrayPool<char>.Shared;
            var buffer = pool.Rent(size);
            try
            {
                var span = buffer.AsSpan(0, size);
                span.Fill('A');
                var input = new string(span);

                var wm = new WildcardMatch("*A*");
                Assert.True(wm.IsMatch(input));
            }
            finally
            {
                pool.Return(buffer);
            }
        }
    }
}
