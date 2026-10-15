# Complete matching comparison

Mean ± BenchmarkDotNet 99.9% confidence-interval half-width, nanoseconds per match. Each cell retains BenchmarkDotNet’s original outlier policy; unfiltered iteration output is in benchmark.txt. These intervals concern iteration measurements, not independent machines.

| Implementation and call mode | Literal | Simple | MultiStar | LongText |
| --- | ---: | ---: | ---: | ---: |
| DotNetGlob (managed) | 15.868 ± 0.026 | 22.198 ± 0.058 | 59.702 ± 0.552 | 7312.846 ± 78.750 |
| FastWildcard (managed) | 74.317 ± 0.687 | 83.305 ± 0.166 | 68.088 ± 0.045 | 281.658 ± 0.615 |
| TeddDirectReused (managed) | 5.233 ± 0.002 | 4.562 ± 0.001 | 6.622 ± 0.062 | 183.406 ± 1.898 |
| TeddDirectStatic (managed) | 14.100 ± 0.019 | 19.155 ± 0.007 | 35.119 ± 0.010 | 222.323 ± 0.503 |
| TeddRegexCompiled (managed) | 36.500 ± 0.223 | 37.433 ± 0.144 | 52.236 ± 0.281 | 395.955 ± 0.872 |
| TeddRegexReused (managed) | 87.401 ± 0.203 | 82.387 ± 0.280 | 216.131 ± 1.564 | 24308.254 ± 12.887 |
| TeddRegexStatic (managed) | 219.419 ± 4.298 | 167.174 ± 0.859 | 289.405 ± 1.471 | 24410.997 ± 16.692 |
| WildcardMatch (managed) | 29.964 ± 0.062 | 35.021 ± 0.111 | 136.359 ± 1.767 | 13473.288 ± 192.232 |
| GitWildmatch (batch_cloned) | 26.262 ± 0.322 | 28.950 ± 0.094 | 28.904 ± 0.264 | 841.936 ± 0.806 |
| Krauss (batch_cloned) | 9.420 ± 0.179 | 9.557 ± 0.007 | 9.602 ± 0.046 | 235.801 ± 0.417 |
| Pcre2Jit (batch_cloned) | 16.053 ± 0.077 | 16.183 ± 0.006 | 22.515 ± 0.088 | 1472.601 ± 11.717 |
| ZemaWildcards (batch_cloned) | 78.486 ± 0.555 | 81.219 ± 0.558 | 152.846 ± 0.351 | 22056.586 ± 16.876 |
| GitWildmatch (batch_shared) | 26.025 ± 0.021 | 28.987 ± 0.063 | 28.240 ± 0.019 | 841.494 ± 0.908 |
| Krauss (batch_shared) | 9.325 ± 0.003 | 9.560 ± 0.005 | 9.582 ± 0.042 | 234.287 ± 0.598 |
| Pcre2Jit (batch_shared) | 15.810 ± 0.006 | 16.191 ± 0.009 | 22.487 ± 0.088 | 1466.610 ± 7.726 |
| ZemaWildcards (batch_shared) | 79.615 ± 1.495 | 83.047 ± 2.153 | 152.514 ± 0.454 | 22050.382 ± 11.732 |
| GitWildmatch (normal_cloned) | 27.814 ± 0.013 | 31.785 ± 0.074 | 30.014 ± 0.020 | — |
| GitWildmatch (suppressed_cloned) | 27.235 ± 0.021 | 30.269 ± 0.059 | 29.704 ± 0.021 | — |
| Krauss (normal_cloned) | 11.116 ± 0.010 | 11.337 ± 0.005 | 11.301 ± 0.026 | — |
| Krauss (suppressed_cloned) | 10.516 ± 0.007 | 11.029 ± 0.004 | 10.976 ± 0.019 | — |
| ZemaWildcards (normal_cloned) | 79.641 ± 0.459 | 81.729 ± 0.403 | 157.228 ± 3.112 | — |
| ZemaWildcards (suppressed_cloned) | 78.923 ± 0.899 | 81.357 ± 0.625 | 153.148 ± 0.300 | — |
| GitWildmatch (normal_shared) | 28.173 ± 0.011 | 32.079 ± 0.088 | 31.918 ± 0.057 | — |
| GitWildmatch (suppressed_shared) | 27.478 ± 0.009 | 30.645 ± 0.056 | 31.601 ± 0.058 | — |
| Krauss (normal_shared) | 11.413 ± 0.005 | 11.634 ± 0.003 | 11.603 ± 0.041 | — |
| Krauss (suppressed_shared) | 10.809 ± 0.003 | 11.329 ± 0.006 | 11.296 ± 0.029 | — |
| ZemaWildcards (normal_shared) | 79.766 ± 0.591 | 82.448 ± 0.613 | 154.918 ± 0.452 | — |
| ZemaWildcards (suppressed_shared) | 80.353 ± 1.980 | 81.924 ± 0.590 | 154.528 ± 1.191 | — |

Native byte buffers and managed UTF-16 inputs are prepared before timing. Batch mode uses one normal transition per 256 matches; normal/suppressed modes cross once per input into the identical native export. All modes include their loop and dispatch costs. Suppression is qualified only for the fixed three short fixtures and Krauss/Git/Zema. PCRE2 and long calls retain normal transitions.

The reused Tedd matcher retains pattern preparation. Static Tedd timings include classification on every call. Native wildcard implementations interpret their patterns per match; PCRE2 retains JIT compilation. Construction is reported separately and is excluded from the table.
