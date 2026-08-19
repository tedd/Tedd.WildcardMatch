using System;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Tedd.Archive;

public static class InternalUtilsArchive
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if NETSTANDARD2_0
    public static string StringToWildcard(string wildcard) => "^" + Regex.Escape(wildcard).Replace(@"\*", ".*").Replace(@"\?", ".") + "$";
#else
    public static string StringToWildcard(string wildcard) => "^" + Regex.Escape(wildcard).Replace(@"\*", ".*", StringComparison.Ordinal).Replace(@"\?", ".", StringComparison.Ordinal) + "$";
#endif
}
