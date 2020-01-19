using System;
using Xunit;
using Tedd;
//using WildcardMatch = Tedd.WildcardMatch;

namespace Tedd.WildcardMatchTests
{
    public class ExtensionsTest
    {
        [Fact]
        public void Questionmark_match_letter() => Assert.True(TestData._lorem.IsWildcardMatch("Lor?m"));
        [Fact]
        public void Questionmark_not_match_case() => Assert.False(TestData._lorem.IsWildcardMatch("mauris"));
        [Fact]
        public void Questionmark_match_case() => Assert.True(TestData._lorem.IsWildcardMatch("Mauris"));
        [Fact]
        public void Questionmark_ignore_case() => Assert.True(TestData._lorem.IsWildcardMatch("mauris", true));
        [Fact]
        public void Questionmark_match_space() => Assert.True(TestData._lorem.IsWildcardMatch("finibus?vulputate"));
        [Fact]
        public void Questionmark_match_newline() => Assert.False(TestData._lorem.IsWildcardMatch("\r?\r"));
        [Fact]
        public void Backslash_not_processed() => Assert.True(TestData.loremAdv.IsWildcardMatch("\\?1"));
        [Fact]
        public void Complex_star_pattern() => Assert.True(TestData._lorem.IsWildcardMatch("Lorem*.*.*e*.*Cras*.*i*"));
    }
}
