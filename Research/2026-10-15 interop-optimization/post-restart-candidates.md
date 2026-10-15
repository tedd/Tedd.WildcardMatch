# Post-restart candidate experiments

All rows use the post-restart catalog after known competing benchmarks were stopped. Ratios are descriptive paired candidate/control estimates from ten blocks, with two warmups and fixed seed 20261015. The two-namespace screening caller does not establish independent source promotion; final adoption uses separate normal builds with identical callers.

| Candidate | Control | Literal | Simple | MultiStar | Diverse | LongText | Dense4096 | Dense16384 | Newline | Disposition |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| ordinal | provisional packed source | - | - | 0.9940 | 0.9863 | 1.0519 | 1.0202 | 1.0030 | - | Not selected: no useful long-text benefit. |
| swar | provisional packed source | - | - | 1.1588 | 1.0438 | 1.0141 | - | - | - | Not selected: no useful short-star benefit. |
| anchor | provisional packed source | - | - | 1.0569 | 0.9187 | 0.8913 | 0.5239 | 0.5728 | 0.5300 | Selected for new source confirmation: omit rechecks of the proven anchor, retain surrounding validation. |
| packedfirst | provisional packed source | 1.2212 | 1.1670 | 0.9610 | 1.0712 | 1.1483 | - | - | - | Not selected: weak short-star benefit with worse common-path costs. |
| minimum | anchor-improved source | 1.0764 | 1.0283 | 1.0020 | 1.0392 | 1.0746 | - | - | - | Not selected: no useful caller benefit. |
| sequence | provisional packed source | 1.0479 | 1.0102 | 1.0594 | 1.2704 | 1.0340 | - | - | - | Not selected: no useful broad-pattern benefit. |
| nolf | provisional packed source | 1.0445 | 1.0117 | 1.0014 | 1.0109 | 0.9967 | - | - | - | Selected for new confirmation: defer LF validation in the proven default regime. |
