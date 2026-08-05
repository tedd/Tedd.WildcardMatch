using System;
using System.Buffers;
using Xunit;
using Tedd;

namespace Tedd.WildcardMatchTests
{
    public class EdgeCaseTests
    {
        [Fact]
        public void StaticIsMatch_NullInput_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => WildcardMatch.IsMatch(null, "pattern"));
            Assert.Throws<ArgumentNullException>(() => WildcardMatch.IsMatch(null, "pattern", WildcardOptions.None));
        }

        [Fact]
        public void StaticIsMatch_NullPattern_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => WildcardMatch.IsMatch("input", null));
            Assert.Throws<ArgumentNullException>(() => WildcardMatch.IsMatch("input", null, WildcardOptions.None));
        }

        [Fact]
        public void ExtensionMethod_NullInput_ThrowsArgumentNullException()
        {
            string input = null;
            Assert.Throws<ArgumentNullException>(() => input.IsWildcardMatch("pattern"));
        }

        [Fact]
        public void ExtensionMethod_NullPattern_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => "input".IsWildcardMatch(null));
        }

        [Fact]
        public void Constructor_NullPattern_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new WildcardMatch(null));
            Assert.Throws<ArgumentNullException>(() => new WildcardMatch(null, WildcardOptions.None));
            Assert.Throws<ArgumentNullException>(() => new WildcardMatch(null, WildcardOptions.None, TimeSpan.FromSeconds(1)));
        }

        [Fact]
        public void InstanceIsMatch_NullInput_ThrowsArgumentNullException()
        {
            var wm = new WildcardMatch("pattern");
            Assert.Throws<ArgumentNullException>(() => wm.IsMatch(null));
        }

        [Theory]
        [InlineData("", "")]
        [InlineData("input", "")]
        [InlineData("", "pattern")]
        public void EmptyStrings_DoesNotThrow(string input, string pattern)
        {
            var result1 = WildcardMatch.IsMatch(input, pattern);
            var result2 = input.IsWildcardMatch(pattern);
            Assert.IsType<bool>(result1);
            Assert.IsType<bool>(result2);
        }

        [Fact]
        public void LargeString_AllocationFree_Test()
        {
            int length = 10_000;
            char[] buffer = ArrayPool<char>.Shared.Rent(length);
            try
            {
                Span<char> span = buffer.AsSpan(0, length);
                span.Fill('A');
                string largeInput = new string(span);

                Assert.True(WildcardMatch.IsMatch(largeInput, "*A*"));
                Assert.False(WildcardMatch.IsMatch(largeInput, "*B*"));
            }
            finally
            {
                ArrayPool<char>.Shared.Return(buffer);
            }
        }
    }
}
