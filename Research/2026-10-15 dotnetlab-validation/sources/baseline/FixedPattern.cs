using System;
using System.Runtime.CompilerServices;
#if NET10_0_OR_GREATER
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
#endif

namespace Tedd;

internal sealed class FixedPattern
{
    private readonly string _pattern;
    private readonly bool _singleline;
    private readonly int _leadingQuestions;
#if NET10_0_OR_GREATER
    private readonly Vector128<ushort> _pattern0, _pattern1, _mask0, _mask1;
    private readonly Block[]? _blocks;
    private readonly struct Block
    {
        internal readonly Vector256<ushort> Pattern, Mask;
        internal Block(Vector256<ushort> pattern, Vector256<ushort> mask) { Pattern = pattern; Mask = mask; }
    }
#endif

    private FixedPattern(string pattern, bool singleline)
    {
        _pattern = pattern;
        _singleline = singleline;
        if (pattern.Length >= 32 && pattern[0] == '?' && pattern[1] == '?')
        {
            int run = 2;
            while (run < pattern.Length && pattern[run] == '?') run++;
            _leadingQuestions = run;
        }
#if NET10_0_OR_GREATER
        if (Vector256.IsHardwareAccelerated && pattern.Length > 16 && pattern.Length <= 8192 && _leadingQuestions != pattern.Length)
        {
            int count = (pattern.Length + 15) / 16;
            _blocks = new Block[count];
            // H-054: one immutable array pairs masks with values; cap payload at 32 KiB.
            // Every lane is initialized and each overlapping tail load ends at Length.
            Span<ushort> mask = stackalloc ushort[16];
            ref ushort p = ref Unsafe.As<char, ushort>(ref MemoryMarshal.GetReference(pattern.AsSpan()));
            for (int block = 0; block < count; block++)
            {
                int offset = Math.Min(block * 16, pattern.Length - 16);
                for (int lane = 0; lane < 16; lane++) mask[lane] = pattern[offset + lane] == '?' ? (ushort)0 : ushort.MaxValue;
                _blocks[block] = new Block(Vector256.LoadUnsafe(ref p, (nuint)offset), Vector256.LoadUnsafe(ref MemoryMarshal.GetReference(mask)));
            }
        }
        if (Vector128.IsHardwareAccelerated && pattern.Length >= 8 && pattern.Length <= 16)
        {
            Span<ushort> mask = stackalloc ushort[pattern.Length];
            for (int i = 0; i < mask.Length; i++) mask[i] = pattern[i] == '?' ? (ushort)0 : ushort.MaxValue;
            ref ushort p = ref Unsafe.As<char, ushort>(ref MemoryMarshal.GetReference(pattern.AsSpan()));
            _pattern0 = Vector128.LoadUnsafe(ref p);
            _pattern1 = Vector128.LoadUnsafe(ref p, (nuint)(pattern.Length - 8));
            _mask0 = Vector128.LoadUnsafe(ref MemoryMarshal.GetReference(mask));
            _mask1 = Vector128.LoadUnsafe(ref MemoryMarshal.GetReference(mask), (nuint)(pattern.Length - 8));
        }
#endif
    }

    internal static FixedPattern? Create(string pattern, bool singleline) =>
        pattern.IndexOf('*') < 0 && pattern.IndexOf('?') >= 0 ? new FixedPattern(pattern, singleline) : null;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsMatch(ReadOnlySpan<char> input)
    {
        int length = _pattern.Length;
        if (input.Length != length && !(input.Length == length + 1 && input[length] == '\n')) return false;
#if NET10_0_OR_GREATER
        if (Vector256.IsHardwareAccelerated && _blocks != null)
        {
            // H-035: fuse literal mismatch and wildcard LF errors before reduction.
            // Only the logical input prefix is loaded; literal LF remains literal.
            ref ushort text = ref Unsafe.As<char, ushort>(ref MemoryMarshal.GetReference(input));
            for (int block = 0; block < _blocks!.Length; block++)
            {
                int offset = Math.Min(block * 16, length - 16);
                Vector256<ushort> a = Vector256.LoadUnsafe(ref text, (nuint)offset);
                ref readonly Block plan = ref _blocks[block];
                Vector256<ushort> mask = plan.Mask;
                Vector256<ushort> bad = (a ^ plan.Pattern) & mask;
                if (!_singleline) bad |= Vector256.Equals(a, Vector256.Create((ushort)'\n')) & ~mask;
                if (!Vector256.EqualsAll(bad, Vector256<ushort>.Zero)) return false;
            }
            return true;
        }
        if (Vector128.IsHardwareAccelerated && length >= 8 && length <= 16)
        {
            // H-035: fuse literal mismatch and wildcard LF errors before reduction.
            // Only the logical input prefix is loaded; literal LF remains literal.
            ref ushort text = ref Unsafe.As<char, ushort>(ref MemoryMarshal.GetReference(input));
            Vector128<ushort> a = Vector128.LoadUnsafe(ref text);
            Vector128<ushort> b = Vector128.LoadUnsafe(ref text, (nuint)(length - 8));
            Vector128<ushort> bad = ((a ^ _pattern0) & _mask0) | ((b ^ _pattern1) & _mask1);
            if (!_singleline)
            {
                Vector128<ushort> lf = Vector128.Create((ushort)'\n');
                bad |= (Vector128.Equals(a, lf) & ~_mask0) | (Vector128.Equals(b, lf) & ~_mask1);
            }
            return Vector128.EqualsAll(bad, Vector128<ushort>.Zero);
        }
#endif
        // H-020 portable fallback also bounds the plan footprint for huge patterns.
        if (!_singleline && input.Slice(0, _leadingQuestions).IndexOf('\n') >= 0) return false;
        for (int i = _leadingQuestions; i < length; i++)
        {
            char token = _pattern[i];
            if (token == '?' ? !_singleline && input[i] == '\n' : token != input[i]) return false;
        }
        return true;
    }
}
