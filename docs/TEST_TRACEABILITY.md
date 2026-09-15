# Planning-v1 Test Traceability

Status: test designs only; runtime cases NOT RUN. Owner approval is not test execution.
The contract is [Ruleset planning-v1](RULESET_CONTRACT.md), plus [Setup ADR](SETUP_ADR.md).

| Test | Rule | Owner / tier | Fixture and expected result |
| --- | --- | --- | --- |
| T-001 | R-001 | Product/QA, table | 2-4 seats accepted; 1/5 rejected; start at 1; six gives no bonus |
| T-002 | R-002 | Architecture, replay | Every counter/version/deadline hashed; terminal new action rejects |
| T-003 | R-003 | Rules, transition | Enumerate phase/action pairs; only declared choices accepted; internals need no UI |
| T-004 | R-004 | QA, fault | Timeout matches default; pause emits none; 400 nonqualifying rounds draw; Quit abandons |
| T-005 | R-005 | Rules, table/property | Exhaust squares 1-99 and dice 1-6; 98+2 qualifies, 98+5=97, 99+6=95 |
| T-006 | R-006 | Rules, table | Shield consumes once at head; no transport chain; coin decline cannot retrigger |
| T-007 | R-007 | QA, property | Eight objects; unique endpoints outside trees/50/95-100; exhaustion terminates before die |
| T-008 | R-008 | QA, lifecycle | Failed placement unchanged; one snake per owner; created t expires at t+2N start |
| T-009 | R-009 | Economy, table | Bank9 rejects movement; bank10 buys; cooldown difference1 rejects, 2 allows; income cap |
| T-010 | R-010 | Rules, table | All six coin outcomes plus decline; bank floors at0; moved destination triggers nothing |
| T-011 | R-011 | Rules, lifecycle | Tree credits once, respawns harvest+2N; standing player gets no passive award |
| T-012 | R-012 | Rules, table | Turns3/6/9 gain energy; turn9 converts shield; existing shield preserves energy cap |
| T-013 | R-013 | Accessibility, table | Turn10 portal only below95; +10 cap95; same human/AI choice and timeout |
| T-014 | R-014 | Rules, transition | Qualification income then chase freeze; purchased snakes removed; only old shield usable |
| T-015 | R-015 | QA, boundary | Four seats at most9 chase dice; bank9 stops; bank10 buys once; Stop advances queue |
| T-016 | R-016 | Rules, settlement | A40/B40 tie goes to earlier qualifier; C50 wins; settlement cannot execute twice |
| T-017 | R-017 | Architecture, vector | Zero-seed vectors below; stream isolation; Uniform rejection boundary and cap |
| T-018 | R-018 | Security, replay | Same bytes/hash across hosts; reject duplicate keys, unknown schema, tampered config |
| T-019 | R-019 | QA, conformance | Same fixture pack on external C#, Unity Mono, Android IL2CPP; every event/hash equal |
| T-020 | R-020 | QA, fault | Generation/RNG failure restores fee/layout/cursors; one terminal event/version/receipt |
| T-021 | ADR action interface | Backend, fault | Same-ID same-payload retry receipt; changed payload Conflict; wrong actor/version unchanged |
| T-022 | ADR budgets | Client, device | Physical4GB floor, 20-minute run: frame/memory/start/action targets measured |
| T-023 | ADR accessibility | Accessibility, manual | 720p numbers, shape/text cues, visual audio alternatives, single-tap parity |
| T-024 | ADR offline scope | Security, scope | No network/accounts/chat/ads/analytics or privileged secrets in offline build |
| T-025 | AI policies | AI/QA, policy | Same observation/profile gives same legal action; each policy priority branch |
| T-026 | Simulation design | Simulation, statistical | Paired seeds, rotated seats, confidence intervals, failure/abort reports |

## Initial RNG reference vectors

Seed: 32 zero bytes. Counter: 0. Algorithm: BR-SHA256-CTR-v1.
Computed using Python hashlib as planning diagnostics on 2026-09-07, not C#/Unity
conformance. REPLAY_SCHEMA.md and REPLAY_FIXTURES.md provide the concrete schema and initial
state/Skip pack. Full transition conformance remains required before GATE-07 passes.

| Label | SHA-256 digest | First uint32 | Uniform(6) |
| --- | --- | --- | --- |
| board | 9064dbf91e28b0a4a1dac97df505dcff19830bfb058941a6dee75993f3576b81 | 2422529017 | 1 |
| dice | 617783f3a095ecc678d55e7d9f662ecc78762906b5ac9b622b3c7742eb150413 | 1635222515 | 5 |
| coin | ee09737c6776db2dfea4543d318d7e14aca95ce42a75f45c1cc39aca3234633d | 3993596796 | 0 |
| ai | a87f68037a6839f528379dda5fd82094f0d9f3bee2a5a663d093858d3219d784 | 2826921987 | 3 |

First dice result is 6. For Uniform(6), 4294967292-4294967295 reject;
4294967291 accepts. Stub raw draws to test 128-rejection failure deterministically.

## Phase 2 case expansion

[Transition and acceptance cases](TRANSITION_CASES.md) supplies C-001 through C-026
and boundary variants for every family above. Semantic cases are not full canonical
fixtures and do not change NOT RUN statuses.

## Coverage limits

Every identified rule has a test family, but the initial serialized pack is not exhaustive and executable assertions are absent.
This matrix is necessary planning progress, not sufficient evidence to pass GATE-09.
