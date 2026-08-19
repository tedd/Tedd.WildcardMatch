using System;
using System.Buffers;
using System.Collections.Generic;
using Xunit;
using Tedd;

namespace Tedd.WildcardMatchTests
{
    public class ParameterizedEdgeCaseTests
    {
        [Theory]
        [InlineData(null)]
        public void InternalUtils_StringToWildcard_Null(string wildcard)
        {
            Assert.Throws<ArgumentNullException>(() => InternalUtils.StringToWildcard(wildcard));
        }

        [Theory]
        [InlineData(null, "pattern")]
        [InlineData("input", null)]
        [InlineData(null, null)]
        public void StaticIsMatch_NullArguments(string input, string wildcard)
        {
            Assert.Throws<ArgumentNullException>(() => WildcardMatch.IsMatch(input, wildcard));
        }

        [Theory]
        [InlineData(null, "pattern", WildcardOptions.None)]
        [InlineData("input", null, WildcardOptions.None)]
        [InlineData(null, null, WildcardOptions.None)]
        public void StaticIsMatch_WithOptions_NullArguments(string input, string wildcard, WildcardOptions options)
        {
            Assert.Throws<ArgumentNullException>(() => WildcardMatch.IsMatch(input, wildcard, options));
        }

        [Theory]
        [InlineData(null)]
        public void Constructor_NullWildcard(string wildcard)
        {
            Assert.Throws<ArgumentNullException>(() => new WildcardMatch(wildcard));
        }

        [Theory]
        [InlineData(null, WildcardOptions.None)]
        public void Constructor_WithOptions_NullWildcard(string wildcard, WildcardOptions options)
        {
            Assert.Throws<ArgumentNullException>(() => new WildcardMatch(wildcard, options));
        }

        [Theory]
        [InlineData("test*")]
        [InlineData("")]
        [InlineData("*")]
        public void Constructor_InstanceIsMatch_NullInput(string wildcard)
        {
            // Just some valid constructor input so we can test the instance IsMatch
            var match = new WildcardMatch(wildcard);
            Assert.Throws<ArgumentNullException>(() => match.IsMatch(null));
        }

        [Theory]
        [InlineData(null, "pattern")]
        [InlineData("input", null)]
        public void Extensions_IsWildcardMatch_NullArguments(string input, string wildcard)
        {
            Assert.Throws<ArgumentNullException>(() => input.IsWildcardMatch(wildcard));
        }

        [Theory]
        [InlineData("", "")]
        [InlineData("", "*")]
        [InlineData("a", "*")]
        [InlineData("a", "?")]
        public void Boundary_EmptyStrings(string input, string wildcard)
        {
            Assert.Equal(input == wildcard || wildcard == "*" || (input.Length == 1 && wildcard == "?"), WildcardMatch.IsMatch(input, wildcard));
        }

        public static IEnumerable<object[]> LargeBufferData()
        {
            int size = 100000;
            char[] buffer = ArrayPool<char>.Shared.Rent(size);
            try
            {
                new Span<char>(buffer, 0, size).Fill('A');
                string largeString = new string(buffer, 0, size);

                new Span<char>(buffer, 0, size).Fill('?');
                string largePattern = new string(buffer, 0, size);

                yield return new object[] { largeString, largeString, true };
                yield return new object[] { largeString, "*", true };
                yield return new object[] { largeString, "A*A", true };
                yield return new object[] { largeString, largePattern, true };
            }
            finally
            {
                ArrayPool<char>.Shared.Return(buffer);
            }
        }

        [Theory]
        [MemberData(nameof(LargeBufferData))]
        public void Boundary_LargeBuffers(string input, string wildcard, bool expectedResult)
        {
            Assert.Equal(expectedResult, WildcardMatch.IsMatch(input, wildcard));
        }
    }
}
