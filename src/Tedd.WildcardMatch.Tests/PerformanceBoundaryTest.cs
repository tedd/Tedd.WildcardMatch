using System;
using System.Buffers;
using Xunit;
using Tedd;

namespace Tedd.WildcardMatchTests
{
    public class PerformanceBoundaryTest
    {
        [Theory]
        [InlineData(10000, "*")]
        [InlineData(10000, "a*z")]
        public void LargeInput_MatchesEfficiently(int size, string pattern)
        {
            var pool = ArrayPool<char>.Shared;
            var buffer = pool.Rent(size);

            try
            {
                var span = new Span<char>(buffer, 0, size);
                span.Fill('a');

                // If the pattern is "a*z", let's manually terminate it so it matches correctly, otherwise false expected
                if (pattern == "a*z")
                {
                    span[size - 1] = 'z';
                }

                var inputString = span.ToString();

                var wm = new WildcardMatch(pattern);
                var result = wm.IsMatch(inputString);

                Assert.True(result);
            }
            finally
            {
                pool.Return(buffer);
            }
        }
    }
}
