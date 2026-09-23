using System;
using System.Text.RegularExpressions;

namespace Tedd.Archive
{
    public static class LegacyInternalUtils
    {
        public static string StringToWildcard(string wildcard) => "^" + Regex.Escape(wildcard).Replace(@"\*", ".*").Replace(@"\?", ".") + "$";
    }
}
