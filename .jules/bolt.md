## 2024-05-18 - InternalUtils StringToWildcard Optimization
**Observation:** `Regex.Escape` coupled with multiple `Replace` calls creates superfluous heap allocations necessitating Garbage Collection cycles.
**Strategic Action:** Substituted `Regex.Escape` with exact-capacity `StringBuilder` iteration to eliminate intermediate array allocations, reducing runtime and space complexity directly to O(n) space and time.
