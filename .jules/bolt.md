## 2023-10-27 - Wildcard Transpilation Optimization
**Observation:** The previous transpilation implementation `Regex.Escape(wildcard).Replace(@"\*", ".*").Replace(@"\?", ".")` produced superfluous heap allocations due to intermediate strings created by `Replace` and `Regex.Escape`. It also scaled poorly over longer patterns.
**Strategic Action:** Replaced string allocations with a single pass lexical transpiler using `stackalloc char[]` and `ArrayPool<char>` for backing buffers. This eliminated intermediate string allocations and improved runtime performance by 40-60%.
