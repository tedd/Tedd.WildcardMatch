using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Xunit;

namespace Tedd.WildcardMatchTests
{
    public class TranslationTest
    {
        private static string Reference(string pattern) =>
            "^" + Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", ".") + "$";

        [Fact]
        public void Translation_PreservesEveryUtf16CodeUnit()
        {
            for (int character = char.MinValue; character <= char.MaxValue; character++)
            {
                string pattern = "a" + (char)character + "*?\\" + (char)character;
                Assert.Equal(Reference(pattern), new WildcardMatch(pattern).WildcardRegex);
            }
        }

        [Theory]
        [InlineData("")]
        [InlineData("plain]text}")]
        [InlineData("\\*\\?\\\\**??")]
        [InlineData(" \t\n\r\f#.$^+()[]{|}*?")]
        [InlineData("Åßİı😀*?")]
        public void Translation_PreservesRegexText(string pattern)
        {
            Assert.Equal(Reference(pattern), new WildcardMatch(pattern).WildcardRegex);
        }

        [Fact]
        public void Translation_PreservesLongLiteralAndSparseWildcardPatterns()
        {
            string literal = new string('a', 10000);
            foreach (string pattern in new[] { literal, literal + "?", "*" + literal, literal + "\\*" })
                Assert.Equal(Reference(pattern), new WildcardMatch(pattern).WildcardRegex);
        }

        [Theory]
        [InlineData("en-US")]
        [InlineData("tr-TR")]
        public void Matching_AgreesWithRegexAcrossOptionsAndCultures(string culture)
        {
            CultureInfo previous = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo(culture);
                var random = new Random(19373);
                const string alphabet = "abIİı*?\\[](){}.+^$|# \t\r\n\fÅ\0";
                var options = new[] { WildcardOptions.None, WildcardOptions.IgnoreCase,
                    WildcardOptions.Singleline, WildcardOptions.RightToLeft,
                    WildcardOptions.IgnoreCase | WildcardOptions.CultureInvariant,
                    (WildcardOptions)RegexOptions.IgnorePatternWhitespace };
                for (int trial = 0; trial < 2000; trial++)
                {
                    var pattern = new char[random.Next(12)];
                    var input = new char[random.Next(18)];
                    for (int i = 0; i < pattern.Length; i++) pattern[i] = alphabet[random.Next(alphabet.Length)];
                    for (int i = 0; i < input.Length; i++) input[i] = alphabet[random.Next(alphabet.Length)];
                    string wildcard = new string(pattern);
                    string text = new string(input);
                    foreach (WildcardOptions option in options)
                    {
                        bool expected = Regex.IsMatch(text, Reference(wildcard), (RegexOptions)option);
                        Assert.Equal(expected, WildcardMatch.IsMatch(text, wildcard, option));
                        Assert.Equal(expected, new WildcardMatch(wildcard, option, TimeSpan.FromSeconds(1)).IsMatch(text));
                    }
                    Assert.Equal(Regex.IsMatch(text, Reference(wildcard)), WildcardMatchExtensions.IsWildcardMatch(text, wildcard));
                }
            }
            finally { CultureInfo.CurrentCulture = previous; }
        }

        [Fact]
        public void Translation_PreservesRoutingBoundaryPatterns()
        {
            foreach (int length in new[] { 127, 128, 129, 256, 1024 })
                for (int count = 0; count <= 34; count++)
                    foreach (bool atEnd in new[] { false, true })
                    {
                        var characters = new string('a', length).ToCharArray();
                        for (int i = 0; i < count; i++)
                            characters[atEnd ? length - count + i : i] = i % 2 == 0 ? '*' : '?';
                        string pattern = new string(characters);
                        Assert.Equal(Reference(pattern), new WildcardMatch(pattern).WildcardRegex);
                    }
        }

        [Fact]
        public void NullWildcard_PreservesExceptionParameterName()
        {
            string expected = Assert.Throws<ArgumentNullException>(() => Regex.Escape(null)).ParamName;
            Assert.Equal(expected, Assert.Throws<ArgumentNullException>(() => WildcardMatch.IsMatch("input", null)).ParamName);
            Assert.Equal(expected, Assert.Throws<ArgumentNullException>(() => new WildcardMatch(null)).ParamName);
            Assert.Equal(expected, Assert.Throws<ArgumentNullException>(() => WildcardMatchExtensions.IsWildcardMatch("input", null)).ParamName);
        }
    }
}
