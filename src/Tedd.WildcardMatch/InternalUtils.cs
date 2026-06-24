using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Tedd
{
    internal static class InternalUtils
    {
        /// <summary>
        /// Converts a wildcard string to a Regex string pattern in a single pass.
        /// Time Complexity: O(N) where N is the length of the wildcard string.
        /// Space Complexity: O(N) auxiliary space for the resulting string buffer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string StringToWildcard(string wildcard)
        {
            if (string.IsNullOrEmpty(wildcard)) return "^$";

            int length = 2; // ^ and $
            int wildcardLength = wildcard.Length;
            for (int i = 0; i < wildcardLength; i++)
            {
                char c = wildcard[i];
                if (c == '*') length += 2;
                else if (c == '?') length += 1;
                else if (IsRegexChar(c))
                {
                    if (c == '\x85') length += 4; // \x85
                    else length += 2;
                }
                else length += 1;
            }

#if NETSTANDARD2_0
            var sb = new StringBuilder(length);
            sb.Append('^');
            for (int i = 0; i < wildcardLength; i++)
            {
                char c = wildcard[i];
                if (c == '*')
                {
                    sb.Append('.').Append('*');
                }
                else if (c == '?')
                {
                    sb.Append('.');
                }
                else if (IsRegexChar(c))
                {
                    sb.Append('\\');
                    if (c == '\t') sb.Append('t');
                    else if (c == '\n') sb.Append('n');
                    else if (c == '\f') sb.Append('f');
                    else if (c == '\r') sb.Append('r');
                    else if (c == '\v') sb.Append('v');
                    else if (c == '\x85') sb.Append("x85");
                    else sb.Append(c);
                }
                else
                {
                    sb.Append(c);
                }
            }
            sb.Append('$');
            return sb.ToString();
#else
            return string.Create(length, wildcard, (span, state) => {
                span[0] = '^';
                int pos = 1;
                int stateLength = state.Length;
                for (int i = 0; i < stateLength; i++) {
                    char c = state[i];
                    if (c == '*') {
                        span[pos++] = '.';
                        span[pos++] = '*';
                    } else if (c == '?') {
                        span[pos++] = '.';
                    } else if (IsRegexChar(c)) {
                        span[pos++] = '\\';
                        if (c == '\t') span[pos++] = 't';
                        else if (c == '\n') span[pos++] = 'n';
                        else if (c == '\f') span[pos++] = 'f';
                        else if (c == '\r') span[pos++] = 'r';
                        else if (c == '\v') span[pos++] = 'v';
                        else if (c == '\x85') {
                            span[pos++] = 'x';
                            span[pos++] = '8';
                            span[pos++] = '5';
                        }
                        else span[pos++] = c;
                    } else {
                        span[pos++] = c;
                    }
                }
                span[pos] = '$';
            });
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsRegexChar(char c)
        {
            switch (c)
            {
                case '\t':
                case '\n':
                case '\f':
                case '\r':
                case '\v':
                case '\x85':
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
}
