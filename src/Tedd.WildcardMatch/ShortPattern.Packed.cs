using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#if NET10_0_OR_GREATER
using System.Numerics;
using System.Runtime.Intrinsics;
#endif

namespace Tedd;

internal sealed partial class ShortPattern
{
    private readonly ulong _packedValue, _packedMask;
    private readonly int _packedLength;
    private readonly char _packedTail;

    internal bool IsPacked => _packedLength != 0;

    private ShortPattern(ulong value, ulong mask, int length, char tail, bool singleline)
    {
        _literals = Array.Empty<Entry>();
        _packedValue = value;
        _packedMask = mask;
        _packedLength = length;
        _packedTail = tail;
        _singleline = singleline;
    }

    private static ShortPattern? CreatePacked(string pattern, bool singleline)
    {
        // Two star-separated segments: 3–4 UTF-16 units, then one literal.
        // Other shapes and big-endian machines retain the bitmap matcher.
        if (!BitConverter.IsLittleEndian || pattern.Length < 7 || pattern[0] != '*'
            || pattern[pattern.Length - 1] != '*' || pattern.IndexOf('\n') >= 0) return null;
        int start = 0;
        while (start < pattern.Length && pattern[start] == '*') start++;
        int end = start;
        while (end < pattern.Length && pattern[end] != '*') end++;
        int length = end - start;
        if (length < 3 || length > 4) return null;
        int next = end;
        while (next < pattern.Length && pattern[next] == '*') next++;
        if (next >= pattern.Length || pattern[next] == '?') return null;
        char tail = pattern[next++];
        if (next >= pattern.Length) return null;
        while (next < pattern.Length && pattern[next] == '*') next++;
        if (next != pattern.Length) return null;

        ulong value = 0, mask = 0;
        for (int i = 0; i < length; i++)
            if (pattern[start + i] != '?')
            {
                value |= (ulong)pattern[start + i] << (16 * i);
                mask |= 65535UL << (16 * i);
            }
        return new ShortPattern(value, mask, length, tail, singleline);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong ReadWord(ReadOnlySpan<char> input, int offset)
    {
#if NET10_0_OR_GREATER
        return Unsafe.ReadUnaligned<ulong>(ref Unsafe.As<char, byte>(
            ref Unsafe.Add(ref MemoryMarshal.GetReference(input), offset)));
#else
        return MemoryMarshal.Read<ulong>(MemoryMarshal.AsBytes(input.Slice(offset, 4)));
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool MatchPacked(ReadOnlySpan<char> input)
    {
        int length = input.Length;
        // This plan has no literal LF, so only the optional final anchor LF is
        // removable. Interior LF validation can wait until a candidate succeeds.
        if (!_singleline && length > 0 && input[length - 1] == '\n') length--;
#if NET10_0_OR_GREATER
        if (Vector128.IsHardwareAccelerated && (uint)(length - 4) < 4)
        {
            // Overlapping first/last four-unit reads cover precisely 4–7 units.
            // The upper half maps lane i to input index i + length - 8.
            Vector128<ushort> data = Vector128.Create(ReadWord(input, 0), ReadWord(input, length - 4)).AsUInt16();
            uint bits = Vector128.Equals(data, Vector128.Create((ushort)_packedTail)).AsByte().ExtractMostSignificantBits();
            int slot = (31 - BitOperations.LeadingZeroCount(bits)) >> 1;
            int tail = slot >= 4 ? slot + length - 8 : slot;
            int last = tail - _packedLength;
            for (int i = 0; i <= last; i++)
                if ((ReadWord(input, i) & _packedMask) == _packedValue)
                    return _singleline || !Vector128.EqualsAny(data, Vector128.Create((ushort)'\n'));
            return false;
        }
#endif
        return MatchPackedScalar(input, length);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private bool MatchPackedScalar(ReadOnlySpan<char> input, int length)
    {
        if (length <= _packedLength) return false;
        int finalTail = input.Slice(0, length).LastIndexOf(_packedTail);
        int finalStart = finalTail - _packedLength;
        // A successful first segment must finish before the rightmost tail.
        // Width >= 3 plus the tail proves i + 4 <= length for every word read.
        for (int i = 0; i <= finalStart; i++)
            if ((ReadWord(input, i) & _packedMask) == _packedValue)
                return _singleline || input.Slice(0, length).IndexOf('\n') < 0;
        return false;
    }
}
