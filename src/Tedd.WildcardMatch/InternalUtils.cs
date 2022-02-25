using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Tedd;

    internal class InternalUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if NETSTANDARD2_0
    public static string StringToWildcard(string wildcard) => "^" + Regex.Escape(wildcard).Replace(@"\*", ".*").Replace(@"\?", ".") + "$";
#else
    public static string StringToWildcard(string wildcard) => "^" + Regex.Escape(wildcard).Replace(@"\*", ".*", StringComparison.Ordinal).Replace(@"\?", ".", StringComparison.Ordinal) + "$";
#endif
}
