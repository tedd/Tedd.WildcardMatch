using System;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Tedd;

internal static class InternalUtils
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string? StringToWildcard(string wildcard)
    {
        if (wildcard == null) return null;
        var length = wildcard.Length;
        // Big O complexity: Time O(n), Space O(n) where n is wildcard length.
        // It preallocates and manually writes regex.
        var p = new char[length * 2 + 2];
        p[0] = '^';
        int c = 1;
        for (int i = 0; i < length; i++)
        {
            char w = wildcard[i];
            if (w == '*')
            {
                p[c++] = '.';
                p[c++] = '*';
            }
            else if (w == '?')
            {
                p[c++] = '.';
            }
            else if (IsMetachar(w))
            {
                p[c++] = '\\';
                p[c++] = w;
            }
            else
            {
                p[c++] = w;
            }
        }
        p[c++] = '$';
        return new string(p, 0, c);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsMetachar(char ch)
    {
        switch (ch)
        {
            case '\\':
            case '*':
            case '+':
            case '?':
            case '|':
            case '{':
            case '[':
            case '(':
            case ')':
            case '^':
            case '$':
            case '.':
            case '#':
            case ' ':
            case '\t':
            case '\n':
            case '\r':
            case '\f':
            case '\v':
                return true;
            default:
                return false;
        }
    }
}
