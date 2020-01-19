using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Tedd
{
    public static class WildcardMatchExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsWildcardMatch(this string input, string wildcard, bool ignoreCase = false) => Regex.IsMatch(input, InternalUtils.StringToWildcard(wildcard), ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None);
    }
}
