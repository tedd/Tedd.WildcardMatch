using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Tedd
{
    internal class InternalUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string StringToWildcard(string wildcard) => Regex.Escape(wildcard).Replace(@"\*", ".*").Replace(@"\?", ".");
    }
}
