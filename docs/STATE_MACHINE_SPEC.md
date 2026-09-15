# State Machine Specification

Status: Phase 2 merged planning specification; runtime validation pending.
Authority: [planning-v1](RULESET_CONTRACT.md) R-002/003/004/014/015/020,
[replay schema](REPLAY_SCHEMA.md), and [action interface](SETUP_ADR.md).

## Initialization and persisted boundaries

Validate configuration, generate the initial layout, start seat 0, and expose
PRE_ROLL_CHOICES at version 0. Initial setup failure returns InvalidConfiguration;
it has no accepted external action. Internal phases are synchronous parts of a transaction,
not places where another command or animation acknowledgement may intervene.

| Persisted phase / choice | New legal actions | Host Timeout default | Successful continuation |
| --- | --- | --- | --- |
| PRE_ROLL_CHOICES / purchase | BuyMovement, BuySnake, Skip, Quit | Skip | Purchase or skip opens DICE_ROLL_REQUESTED; Quit abandons |
| DICE_ROLL_REQUESTED / roll | Roll, Quit | Roll | Generate/lock board, draw die, resolve movement and landing |
| OPTIONAL_LANDING_CHOICE / coin | AcceptCoin, DeclineCoin, Quit | DeclineCoin | Resolve coin, eligible tree and portal, or normal turn end |
| POST_CHOICE_RESOLUTION / portal | AcceptPortal, DeclinePortal, Quit | DeclinePortal | Apply/decline portal, then economy and turn end |
| CHASE_CHOICE / chase | BuyAttempt, Stop, Quit | Stop | Paid roll or next queued seat; settlement after final seat |
| SETTLED, ABANDONED, FAILED, DRAW / none | None | Reject | Terminal; identical accepted retry still returns original receipt |

Quit uses the R-003 active-human rule; host close records active-seat Quit even for AI.
Timeout origin is host-authenticated. All unlisted action/phase pairs reject without
state, RNG, history or event changes. Input validation failures stay at the current choice.

## Normal turn continuation

TURN_START increments the active seat's normal-turn count, expires purchased snakes
and reactivates due trees. PRE_ROLL_CHOICES permits at most one successful purchase.
Roll continues through BOARD_RESHUFFLE, LAYOUT_LOCKED, DICE_RESOLVED, NORMAL_MOVEMENT,
LANDING_RESOLUTION, any coin and portal prompts, ECONOMY_AND_COOLDOWN_UPDATE, WIN_CHECK
and TURN_END. The full order and skips are in R-003/005/006/010/013/014.

Exact dice qualification skips remaining landing choices, completes normal economy,
then starts chase. Otherwise turn end advances globalTurn and seat. After seatCount-1
completes the 400th normal round without qualification, terminate DrawTurnLimit before
starting another normal turn. Each subsequent external choice has a fresh host timer;
offline pause suspends it without changing domain state.

## Chase continuation and settlement

Enter chase immediately after first qualification. Clear purchased snakes, preserve
shields, disable normal rewards/features, and queue all other seats cyclically. Initialize
three attempts for each queued seat. An affordable BuyAttempt consumes one attempt and
fee, generates and locks the board, draws and resolves one die and transport/shield,
checks qualification and advances globalTurn. Normal-turn counts do not change.

A qualifier leaves the queue; Stop forfeits the remaining attempts without incrementing
globalTurn. No affordable attempt or zero remaining attempts advances to the next seat.
After the last seat, apply R-016 ranking once, set SETTLED/winner and close the choice.
No free rolls or bonuses occur. All terminal states have no pending choice.

## Validation, retries and transactions

Validate match/actor authority, then accepted ID lookup. Same canonical payload under
an accepted ID returns the original receipt, including after termination; changed payload
rejects Conflict. New IDs must be the next per-actor accepted ID, then pass terminal,
expectedVersion, phase and payload legality checks. Rejections consume no IDs.

Each new accepted command and automatic continuation commits one version increment,
one accepted-history entry and one event. A coin/portal prompt ends that transaction;
its answer is a separate command. Event and receipt hashes are derived outside state.
On generation/RNG failure R-020 restores the pre-command values before committing only
the prescribed terminal failure record, event and version. In particular, no chase fee
or RNG consumption from the failed continuation survives.

## Host and future network ownership

Application routes intents; infrastructure supplies clock/seed/storage adapters;
presentation observes results. Host timers emit recorded Timeout inputs. Offline replay
restores a verified prefix; timer/animation metadata is excluded from authoritative hashes.
Online reconnect, AFK and security testing remain deferred to the separate server release
in the ADR. No offline host is represented as a secure multiplayer server.

## Acceptance evidence

[Transition cases](TRANSITION_CASES.md) and T-002/003/004/014/015/016/020/021 cover the
boundaries above. Serialized fixtures exist only for the initial pack; exhaustive concrete
canonical transitions and executed C#/Mono/IL2CPP agreement remain BLOCKED.
