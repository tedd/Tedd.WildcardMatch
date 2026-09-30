using System;
using System.Runtime.CompilerServices;

namespace Tedd;

internal static class InternalUtils
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string StringToWildcard(string wildcard)
    {
        // Big O: Time complexity O(N), Space complexity O(N) where N is length of wildcard pattern
        if (wildcard == null)
        {
            throw new ArgumentNullException(nameof(wildcard));
        }

        if (wildcard.Length == 0)
        {
            return "^$";
        }

        int length = wildcard.Length;
        int exactSize = 2; // for ^ and $
        for (int i = 0; i < length; i++)
        {
            char c = wildcard[i];
            if (c == '*') exactSize += 2;
            else if (c == '?') exactSize += 1;
            else if (IsRegexSpecial(c)) exactSize += 2;
            else exactSize += 1;
        }

#if NETSTANDARD2_0
        char[] buffer = new char[exactSize];
        int pos = 0;
        buffer[pos++] = '^';

        for (int i = 0; i < length; i++)
        {
            char c = wildcard[i];
            if (c == '*')
            {
                buffer[pos++] = '.';
                buffer[pos++] = '*';
            }
            else if (c == '?')
            {
                buffer[pos++] = '.';
            }
            else
            {
                if (IsRegexSpecial(c))
                {
                    buffer[pos++] = '\\';
                    if (c == '\t') buffer[pos++] = 't';
                    else if (c == '\n') buffer[pos++] = 'n';
                    else if (c == '\f') buffer[pos++] = 'f';
                    else if (c == '\r') buffer[pos++] = 'r';
                    else buffer[pos++] = c;
                }
                else
                {
                    buffer[pos++] = c;
                }
            }
        }

        buffer[pos++] = '$';
        return new string(buffer, 0, exactSize);
#else
        return string.Create(exactSize, wildcard, (buffer, state) => {
            int pos = 0;
            buffer[pos++] = '^';

            int len = state.Length;
            for (int i = 0; i < len; i++)
            {
                char c = state[i];
                if (c == '*')
                {
                    buffer[pos++] = '.';
                    buffer[pos++] = '*';
                }
                else if (c == '?')
                {
                    buffer[pos++] = '.';
                }
                else
                {
                    if (IsRegexSpecial(c))
                    {
                        buffer[pos++] = '\\';
                        if (c == '\t') buffer[pos++] = 't';
                        else if (c == '\n') buffer[pos++] = 'n';
                        else if (c == '\f') buffer[pos++] = 'f';
                        else if (c == '\r') buffer[pos++] = 'r';
                        else buffer[pos++] = c;
                    }
                    else
                    {
                        buffer[pos++] = c;
                    }
                }
            }
            buffer[pos++] = '$';
        });
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsRegexSpecial(char c)
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
            case '*':
            case '+':
            case '.':
            case '?':
            case '[':
            case '\\':
            case '^':
            case '{':
            case '|':
                return true;
            default:
                return false;
        }
    }
}
