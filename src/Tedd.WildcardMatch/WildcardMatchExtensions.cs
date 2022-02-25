using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Tedd;

    public static class WildcardMatchExtensions
    {
        /// <summary>
        /// Check if wildcard string matches input string.
        /// </summary>
        /// <param name="input">The string to search.</param>
        /// <param name="wildcard">The wildcard pattern to search for.</param>
        /// <param name="ignoreCase">Ignore casing.</param>
        /// <returns>True if wildcard pattern matches input string.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsWildcardMatch(this string input, string wildcard, bool ignoreCase = false) => Regex.IsMatch(input, InternalUtils.StringToWildcard(wildcard), ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None);
    }