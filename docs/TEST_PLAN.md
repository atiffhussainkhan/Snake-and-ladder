# Test Plan

Status: Phase 2 merged planning specification; runtime tests NOT RUN
Authority: `SNAKES_AND_LADDERS_ENGINEERING_PLAN.md`

## Traceability

See [planning-v1 rule-to-test matrix and initial RNG vectors](TEST_TRACEABILITY.md).
All runtime statuses remain NOT RUN. Every selected R-001 through R-020 maps to a test.

Every normative `MUST` rule receives a stable test ID, owner, fixture/config, expected result, and validation tier before sign-off.

## Required layers

- Unit/table: movement, finish, bounce-back, effects, shields, coin, trees, butterflies, purchases, expiry, economy, and settlement.
- Property/fuzz: illegal placement, no loops, bounded chains, valid squares, non-negative bank, idempotency, and termination.
- Determinism/replay: seeded boards, action logs, state hashes, schema compatibility, tamper rejection, and cross-runtime vectors.
- Contract/conformance: C# domain, Unity adapters, AI, server, and simulator share outcomes.
- Integration/network: authority, stale/duplicate intents, reconnect, resync, AFK, abandonment, and faults.
- Security/privacy: forged outcomes, unauthorized purchases, replay tampering, secret exposure, abuse, rate limits, child safety, and retention.
- Accessibility/performance: readable numbers, colour-independent cues, reduced motion, audio alternatives, motor access, and low-end mobile budgets.
- Statistical: paired seeds, confidence intervals, seats, strategy mixtures, exploit bots, and regression seeds.

## Tooling boundary

Unity EditMode/PlayMode with Test Framework 1.6.0 is selected in SETUP_ADR.md.
Playwright is not selected for the native Unity client. Package resolution and execution
remain unverified; this planning document installs no dependency.

## Acceptance

The rule-to-test matrix is complete, fast checks are CI-eligible, expensive sweeps are bounded/nightly, and two independent reviewers approve coverage.

## Fixture and execution handoff

[Transition cases](TRANSITION_CASES.md) adds planning cases for every T-001 through T-026
family. These are semantic expectations, not serialized golden states or executed tests.
For each future executable case record its ID, complete pre-state/configuration, action,
RNG inputs/cursors, ordered event, canonical post-state/hash and expected rejection/result.
Fault-only stubs are test harness inputs, never new production replay parameters.

Run structural documentation checks now. After technical authorization, run fast domain
unit/property/replay cases on the external C# host, reproduce identical fixtures in Unity
Mono and Android IL2CPP, then run adapter, device and statistical tiers. A build must not
claim cross-runtime equivalence from a Python hash diagnostic. Archive runtime/editor,
source/config/fixture hashes, command, exit status and failing case IDs with each result.

Online authority/privacy/network cases remain deferred to the online release gate;
offline scope audit T-024 is still required for the first client. Device T-022/023 and
statistical T-026 results cannot be inferred from passing domain tests. The selected
Unity package/toolchain versions remain unverified locally.
