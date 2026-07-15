using System;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Tedd.Archive
{
    [Flags]
    public enum WildcardOptionsLegacy
    {
        None = RegexOptions.None,
        IgnoreCase = RegexOptions.IgnoreCase,
        Multiline = RegexOptions.Multiline,
        ExplicitCapture = RegexOptions.ExplicitCapture,
        Compiled = RegexOptions.Compiled,
        Singleline = RegexOptions.Singleline,
        IgnorePatternWhitespace = RegexOptions.IgnorePatternWhitespace,
        RightToLeft = RegexOptions.RightToLeft,
        ECMAScript = RegexOptions.ECMAScript,
        CultureInvariant = RegexOptions.CultureInvariant
    }

    public class WildcardMatchLegacy
    {
        private readonly Regex _regex;

        public string Wildcard { get; private set; }
        public string WildcardRegex { get; private set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsMatch(string input, string wildcard, bool ignoreCase = false) => Regex.IsMatch(input, InternalUtilsLegacy.StringToWildcard(wildcard), ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsMatch(string input, string wildcard, WildcardOptionsLegacy options) => Regex.IsMatch(input, InternalUtilsLegacy.StringToWildcard(wildcard), (RegexOptions)options);

        public WildcardMatchLegacy(string wildcard) : this(wildcard, WildcardOptionsLegacy.None, Regex.InfiniteMatchTimeout) { }

        public WildcardMatchLegacy(string wildcard, WildcardOptionsLegacy options) : this(wildcard, options, Regex.InfiniteMatchTimeout) { }

        public WildcardMatchLegacy(string wildcard, WildcardOptionsLegacy options, TimeSpan timeout)
        {
            Wildcard = wildcard;
            WildcardRegex = InternalUtilsLegacy.StringToWildcard(wildcard);
            _regex = new Regex(WildcardRegex, (RegexOptions)options, timeout);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsMatch(string input) => _regex.IsMatch(input);
    }
}
