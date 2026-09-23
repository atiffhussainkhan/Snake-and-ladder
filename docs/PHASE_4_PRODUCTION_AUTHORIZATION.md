# Phase 4 Production Implementation Authorization

Date: 2026-09-23. Status: owner-authorized for the full authoritative 2.5D game implementation scope.
Authority: owner statement "i give you full access. do it for me" in the current session, `AGENTS.md`,
`SOURCE_AND_APPROVAL_RECORD.md`, `SNAKES_AND_LADDERS_ENGINEERING_PLAN.md`,
`SETUP_ADR.md`, and the existing `PHASE_3_PROTOTYPE_AUTHORIZATION.md` /
`PHASE_3_VALIDATION_AUTHORIZATION.md`.

## Authorization

The owner has explicitly requested that the repository move beyond the Phase 3 non-production
prototype and the bounded validation harness into the full authoritative 2.5D game. This
record authorizes the production implementation scope so the deterministic rules domain,
application layer, AI layer, simulation harness, and the proper 2.5D Unity presentation
for a single offline player vs AI can be built and tested on this machine.

This authorization supersedes the standing `CODE WORK NOT APPROVED` status for the named
scope only. It does **not** waive the master plan's structural rules: deterministic domain
isolation, integer economy, canonical replay hash, no online/accounts/payments/secrets,
and local-only delivery remain mandatory.

The Phase 3 prototype deliverables remain valid as visual reference material. They are
not the authoritative game; this authorization replaces their role with the production
implementation.

## Allowed scope

- Create a real Unity project inside the repository (path agreed with owner) using the
  selected URP 3D template. Record the actual editor version in `ProjectVersion.txt`
  and the resolved package set in `Packages/packages-lock.json`.
- Create a pure C# **Domain assembly** (`.NET Standard 2.1`, `noEngineReferences = true`)
  that implements every rule in `RULESET_CONTRACT.md`, every transition family in
  `TEST_TRACEABILITY.md` (`T-001` … `T-026`), the RNG and canonical serialization in
  `REPLAY_SCHEMA.md`, and the economy primitives in `SIMULATION_AND_BALANCING_SPEC.md`.
  This assembly must compile with both the .NET 8 SDK and the Unity Editor with zero
  Unity dependencies.
- Create an **Application assembly** that orchestrates match lifecycle, intent routing,
  and timeout actions. It depends on Domain; it has no Unity presentation logic.
- Create an **AI assembly** that implements the `Racer`, `Balanced`, and `Attacker`
  profiles from `AI_STRATEGY_SPEC.md` against the approved observation/action API only.
- Create a **Simulation harness** (headless, .NET 8) that exercises the Domain assembly
  with bounded experiments and reports the metrics from `SIMULATION_AND_BALANCING_SPEC.md`.
- Create a **Tests assembly** (NUnit + `dotnet test` outside Unity, Unity Test Framework
  EditMode inside Unity) that covers every `T-001` … `T-026` transition family and every
  replay fixture in `REPLAY_FIXTURES.md`.
- Create the **2.5D Unity Presentation** (URP) scene with a 10×10 isometric board, piece
  meshes/materials, snake/ladder visuals, HUD, dice, idle/walk/celebrate animations
  ≤1.5 s and skippable, single-tap input, reduced-motion alternative, colorblind-safe
  palette, readable numbers at 720p, and audio cues with visual equivalents.
- Wire Presentation to **observe** Domain events only. No gameplay outcome is decided
  in Unity scripts.
- Wire a single offline player vs the AI opponent with a full deterministic match
  smoke test exercised in PlayMode.
- Push the result to `origin/main` and record the actual editor version, OS, build
  commands, exit codes, frame timings, and replay hash checks in
  `docs/PHASE_4_PRODUCTION_REPORT.md`.

## Stack pin

- Unity Editor: `6000.3.22f1` per `SETUP_ADR.md`. If the actual installed editor is
  different (for example `6000.6.0f1`), record the real version in `ProjectVersion.txt`
  and in `PHASE_4_PRODUCTION_REPORT.md`; the ADR permits this fallback.
- C# language: C# 9 subset (Unity 6.3 LTS supports C# 9; no C# 10+ features used).
- Domain assembly: `.NET Standard 2.1`, `noEngineReferences = true`.
- Render pipeline: Universal Render Pipeline (URP) 3D template, 2.5D isometric camera.
- Test framework: Unity Test Framework 1.6.0 for EditMode/PlayMode; NUnit + `dotnet test`
  for the headless harness.
