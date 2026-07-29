using System;
using System.Buffers;
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
        public void StaticIsMatch_NullReferences_ThrowsArgumentNullException(string input, string pattern)
        {
            Assert.Throws<ArgumentNullException>(() => WildcardMatch.IsMatch(input, pattern));
        }

        [Theory]
        [InlineData(null, "*")]
        [InlineData("input", null)]
        [InlineData(null, null)]
        public void ExtensionIsWildcardMatch_NullReferences_ThrowsArgumentNullException(string input, string pattern)
        {
            Assert.Throws<ArgumentNullException>(() => input.IsWildcardMatch(pattern));
        }

        [Theory]
        [InlineData(null)]
        public void InstanceConstructor_NullReference_ThrowsArgumentNullException(string pattern)
        {
            Assert.Throws<ArgumentNullException>(() => new WildcardMatch(pattern));
        }

        [Fact]
        public void InstanceIsMatch_NullReference_ThrowsArgumentNullException()
        {
            var wm = new WildcardMatch("*");
            Assert.Throws<ArgumentNullException>(() => wm.IsMatch(null));
        }

        [Theory]
        [InlineData("", "", true)]
        [InlineData("", "*", true)]
        [InlineData("a", "*", true)]
        [InlineData("", "?", false)]
        [InlineData("a", "?", true)]
        [InlineData("abc", "a*c", true)]
        [InlineData("abc", "a?c", true)]
        [InlineData("abc", "a?b", false)]
        public void IsMatch_EmptyAndBoundaryConditions(string input, string pattern, bool expected)
        {
            Assert.Equal(expected, WildcardMatch.IsMatch(input, pattern));
            Assert.Equal(expected, input.IsWildcardMatch(pattern));

            var wm = new WildcardMatch(pattern);
            Assert.Equal(expected, wm.IsMatch(input));
        }

        [Fact]
        public void IsMatch_MaximumBufferDimensions_ShouldMatch()
        {
            var length = 100_000;
            var pool = ArrayPool<char>.Shared;
            var buffer = pool.Rent(length);
            try
            {
                buffer.AsSpan(0, length).Fill('A');
                var input = new string(buffer, 0, length);
                Assert.True(WildcardMatch.IsMatch(input, "*A*"));
                Assert.True(input.IsWildcardMatch("*A*"));

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
