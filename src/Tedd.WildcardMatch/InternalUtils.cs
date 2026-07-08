using System;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Tedd;

internal static class InternalUtils
{
    /// <summary>
    /// Converts a wildcard string into a Regex string. Time Complexity: O(N) where N is length of wildcard. Space Complexity: O(N) for StringBuilder allocation.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string StringToWildcard(string wildcard)
    {
        if (string.IsNullOrEmpty(wildcard))
        {
            return "^$";
        }

        var sb = new StringBuilder(wildcard.Length + 10);
        sb.Append('^');
        for (int i = 0; i < wildcard.Length; i++)
        {
            char c = wildcard[i];
            if (c == '*')
            {
                sb.Append(".*");
            }
            else if (c == '?')
            {
                sb.Append('.');
            }
            else
            {
                switch (c)
                {
                    case '\t':
                    case '\n':
                    case '\f':
                    case '\r':
                    case ' ':
                    case '#':
                    case '$':
                    case '(':
                    case ')':
                    case '+':
                    case '.':
                    case '[':
                    case '\\':
                    case '^':
                    case '{':
                    case '|':
                        sb.Append('\\');
                        sb.Append(c);
                        break;
                    default:
                        sb.Append(c);
                        break;
                }
            }
        }
        sb.Append('$');
        return sb.ToString();
    }
}
