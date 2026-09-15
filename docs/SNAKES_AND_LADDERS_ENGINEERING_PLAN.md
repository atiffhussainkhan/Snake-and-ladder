# Snakes and Ladders 2.5D: Senior Engineering Review and Planning Plan

Status: pre-production planning only
Source: repository baseline cd4847126fdd23e9d4bc655dde3092ae077bbd22 accepted by owner as complete on 2026-09-07; see SOURCE_AND_APPROVAL_RECORD.md
Code-change policy: standing owner approval recorded; production code still requires technical gate evidence
Repository target: `mintvaultuk-byte/snakes-and-ladders-`
Product domain: `https://blockrivals.co`
Plan revision: 10
Selected client stack: Unity + C# (planning decision; project not created)
Current authorization: `CODE WORK NOT APPROVED` (technical prerequisites pending; owner approval recorded)

Conversation handoff: [Planning and repository setup chat](https://chatgpt.com/s/cx_6a996303b0e88191b0673075e58ddab9)
Handoff purpose: preserve the decision history, senior-review rationale, repository setup
steps, and pre-code constraints for the Codex IDE task. Treat the current repository
documents and later approved decisions as normative when they differ from conversational
discussion.

Review provenance: lead Sol synthesis plus independent architecture, QA/determinism,
simulation, and delivery-orchestration reviews. The architecture and QA reviews were
performed independently and converged on the same principal blockers: incomplete
source text, undefined authoritative ordering, unresolved effect precedence, economy
scale mismatch, Final Chase ambiguity, and missing deterministic replay contracts.

## Current decision precedence

The owner delegated setup/specification choices and accepted the current repository as
complete source. See [source and approval record](SOURCE_AND_APPROVAL_RECORD.md).
[Setup ADR](SETUP_ADR.md), [planning-v1 rules](RULESET_CONTRACT.md), and
[test traceability](TEST_TRACEABILITY.md) supersede provisional choices in this plan
for this version. [Gate review](PLANNING_GATE_REVIEW.md) records current evidence.
Earlier critical findings remain in the ledger for traceability, with updated dispositions.
The initial Phase 1 review is historical; its addendum records the full re-review.
[Phase 2 merge report](PHASE_2_REVIEW.md) records the current six-specification merge,
semantic cases and new finding P2-004; prior gate evidence remains historical.

## 1. Repository Access Gate

### Current readiness snapshot

| Area | Status | Meaning |
| --- | --- | --- |
| Planning-document structure | PASS | Required review, governance, environment, test, and reporting sections are present |
| Target GitHub URL | BLOCKED | Git fetch succeeded; authenticated metadata/settings evidence remains outstanding |
| Local/Codex repository worktree | PASS | Git history restored; baseline cd48471 and new planning artifacts documented in PLANNING_GATE_REVIEW.md |
| Git history baseline | PASS | Review baseline is `81f07e210ff8cd202d8fb3f350eacd34996a779a`; earlier planning history is retained |
| Complete authoritative source | PASS | Owner accepts baseline repository as complete; missing handover tail deferred in SOURCE_AND_APPROVAL_RECORD.md |
| Overnight specification pass | BLOCKED | Draft specifications are prepared, but full transition conformance and gate sign-off remain outstanding |
| Production game implementation | FAIL | Explicitly not authorized |

An HTTP-style `200` is not itself the acceptance goal. Readiness requires authenticated
repository metadata and contents to be returned successfully, the intended branch and
commit to be identified, and a readable local/Codex worktree to be available. A public
web page loading successfully would not prove that Codex can read repository settings
or safely write the approved planning artifact.

Repository evidence refreshed on 2026-09-07:

- The supplied local folder initially had no `.git` metadata.
- Git was initialized and existing `origin/main` history fetched without replacing local files.
- The review baseline is `81f07e210ff8cd202d8fb3f350eacd34996a779a` on `main`.
- `origin` is `https://github.com/mintvaultuk-byte/snakes-and-ladders-.git`.
- Root instructions and planning configuration are present; no game implementation exists.
- See [Phase 1 review](PHASE_1_REVIEW.md) for inventory, fixes, and outstanding decisions.
- Fetch proves remote Git read access; authenticated metadata/settings access and
  independent gate approvals are not inferred from that result.

Before implementation starts, one of these must be true:

- The repository is opened as a Codex saved project.
- A local checkout of the repository is opened in VS Code and Codex.
- The GitHub connector has access to the private repository.
- The repository URL/name is corrected if the trailing dash is wrong.

Before the overnight planning pass starts, all of these must be true:

- The repository is accessible through the intended authenticated route.
- The exact local path, branch, and source commit are recorded.
- Repository instructions and skills are readable.
- The missing handover tail is recovered, or the product owner explicitly declares the
  truncated source complete and records that decision in the disposition ledger.
- A versioned source packet and output/reporting location are agreed.

The six required specifications are planning drafts. Saving them to Git does not
constitute rules freeze or independent approval.

### 1.1 Evidence and review rules

When repository access becomes available, every factual repository finding must cite the
exact path and, where relevant, the exact source section or line. Assumptions and
inferences must be labelled. Secret values must never be reproduced.

Run inexpensive structural checks before semantic review. At minimum inspect:

- Git status, current branch, remotes, tracked files, and ignore rules.
- `AGENTS.md`, `.agents`, `.codex`, repository skills, and instruction precedence.
- README files, specifications, decision records, reports, and internal links.
- Package manifests, workspace declarations, lockfiles, runtime versions, and scripts.
- Source, test, fixture, configuration, build, CI, deployment, and environment files.
- Duplicate identifiers, inconsistent terminology, incomplete templates, stale files,
  manifest/lockfile drift, broken references, and accidentally committed secrets.

Repository-dependent facts remain `BLOCKED` until this inspection is possible. They
must not be inferred from the project title, the handover, or a generic game template.

## 2. Senior Review Verdict

The handover is strong as a product vision, but it is not yet safe to implement as written.

The main risk is not technical difficulty. The main risk is that too many mechanics are still provisional, and several mechanics affect the same game state: movement, board hazards, economy, shields, temporary snakes, finish rules, and Final Chase. If implementation begins before those contracts are frozen, the team will build multiple incompatible versions of the game.

The correct next step is a planning and simulation pass, not game-code development.

This is a hard gate. The overnight pass may create or revise specification documents,
decision registers, traceability matrices, examples, and non-executable test designs.
It must not create production game code, migrations, assets, dependencies, generated
projects, or remote changes until the planning gate is explicitly approved.

## 3. Critical Findings

Severity: blocking

- The final handover text appears truncated at `Dominant/ex`. The full simulator metric list and any final sections after that point must be recovered before locking scope.
- The canonical turn sequence is described for dynamic snakes/ladders, but not for every mechanic. Golden Coin, butterflies, purchased movement, purchased snakes, Treasure Trees, Final Chase, and bounce-back finishing all need one deterministic event order.
- The economy is intentionally provisional. Prices, income rates, cooldowns, and lifespans cannot be implemented as fixed constants until the simulator proves they do not create a dominant strategy.
- Final Chase is promising, but not fully specified. It needs exact dice rules, board reshuffle behavior, economy freeze rules, qualified-finisher ordering, and tie rules.
- Dynamic snakes/ladders and anchored purchased snakes need a conflict-resolution model. Without it, visual state and simulation state will drift.
- The Royal/Portal Butterfly is emotionally strong, but currently underspecified: spawn timing, capture probability, owner priority, and interaction with turn timing must be defined.
- The 8-second turn estimate is probably too optimistic for animated 2.5D mobile play once player choice prompts, purchases, snake movement, and Final Chase are included.

Severity: high

- "Natural finish" needs a single formal definition. Current text implies exact finish with bounce-back, but purchased movement and portal movement stop at 95.
- "Only active player can collect butterflies" must be reconciled with continuous flying butterflies and multiplayer visual state.
- Treasure Tree respawn timing must be measured in turns, player-turns, or full rounds.
- Purchased movement skipping all intermediate triggers is good, but must also define whether the landing square can trigger trees, snakes/ladders, coin, and shields.
- The simulator must use the same rules engine as production logic or it will produce false confidence.
- Server-authoritative randomness is required for multiplayer and should be designed from day one, even if the first build is local-only.

Severity: medium

- Accessibility is missing from the plan: readable numbers, colorblind safety, reduced motion, audio cue alternatives, and child-friendly prompts.
- Monetization/platform goals are mentioned indirectly through Facebook/mobile pacing, but there is no explicit offline/online split or save/progression model.
- AI difficulty is mentioned, but the expected behavior by difficulty tier is not yet defined.

### 3.1 Required finding format

Every P0-P3 finding in the formal review must contain:

| Field | Requirement |
| --- | --- |
| ID | Stable identifier such as `ARCH-P0-001` |
| Severity | P0 blocking, P1 critical, P2 important, or P3 advisory |
| Evidence | Exact repository path and source section; label inference explicitly |
| Impact | Concrete correctness, security, delivery, fairness, or cost consequence |
| Remediation | Required decision or corrective planning action |
| Owner | Product, rules, architecture, backend, client, QA, simulation, security, or operations |
| Acceptance criterion | Observable condition proving closure |
| Gate affected | One or more pre-code gate IDs |

P0 means implementation cannot safely start. P1 must be resolved or explicitly deferred
out of the first implementation scope by the lead and product owner. P2 and P3 may not
silently disappear; they still require dispositions.

### 3.2 P0/P1 finding register

Evidence paths below refer to the current repository; `BLOCKED` means the required
artifact is absent, not that a generic assumption is acceptable.

| ID | Severity | Evidence | Impact | Remediation | Owner | Acceptance criterion | Gate | Disposition |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `SRC-P0-001` | P0 | SOURCE_AND_APPROVAL_RECORD.md, owner decision and immutable baseline | Scope, metrics and decisions may be omitted | Recover and hash the complete source packet, or obtain owner declaration that the truncated source is complete | Product owner | Complete versioned source and owner decision are recorded | `GATE-01` | Accepted; source omission resolved by owner disposition |
| `REPO-P0-001` | P0 | `PHASE_1_REVIEW.md`, inventory and Git recovery; instructions/configuration now present | Stack, conventions and implementation boundaries cannot be grounded | Verify remote access and baseline commit; inventory the repository when populated | Architecture/operations | Authenticated metadata, branch, commit and complete inventory are cited | `GATE-02`, `GATE-08` | Accepted; Blocked |
| `RULE-P0-001` | P0 | `docs/SNAKES_AND_LADDERS_ENGINEERING_PLAN.md` §6–§7 | Different clients/agents can resolve one turn differently | Freeze match/turn state machines and effect matrix with adjudicated examples | Rules lead | Every action has legal phase, ordering, timeout and deterministic outcome | `GATE-03`, `GATE-04` | Accepted; Open |
| `DET-P0-001` | P0 | Plan §5 and §8; no rules package or vectors exists | Replays, multiplayer and simulations can diverge | Approve stream domains, algorithm/version, commitments, canonical log and hashes | Architecture/security | Cross-runtime golden vectors reproduce state hashes and reject tampering | `GATE-07` | Accepted; Open |
| `ECO-P0-001` | P0 | Handoff economics summarized in plan §3 | Dominant strategies, runaway wealth or unusable purchases | Normalize units and tune from paired simulations with confidence intervals | Economy/product | Source/sink model meets approved targets across seats and modes | `GATE-05` | Accepted; Open |
| `CHASE-P0-001` | P0 | Plan §7 and §8.1 | Winner and seat fairness are not adjudicable | Freeze Final Chase action, feature, cost, ordering, tie and settlement matrices | Rules/product | All qualification/settlement scenarios have expected outcomes and tests | `GATE-06` | Accepted; Open |
| `FINISH-P1-001` | P1 | Plan §7: natural finish and 95 cap remain questions | Players may finish inconsistently by movement source | Define exact finish, bounce-back, pass-through and trigger rules for every source | Rules/QA | Boundary matrix and property tests cover squares 95–100 | `GATE-03`, `GATE-04` | Accepted; Open |
| `BUTTER-P1-001` | P1 | Plan §3: active-player collection and Royal Butterfly timing unresolved | Latency/device skill can decide outcomes unfairly or stall turns | Choose deterministic opportunity model, timeout and accessibility-equivalent input | Product/accessibility/QA | Same seed and declared inputs yield same outcome across clients/devices | `GATE-04`, `GATE-09` | Accepted; Open |
| `OBJECT-P1-001` | P1 | Plan §7: dynamic and purchased objects can conflict | Board legality and visual state can diverge | Define endpoint occupancy, precedence, expiry, respawn units, chain bound and failure handling | Rules/QA | Generator never emits illegal/looping layouts; lifecycle tests pass | `GATE-04`, `GATE-08` | Accepted; Open |
| `MOBILE-P1-001` | P1 | Plan §3: 8-second estimate is unvalidated; no device artifact exists | Mobile pacing, thermal behavior and accessibility may fail | Establish device floor and measured frame/memory/network/animation budgets | Client/product | Representative low-end device evidence meets approved budgets | `GATE-02`, `GATE-12` | Accepted; Open |
| `NET-P1-001` | P1 | Plan §5 and §9: authority/reconnect policy is required but not specified | Forged, stale or duplicated intents can corrupt matches | Publish protocol, resync, AFK, abandonment and moderation contract | Backend/security | Fault/security tests prove server-only outcomes and deterministic recovery | `GATE-08`, `GATE-09` | Deferred with owner to online release; offline audit pending (PLANNING_GATE_REVIEW.md) |
| `SAFETY-P1-001` | P1 | Plan §9 and §8.1: child safety/privacy requirements need product decisions | Unacceptable data, chat, purchase or retention risk | Define age, consent, reporting, moderation, minimisation, retention and deletion controls | Product/security | Privacy/security review signs off abuse cases and data flows | `GATE-02`, `GATE-09` | Deferred with owner to online release; offline audit pending (PLANNING_GATE_REVIEW.md) |

## 4. Planning Principles

The project should treat the rules engine as the source of truth.

Production gameplay, AI decisions, replay validation, multiplayer authority, and Monte Carlo simulation should all call the same deterministic core where practical. Rendering should observe state transitions, not invent gameplay outcomes.

The first permanent deliverable should be a set of markdown specifications. Code should begin only after these documents are reviewed and accepted:

- `docs/GAME_RULES_SPEC.md`
- `docs/STATE_MACHINE_SPEC.md`
- `docs/SIMULATION_AND_BALANCING_SPEC.md`
- `docs/AI_STRATEGY_SPEC.md`
- `docs/TEST_PLAN.md`
- `docs/AGENT_ORCHESTRATION_PLAN.md`

This file can be used as the first version of `docs/AGENT_ORCHESTRATION_PLAN.md` or as a master planning file.

### 4.1 Required repository inventory report

The repository review must report, without modifying the repository:

1. Actual folder and file structure.
2. Apparent purpose of every top-level folder.
3. Missing, incorrectly placed, duplicated, obsolete, or ambiguously named artifacts.
4. Package/workspace boundaries and permitted dependency direction.
5. Configuration and environment-variable ownership.
6. Test layout and missing test layers.
7. Build, lint, format, type-check, validation, and CI arrangements.
8. Determinism, replay, persistence, simulation, and multiplayer-authority risks.
9. Security concerns, reporting paths without revealing secret values.
10. Whether the actual repository—not merely this concept—is ready for implementation.

## 5. Canonical Game State

Every implementation plan should model the game around explicit state:

- Match config: player count, mode, board size, seed, ruleset version.
- Board state: dynamic snakes, dynamic ladders, anchored purchased snakes, Treasure Trees, Golden Coin, Royal Butterfly state.
- Player state: square, bank, butterfly energy, shield readiness, qualified-finished state, extra attempts used, AI profile.
- Turn state: active player, phase, dice result, pending choices, triggered events, animation locks.
- Economy state: income, spend history, purchase cooldowns, active purchased objects.
- Random state: seeded stream labels for dice, board layout, butterflies, trees, coin outcomes, AI tie-breaks.

No rule should depend on animation timing. Animations should be cancellable/replayable presentations of already-authoritative state transitions.

## 6. Proposed Turn State Machine

Freeze this before implementation:

1. `TURN_START`
2. `PRE_ROLL_CHOICES`
3. `DICE_ROLL_REQUESTED`
4. `BOARD_RESHUFFLE`
5. `LAYOUT_LOCKED`
6. `DICE_RESOLVED`
7. `NORMAL_MOVEMENT`
8. `LANDING_RESOLUTION`
9. `OPTIONAL_LANDING_CHOICE`
10. `POST_CHOICE_RESOLUTION`
11. `ECONOMY_AND_COOLDOWN_UPDATE`
12. `WIN_CHECK`
13. `TURN_END`

Landing resolution must have a fixed priority. Proposed initial priority:

1. Check finish/bounce behavior.
2. Resolve locked snake or ladder on landing square.
3. Resolve Butterfly Shield if the landing event is a snake attack.
4. Resolve Golden Coin only if the final landed square is 50.
5. Resolve Treasure Tree only if the final landed square has an active tree.
6. Resolve Royal Butterfly only through its own capture event, not passive square landing.
7. Enter Final Chase if a player qualifies at 100.

This historical proposal is replaced by R-005 through R-006 in RULESET_CONTRACT.md.

## 7. Rule Questions To Freeze

These historical questions are adjudicated by RULESET_CONTRACT.md; retained for traceability:

- Does purchased movement happen before or after the normal dice roll?
- Can a player buy movement and buy sabotage on the same turn?
- Does landing after purchased movement trigger snakes, ladders, trees, or Golden Coin?
- During Final Chase, do dynamic snakes/ladders reshuffle before every extra roll?
- During Final Chase, can Butterfly Shield still be gained or only consumed?
- Can Golden Coin trigger during Final Chase?
- Are anchored purchased snakes removed during Final Chase or allowed to expire naturally?
- Can a dynamic snake/ladder endpoint land on a purchased snake head?
- What exact squares are forbidden in the finish zone: 95 to 100, 96 to 100, or only 100?
- Is square 95 a hard cap for all non-dice movement, including Golden Coin positive outcomes from late variants?
- How is Royal Butterfly capture offered fairly when only the active player can collect?
- What happens if a player on square 50 declines the coin and a future board reshuffle creates a snake head on square 50? Current rule forbids square 50, so tests must enforce that.
- What is the exact tie-break if multiple players qualify with the same remaining bank?

## 8. Simulation Requirements

## 8.1 Required rules-freeze coverage matrix

The overnight pass must complete this matrix in the permanent specifications before
any implementation gate can pass. `Provisional` is not an implementation contract.

| Area | Required contract to freeze | Required evidence/test family |
| --- | --- | --- |
| Scope | MVP, supported modes, product/non-goals, progression and online/offline boundary | Scope decision plus acceptance checklist |
| State and machines | Canonical match/player/board/economy/turn state; legal actions, timers, cancellation, settlement and reconnect transitions | State-machine tables, transition tests, replay fixtures |
| Movement | Dice, purchased movement, snake/ladder, Golden Coin, portal, bounce-back, exact finish, intermediate/destination-trigger policy | Movement-source matrix, boundary/property tests |
| Effects | Total precedence order; one-square conflicts; bounded chains; shield consumption; declined choices; idempotent resolution | Effect matrix and fuzz/conformance tests |
| Dynamic objects | Deterministic generation, legality, reshuffle timing, expiry/respawn units, anchored purchased-snake coexistence and no-loop/failure handling | Seed vectors, legality properties, lifecycle tests |
| Economy | Coin unit, sources/sinks, prices, cooldowns, lifespan, expected income/spend, balance targets and anti-hoarding/anti-stall limits | Economy model, paired simulations, exploit-bot results |
| Special mechanics | Golden Coin outcomes; normal butterflies; Butterfly Shield; Royal/Portal Butterfly spawn, eligibility, capture timeout, ownership, reward and accessibility parity; Treasure Trees | Feature decision tables, latency-neutral tests, accessibility scenarios |
| Sabotage | Purchased snake targeting, placement legality, ownership, visibility, expiry, conflict with dynamic objects and refund/failure behavior | Authority and adversarial-action tests |
| Final Chase | Qualification, extra attempts, dice/layout/effect availability, bank freeze/deduction, ordering, ties, affordability, voluntary stop and settlement | Complete action/feature/settlement matrix and seat-fairness simulation |
| AI | Strategy boundaries for Racer, Balanced, Attacker and other tiers; no hidden information; deterministic tie-breaks and equivalent butterfly opportunities | Policy tests and exploit/adaptive-bot simulations |
| RNG/replay | Versioned independent streams, secure seed handling, commitments, canonical action log, schema evolution, state hash and golden vectors | Cross-runtime replay and tamper/conformance tests |
| Multiplayer | Server authority, intent validation, duplicate/stale messages, reconnect/resync, AFK, abandonment, moderation/reporting and anti-cheat | Protocol contract, fault/integration/security tests |
| Non-functional | Mobile device floor, frame/memory/battery/network/animation budgets; readable numbers, colour safety, reduced motion, audio alternatives and motor access | Device matrix, performance and accessibility evidence |
| Child safety/privacy | Age-appropriate accounts, names/chat/invites, consent, purchases/ads, reporting/moderation, data minimisation, retention and deletion | Privacy/security review and abuse-case tests |

The matrix is a completeness check, not permission to fill gaps with assumptions. Each
normative `MUST` statement must map to a stable test ID and an owner in the final
traceability report.

The simulator is mandatory before balance constants are locked.

Minimum simulation contract:

- Use the same deterministic rules package as the production game.
- Record ruleset version, config hash, seed, strategy profiles, and result metrics.
- Support replaying any sampled match from seed and config.
- Run at least 1,000,000 matches across accepted config sets before economy lock.
- Report confidence intervals, not just raw percentages.
- Include first-player/seat advantage metrics.
- Include exploit bots, not only normal bots.
- Fail the build if impossible states, loops, illegal placements, or non-terminating matches exceed the accepted threshold.

Primary acceptance ranges should be decided before tuning. Initial proposed targets:

- Standard match median: 8 to 12 minutes.
- Standard match P95: under 18 minutes unless Party/Long mode is selected.
- No single base strategy above 40 percent win rate in a balanced 4-strategy tournament.
- First-player advantage within 5 percentage points after enough trials.
- Attacker strategy not below half the win rate of Racer/Balanced after tuning.
- Original first finisher wins Final Chase often enough to matter, but not so often that chasers feel cosmetic.

## 9. Testing Strategy

Testing should be planned before code starts.

Required test layers:

- Unit tests for placement legality, movement, bounce-back, shields, Golden Coin, purchases, expiry, and bank changes.
- Property tests for illegal overlaps, no loops, no negative bank, no out-of-range square, and deterministic replay.
- Snapshot tests for seeded board generation.
- Simulator regression tests for known seeds and small deterministic scenarios.
- AI policy tests for each strategy archetype.
- Multiplayer authority tests for server-controlled dice/layout seeds and client rejection of forged outcomes.
- UI smoke tests for board readability, mobile layout, and major event animations.
- Unity PlayMode end-to-end tests for a complete match, Golden Coin, shield block, purchased snake, portal cap at 95, and Final Chase.

SETUP_ADR.md selects Unity Test Framework EditMode/PlayMode for the native client.
Playwright is not selected. Package resolution and execution remain unverified.

### 9.1 Validation tiers

- Structural: links, IDs, terminology, templates, manifests, lockfiles, config drift.
- Unit/table: every rule boundary and effect-precedence cell.
- Property/fuzz: invariants, illegal actions, generation exhaustion, and bounded chains.
- Contract/conformance: shared rules behavior across server, client, bots, and simulator.
- Determinism/replay: seed/config/action-log reproduction and state hashes.
- Integration/network: authority, duplicate/stale intents, reconnect, resync, and faults.
- Simulation/statistics: paired seeds, confidence intervals, exploit policies, and seats.
- Non-functional: mobile performance, readability, accessibility, privacy, and security.

Each normative `MUST` rule must map to at least one test identifier before planning is
signed off. Fast checks belong in CI; large statistical, fuzz, soak, and replay sweeps
belong in a versioned overnight/nightly tier.

## 10. Provisional Repository Structure

This is a review hypothesis, not an instruction to create folders. The formal reviewer
must reconcile it against the repository's actual language, framework, package manager,
deployment topology, and current conventions. Until then, structure approval is
`BLOCKED`.

```text
/
|-- repository instructions and existing specification/reporting locations
|-- package/workspace manifest and exactly matching lockfile
|-- .gitignore
|-- .env.example                 # documentation only; no real credentials
|-- config/                      # versioned non-secret rules and mode configuration
|-- src/
|   |-- application/             # use cases and orchestration
|   |-- domain/                  # pure deterministic game logic
|   |   |-- rules/
|   |   |-- effects/
|   |   |-- economy/
|   |   |-- final-chase/
|   |   `-- replay/
|   |-- infrastructure/          # persistence, network, RNG providers, telemetry
|   |-- interfaces/              # server/client/CLI or engine adapters
|   `-- shared/                  # narrowly scoped cross-cutting types/utilities
|-- tests/
|   |-- unit/
|   |-- integration/
|   |-- contract/
|   |-- determinism/
|   |-- simulation/
|   `-- fixtures/
`-- scripts/                     # bounded build/validation/simulation entry points
```

Dependency direction should point inward toward the pure domain rules. Domain code must
not depend on rendering, networking, wall-clock time, environment variables, persistence,
or platform APIs. The actual repository review must confirm whether these conceptual
boundaries belong in one package, a workspace, or another stack-native arrangement.

### 10.1 Selected Unity + C# mapping

The selected client stack is **Unity + C#**. This records a planning decision only; it
does not authorize creating a Unity project, installing packages, or writing scripts.

The proposed Unity mapping is:

- `Assets/Domain/`: pure C# rules, state transitions, economy, effects, Final Chase,
  RNG interfaces, replay serialization, and state hashing.
- `Assets/Application/`: match orchestration and use cases that depend on Domain.
- `Assets/Infrastructure/`: Unity/network/persistence adapters behind interfaces.
- `Assets/Presentation/`: scenes, prefabs, animation, input, audio, and UI; it may
  observe domain events but must not decide gameplay outcomes.
- `Assets/AI/`: policy implementations using only the approved observation/action API.
- `Assets/Simulation/`: offline C# simulator adapter or separate .NET-compatible
  assembly, subject to repository/package review.
- `Assets/Tests/`: EditMode/PlayMode tests only after the Unity version and test
  strategy are approved.

The domain assembly must remain usable without a Unity scene, `MonoBehaviour`, frame
clock, physics engine, renderer, or platform API. Unity version, backend, platform floor, package policy and server boundary are selected
in SETUP_ADR.md under DEC-002/009/010; installed compatibility is unverified.

## 11. Proposed Environment Contract

Do not create `.env` or invent credentials during planning. The following names are
provisional contracts to validate against the selected stack. Public client variables
must use the framework's explicit public prefix only after the framework is known.

| Variable | Purpose | Required | Scope | Safe default/example | Secret | Validation | Owner |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `APP_ENV` | Runtime environment name | Yes | server/build/test/tooling | `development` | No | Enum: development/test/staging/production | Operations |
| `LOG_LEVEL` | Structured log threshold | No | server/test/simulation | `info` | No | Supported level enum | Operations |
| `DATABASE_URL` | Authoritative persistence connection | Production only | server | blank in example | Yes | Valid supported URI; fail closed | Backend/Operations |
| `AUTH_SECRET` | Session/token signing material | If authentication is enabled | server | blank in example | Yes | Minimum entropy/length; never default in production | Security/Backend |
| `RULESET_VERSION` | Select versioned normative rules/config | Yes | server/test/simulation | documented development version | No | Must match bundled registry entry | Rules lead |
| `REPLAY_SCHEMA_VERSION` | Replay serialization contract | Yes | server/test/simulation | documented current version | No | Supported schema enum | Architecture |
| `MATCH_SEED_POLICY` | Controls secure/random/fixed seed policy | Yes | server/test/simulation | `secure` outside tests | No | Enum; fixed forbidden in production | Backend/QA |
| `SIM_CONFIG_PATH` | Versioned simulation experiment config | Simulation only | simulation/tooling | safe relative fixture path | No | Must remain inside approved config tree | Simulation |
| `SIM_WORKERS` | Bounded simulator concurrency | No | simulation/tooling | conservative integer | No | Positive integer with resource cap | Simulation/Operations |
| `SIM_SEED` | Reproducible experiment seed | Test/simulation only | test/simulation | documented fixed test seed | No | Integer/string canonicalization | QA/Simulation |
| `STATE_HASH_ALGORITHM` | Replay/conformance hash algorithm | Yes | server/test/simulation | approved versioned algorithm | No | Allowlisted algorithm only | Architecture/Security |
| `PUBLIC_API_ORIGIN` | Client-visible authoritative API origin | Client build only | client/build | local safe URL | No | HTTPS required outside local development | Client/Operations |
| `PUBLIC_APP_ORIGIN` | Canonical public application origin | Yes for deployed client | client/build/server | `https://blockrivals.co` in production | No | Absolute HTTPS origin; no path/query; environment-specific outside production | Client/Operations |
| `TELEMETRY_ENDPOINT` | Optional approved telemetry collector | No | server/client/tooling | blank/disabled | Usually no | Approved HTTPS origin and privacy review | Operations/Privacy |
| `ERROR_REPORTING_DSN` | Error-reporting credential/config | No | server or explicitly public client DSN | blank | Treat per provider | Provider-specific validation; privacy review | Operations/Security |

Ruleset version, replay schema, and production seed policy are authoritative operational
controls, but deterministic game configuration should normally be captured in versioned
config and replay metadata rather than mutable environment variables alone.

Never expose database credentials, private signing keys, privileged service tokens,
server RNG secrets, administrative credentials, or private telemetry credentials to a
browser/client bundle. Environment-specific secrets belong in the deployment secret
store, not Git.

`.gitignore` must exclude `.env`, `.env.*`, local secret overrides, credentials, private
keys, generated replay dumps that may contain user data, and local simulation output;
it should explicitly allow a sanitized `.env.example`. That example uses blank values
for secrets, safe documented defaults for non-secrets, and placeholders only where a
format example is needed.

## 12. Agent Orchestration Plan

Sol is the main brain and final authority. Other agents should produce evidence, not final decisions.

Use official OpenAI model guidance as the routing basis: GPT-5.6 Sol for flagship complex reasoning/coding, GPT-5.6 Terra for balanced quality/cost work, and GPT-5.6 Luna only for cost-sensitive high-volume/simple checks. GPT-5.5 remains useful for strong production workflow review when the task does not require the newest frontend/design strengths.

Recommended overnight planning pass:

| Role | Model | Effort | Purpose | Output |
| --- | --- | --- | --- | --- |
| Sol Lead Architect | `gpt-5.6-sol` | `xhigh` | Own final rules contract, resolve conflicts, merge reviews | Final accepted markdown specs |
| Sol Independent Reviewer | `gpt-5.6-sol` | `high` | Challenge the architecture and find contradictions | Findings with severity and required edits |
| Systems Planner | `gpt-5.6-terra` | `high` | Break mechanics into modules, APIs, data/state contracts | Implementation dependency map |
| QA and Simulation Reviewer | `gpt-5.5` | `high` | Build test matrix, simulator gates, exploit strategies | Test and balancing checklist |
| Repetition/Format Checker | `gpt-5.6-luna` | `low` | Cheap pass for markdown consistency, broken links, repeated TODOs | Formatting report only |

These assignments are starting defaults, not entitlements to spend. Each task should
use the lowest model and effort that passes its acceptance rubric. Promote a failed
task one step at a time; do not begin at `max` or Ultra merely because it is available.
Reserve Ultra/multi-agent execution for independent workstreams with explicit merge
gates. Reserve `max` for a measured quality gain on the hardest quality-first review.

Do not use Luna for final rule judgment. Do not use low-effort models for economy conclusions, simulator interpretation, or multiplayer authority.

## 13. Overnight Workflow

Phase 0: access and source validation

- Confirm repository path, branch, and default package manager.
- Read `AGENTS.md`, `.codex`, `.agents`, repository skills, README, package config, tests, and existing docs.
- Confirm whether this is a blank repo or an existing codebase.
- Recover the missing end of the handover if available.

Phase 1: independent planning

- Sol reviewer audits the design for rule contradictions.
- Terra maps systems and implementation order.
- GPT-5.5 designs tests, simulator metrics, and exploit bots.
- All agents must cite the exact source file/section or repo file they relied on.
- Agents must write reports to separate files or return them to Sol; they must not
  concurrently edit a shared normative specification.

Phase 2: Sol merge

Current delivery: complete for the specification-merge scope; see PHASE_2_REVIEW.md.
This is distinct from section 14 build-order item 2, the gated C# domain assembly.

- Sol lead produces the permanent markdown specs.
- Sol lead explicitly accepts, rejects, or defers every serious finding.
- Deferred items become tracked planning blockers.

Phase 3: cross-check

- Sol reviewer re-reads the merged specs for contradictions.
- GPT-5.5 re-checks whether every rule has a test.
- Terra re-checks whether every planned module has a dependency owner.
- Luna can run a cheap formatting/readability pass only after the content is stable.
- Sol records every P0/P1 finding in a disposition ledger as accepted, rejected with
  rationale, deferred with owner, or duplicate. No finding may disappear silently.
- At least two independent reviewers must agree that each pre-code gate is satisfied.

Phase 4: implementation authorization gate

Implementation may begin only if:

- The repository is connected.
- The planning specs are committed.
- All blocking rule questions are resolved or explicitly deferred from MVP.
- The simulator build order is accepted.
- The user confirms code work can start.
- The lead Sol publishes a signed-off planning report containing source commit,
  ruleset version, unresolved-decision count, reviewer dispositions, and gate status.

### 13.1 Consolidated review output contract

The overnight pass returns one consolidated lead report in the existing approved
reporting location. It must contain, in order:

1. Executive verdict.
2. Repository inventory.
3. Current architecture assessment.
4. Structural and configuration findings.
5. Critical blockers graded P0-P3.
6. Rules and source-authority gaps.
7. Determinism and replay assessment.
8. Economy and simulation assessment.
9. Multiplayer-authority assessment.
10. Repository tree reconciled against actual paths.
11. Environment contract reconciled against the actual stack.
12. Test and validation strategy.
13. Disposition ledger.
14. Decision register.
15. Pre-code checklist with `PASS`, `FAIL`, or `BLOCKED` for every gate.
16. Recommended planning sequence.
17. Exactly one final authorization statement: `CODE WORK APPROVED` or
    `CODE WORK NOT APPROVED`.

If repository access or source material is missing, affected sections must say
`BLOCKED`; reviewers must not substitute generic facts or quietly omit the section.

## 14. Suggested Build Order After Plan Approval

No code should be written yet, but the implementation order should be:

1. Repository setup review: Unity version, C#/.NET profile, package policy, test runner, formatting, and CI.
2. Headless deterministic C# domain assembly with no Unity dependencies.
3. Seeded replay, config hashing, and cross-runtime vectors.
4. Placement legality and deterministic board-generation engine.
5. Movement, effect precedence, and landing resolution.
6. Economy primitives and simulator-facing configuration.
7. Purchased movement, purchased snakes, and lifecycle rules.
8. Golden Coin, Treasure Trees, Butterfly Shield, and Royal Butterfly.
9. Final Chase.
10. AI strategy profiles and legal-action adapter.
11. Monte Carlo simulator and report generator.
12. Unity EditMode/PlayMode harness with a minimal 2D debug board.
13. Unity 2.5D presentation, animation, input, and accessibility layer.
14. Multiplayer authority, reconnect, and client synchronization.
15. Full mobile performance, privacy, safety, and release-readiness pass.

The simulator should be usable before polished rendering. The game should not be balanced by visual playtesting alone.

## 15. Agent Prompt Templates

Use these only after the repository is connected.

### Sol Lead Architect

Review the Snakes and Ladders 2.5D handover and all repository instructions. Do not edit game code. Produce or revise the permanent markdown specs only. Resolve rules into deterministic state transitions, record all unresolved blockers, and make every rule testable. You are the final authority, but you must explicitly account for findings returned by the other agents.

Model: `gpt-5.6-sol`
Effort: `xhigh`

### Sol Independent Reviewer

Act as an adversarial senior engineer reviewing the planning specs for contradictions, undefined states, impossible implementation paths, missing tests, economy exploits, and multiplayer determinism risks. Do not edit files unless asked. Return findings ordered by severity with concrete spec references.

Model: `gpt-5.6-sol`
Effort: `high`

### Terra Systems Planner

Map the accepted game plan into modules, data models, APIs, implementation sequence, and ownership boundaries. Optimize for cost-aware execution and avoid speculative engineering. Do not implement code. Return a dependency map and a list of build phases that can be parallelized safely.

Model: `gpt-5.6-terra`
Effort: `high`

### GPT-5.5 QA and Simulation Reviewer

Design the test strategy and simulation validation plan. Focus on deterministic replay, property tests, economy exploit bots, edge cases, multiplayer authority, and acceptance gates. Do not implement code. Return a test matrix and simulation sign-off checklist.

Model: `gpt-5.5`
Effort: `high`

### Luna Formatting Checker

Review only markdown consistency, broken internal links, duplicated TODOs, inconsistent terminology, and obvious spelling mistakes. Do not make rule decisions.

Model: `gpt-5.6-luna`
Effort: `low`

## 16. Credit-Saving Rules

- Do one source-gathering pass and share the same source summary with all agents.
- Keep Sol for final architecture, contradictions, and high-risk decisions.
- Use Terra for module breakdown, implementation sequencing, and cost-aware planning.
- Use GPT-5.5 for QA/test/simulation critique where it can independently challenge the plan.
- Use Luna only for cheap repetitive checks after content stabilizes.
- Avoid rerunning million-match simulations during early planning; first build fast smoke simulations, then scale after deterministic replay works.
- Require every agent output to be structured so Sol can merge without reading long narrative twice.
- Do not parallelize edits to the same markdown files.
- Do not parallelize implementation before the rules engine interfaces are approved.
- Cache and reuse a versioned source packet; do not resend the entire handover when a
  bounded excerpt, decision ID, or artifact hash is sufficient.
- Run cheap structural checks before semantic reviews so expensive agents do not spend
  time on missing files, broken links, duplicated IDs, or incomplete templates.
- Every promotion to a stronger model or higher reasoning effort must record the failed
  rubric item it is intended to fix.

## 17. Disposition Ledger

The authoritative P0/P1 disposition ledger is the finding register in §3.2, which
contains the required evidence, impact, remediation, owner, acceptance criterion, gate,
and current disposition in one place. P2/P3 findings must be assigned stable IDs and
dispositions during the overnight pass; they must not be silently dropped.

Allowed dispositions are `Accepted`, `Rejected with rationale`, `Deferred with owner`,
and `Duplicate with reference`. No row may be deleted merely because the specification
changed; closure requires evidence and reviewer sign-off.

## 18. Decision Register

| Decision ID | Decision required | Owner | Dependency | Required evidence | Status |
| --- | --- | --- | --- | --- | --- |
| `DEC-001` | Recover and version the complete handover | Product owner | None | Complete source hash and provenance | Decided: owner repository-source disposition and blob hashes recorded |
| `DEC-002` | Select engine/client platform and supported device floor | Architecture/product | Repository inventory | Unity + C# decision, ADR, supported device floor, and performance constraints | Decided: SETUP_ADR.md; device measurement pending |
| `DEC-003` | Freeze canonical turn and match state machines | Rules lead | Complete source | Adjudicated examples and reviewer agreement | Selected: RULESET_CONTRACT.md R-002 through R-004; freeze evidence pending |
| `DEC-004` | Freeze movement/effect precedence and chain bound | Rules/QA | `DEC-003` | Complete matrices and property invariants | Selected: R-005 through R-008 and R-020; validation pending |
| `DEC-005` | Approve RNG domains, PRNG/version, commitment, and replay schema | Architecture/security | `DEC-003` | Golden vectors and threat review | Selected: REPLAY_SCHEMA.md and initial fixtures; cross-runtime and full transition evidence pending |
| `DEC-006` | Normalize economy units, income targets, sinks, and price ranges | Economy/product | `DEC-003` | Model plus staged simulation design | Selected experiment: R-009 and simulation policy; empirical balance pending |
| `DEC-007` | Fully adjudicate Final Chase | Rules/product | `DEC-003`-`DEC-006` | Feature matrix and seat-fairness analysis | Selected: R-014 through R-016; seat-fairness results pending |
| `DEC-008` | Choose butterfly interaction and accessibility-equivalent policy | Product/accessibility/QA | Platform decision | Latency/device fairness acceptance plan | Decided: R-012/013; single-tap and deterministic opportunity policy |
| `DEC-009` | Approve server authority, reconnect, AFK, and abandonment policy | Backend/product/security | Platform/backend ADR | Protocol and fault scenarios | Decided: offline first; future server policy in SETUP_ADR.md |
| `DEC-010` | Approve repository/package structure and dependency direction | Architecture | Repository inventory and Unity + C# decision | Reconciled Unity tree citing actual paths | Decided: SETUP_ADR.md interfaces/packages; installed verification pending |

## 19. Pre-Code Gate Checklist

| Gate | Requirement | Status | Evidence required to pass |
| --- | --- | --- | --- |
| `GATE-01` | Complete authoritative source is available and versioned | PASS | SOURCE_AND_APPROVAL_RECORD.md: owner disposition, immutable baseline and SHA-256 manifest |
| `GATE-02` | Repository inventory and instructions are reviewed | PASS | PLANNING_GATE_REVIEW.md: root instructions, inventory and verified Git route |
| `GATE-03` | Authoritative rule ordering is defined | PASS | RULESET_CONTRACT.md R-002/003/004/015/020; two design reviews |
| `GATE-04` | Effect precedence/conflict resolution is complete | PASS | RULESET_CONTRACT.md R-005 through R-013 and R-020; design reviews |
| `GATE-05` | Economy units and scales are reconciled | PASS | Integer sources/sinks and SIMULATION_AND_BALANCING_SPEC.md experiment policy; empirical balance pending |
| `GATE-06` | Final Chase is unambiguous | PASS | RULESET_CONTRACT.md R-014 through R-016: action/feature/settlement/tie contract |
| `GATE-07` | Deterministic RNG and replay contracts are documented | BLOCKED | REPLAY_SCHEMA.md and initial fixtures exist; full conformance and transition fixture evidence pending |
| `GATE-08` | Rules-engine interfaces and dependency direction are approved | PASS | SETUP_ADR.md: interfaces, dependency direction and action semantics; runtime verification pending |
| `GATE-09` | Tests trace every normative rule | BLOCKED | TEST_TRACEABILITY.md: 26 families; exhaustive concrete runtime fixtures pending |
| `GATE-10` | At least two independent reviewers approve every gate | BLOCKED | PLANNING_GATE_REVIEW.md: two per-gate reviews; blocked prerequisites not approved |
| `GATE-11` | Every P0/P1 finding has an approved disposition | PASS | PLANNING_GATE_REVIEW.md: both reviewers approve dispositions; technical acceptance remains open |
| `GATE-12` | Lead planning report is published | PASS | PLANNING_GATE_REVIEW.md: source, ruleset, open counts, dispositions and gates |
| `GATE-13` | User explicitly authorizes implementation | BLOCKED | Standing owner approval in SOURCE_AND_APPROVAL_RECORD.md; technical prerequisites still blocked |

Current decision: `CODE WORK NOT APPROVED`.

Current pre-code gate totals: **PASS 9, FAIL 0, BLOCKED 4**. Repository readiness
items marked `PASS` above do not override failed or blocked pre-code gates.

## 20. Definition Of Planning Done

Planning is done when:

- All required markdown specs exist in the repository.
- The canonical event order is frozen.
- Every mechanic has explicit state, inputs, outputs, and tests.
- Simulator metrics and acceptance thresholds are documented.
- Economy values are marked provisional until simulator evidence is available.
- Agent roles and model settings are documented.
- Repository instructions and skills have been read and reflected.
- The user gives explicit approval to begin code implementation.

## 21. Planning Pass Result

The planning pass produced these draft, planning-only artifacts in the approved
`docs/` location:

- `docs/GAME_RULES_SPEC.md`
- `docs/STATE_MACHINE_SPEC.md`
- `docs/SIMULATION_AND_BALANCING_SPEC.md`
- `docs/AI_STRATEGY_SPEC.md`
- `docs/TEST_PLAN.md`
- `docs/AGENT_ORCHESTRATION_PLAN.md`

No source code, dependency, migration, asset, deployment, `.env`, credential, or
generated application was created. The merged specifications preserve unexecuted technical obligations; prior planning
gate passes and remaining blockers are recorded in section 19. The repository contains documentation and planning configuration only; see the
current inventory in `PHASE_2_REVIEW.md`.

Phase status: **Phase 1 re-review and Phase 2 specification merge complete; bounded Phase 3 validation and a non-production graphical prototype authorized; production technical validation blocked**. The Phase 1
review confirmed that Unity project files, C# source/manifests, packages, tests, assets,
`.env`, executable formatters, CI, and build automation are absent by design.
`AGENTS.md`, `.gitignore`, `.editorconfig`, and sanitized `.env.example` are present. No compile errors
can be reported because no code exists. Loop/error controls remain unverified for
dynamic-object generation, effect-chain bounds, generation exhaustion, match
termination, timeouts, and terminal-state handling.

SETUP_ADR.md now selects Unity version, C# profile, package policy, test runner,
formatting/CI policy, device floor and server boundary. Installation, concrete replay
fixtures and executed technical evidence remain pending; see PLANNING_GATE_REVIEW.md.

Remaining master findings requiring technical evidence are `REPO-P0-001`, `RULE-P0-001`, `DET-P0-001`,
`ECO-P0-001`, `CHASE-P0-001`, `FINISH-P1-001`, `BUTTER-P1-001`, `OBJECT-P1-001`,
`MOBILE-P1-001`, `NET-P1-001`, and `SAFETY-P1-001`.

## 22. Immediate Next Action

Use [PHASE_3_PROTOTYPE_AUTHORIZATION.md](PHASE_3_PROTOTYPE_AUTHORIZATION.md) for the
owner-authorized non-production graphical prototype scope. Use
[PHASE_3_VALIDATION_AUTHORIZATION.md](PHASE_3_VALIDATION_AUTHORIZATION.md) for
the bounded validation-only scope that resolves the authorization ambiguity in P2-004.
Use PHASE_2_REVIEW.md for the current merge; PLANNING_GATE_REVIEW.md retains prior
independent evidence. The source disposition and owner approval are already recorded
and must not be requested again. Complete remaining transition fixtures, conformance
and technical gate reviews before production code. This file is already at the
approved repository path:

`docs/SNAKES_AND_LADDERS_ENGINEERING_PLAN.md`
