# Phase 2 Transition and Acceptance Cases

Status: semantic planning cases; all runtime results NOT RUN.
Authority: [ruleset](RULESET_CONTRACT.md), [schema](REPLAY_SCHEMA.md),
[ADR](SETUP_ADR.md), [traceability](TEST_TRACEABILITY.md).

These cases extend review coverage without claiming complete serialized fixtures.
Each case uses otherwise legal state, the active authorized actor, the next per-actor
ID and current expectedVersion unless stated otherwise. RNG outcomes below are controlled
unit-test preconditions, not proof that a particular production seed produces them.
For acceptance, materialize full canonical states/events/hashes and replayable seeds;
invalid generator/RNG stubs belong only in the fault harness. No test has executed here.

## Rule and boundary cases

| Case / family | Precondition and input | Expected result |
| --- | --- | --- |
| C-001 / T-001 | Standard configuration with 2, 3 or 4 valid profiles; separately 1 or 5 seats | Valid seats start at square 1/bank 40; invalid counts reject before generation; six never grants another normal turn |
| C-002 / T-002 | F-001 initial state then Skip; compare each persisted field | Exact F-001 resulting state/hash; version/eventCursor 1; globalTurn 0; no RNG change |
| C-003 / T-003 | Enumerate every schema action against every persisted phase | Only STATE_MACHINE_SPEC table pairs accepted, subject to authority/payload; all other pairs unchanged; retry exception follows ADR |
| C-004 / T-004 | Timeout at purchase, roll, coin, portal and chase; separately Quit at each legal human choice | Same game effects as Skip/Roll/DeclineCoin/DeclinePortal/Stop but recorded action type Timeout; Quit yields ABANDONED with no winner |
| C-004b / T-004 | Offline timer paused; separately last seat completes round 400 without qualifier | Pause emits no Timeout; round bound yields DRAW/draw-turn-limit, no next normal turn |
| C-005 / T-005 | All 594 combinations of square 1-99 and die1-6 | q=p+d when <=100, otherwise200-p-d; qualify iff p+d=100; 98+2 qualifies,98+5=97,99+6=95 |
| C-005b / T-005 | BuyMovement from94, then attempt at 95; portal from90; coin -10 from5 in isolated movement unit | Purchase reaches95; purchase at 95 rejects; portal reaches95; isolated coin movement clamps1; no destination triggers/qualification |
| C-006 / T-006 | Die lands on legal snake head 72->44 with shield true; repeat unshielded | Shielded stays72 and shield=false; unshielded reaches44; no second transport |
| C-006b / T-006 | Normal landing50, DeclineCoin, normal turn 10 | Coin choice closes once; eligible portal follows; coin cannot reopen; economy waits until portal answer |
| C-007 / T-007 | Zero seed setup; regenerate before normal/chase die | Setup matches F-001 eight objects/cursor 8; each later board has eight objects with unique legal endpoints and reserved tree/purchased squares |
| C-007b / T-007 | Fault harness supplies no legal pair during generation | Terminal invalid-configuration; no partial layout/die or retained fee; rollback C-020 |
| C-008 / T-008 | Two seats, purchase legal snake at globalTurn 3 | expiresAt 7; present before 7; removed at TURN_START 7; validation excludes visible endpoints/trees/50/95-100 |
| C-008b / T-008 | Existing owned live snake or endpoint collision, then BuySnake | Reject unchanged bank/RNG/purchase index; corrected same unaccepted ID may succeed |
| C-009 / T-009 | BuyMovement with bank 9 then10 at legal square/cooldown | Bank9 rejects; bank 10 becomes0, advances to roll choice; no income until normal turn completes |
| C-009b / T-009 | lastMoveBuy 2, normalTurns 3 then4; bank 999999 at turn income | Difference1 rejects,2 allows; income saturates1000000; snake cooldown is independent but only one purchase per turn |
| C-010 / T-010 | At normal coin choice square 50, bank 40, shield=false; force outcomes1 through 6 | Positions40/60 for1/2; bank 60/30 for3/4; shield=true for5; unchanged for6; accept consumes coin RNG only |
| C-010b / T-010 | Coin outcome4 at bank 5; outcome5 with shield true; DeclineCoin | Bank floors0; shield does not stack; decline consumes no coin RNG; movement outcomes skip destination features |
| C-011 / T-011 | Two seats, land on live tree15 at globalTurn 3, bank 40 | Harvest bank 50, tree dormant respawnAt 7; normal income later gives52; TURN_START 7 reactivates; standing there gives no passive credit |
| C-012 / T-012 | Economy updates turns3/6/9 starting energy 0/shield=false | Energy1 then2 then0/shield=true; shield already true preserves capped energy 3 |
| C-012b / T-012 | energy 3/shield=true; shield consumed by snake | No immediate conversion during landing; next normal economy converts to energy 0/shield=true even if turn not divisible by3 |
| C-013 / T-013 | Normal turn 10 at 90 after other features; AcceptPortal, DeclinePortal or Timeout in separate cases | Accept reaches95 with no extra reward; decline/timeout stays90; at 95 no offer; identical human/AI opportunity |
| C-014 / T-014 | Normal die98+2, bank 40, live purchased snakes | Qualify and credit normal income to 42; complete normal turn/global index; remove purchased snakes; enter other-seat chase; no coin/tree/portal prompt |
| C-015 / T-015 | Chase bank 10/attempts 3, BuyAttempt, nonqualifying die | Bank0/attempts 2 after successful attempt; globalTurn+1; next seat because no affordability; no normal count/income change |
| C-015b / T-015 | Chase bank 9; separately affordable Stop; four-seat match | No paid roll at bank 9; Stop forfeits attempts without index increment; no match exceeds9 paid chase dice |
| C-016 / T-016 | Qualification order A,B,C with final banks40,40,50 | Rank C,A,B before all unqualified seats; equal banks favor earlier qualification; terminal new action rejects; settlement never repeats |
| C-016b / T-016 | Unqualified seats equal square/bank | Tie follows cyclic order after first qualifier; no random tie break |
| C-017 / T-017 | Zero seed labels/counters from traceability; Uniform 6 raw values4294967291 then4294967292 | First value accepted, second rejected; four initial digest vectors reproduce; coin draws do not alter dice cursor |
| C-017b / T-017 | 128 consecutive rejected raw draws; n0 or1000001 | Rejection cap triggers randomness-failure; out-of-contract n rejected by RNG interface; never substitute another algorithm |
| C-018 / T-018 | F-001 and F-002; duplicate/unknown keys, float/null/non-ASCII value, changed config/hash | Canonical bytes match; malformed shape/value rejected in schema order; corruption never silently accepted |
| C-018b / T-018 | Envelope above16MiB,10001 actions or mismatched action/event lengths | Reject limits/shape before execution; never truncate |
| C-019 / T-019 | Complete shared pack on external C#, Unity Mono and Android IL2CPP | Every canonical state/event/hash identical; absent runtime or fixture remains BLOCKED |
| C-020 / T-020 | Chase bank 40/attempts 3 then BuyAttempt with generation or RNG injected failure | Restore bank 40/attempts before terminal reset; phaseFAILED, attempts 0, choice none; one failure receipt/event/version; pre-action layout/RNG retained |
| C-021 / T-021 | Repeat F-001 accepted Skip; change type toRoll under ID1; new ID3; stale version | Retry original receipt without mutation; changed payload Conflict; sequence gap/stale reject unchanged in ADR precedence |
| C-021b / T-021 | Correct payload but wrong actor/match; forged human Timeout origin | Authority rejects before accepted-ID lookup; host timer origin cannot be supplied by untrusted action payload |
| C-022 / T-022 | Physical floor device and20-minute representative run | Record p95 frame<=33.3ms,peak<=512MiB,start<=10s,domain p99<=5ms plus temperature; absent device means BLOCKED |
| C-023 / T-023 | 720p, color cues removed, audio muted, reduced motion enabled | Numbers remain readable; shape/text and visual audio equivalents present; single-tap choices equivalent; timings meet ADR |
| C-024 / T-024 | Future offline dependency/build audit | No accounts/chat/network/ads/analytics/privileged secrets; no offline compliance PASS from documentation alone |
| C-025 / T-025 | Legal movement at bank 30; legal snake at bank 60 when movement unavailable; Saver at every choice | Balanced prioritizes movement then Attacker placement; Saver skips/declines coin/accepts portal/rolls/stops chase; policy choices consume no RNG |
| C-025b / T-025 | Opponent leader square 60; legal heads 61,62 with multiple tails; no head above leader | Attacker chooses head 61/lowest legal tail; excludes self from leader selection; no valid head gives Skip; only Attacker accepts coin |
| C-026 / T-026 | Paired rotated seed corpus; interrupted job; interval crossing threshold | Report all per-cell denominators/uncertainty; resume unique completed seed IDs; incomplete/threshold-crossing results cannot PASS |

## Coverage limits and remaining work

Every test family has at least one concrete planning scenario. This is not exhaustive
runtime coverage: full serialized before/after fixtures, action-phase Cartesian cases,
all movement/property assertions, failure injection implementations and observation/API
conformance still need implementation-phase evidence. C-005b includes isolated unit
preconditions that are not claimed as reachable full-match states. Do not load such states
as production replay initialState; the schema requires regenerating setup.

Traceable families are sufficient for a Phase 2 specification merge, not for promoting
GATE-07, GATE-09, GATE-10 or GATE-13. Preserve the existing blocked verdicts.
