using System;
using Xunit;
using Tedd;

namespace Tedd.WildcardMatchTests
{
    public class TimeoutTest
    {
        [Fact]
        public void Constructor_Timeout_ThrowsRegexMatchTimeoutException_OnLongMatch()
        {
            var pattern = "a*a*a*a*a*a*a*a*a*a*a*a*a*a*a*a*a*a*a*a*a*a*a*a*a*a*b";
            var input = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";

            var wm = new WildcardMatch(pattern, WildcardOptions.None, TimeSpan.FromMilliseconds(1));
            Assert.Throws<System.Text.RegularExpressions.RegexMatchTimeoutException>(() => wm.IsMatch(input));
        }

        [Fact]
        public void StaticIsMatch_OptionsMethod()
        {
            Assert.True(WildcardMatch.IsMatch("a", "*", WildcardOptions.None));
            Assert.True(WildcardMatch.IsMatch("A", "*a*", WildcardOptions.IgnoreCase));
        }
    }
}
