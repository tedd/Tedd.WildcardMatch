using System;
using System.Text.RegularExpressions;

namespace Tedd.Archive
{
    public static class InternalUtilsLegacy
    {
#if NETSTANDARD2_0
        public static string StringToWildcard(string wildcard) => "^" + Regex.Escape(wildcard).Replace(@"\*", ".*").Replace(@"\?", ".") + "$";
#else
        public static string StringToWildcard(string wildcard) => "^" + Regex.Escape(wildcard).Replace(@"\*", ".*", StringComparison.Ordinal).Replace(@"\?", ".", StringComparison.Ordinal) + "$";
#endif
    }
}
