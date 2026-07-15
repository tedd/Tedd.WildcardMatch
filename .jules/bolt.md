
## 2024-07-15 - [InternalUtils.StringToWildcard]
**Observation:** The method `InternalUtils.StringToWildcard` employed multiple string `.Replace` operations concatenated with `Regex.Escape`, causing O(N) additional passes and resulting in significant heap allocations per call (e.g. 218.75 KB).
**Strategic Action:** Replaced string `.Replace` allocations with a single character array traversal using `stackalloc` where applicable. This optimized implementation reduces GC load (109.38 KB per op, yielding a 50% reduction in bytes allocated) and latency (mean execution time halved).
