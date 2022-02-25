
# Tedd.WildcardMatch
Fast and reliable .Net library for wildcard (\* and ?) matching capable of complex pattern matching.

Available as NuGet package: https://www.nuget.org/packages/Tedd.WildcardMatch

## Fast
One match takes 0.0000003278 milliseconds on a modern computer. There are at time of writing (jan 2020) faster libraries, but they are not reliable.

## Reliable
Many (of not most) examples of wildcard matching found on the web fail to implement proper support for wildcard patterns, meaning they will not always give the intended result. This library uses the Regex engine in .Net to implement proper wildcard support. This means it capable of reliably matching complex wildcard patterns.
(See further down for example of how the "FastWildcard"-library advertising "no edge-cases" in the NuGet listing breaks down on a simple match.)

# Example

## Extension method
```csharp
// Standard matching (case sensitive)
var match = "Lorem ipsum".IsWildcardMatch("or*ips?m");        
// Will not match because L in Lorem is incorrect case
var caseNotMatch = "Lorem ipsum".IsWildcardMatch("lor?m");   
// Set it to ignore case
var caseMatch = "Lorem ipsum".IsWildcardMatch("lor?m", true); 
```
## Static
```csharp
// Standard matching (case sensitive)
var alsoMatch = WildcardMatch.IsMatch("Lorem", "L??em");
// Use Options to set it to ignore case
var andThis = WildcardMatch.IsMatch("lorem", "L?REM", WildcardOptions.IgnoreCase);
```
## Instance
```csharp
var wm = new WildcardMatch("or*ips?m");
// Standard match
var match1 = wm.IsMatch("Lorem ipsum");
// Reuse object for faster second match
var match2 = wm.IsMatch("Bored Chipsom");

// A compiled instance is slighly slower at startup
var wmc = new WildcardMatch("or*ips?m", WildcardOptions.Compiled | WildcardOptons.IgnoreCase);
// But matching is faster
var match3 = wmc.IsMatch("More ipsums");

```

# WildcardOptions
| Option          | Description  |
|--|--|
| None            | Specifies that no options are set. |
| IgnoreCase      | Specifies case-insensitive matching. |
| Singleline      | Specifies single-line mode. Changes the meaning of the star (*) and questionmark (?) so they match every character (instead of every character except \n). |
| Compiled        | Specifies that the regular expression is compiled to an assembly. This yields faster execution but increases startup time. |
| CultureInvariant| Specifies that cultural differences in language is ignored. |
| RightToLeft     | Specifies that the search will be from right to left instead of from left to right. |

# Tips on performance

## User input
Remember that overuse of multiple wildcards (especially star) on large amounts of text may lead to high CPU usage. By default the matcher will run infinitely. If you want to limit the time it runs to for example 0.1 seconds then you must provide a timeout parameter when creating class instance.

In case where you take wildcard from user it is advicable to implement a limit so that user can't do Denial Of Service by crafting special wildcard patterns.

## Slow (0.000004 seconds to match)
Using extension method or static methods invokes parsed execution, this is relatively slow by all means. "Slow" in this context means less than 0.000001 seconds, so unless you are planning to parse a lot of matches you won't notice it.

## Faster: Reusing instance
If you intend to reuse same pattern om multiple matches then it may beneficial to use an instanced WildcardMatch. By instancing the pattern match you save the setup-time.

## Fastest: Precompiled
Providing the WildcardOptions.Compiled option will cause slightly higher start cost as the match is compiled into the assembly, but give better performance on matches. One of the major benefints here is that once object is set up it can perform matching with zero memory allocations, which is good for GC.<br />
This approach scales very well, giving very high performance on complext pattern matches and will not lead to GC hickups.

