# Ruleset Contract: planning-v1

Status: selected design under delegated owner approval; executable validation pending.
This contract resolves provisional mechanics for this version. Its identified rules
override earlier proposed ordering and unanswered questions in the master plan and
drafts. Balance parameters are experiment inputs, not production-locked constants.

## Scope and state

R-001: Offline matches have 2-4 seats, human or AI, on squares 1-100; all start at 1.
Seat order is fixed by configuration. No bonus roll for a six. Standard mode is the
only initial mode. Cosmetics, progression, online services and monetization are deferred.
All mechanics below remain part of this mode; no feature is silently removed.

R-002: State contains ruleset/schema IDs, immutable config hash, match ID, master seed,
per-domain RNG counters, phase, global turn index, active seat, board objects with IDs,
positions, banks, energy, shield, cooldown deadlines, pending choice, qualification
order, chase queue, attempts remaining, accepted action IDs, event cursor and result.
Also include state version, each seat's normal-turn count, last successful purchase
index by type (-1 for never), and each tree's live flag and respawn deadline.
Normal-turn count increments at that seat's TURN_START, first turn = 1. Global index
starts at 0 and advances after each completed normal turn or paid chase attempt.
Use integers only. Terminal state rejects new actions; identical accepted retries
return their stored acknowledgement.
Secrets and future RNG state are excluded from player observations.

## Match and turn transitions

R-003: Setup validates configuration, then generates the board, then starts seat 0.
Validation failure returns a terminal InvalidConfiguration result, never a partial match.
The normal sequence is TURN_START, PRE_ROLL_CHOICES, DICE_ROLL_REQUESTED,
BOARD_RESHUFFLE, LAYOUT_LOCKED, DICE_RESOLVED, NORMAL_MOVEMENT, LANDING_RESOLUTION,
OPTIONAL_LANDING_CHOICE, POST_CHOICE_RESOLUTION, ECONOMY_AND_COOLDOWN_UPDATE,
WIN_CHECK, TURN_END. Internal phases advance automatically; no UI acknowledgement is
needed. A choice opens only at its named phase. Each accepted external action completes
all following internal phases until the next choice or terminal result.

| Phase | Legal external actions | Default on explicit Timeout | Next phase |
| --- | --- | --- | --- |
| PRE_ROLL_CHOICES | BuyMovement, BuySnake, Skip | Skip | DICE_ROLL_REQUESTED |
| DICE_ROLL_REQUESTED | Roll | Roll | BOARD_RESHUFFLE |
| OPTIONAL_LANDING_CHOICE | AcceptCoin, DeclineCoin | DeclineCoin | POST_CHOICE_RESOLUTION |
| POST_CHOICE_RESOLUTION, when portal offered | AcceptPortal, DeclinePortal | DeclinePortal | ECONOMY_AND_COOLDOWN_UPDATE |
| CHASE_CHOICE | BuyAttempt, Stop | Stop | Chase roll or next seat |
| All other phases | None | No-op rejection | Internal transition only |

Quit is legal for the active human at every external choice, including roll and chase.
It immediately terminates Abandoned. A local host close is recorded as active-seat Quit,
even if that seat is AI-controlled. No action is accepted during an internal transition.

R-004: A single host-owned 20-second choice timer emits a recorded Timeout action.
The domain never reads time. Timer resets per choice; timeout IDs and expected versions
obey the same validation as other actions. Pausing offline play suspends host timers.
Animation time never delays or changes authority. Quit produces Abandoned with no winner.
After 400 complete normal rounds without qualification, terminate DrawTurnLimit.

## Movement and precedence

R-005: Only dice movement can qualify at 100. Dice are uniform integers 1-6.
From p with roll d: q=p+d; if q>100 use 200-q. Passing 100 is not qualification.
Examples: 98+2 qualifies; 98+5 lands 97; 99+6 lands 95.
No movement source triggers intermediate squares.

| Source | Destination | Destination effects | Can qualify |
| --- | --- | --- | --- |
| Normal/chase die | Exact finish or reflected bounce | Locked snake/ladder, then applicable landing features | Yes, before any feature |
| Purchased movement | min(95, old position+5); unavailable at 95+ | None; normal die still follows | No |
| Snake/ladder | Locked endpoint | No second transport; then landing features | No |
| Golden Coin movement | clamp(position +/- 10, 1, 95) | None | No |
| Royal Portal | min(95, position+10); unavailable at 95+ | None | No |

