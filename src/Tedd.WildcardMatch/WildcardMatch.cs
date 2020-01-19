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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsMatch(string input, string wildcard, bool ignoreCase = false) => Regex.IsMatch(input, InternalUtils.StringToWildcard(wildcard), ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsMatch(string input, string wildcard, WildcardOptions options) => Regex.IsMatch(input, InternalUtils.StringToWildcard(wildcard), (RegexOptions)options);

        public WildcardMatch(string wildcard) : this(wildcard, WildcardOptions.None) { }
        public WildcardMatch(string wildcard, WildcardOptions options) : this(wildcard, options, Regex.InfiniteMatchTimeout) { }
        public WildcardMatch(string wildcard, WildcardOptions options, TimeSpan timeout) => _regex = new Regex(InternalUtils.StringToWildcard(wildcard), (RegexOptions)options, timeout);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsMatch(string input) => _regex.IsMatch(input);
    }
}
