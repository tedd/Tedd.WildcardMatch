
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

# Benchmark

## Note
An important note first: One of the other libraries in NuGet we are testing here, FastWildcard (which promises no edge cases), fails this simple edge case. Therefore the tests are kept simple and we are not testing high complexity.
``` csharp
FastWildcard.FastWildcard.IsMatch("abcd", "*a?c*e*")
true
FastWildcard.FastWildcard.IsMatch("ababcd", "*a?c*e*")
false
```

## Benchmark
Benchmarking this against similar libraries in NuGet we see that this library is 2-50 times slower. Keep in mind that complex queries could not be run because FastWildcard breaks.

Each run does 1000 matches, so timing has to be divided by 1000.

``` ini

BenchmarkDotNet=v0.12.0, OS=Windows 10.0.18363
AMD Ryzen 9 3950X, 1 CPU, 32 logical and 16 physical cores
.NET Core SDK=3.1.101
  [Host]     : .NET Core 3.1.1 (CoreCLR 4.700.19.60701, CoreFX 4.700.19.60801), X64 RyuJIT  [AttachedDebugger]
  Job-MUNHVL : .NET Core 3.1.1 (CoreCLR 4.700.19.60701, CoreFX 4.700.19.60801), X64 RyuJIT

Jit=RyuJit  Platform=X64  Runtime=.NET Core 3.1  
Force=True  LaunchCount=1  

```
### Simple matches

|                   Method |        Mean |     Error |    StdDev |         Min |         Max |      Median |         P95 |         P90 | Iterations |     Op/s |  Ratio | RatioSD | Baseline |     Gen 0 | Gen 1 | Gen 2 |  Allocated | TotalIssues/Op | BranchInstructions/Op | BranchMispredictions/Op |
|------------------------- |------------:|----------:|----------:|------------:|------------:|------------:|------------:|------------:|-----------:|---------:|-------:|--------:|--------- |----------:|------:|------:|-----------:|---------------:|----------------------:|------------------------:|
|         Tedd:Simple,Slow |    956.8 us |   8.06 us |   7.14 us |    946.6 us |    975.4 us |    957.8 us |    966.2 us |    961.3 us |      14.00 | 1,045.13 |   2.92 |    0.03 |       No |   39.0625 |     - |     - |   328009 B |      2,359,977 |               811,155 |                  12,312 |
|  Tedd:Simple,Precompiled |    327.8 us |   2.58 us |   2.29 us |    325.2 us |    332.8 us |    327.3 us |    332.2 us |    331.2 us |      14.00 | 3,050.39 |   1.00 |    0.00 |      Yes |         - |     - |     - |        3 B |        649,149 |               319,021 |                   3,646 |
|   WildcardMatcher:Simple |    542.1 us |   3.84 us |   3.59 us |    536.3 us |    548.3 us |    542.0 us |    547.3 us |    546.7 us |      15.00 | 1,844.71 |   1.65 |    0.01 |       No |   82.0313 |     - |     - |   688009 B |      1,047,506 |               493,320 |                   6,628 |
|      FastWildcard:Simple |    172.9 us |   2.25 us |   2.10 us |    169.9 us |    176.1 us |    173.4 us |    175.6 us |    175.2 us |      15.00 | 5,783.87 |   0.53 |    0.01 |       No |    2.6855 |     - |     - |    24000 B |        452,903 |               154,223 |                   1,972 |


### Complex matches
|                   Method |        Mean |     Error |    StdDev |         Min |         Max |      Median |         P95 |         P90 | Iterations |     Op/s |  Ratio | RatioSD | Baseline |     Gen 0 | Gen 1 | Gen 2 |  Allocated | TotalIssues/Op | BranchInstructions/Op | BranchMispredictions/Op |
|------------------------- |------------:|----------:|----------:|------------:|------------:|------------:|------------:|------------:|-----------:|---------:|-------:|--------:|--------- |----------:|------:|------:|-----------:|---------------:|----------------------:|------------------------:|
|        Tedd:Complex,Slow | 89,457.3 us | 436.78 us | 341.01 us | 88,778.8 us | 89,946.7 us | 89,585.9 us | 89,815.0 us | 89,698.3 us |      12.00 |    11.18 | 272.84 |    2.04 |       No |         - |     - |     - |   633488 B |    168,288,256 |            83,783,522 |                 722,132 |
| Tedd:Complex,Precompiled |  6,130.9 us |  53.47 us |  47.40 us |  6,058.0 us |  6,201.4 us |  6,147.1 us |  6,196.0 us |  6,181.5 us |      14.00 |   163.11 |  18.70 |    0.21 |       No |         - |     - |     - |        5 B |     14,527,464 |             7,241,555 |                  66,272 |
|    WildcardMatch:Complex | 11,241.4 us |  82.80 us |  77.45 us | 11,162.3 us | 11,402.6 us | 11,216.0 us | 11,359.7 us | 11,337.6 us |      15.00 |    88.96 |  34.27 |    0.30 |       No | 1953.1250 |     - |     - | 16368010 B |     22,756,797 |            10,443,898 |                 118,754 |
|     FastWildcard:Complex |  1,781.5 us |  18.02 us |  16.86 us |  1,746.1 us |  1,800.8 us |  1,786.2 us |  1,800.8 us |  1,800.5 us |      15.00 |   561.31 |   5.44 |    0.06 |       No |    1.9531 |     - |     - |    24000 B |      4,337,896 |             1,457,561 |                  21,295 |