R-006: Resolve exact dice qualification first; otherwise find a transport head/foot
at the dice destination in the locked layout. A ready shield consumes once and prevents
snake movement, leaving the player on the head; it does not block ladders. At most one
transport applies. Then evaluate Golden Coin at final square 50, then a live tree at
the final square. Resolve coin choice before tree evaluation. Coin-generated movement
skips all destination features, including trees. Last, offer the turn's eligible portal.
All transient resolution flags persist until turn end so declines cannot retrigger.

## Board objects

R-007: Dynamic board has four snakes and four ladders, regenerated before every normal
and chase die. Endpoint candidates are distinct pairs from squares 2-94 excluding 50.
For snakes head>tail; for ladders foot<top. Every endpoint is unique across all dynamic
objects, live purchased snakes and fixed tree squares. This prohibits transport chains
and cycles by construction. Occupied player squares are allowed; reshuffle never moves
a player until that player's next dice landing. Square 95-100 and 50 are forbidden endpoints.

Generation discards the prior dynamic layout; reserve only trees and live purchased
snakes initially, then endpoints selected in this generation. It is bounded: enumerate legal pairs in ascending (start,end) order for each
object, choose one uniformly using the board RNG, reserve its endpoints, repeat snakes
then ladders in ID order. If no pair remains, terminate InvalidConfiguration before a
die is consumed or money charged. Never retry indefinitely or emit a partial layout.
Trees occupy 15, 35, 65, 85; those squares are reserved even when harvested.

R-008: Bought snakes are visible and affect any seat, including their owner. During
PRE_ROLL_CHOICES select head>tail from 2-94 excluding 50, fixed trees and all existing
object endpoints. Placement is validated against the currently visible board before
charging; subsequent reshuffles reserve it. Maximum one live purchased snake per owner.
Its expiry index is creation global turn + 2*seatCount; remove it at TURN_START when
current turn index reaches that value. Index starts at zero and increments after every
normal/chase turn. Failed purchase costs nothing and consumes no RNG or purchase slot.
No refund after a valid placement. The first pre-roll board is the setup layout.

## Economy and special features

R-009: One coin is one integer bank unit. Start bank=40, energy=0, shield=false.
Credit +2 at the end of each completed normal turn, including a qualifying turn.
Purchased movement costs 10, purchased snake 20; one purchase total per normal turn.
Balances cannot go below zero or exceed 1,000,000; income saturates at that cap.
Purchases require full affordability and at least two of that owner's normal turn
indices since the same purchase type last succeeded (first purchase unrestricted).
Purchase state is committed atomically. These values remain simulation candidates.

R-010: Golden Coin is offered once on final landing at 50 during a normal turn.
Decline has no effect. Accept uses Uniform(6)+1 on the independent coin stream:
1 move -10; 2 move +10; 3 bank+20; 4 bank=max(0,bank-10); 5 shield=true; 6 no effect.
An existing shield does not stack. No repeat offer until a later normal turn's landing.

R-011: A live tree credits +10 once to the active landing player, then becomes dormant.
Respawn index = harvest global turn + 2*seatCount; reactivate at TURN_START at or after
that index. There is no passive collection while standing on a tree. Multiple players
may share any square. Purchased and portal movement do not collect trees.

R-012: On every third normal turn of each seat, credit one butterfly energy at economy
update. Energy cap is 3; if energy reaches 3 and shield is false, consume 3 and set shield
true. If shield is already true, keep energy at its cap. After a later shield consumption,
conversion waits for the next normal economy update. There is no visual tap race.

R-013: On every tenth normal turn of that seat, offer a Royal Portal after coin/tree
resolution if position<95 and the player has not qualified. Accept moves +10 capped
at 95; decline does nothing. No cost, RNG, passive capture or ownership race. The same
choice and timeout are available to human and AI. Qualification skips remaining choices.

## Final Chase and settlement

R-014: A normal exact dice finish qualifies its seat. Complete that turn's economy
update, then enter Final Chase immediately; no remaining normal turns in the round.
Freeze banks except chase fees, preserve shields, remove all purchased snakes, disable
income, tree, coin, butterflies, portals and purchases. Dynamic transports still reshuffle
each chase roll, and existing shields can only be consumed, never gained.

