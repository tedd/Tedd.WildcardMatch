using Xunit;

namespace Tedd.WildcardMatchTests
{
    public class MethodTest
    {
        [Fact]
        public void IsMatch()
        {
            Assert.True(WildcardMatch.IsMatch(TestData._lorem, "L??em"));
        }
        [Fact]
        public void IsMatch_case()
        {
            Assert.False(WildcardMatch.IsMatch(TestData._lorem, "mauris"));
            Assert.True(WildcardMatch.IsMatch(TestData._lorem, "mauris",true));
        }
        [Fact]
        public void IsMatch_options()
        {
            Assert.True(WildcardMatch.IsMatch(TestData._lorem, "mauris", WildcardOptions.IgnoreCase));
        }
    }
}