
## 2023-10-25 - InternalUtils.StringToWildcard Allocation Reduction
**Observation:** `InternalUtils.StringToWildcard` previously utilized `Regex.Escape` followed by multiple `string.Replace` calls. This chained allocation strategy instantiated redundant intermediate string objects and character arrays, severely impacting garbage collection for frequent complex wildcard parsing operations.
**Strategic Action:** Transposed the parsing algorithm to size an exact buffer a-priori (time complexity O(N)), iterating the source string to directly populate a `char[]` buffer and bypassing `.Replace()` operations entirely. Emitted strings are instantiated via a single span `new string(buffer, 0, exactSize)`, dramatically reducing heap allocation overhead.

## 2023-10-25 - Fix Regex escape semantics in StringToWildcard
**Observation:** `Regex.Escape` explicitly maps control characters like `\n`, `\r`, `\t`, and `\f` to a backslash followed by their corresponding alphabetical letter.
**Strategic Action:** Updated `StringToWildcard` to emit the correct alphabetic characters for control keys to match the semantic behavior of `.NET Regex.Escape`. Implemented zero allocation `string.Create` for newer frameworks while utilizing a correctly sized array buffer allocation for `.NET Standard 2.0`.