R-015: Queue every other seat once in cyclic order after the first qualifier. A queued
seat at CHASE_CHOICE may stop or pay 10 for a die attempt, maximum three attempts.
Initialize attemptsRemaining=3 for each queued seat. BuyAttempt decrements once and
runs BOARD_RESHUFFLE, LAYOUT_LOCKED, DICE_RESOLVED, NORMAL_MOVEMENT,
LANDING_RESOLUTION (transport/shield only), WIN_CHECK, TURN_END, then CHASE_CHOICE
or next seat/settlement. Each completed paid attempt advances global index; Stop does
not. Normal-turn counts and normal choice/income phases are untouched.
Insufficient funds is equivalent to Stop. Charge before its die, apply bounce and
transport/shield rules, then either qualify or offer its next affordable attempt.
Qualification ends that seat's chase. Stop forfeits all remaining attempts. After the
last seat, settle. No free chase roll, normal income, or extra attempt for rolling six.

R-016: Rank qualified seats by remaining bank descending, then qualification order
ascending. First place wins; no random tie-break. Unqualified seats follow, sorted by
square descending, bank descending, then cyclic order after the first qualifier.
Example: A qualifies with 40; B pays twice from 60 and qualifies with 40; A wins the tie.
C pays once from 60 and qualifies with 50; C wins. A seat with 9 cannot buy an attempt.
Final Chase is bounded by 3*(seatCount-1) rolls. Settlement executes exactly once.

## Replay and randomness

R-017: Algorithm BR-SHA256-CTR-v1 uses a 32-byte master seed. For stream label L and
counter c starting at zero, hash UTF-8 `BR1:` + L + `:` + 64 lowercase seed hex digits
+ `:` + invariant decimal c, without newline. Interpret the first four digest bytes
as unsigned big-endian uint32; increment c for every block. Labels are board, dice,
coin, ai. For Uniform(n), require 1<=n<=1,000,000; reject draws >= floor(2^32/n)*n,
then return draw mod n. After 128 rejected draws return terminal RandomnessFailure;
never silently switch algorithm. Dice use Uniform(6)+1. No System.Random or Unity RNG.
Host generates the seed; test/simulation seeds are explicit. Online seed secrecy and
commit/reveal require the future server release review, not offline security claims.

R-018: Replay schema BR-REPLAY-1 stores configuration, seed, accepted external actions
including timeouts, and ordered event records. Hashing uses SHA-256 over canonical UTF-8
JSON: keys recursively sorted ordinal ASCII, integers in decimal without leading zeros,
no whitespace, no floats, ASCII schema keys and string values, booleans lowercase,
no null fields, arrays in schema order. JSON quotes/backslashes/control characters use
exact escaping: quote as backslash-quote, backslash doubled, control bytes 0x00-0x1f
as lowercase six-character \u00xx, never short escapes; slash is unescaped. State hash excludes its own hash field and host
timer/animation state. Include all R-002 state fields, including RNG cursors and action
history as accepted payloads and outcome codes, not receipt hashes (avoiding a
self-referential hash); receipt hashes are derived outputs. Configuration is embedded in the replay and its hash is recomputed on load.
Unknown schema/ruleset IDs, duplicate keys, non-ASCII values and config/hash mismatch
reject before play. Re-execution must match each event and state hash. An unkeyed hash
detects corruption, not an attacker rewriting both data and hash; no authenticity claim.

R-019: REPLAY_SCHEMA.md defines concrete field/event/action encoding; REPLAY_FIXTURES.md
contains initial reference states. Before rules freeze, complete canonical state fixtures, RNG vectors, invalid-input
cases and expected transitions for all IDs in TEST_PLAN.md. Executed cross-runtime
verification and million-match balance evidence remain separate gates. This contract
does not claim those results exist.

## Failure transactions

R-020: Each accepted external action and its internal continuation is a transaction.
On generation exhaustion or RNG rejection-limit failure, restore the pre-action bank,
layout, counters, attempts and events, then commit one terminal failure result/event,
one state-version increment and the failed action receipt. No chase fee is retained.
The terminal diagnostic records the error code and failing stream/object ID without
revealing seed material to observers. Replaying that accepted action reproduces failure.
Invalid user input is a rejection, not terminal, and changes nothing including counters.
