using Xunit;

namespace Tedd.WildcardMatchTests
{
    public class InstanceTest
    {
        [Fact]
        public void Wilcard_property()
        {
            var pattern = "test*test?test";
            var wm = new WildcardMatch(pattern);
            Assert.Equal(pattern, wm.Wildcard);
        }

        [Fact]
        public void WilcardRegex_property()
        {
            var pattern = "test*test?test";
            var regexPattern = "test.*test.test";
            var wm = new WildcardMatch(pattern);
            Assert.Equal(regexPattern, wm.WildcardRegex);
        }

        [Fact]
        public void IsMatch()
        {
            var text = "abcdefghijklmnop";
            var wm = new WildcardMatch("?b?d?f?h");
            Assert.True(wm.IsMatch(text));
        }

        [Fact]
        public void OptionsSet()
        {
            var text = "abcdefghijklmnop";
            var wm1 = new WildcardMatch("?B?D?F?H");
            Assert.False(wm1.IsMatch(text));
            var wm2 = new WildcardMatch("?B?D?F?H", WildcardOptions.IgnoreCase);
            Assert.True(wm2.IsMatch(text));
        }
    }
}