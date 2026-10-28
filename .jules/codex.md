## 2024-06-17 - Architectural Drift: Partial vs. Full String Matching
**Observation:** A discrepancy exists between the documented examples in README.md and the deterministic execution flow in `InternalUtils.cs`. The framework mathematically enforces full-string matches through automated boundary anchoring (`^` and `$`). The documentation erroneously provides examples that imply partial substring matching (e.g., matching "Lorem ipsum" against `or*ips?m`), which syntactically fail the validation step.
**Strategic Action:** Update the epistemological interface (README.md) to accurately reflect the framework's strict boundary-anchored matching logic. Synchronize all code blocks to ensure successful deterministic execution under the current framework paradigm. Update `.NET` compatibility declarations where applicable.

## 2026-10-07 - Lexical and Structural Architecture Sync
**Observation:** The README.md exhibited implicit variable declarations (`var`) conflicting with modern typological transparency directives, and lacked explicit documentation regarding its actual execution model (transpilation) vs. non-existent mechanisms.
**Strategic Action:** Updated code blocks to target-typed `new()` syntax and appended an 'Architectural Execution Flow' section clarifying the transpilation architecture and explicitly denying hierarchical data binding usage.
