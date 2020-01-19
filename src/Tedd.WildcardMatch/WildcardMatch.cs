using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Tedd
{
    public class WildcardMatch
    {
        private readonly Regex _regex;

        /// <summary>
        /// Wildcard pattern this instance will match.
        /// </summary>
        public string Wildcard
        {
            get;
            private set;
        }

        /// <summary>
        /// Regex pattern used by the underlying regex engine to match wildcard.
        /// </summary>
        public string WildcardRegex
        {
            get;
            private set;
        }

        /// <summary>
        /// Check if wildcard string matches input string.
        /// </summary>
        /// <param name="input">The string to search.</param>
        /// <param name="wildcard">The wildcard pattern to search for.</param>
        /// <param name="ignoreCase">Ignore casing.</param>
        /// <returns>True if wildcard pattern matches input string.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsMatch(string input, string wildcard, bool ignoreCase = false) => Regex.IsMatch(input, InternalUtils.StringToWildcard(wildcard), ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None);
        /// <summary>
        /// Check if wildcard string matches input string.
        /// </summary>
        /// <param name="input">The string to search.</param>
        /// <param name="wildcard">The wildcard pattern to search for.</param>
        /// <param name="options">Options to pass to engine.</param>
        /// <returns>True if wildcard pattern matches input string.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsMatch(string input, string wildcard, WildcardOptions options) => Regex.IsMatch(input, InternalUtils.StringToWildcard(wildcard), (RegexOptions)options);

        /// <summary>
        /// Creates an instance of wildcard pattern matching suited for reuse.
        /// </summary>
        /// <param name="wildcard">Wildcard pattern to match.</param>
        public WildcardMatch(string wildcard) : this(wildcard, WildcardOptions.None, Regex.InfiniteMatchTimeout) { }

        /// <summary>
        /// Creates an instance of wildcard pattern matching suited for reuse.
        /// </summary>
        /// <param name="wildcard">Wildcard pattern to match.</param>
        /// <param name="options">Option flags to pass to engine. Default is None.</param>
        public WildcardMatch(string wildcard, WildcardOptions options) : this(wildcard, options, Regex.InfiniteMatchTimeout) { }

        /// <summary>
        /// Creates an instance of wildcard pattern matching suited for reuse.
        /// </summary>
        /// <param name="wildcard">Wildcard pattern to match.</param>
        /// <param name="options">Option flags to pass to engine. Default is None.</param>
        /// <param name="timeout">How long engine should attempt to resolve pattern. Default is infinitely.</param>
        public WildcardMatch(string wildcard, WildcardOptions options, TimeSpan timeout)
        {
            Wildcard = wildcard;
            WildcardRegex = InternalUtils.StringToWildcard(wildcard);
            _regex = new Regex(WildcardRegex, (RegexOptions)options, timeout);
        }

        /// <summary>
        /// Check if current wildcard matches input string.
        /// </summary>
        /// <param name="input">String to match.</param>
        /// <returns>True if wildcard pattern matches input string.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsMatch(string input) => _regex.IsMatch(input);
    }
}
