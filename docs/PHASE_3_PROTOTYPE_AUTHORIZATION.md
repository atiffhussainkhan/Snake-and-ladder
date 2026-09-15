# Phase 3 Non-Production Prototype Authorization

Date: 2026-09-07. Status: authorized by the owner for a disposable graphical prototype.
Authority: owner request in the current task, `AGENTS.md`, and the master engineering plan.

## Authorization

The owner has explicitly requested that the repository move beyond documentation and
create a playable visual prototype. This record authorizes a **non-production Phase 3
prototype** so the board, camera, input, movement presentation, and early rule examples
can be viewed in Unity.

This authorization is separate from production implementation approval. It does not pass
`GATE-07`, `GATE-09`, `GATE-10`, or `GATE-13`, and it does not change the master final
authorization from `CODE WORK NOT APPROVED`.

## Allowed prototype scope

- Create a Unity project using the installed editor, subject to recording the actual
  editor version.
- Create a single offline scene with placeholder board geometry, pieces, camera, and
  basic input.
- Use clearly labelled prototype/mock behavior for demonstration only.
- Include a visible prototype disclaimer and keep the project disposable and local-first.
- Commit only project files that are explicitly part of this prototype scope if the owner
  wants the prototype reviewed in GitHub.

## Prohibited scope

- No claim that prototype behavior is the authoritative rules engine.
- No balance, economy, replay, multiplayer, security, accessibility, or device-support
  claims from prototype behavior.
- No online services, accounts, payments, analytics, secrets, persistence, or deployment.
- No replacement of the deterministic validation harness with Unity presentation logic.
- No promotion of any blocked gate or change to `CODE WORK NOT APPROVED`.

## Acceptance for this authorization

The prototype is successful only when Unity opens the project, the scene runs locally,
and the report records the editor version, project path, launch result, known mock rules,
and limitations. A prototype failure does not alter the planning gates.

## Next action

Create the prototype only after recording the actual installed Unity editor version. Keep
production implementation and the deterministic domain assembly gated until the required
technical evidence and independent approvals are complete.
