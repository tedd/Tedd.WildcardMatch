using System;

namespace Tedd
{
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
        /// Specifies that the regular expression is compiled to an assembly. This yields faster execution but increases startup time.
        /// </summary>
        Compiled = 0x0008, // "c"
        /// <summary>
        /// Specifies that the search will be from right to left instead of from left to right.
        /// </summary>
        RightToLeft = 0x0040, // "r"
        /// <summary>
        /// Specifies that cultural differences in language is ignored.
        /// </summary>
        CultureInvariant = 0x0200,
    }
}