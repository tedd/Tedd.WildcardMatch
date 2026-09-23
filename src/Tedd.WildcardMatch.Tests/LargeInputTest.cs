using System;
using System.Buffers;
using Xunit;
using Tedd;

namespace Tedd.WildcardMatchTests
{
    public class LargeInputTest
    {
        [Theory]
        [InlineData(10000, "*")]
        [InlineData(10000, "a*")]
        [InlineData(10000, "*a")]
        public void Match_LargeInput_ReturnsTrue(int size, string pattern)
        {
            var pool = ArrayPool<char>.Shared;
            char[] buffer = pool.Rent(size);

            try
            {
                var span = new Span<char>(buffer, 0, size);
                span.Fill('a');

                string input = new string(span);

                var wm = new WildcardMatch(pattern);
                Assert.True(wm.IsMatch(input));
            }
            finally
            {
                pool.Return(buffer);
            }
        }
    }
}
