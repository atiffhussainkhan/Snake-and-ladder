# Phase 3 Bounded Validation Authorization

Date: 2026-09-07. Status: owner-authorized validation scope; production implementation remains blocked.
Authority: `SNAKES_AND_LADDERS_ENGINEERING_PLAN.md`, `AGENTS.md`, and `PHASE_2_REVIEW.md` finding P2-004.

## Purpose

The owner has authorized the repository to move into Phase 3 validation planning and
execution preparation. This record resolves the authorization ambiguity identified by
P2-004 without waiving technical gates or approving the production game.

Phase 3 is limited to independently checking the merged specifications, materializing
canonical validation fixtures, and executing deterministic conformance evidence once the
required toolchains are available. A runnable production game is not created by this
authorization.

## Allowed work

The following work is authorized within this bounded validation phase:

- Review the committed planning specifications and report contradictions or missing
  rule-to-test coverage.
- Create documentation-only validation manifests, fixture schemas, and evidence reports.
- Create a temporary or clearly isolated validation harness only after its runtime and
  output location are recorded; it must not be a Unity client, production domain
  assembly, generated application, deployment resource, or shipped dependency.
- Materialize canonical pre-state, action, RNG cursor/input, ordered events, post-state,
  canonical bytes, hash, and rejection expectations for `T-001` through `T-026`.
- Run deterministic replay, property, fault, and cross-runtime checks when the selected
  external C# host, Unity Mono, and Android IL2CPP environments are installed and
  independently recorded.
- Record actual commands, tool versions, source/config/fixture hashes, exit statuses,
  failures, and reviewer identities.

## Explicitly prohibited

This authorization does not permit:

- Creating or shipping the Unity game, production C# domain, presentation, AI, network,
  persistence, monetization, or deployment code.
- Installing or committing unapproved runtime dependencies, generated projects, assets,
  migrations, packages, DLLs, credentials, or environment-specific secrets.
- Calling a semantic planning case an executed test or claiming a fixture hash without
  generating and independently checking its canonical bytes.
- Promoting `GATE-07`, `GATE-09`, `GATE-10`, or `GATE-13` to PASS without their required
  evidence and independent review.
- Locking economy constants, claiming device support, or enabling online multiplayer.

## Required evidence before production activation

The following remain mandatory:

1. Complete executable fixtures and expected results for all `T-001` through `T-026`
   families, including failure and rollback cases.
2. A verified external C# SDK and deterministic validation host.
3. Unity 6.3 LTS `6000.3.22f1` with Test Framework 1.6.0 resolved and a recorded
   ProjectVersion/package lock.
4. Matching results on external C#, Unity Mono, and Android IL2CPP where applicable.
5. Property, replay, authority, accessibility, offline-scope, device, and simulation
   evidence required by the applicable test families.
6. Two independent reviewers approving the new evidence and updated gate dispositions.
7. A separate production implementation authorization after the preceding gates pass.

## Current evidence and blockers

At the initial authorization date:

- Planning specifications and semantic cases are committed at the Phase 2 merge.
- No complete serialized transition fixture pack exists.
- The isolated `validation/Phase3FixtureHarness` now reproduces the five F-001
  canonical hashes and F-002 escaping check with .NET SDK `8.0.424`; see
  `PHASE_3_VALIDATION_REPORT.md`.
- Unity Hub is installed and Unity Editor `6000.6.0f1` is executable, but the
  selected plan pin is `6000.3.22f1` and Unity Test Framework resolution is not
  verified.
- Runtime gameplay, device, simulation, and cross-runtime results remain `NOT RUN`
  or `BLOCKED`.

Therefore the current status remains:

- `GATE-07`: BLOCKED.
- `GATE-09`: BLOCKED.
- `GATE-10`: BLOCKED.
- `GATE-13`: BLOCKED for production activation.
- Final authorization: `CODE WORK NOT APPROVED`.

## Handoff

The next permitted action is to obtain the approved validation toolchains, then materialize
and execute the fixture/conformance work under this scope. This document is an explicit
bounded validation authorization, not a rules freeze, runtime result, or production-code
approval.