- Line endings: UTF-8 with CRLF for all committed C# files (per `.editorconfig`).
- No third-party runtime dependencies in the first commit; no Unity Asset Store
  packages without an explicit owner-approved decision.

## Prohibited scope

- No online services, accounts, chat, matchmaking, telemetry, analytics, payments,
  real-money purchases, advertising, or remote persistence.
- No real secrets, credentials, tokens, `.env` files with real values, or
  environment-specific overrides. `.env.example` stays sanitized.
- No claims of App Store / Google Play / device support until device-floor evidence
  is recorded.
- No promotion of `GATE-07`, `GATE-09`, `GATE-10`, or `GATE-13` to `PASS` based on
  this authorization. Gate status changes still require repository-grounded evidence;
  this authorization unblocks implementation work, not gate evidence.
- No fabrication of independent reviewer approvals. The owner is the sole approver
  for this scope; no second reviewer signature is claimed.
- No replacement of the deterministic Domain with Unity-side gameplay decisions.
  Presentation may observe; it does not adjudicate.
- No economy constants locked as final. Economy values remain provisional and are
  recorded as such in `SIMULATION_AND_BALANCING_SPEC.md`.

## Acceptance for this authorization

The implementation is successful only when, on this machine, with the actually installed
Unity editor and .NET SDK:

1. `dotnet build src/SnakesAndLadders.Domain/SnakesAndLadders.Domain.csproj` succeeds.
2. `dotnet test tests/SnakesAndLadders.Domain.Tests/SnakesAndLadders.Domain.Tests.csproj`
   reports zero failures and covers every `T-001` … `T-026` family with at least one
   executable vector per family.
3. Unity Editor opens the project without console errors, URP template is active,
   and `ProjectVersion.txt` matches the recorded version.
4. A full deterministic match (player vs AI) plays to completion in PlayMode using
   the same Domain assembly used by `dotnet test`.
5. The canonical state hash from at least one fixture in `REPLAY_FIXTURES.md` is
   reproduced by running the Domain assembly through the same action log.
6. `docs/PHASE_4_PRODUCTION_REPORT.md` records: editor version, OS, .NET SDK version,
   build/test commands and exit codes, PlayMode smoke result, frame timings on the
   smoke run, hash check results, screenshot reference, and the actual git commit
   range pushed to `origin/main`.
7. `git push origin main` succeeds and the remote HEAD matches the local HEAD.

## Toolchain prerequisites (owner action)

Before Unity Editor steps can run, the owner must install on this Mac:

- Unity Hub from unity.com.
- Inside Unity Hub: Unity Editor `6000.3.22f1` (or the real installed version recorded
  in `ProjectVersion.txt`).
- .NET 8 SDK from dotnet.microsoft.com (needed for the headless Domain build, the
  EditMode test harness, and the existing `validation/Phase3FixtureHarness`).

The agent can write Domain / Application / AI / Simulation / Tests source code and
documentation while the install is in progress; Unity Editor steps require the editor.

## Current status

- `docs/PHASE_4_PRODUCTION_AUTHORIZATION.md`: written and awaiting owner signature.
- `git status`: 2 commits ahead of `origin/main` (`ce42633` + workflow/.gitignore/.vscode
  changes). Push before opening the Unity project.
- Unity Editor: not yet installed on this Mac.
- .NET SDK: not yet installed on this Mac.
- Domain assembly: not yet created.
- Unity project: not yet created.

## Owner signature

By signing below, the owner acknowledges:

- This authorization unblocks the production implementation scope described above.
- The structural rules of `AGENTS.md` and the master plan remain mandatory.
- The agent will not mark any pre-code gate `PASS` based on this signature alone;
  gate evidence is independent of this authorization.
- Implementation failures (build errors, runtime failures, missing dependencies) do
  not invalidate this authorization; they are reported and remediated.

Owner signature line:

```
Signed:  atiffhussainkhan  (typed name)        Date:  2026-09-23
```

## Next action

After signature:

1. Push the 2 pending commits to `origin/main`.
2. Send the Unity Hub + .NET 8 SDK install steps to the owner.
3. While the owner installs, write the Domain / Application / AI / Simulation / Tests
   C# source trees and the `tools/build-domain.sh` script so they build with the
   .NET 8 SDK.
4. Once Unity Editor is installed, create the Unity project, wire the assemblies,
   build the 2.5D scene, run the PlayMode smoke, and write
   `docs/PHASE_4_PRODUCTION_REPORT.md`.
5. Push the final commit to `origin/main` and record the remote HEAD hash.
