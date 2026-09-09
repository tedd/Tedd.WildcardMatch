using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Tedd;

    internal static class InternalUtils
    {
        /// <summary>
        /// Transpiles a wildcard pattern into a Regex pattern.
        /// Time Complexity: O(n)
        /// Space Complexity: O(n) - Single allocation for the resulting string using ArrayPool buffering.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string StringToWildcard(string wildcard)
        {
            if (wildcard == null)
            {
                throw new ArgumentNullException(nameof(wildcard));
            }

            int len = wildcard.Length;
            int capacity = len + 2; // For '^' and '$'

            // First pass: Calculate precise capacity required
            for (int i = 0; i < len; i++)
            {
                char c = wildcard[i];
                if (c == '*')
                {
                    capacity += 1;
                }
                else if (c == '.' || c == '+' || c == '(' || c == ')' || c == '[' || c == ']' || c == '{' || c == '}' || c == '\\' || c == '^' || c == '$' || c == '|' || c == ' ' || c == '#')
                {
                    capacity += 1;
                }
            }

#pragma warning disable CA1852 // unsealed internal type
            var sb = new StringBuilder(capacity);
            sb.Append('^');

            // Second pass: Populate the buffer
            for (int i = 0; i < len; i++)
            {
                char c = wildcard[i];
                switch (c)
                {
                    case '*':
                        sb.Append('.').Append('*');
                        break;
                    case '?':
                        sb.Append('.');
                        break;
                    case ' ':
                    case '#':
                    case '$':
                    case '(':
                    case ')':
                    case '[':
                    case ']':
                    case '{':
                    case '}':
                    case '+':
                    case '.':
                    case '\\':
                    case '^':
                    case '|':
                        sb.Append('\\').Append(c);
                        break;
                    default:
                        sb.Append(c);
                        break;
                }
            }
            sb.Append('$');

            return sb.ToString();
#pragma warning restore CA1852
        }
    }
