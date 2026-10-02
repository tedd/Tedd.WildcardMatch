using System;

namespace Tedd;

    [Flags]
    public enum WildcardOptions
    {
        /// <summary>
        /// Specifies that no options are set.
        /// </summary>
        None = 0x0000,
        /// <summary>
        /// Specifies case-insensitive matching.
        /// </summary>
        IgnoreCase = 0x0001, // "i"
        /// <summary>
        /// Specifies single-line mode. Changes the meaning of the star (*) and questionmark (?) so they match every character (instead of every character except \n).
        /// </summary>
        Singleline = 0x0010, // "s"
        /// <summary>
        /// Compiles the Regex-backed engine. The direct engine accepts this flag without additional compilation.
        /// </summary>
        Compiled = 0x0008, // "c"
        /// <summary>
        /// Searches from right to left in the Regex-backed engine; the direct engine preserves the same Boolean result.
        /// </summary>
        RightToLeft = 0x0040, // "r"
        /// <summary>
        /// Specifies that cultural differences in language is ignored.
        /// </summary>
        CultureInvariant = 0x0200,
    }
