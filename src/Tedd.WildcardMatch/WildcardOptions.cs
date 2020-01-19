using System;

namespace Tedd
{
    [Flags]
    public enum WildcardOptions
    {
        None = 0x0000,
        IgnoreCase = 0x0001, // "i"
        Compiled = 0x0008, // "c"
        RightToLeft = 0x0040, // "r"
        CultureInvariant = 0x0200,
    }
}