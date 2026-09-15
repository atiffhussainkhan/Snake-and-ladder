# BR-REPLAY-1 Schema and Fixture Contract

Status: selected planning schema; C#/Unity conformance NOT RUN. Implements R-018/019.
All objects reject unknown/missing keys, duplicate keys, non-ASCII strings, floats,
nulls and out-of-range integers. Only fields in the tables below are accepted.
Canonical JSON follows R-018, including exact control escaping and sorted object keys.
Empty arrays are retained. All integer arithmetic is checked. No optional fields exist;
use -1 only where explicitly defined. These fields are the complete R-002 representation.

## Configuration

| Key | Type and constraint |
| --- | --- |
| ruleset | Exact string planning-v1 |
| seatCount | Integer 2-4 |
| profiles | seatCount entries, each human, Racer, Saver, Attacker or Balanced |

All R-001 through R-020 numeric parameters are fixed by this ruleset ID; changing one
requires a new ruleset version and fixtures. Experiment candidates use separate version
IDs and must register their full parameter schema before execution. Hash configuration
as this exact object, not a hash of a filename or process environment.

## Authoritative state

| Key | Type and constraint |
| --- | --- |
| schema | Exact BR-STATE-1 |
| ruleset | Exact planning-v1 |
| configHash | 64 lowercase hex characters, SHA-256 of canonical configuration |
| matchId | 1-64 characters from a-z, 0-9, hyphen |
| seed | 64 lowercase hex characters; secret from observers, included in offline replay |
| rng | Object with ai, board, coin, dice unsigned 64-bit counters |
| phase | One of R-003 normal phases, CHASE_CHOICE, SETTLED, ABANDONED, FAILED, DRAW |
| version | Nonnegative signed 64-bit; starts at0 |
| globalTurn | Nonnegative signed 64-bit; first normal turn0 |
| activeSeat | Integer 0 through seatCount-1, retained on terminal state |
| players | Seat-indexed Player objects, schema below |
| dynamic | Eight Transport objects sorted by ID, or empty in failed setup |
| purchased | PurchasedTransport objects sorted by owner |
| trees | Tree objects sorted by square |
| choice | none, purchase, roll, coin, portal or chase; none at terminal |
| qualifications | Unique seat IDs in qualification order |
| chaseQueue | Unique non-first-qualifier seat IDs in cyclic order, or empty before chase |
| chaseIndex | -1 before chase; queue index during chase; queue length after settlement |
| attemptsRemaining | 0 before chase/terminal, 0-3 in chase |
| accepted | AcceptedAction objects sorted by version accepted |
| eventCursor | Nonnegative signed 64-bit; starts0; one per accepted external action |
| result | none, winner, draw-turn-limit, abandoned, invalid-configuration or randomness-failure |
| ranking | All seat IDs in R-016 order at winner, otherwise empty |

| Object | Exact keys and constraints |
| --- | --- |
| Player | bank integer0-1000000; energy0-3; lastMoveBuy and lastSnakeBuy integer -1 or normal-turn index; normalTurns nonnegative integer; shield boolean; square1-100 |
| Transport | id snake-0 through snake-3 or ladder-0 through ladder-3; start/end legal squares from R-007; kind snake or ladder |
| PurchasedTransport | owner valid seat; start/end from R-008; expiresAt nonnegative global turn index |
| Tree | square15/35/65/85; live boolean; respawnAt -1 while live, otherwise nonnegative deadline |
| AcceptedAction | actor valid seat; actionId positive signed64; expectedVersion nonnegative signed64; type legal action enum; payload object below; outcome ok, invalid-configuration or randomness-failure; version resulting positive signed64 |

IDs sort ordinal ASCII. Board generation reservation order is snake-0..3 then
ladder-0..3, even though final serialized arrays sort IDs ladder-first. Sorting never
changes generation order or RNG consumption. Qualified status is derived solely from
qualifications; cooldown deadlines derive from last purchase indices, not redundant fields.
No transient internal phase is exposed for accepting commands; its temporary values
need not survive a completed atomic action. choice records every persistent pending prompt.
The normal round number derives from per-seat normalTurns; a complete round occurs at
normal TURN_END of seatCount-1. Only then test the 400-round bound if no qualifier.

## Actions and event records

Action exact keys are matchId, actor, actionId, expectedVersion, type and payload.
The first four use state/AcceptedAction constraints. Types are BuyMovement, BuySnake,
Skip, Roll, AcceptCoin, DeclineCoin, AcceptPortal, DeclinePortal, BuyAttempt, Stop,
Timeout and Quit. BuySnake payload is exactly {head:integer,tail:integer}; every other
payload is an empty object. Rejected actions never enter accepted history or replay.
Timeout actions are accepted only from the host timer adapter on behalf of activeSeat;
humans/bots cannot forge that origin at the host boundary. Origin is authenticated host
context, not an untrusted payload property. Replay replays recorded authoritative actions.

An identical accepted retry returns its original receipt and produces no event. Every
new accepted action emits exactly one event after its entire internal continuation:

| Event key | Meaning |
| --- | --- |
| cursor | Result state's eventCursor |
| actionId | Accepted command ID |
| actor | Accepted command actor |
| kind | transition on success, terminal-failure for R-020 failure |
| beforeHash | SHA-256 canonical pre-action state |
| afterHash | SHA-256 canonical resulting state |
| outcome | ok, invalid-configuration or randomness-failure |

Events and cached receipts are derived outputs outside hashed state; accepted history
contains payloads/outcome/version only, never a hash that refers back to itself. Snapshot
diffs drive presentation; phase animation details are not authoritative replay events.
R-020 restores pre-action data, then sets FAILED, choice none, result error code,
attemptsRemaining0 and empty ranking; append failed accepted record and increment version
and eventCursor once. The failure stream/object is host diagnostic data outside replay
state; replay reproduces the same failure code without requiring diagnostic wording.

## Replay envelope and validation order

Envelope exact keys: schema (BR-REPLAY-1), config, initialState, actions, events, finalHash.
Config is the configuration above; actions/events arrays have equal length and correspond
one-to-one. initialState is generated from config/matchId/seed through validated setup,
not trusted as an arbitrary save. Recompute and compare it before applying any action.
finalHash is initial hash for an empty replay, otherwise last event afterHash.
Reject oversized inputs before allocation: maximum10000 actions/events,4 MiB accepted
history bytes per state, and16 MiB envelope. Stream processing is permitted. Values above
limits reject rather than truncate. Offline save of a pending choice is a verified replay
prefix; remaining timer duration is presentation/host metadata, not authority.

Validate byte/shape limits, schema and ruleset, field constraints, config hash, regenerated
initial state, then each action/derived event, then final hash. Reject mismatched cursors,
hashes, or action IDs. Never execute unknown migrations implicitly. A future schema
requires an explicit versioned migration plus retained fixtures; old readers reject it.

## Fixtures

[Replay fixtures](REPLAY_FIXTURES.md) provide exact canonical initial configuration,
state and a Skip transition using zero seed and two human seats. The source hashes and
RNG values are planning diagnostics only. T-018/019 require reproducing these bytes and
all transition families on the selected C#/Mono/IL2CPP runtimes before conformance passes.
