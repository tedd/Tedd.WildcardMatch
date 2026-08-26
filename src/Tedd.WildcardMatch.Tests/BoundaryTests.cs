using System;
using System.Buffers;
using Xunit;
using Tedd;

namespace Tedd.WildcardMatchTests
{
    public class BoundaryTests
    {
        [Theory]
        [InlineData(null, "pattern")]
        [InlineData("input", null)]
        [InlineData(null, null)]
        public void IsMatch_Static_Bool_NullInputs_ThrowsArgumentNullException(string input, string wildcard)
        {
            Assert.Throws<ArgumentNullException>(() => WildcardMatch.IsMatch(input, wildcard, false));
        }

        [Theory]
        [InlineData(null, "pattern")]
        [InlineData("input", null)]
        [InlineData(null, null)]
        public void IsMatch_Static_Options_NullInputs_ThrowsArgumentNullException(string input, string wildcard)
        {
            Assert.Throws<ArgumentNullException>(() => WildcardMatch.IsMatch(input, wildcard, WildcardOptions.None));
        }

        [Theory]
        [InlineData(null, "pattern")]
        [InlineData("input", null)]
        [InlineData(null, null)]
        public void IsWildcardMatch_Extension_NullInputs_ThrowsArgumentNullException(string input, string wildcard)
        {
            Assert.Throws<ArgumentNullException>(() => input.IsWildcardMatch(wildcard, false));
        }

        [Fact]
        public void Constructor_NullWildcard_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new WildcardMatch(null));
            Assert.Throws<ArgumentNullException>(() => new WildcardMatch(null, WildcardOptions.None));
            Assert.Throws<ArgumentNullException>(() => new WildcardMatch(null, WildcardOptions.None, TimeSpan.FromSeconds(1)));
        }

        [Fact]
        public void Instance_IsMatch_NullInput_ThrowsArgumentNullException()
        {
            var wm = new WildcardMatch("pattern");
            Assert.Throws<ArgumentNullException>(() => wm.IsMatch(null));
        }

        [Theory]
        [InlineData("", "", true)]
        [InlineData("", "*", true)]
        [InlineData("a", "*", true)]
        [InlineData("", "?", false)]
        public void EmptyString_BoundaryConditions(string input, string wildcard, bool expected)
        {
            Assert.Equal(expected, WildcardMatch.IsMatch(input, wildcard));
            Assert.Equal(expected, input.IsWildcardMatch(wildcard));

            var wm = new WildcardMatch(wildcard);
            Assert.Equal(expected, wm.IsMatch(input));
        }

        [Fact]
        public void LargeBuffer_PatternMatching()
        {
            const int length = 1_000_000;
            var pool = ArrayPool<char>.Shared;
            var buffer = pool.Rent(length);

            try
            {
                Array.Fill(buffer, 'a', 0, length);
                buffer[length - 1] = 'z';
                var input = new string(buffer, 0, length);

                Assert.True(WildcardMatch.IsMatch(input, "*z"));
                Assert.True(WildcardMatch.IsMatch(input, "a*z"));
                Assert.False(WildcardMatch.IsMatch(input, "*y"));
            }
            finally
            {
                pool.Return(buffer);
            }
        }
    }
}
