# Game Rules Specification

Status: Phase 2 merged planning specification; runtime validation pending.
Authority: [master plan](SNAKES_AND_LADDERS_ENGINEERING_PLAN.md),
[planning-v1 contract](RULESET_CONTRACT.md), and [setup ADR](SETUP_ADR.md).
The contract owns numeric values and normative R-001 through R-020 rules. This document
organizes those decisions for implementation review; it does not introduce another ruleset.

## Scope and source precedence

Standard offline matches have 2-4 human/AI seats on squares 1-100. All mechanics in
planning-v1 remain included. Online services, accounts, chat, analytics, real-money
purchases, progression, cosmetics and other modes/platforms are deferred. The owner
has approved these setup choices; technical freeze is still pending.

Use the ruleset contract for mechanics, the replay schema for canonical encoding and
the ADR for application boundaries. Earlier master-plan questions are historical findings,
not alternative rules. Any conflict requires a recorded disposition before implementation.

## State and authority

R-001/002 and [replay schema](REPLAY_SCHEMA.md) define configuration, players, board,
choice, turn counters, lifecycle deadlines, chase state, RNG cursors and accepted history.
The pure C# domain owns transitions. The host supplies validated intents and explicit
Timeout actions; Unity renders results. No domain rule reads environment variables,
networking, persistence, animation progress or time.

## Mechanic ownership and acceptance

| Contract | Selected behavior | Owner | Test family |
| --- | --- | --- | --- |
| R-001/002 | Fixed seats; canonical integer state; no extra roll for six | Rules/architecture | T-001/002 |
| R-003/004 | Atomic continuation to next choice; host timeout defaults; bounded rounds; Quit abandons | Rules/application | T-003/004 |
| R-005 | Dice-only exact finish; reflected overshoot; no intermediate triggers | Rules | T-005 |
| R-006 | Qualification, one transport/shield, coin, tree, portal in that order | Rules/QA | T-006 |
| R-007 | Four snakes/four ladders; bounded pair selection; unique reserved endpoints | Rules | T-007 |
| R-008 | Purchased snake ownership, visible-board legality, expiry by global index | Rules | T-008 |
| R-009 | Integer bank, purchase affordability/cooldowns, bounded income | Economy | T-009 |
| R-010 | Coin choice at 50; six outcomes; movement skips destination features | Rules | T-010 |
| R-011 | Active-player tree harvest; explicit respawn deadline; no passive collection | Rules | T-011 |
| R-012 | Every third normal turn earns energy; shield conversion at economy update | Rules | T-012 |
| R-013 | Every tenth normal turn offers eligible portal; single choice, no tap race | Rules/accessibility | T-013 |
| R-014/015/016 | Immediate chase, bounded paid attempts, frozen features, deterministic ranking | Rules/economy | T-014/015/016 |
| R-017/018/019 | Isolated RNG streams; canonical replay and cross-runtime fixtures | Architecture/QA | T-017/018/019 |
| R-020 | Rejected input unchanged; terminal generator/RNG failure rolls back transaction | Rules/QA | T-020 |

## Movement and feature contract

Use the complete source/destination matrix in R-005. Purchased movement and portal
movement cap at 95 and cannot qualify. Coin movement clamps to 1-95. Only dice landing
at exactly 100 qualifies, before any other feature. Passing 100 reflects instead.
At most one transport applies; no movement triggers intermediate squares. A shield
blocks one snake and is consumed; it does not block ladders. Coin movement suppresses
destination features. Declines do not reopen the same offer. Player co-occupancy is legal.

Trees reserve their squares even while dormant. New purchased snakes validate against
the visible board; each later reshuffle reserves live purchased endpoints. Generation
is bounded and cannot publish a partial board. Failed purchase validation spends nothing.

## Final Chase

R-014 through R-016 settle the earlier design ambiguity. The first qualifier receives
normal turn income, then other seats enter chase in cyclic order. Remove purchased
snakes; freeze banks except attempt fees; disable normal landing rewards and purchases.
Dynamic transports and existing shields remain. Each queued seat can pay for at most
three attempts, stop, or end when unable to afford another attempt. Qualification ends
that seat's chase. Rank qualifiers by bank then qualification order; unqualified seats
follow by square, bank, and the specified cyclic order. Settlement occurs once.

## Examples, invariants and evidence

[Transition cases](TRANSITION_CASES.md) supply concrete planning examples;
[test traceability](TEST_TRACEABILITY.md) owns test-family coverage. Banks stay in range,
positions stay on-board, object endpoints remain legal, rejected input leaves state
unchanged, and every accepted transaction is replayable. These are acceptance obligations,
not executed test results. Economy values remain experiment candidates until simulation.
Two independent final reviews and runtime conformance remain outstanding.
