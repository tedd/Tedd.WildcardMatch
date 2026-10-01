using System.Runtime.CompilerServices;

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
        public static bool IsWildcardMatch(this string input, string wildcard, bool ignoreCase = false) => WildcardMatch.IsMatch(input, wildcard, ignoreCase);

        /// <summary>Matches using the Regex-backed engine.</summary>
        public static bool IsWildcardMatchRegex(this string input, string wildcard, bool ignoreCase = false) => WildcardMatchRegex.IsMatch(input, wildcard, ignoreCase);
    }
