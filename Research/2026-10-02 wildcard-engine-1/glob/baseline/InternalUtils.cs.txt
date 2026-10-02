using System;
using System.Text.RegularExpressions;

namespace Tedd;

internal static class InternalUtils
{
    private static readonly char[] WildcardCharacters = { '*', '?' };
    private static readonly bool[] EscapedCharacters = CreateEscapedCharacters();

    private static bool[] CreateEscapedCharacters()
    {
        var characters = new bool[128];
        foreach (char character in "\t\n\f\r #$()+.[\\^{|")
            characters[character] = true;
        return characters;
    }

    public static string StringToWildcard(string wildcard)
    {
        if (wildcard is null)
#pragma warning disable CA2208 // Preserve the existing Regex.Escape exception metadata.
            throw new ArgumentNullException("str"); // Preserve Regex.Escape's parameter name.
#pragma warning restore CA2208
        if (wildcard.Length == 0)
            return "^$";

        // Keep Regex.Escape's bulk literal scan; scalar rewriting regresses long literals.
        // The wildcard path fuses escaping, replacement and anchors without intermediate strings.
        // Evidence: Research/2026-10-01 wildcard-hotpath-1, H-003/H-007/H-011/H-013.
        int firstWildcard = wildcard.IndexOfAny(WildcardCharacters);
        if (firstWildcard < 0)
            return "^" + Regex.Escape(wildcard) + "$";

        // Bulk library scans outperform scalar rewriting for long, sparse patterns.
        // Stop once wildcard density exceeds 1/32; a fixed count misclassifies long
        // sparse patterns with five or more wildcards (H-015/H-016).
        if (wildcard.Length >= 128)
        {
            int count = 0;
            int denseCount = wildcard.Length / 32 + 1;
            int next = firstWildcard;
            bool hasStar = false;
            bool hasQuestion = false;
            do
            {
                hasStar |= wildcard[next] == '*';
                hasQuestion |= wildcard[next] == '?';
                count++;
                if (count == denseCount)
                    break;
                next = wildcard.IndexOfAny(WildcardCharacters, next + 1);
            } while (next >= 0);

            if (count < denseCount)
            {
                string escaped = Regex.Escape(wildcard);
                if (hasStar)
                    escaped = escaped.Replace(@"\*", ".*");
                if (hasQuestion)
                    escaped = escaped.Replace(@"\?", ".");
                return "^" + escaped + "$";
            }
        }

        // Each UTF-16 code unit emits at most two characters; only the initialized prefix
        // is copied into the result. The escape set deliberately excludes ']' and '}'.
        var buffer = new char[checked(wildcard.Length * 2 + 2)];
        int position = 0;
        buffer[position++] = '^';
        foreach (char character in wildcard)
        {
            if (character == '*')
            {
                buffer[position++] = '.';
                buffer[position++] = '*';
            }
            else if (character == '?')
                buffer[position++] = '.';
            else
            {
                if (character < EscapedCharacters.Length && EscapedCharacters[character])
                    buffer[position++] = '\\';
                buffer[position++] = character switch
                {
                    '\t' => 't',
                    '\n' => 'n',
                    '\r' => 'r',
                    '\f' => 'f',
                    _ => character
                };
            }
        }
        buffer[position++] = '$';
        return new string(buffer, 0, position);
    }
}
