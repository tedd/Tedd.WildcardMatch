# Fresh normal-build confirmation

Epoch 5 delegate-caller confirmation only; earlier epochs are not pooled. Twenty paired fresh-process blocks per scenario, two warmups, High process priority, CPU 30, .NET 10.0.12 x64 managed IL, tiering disabled and matched dependency/runtime configuration. Each paired process is distinct; independence from operating-system load and thermal drift remains an assumption. The original paired budget completed without exclusions or replacements.

Ratio is the engine’s geometric paired candidate/control estimator; interval coverage is 99.706% (0.05 family error across 17 comparisons). Means include the representative delegate caller’s loop, checksum and cancellation guards and should not be substituted for the external delegate benchmark timings. Noninferiority uses the predefined 5% regression boundary.

| Scenario | Original mean ns | Candidate mean ns | Paired ratio [interval] | Within 5% regression guard | Primary classification |
| --- | ---: | ---: | --- | --- | --- |
| reused-Portfolio | 53.696 | 48.073 | 0.8953 [0.8894, 0.9012] | pass | improved |
| reused-Literal | 6.783 | 6.575 | 0.9691 [0.9537, 0.9847] | pass | equivalent_within_margin |
| reused-Simple | 6.915 | 5.893 | 0.8522 [0.8476, 0.8567] | pass | improved |
| reused-MultiStar | 23.261 | 8.449 | 0.3632 [0.3594, 0.3671] | pass | improved |
| reused-LongText | 178.845 | 171.469 | 0.9587 [0.9489, 0.9687] | pass | inconclusive |
| reused-Diverse | 51.368 | 36.764 | 0.7156 [0.7035, 0.7279] | pass | improved |
| reused-Dense4096 | 684504.033 | 328125.557 | 0.4798 [0.4631, 0.4971] | pass | improved |
| reused-Dense16384 | 2986475.586 | 1331407.480 | 0.4468 [0.4247, 0.4701] | pass | improved |
| reused-Newline | 173531.175 | 82536.080 | 0.4758 [0.4650, 0.4869] | pass | improved |
| reused-GeneralShort | 22.157 | 21.686 | 0.9789 [0.9582, 0.9999] | pass | equivalent_within_margin |
| reused-FixedMedium | 8.572 | 7.765 | 0.9059 [0.9004, 0.9114] | pass | improved |
| reused-FixedLong | 59.811 | 59.053 | 0.9874 [0.9764, 0.9984] | pass | equivalent_within_margin |
| reused-LiteralLong | 51.527 | 51.244 | 0.9945 [0.9899, 0.9992] | pass | equivalent_within_margin |
| static-Literal | 15.570 | 15.282 | 0.9815 [0.9738, 0.9894] | pass | equivalent_within_margin |
| static-Simple | 20.476 | 20.523 | 1.0008 [0.9615, 1.0418] | pass | equivalent_within_margin |
| static-MultiStar | 36.612 | 37.236 | 1.0171 [1.0094, 1.0247] | pass | equivalent_within_margin |
| static-LongText | 209.000 | 211.504 | 1.0120 [1.0034, 1.0206] | pass | equivalent_within_margin |

A guard can establish noninferiority without establishing useful primary improvement. Individual `promotion_eligible=false` results on equivalent guards are retained as reported; they are not relabeled as speedups. Source adoption requires useful benefit on the portfolio and targeted multi-star path plus every mandatory noninferiority guard.

All exact execution, plan, build and comparison IDs are in the JSON summary and retained evidence archive. Per-session outputs include PID/start identity, runtime configuration and code hashes, ordered block/arm IDs, completed work, correctness, measured allocation and joined process exit.
