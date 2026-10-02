using System;
using System.Runtime.CompilerServices;

namespace Tedd;

internal sealed class ShortPattern
{
    private readonly Entry[] _literals;
    private readonly ulong _stars, _questions, _accept;
    private readonly bool _singleline;
    private readonly struct Entry
    {
        internal readonly char Code;
        internal readonly ulong Mask;
        internal Entry(char code, ulong mask) { Code = code; Mask = mask; }
    }
    private ShortPattern(Entry[] literals, ulong stars, ulong questions, ulong accept, bool singleline)
    { _literals = literals; _stars = stars; _questions = questions; _accept = accept; _singleline = singleline; }

    internal static ShortPattern? Create(string pattern, bool singleline)
    {
        if (pattern.Length > 63 || pattern.IndexOf('*') < 0) return null;
        // H-055: bounded initialized scratch and collision-free low-bit buckets.
        // Exact Code verification handles arbitrary UTF-16, including NUL collisions.
        Span<char> codes = stackalloc char[63];
        Span<ulong> masks = stackalloc ulong[63];
        int count = 0, tokenIndex = 0;
        ulong stars = 0, questions = 0;
        bool previousStar = false;
        foreach (char token in pattern)
        {
            if (token == '*' && previousStar) continue;
            previousStar = token == '*';
            ulong bit = 1UL << tokenIndex++;
            if (token == '*') stars |= bit;
            else if (token == '?') questions |= bit;
            else
            {
                int index = 0;
                while (index < count && codes[index] != token) index++;
                if (index == count) { codes[count] = token; masks[count++] = 0; }
                masks[index] |= bit;
            }
        }
        int size = 1;
        while (size <= 64)
        {
            bool collision = count > size;
            for (int i = 0; i < count && !collision; i++)
                for (int j = 0; j < i; j++)
                    if ((codes[i] & (size - 1)) == (codes[j] & (size - 1))) { collision = true; break; }
            if (!collision) break;
            size *= 2;
        }
        if (size > 64) return null;
        var entries = new Entry[size];
        for (int i = 0; i < count; i++) entries[codes[i] & (size - 1)] = new Entry(codes[i], masks[i]);
        return new ShortPattern(entries, stars, questions, 1UL << tokenIndex, singleline);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsMatch(ReadOnlySpan<char> input)
    {
        ulong active = 1 | ((1 & _stars) << 1);
        for (int i = 0; i < input.Length; i++)
        {
            char value = input[i];
            if (i == input.Length - 1 && value == '\n' && (active & _accept) != 0) return true;
            ref readonly Entry entry = ref _literals[value & (_literals.Length - 1)];
            ulong literals = entry.Code == value ? entry.Mask : 0;
            bool canConsume = _singleline || value != '\n';
            active = ((active & (literals | (canConsume ? _questions : 0))) << 1) | (canConsume ? active & _stars : 0);
            // Consecutive stars were collapsed: one epsilon step closes every state.
            // At most 63 tokens leave bit 63 for acceptance; no shift by 64 occurs.
            active |= (active & _stars) << 1;
            if (active == 0) return false;
        }
        return (active & _accept) != 0;
    }
}
