# BR-REPLAY-1 Reference Fixtures

Status: planning diagnostics, not executed C#/Unity tests. Schema: [BR-REPLAY-1](REPLAY_SCHEMA.md).

Fixture F-001 uses zero seed, two human seats and match ID fixture-001.
Canonical bytes are the UTF-8 content inside each code block, with no final newline.
Computed independently with Python hashlib/json and the documented bounded pair-selection
algorithm. The strings in these fixtures contain no control characters; F-002 below
provides an escaping check. Recompute on every supported runtime before GATE-07 passes.

## Configuration

SHA-256: `422adc7d5011a3206ee4a10c43e97567a4a8648b63e7834895a2b480237d8dcc`.

```json
{"profiles":["human","human"],"ruleset":"planning-v1","seatCount":2}
```

## Initial state

SHA-256: `89a657e2b56467b5ab3a5ddb2f649cabfc0c6439cd946bfe6fdcf0784584b6a7`.

```json
{"accepted":[],"activeSeat":0,"attemptsRemaining":0,"chaseIndex":-1,"chaseQueue":[],"choice":"purchase","configHash":"422adc7d5011a3206ee4a10c43e97567a4a8648b63e7834895a2b480237d8dcc","dynamic":[{"end":69,"id":"ladder-0","kind":"ladder","start":14},{"end":45,"id":"ladder-1","kind":"ladder","start":8},{"end":63,"id":"ladder-2","kind":"ladder","start":9},{"end":19,"id":"ladder-3","kind":"ladder","start":10},{"end":44,"id":"snake-0","kind":"snake","start":72},{"end":66,"id":"snake-1","kind":"snake","start":79},{"end":17,"id":"snake-2","kind":"snake","start":56},{"end":2,"id":"snake-3","kind":"snake","start":76}],"eventCursor":0,"globalTurn":0,"matchId":"fixture-001","phase":"PRE_ROLL_CHOICES","players":[{"bank":40,"energy":0,"lastMoveBuy":-1,"lastSnakeBuy":-1,"normalTurns":1,"shield":false,"square":1},{"bank":40,"energy":0,"lastMoveBuy":-1,"lastSnakeBuy":-1,"normalTurns":0,"shield":false,"square":1}],"purchased":[],"qualifications":[],"ranking":[],"result":"none","rng":{"ai":0,"board":8,"coin":0,"dice":0},"ruleset":"planning-v1","schema":"BR-STATE-1","seed":"0000000000000000000000000000000000000000000000000000000000000000","trees":[{"live":true,"respawnAt":-1,"square":15},{"live":true,"respawnAt":-1,"square":35},{"live":true,"respawnAt":-1,"square":65},{"live":true,"respawnAt":-1,"square":85}],"version":0}
```

## Action

SHA-256: `547ca657ace388605ef1297509e4f0032eda8cd48cef88b87fd62b2cf8e43753`.

```json
{"actionId":1,"actor":0,"expectedVersion":0,"matchId":"fixture-001","payload":{},"type":"Skip"}
```

## Resulting state

SHA-256: `82f88de95c5a57134018439674608c8adef536685dab77a936d565be831a2f3c`.

```json
{"accepted":[{"actionId":1,"actor":0,"expectedVersion":0,"outcome":"ok","payload":{},"type":"Skip","version":1}],"activeSeat":0,"attemptsRemaining":0,"chaseIndex":-1,"chaseQueue":[],"choice":"roll","configHash":"422adc7d5011a3206ee4a10c43e97567a4a8648b63e7834895a2b480237d8dcc","dynamic":[{"end":69,"id":"ladder-0","kind":"ladder","start":14},{"end":45,"id":"ladder-1","kind":"ladder","start":8},{"end":63,"id":"ladder-2","kind":"ladder","start":9},{"end":19,"id":"ladder-3","kind":"ladder","start":10},{"end":44,"id":"snake-0","kind":"snake","start":72},{"end":66,"id":"snake-1","kind":"snake","start":79},{"end":17,"id":"snake-2","kind":"snake","start":56},{"end":2,"id":"snake-3","kind":"snake","start":76}],"eventCursor":1,"globalTurn":0,"matchId":"fixture-001","phase":"DICE_ROLL_REQUESTED","players":[{"bank":40,"energy":0,"lastMoveBuy":-1,"lastSnakeBuy":-1,"normalTurns":1,"shield":false,"square":1},{"bank":40,"energy":0,"lastMoveBuy":-1,"lastSnakeBuy":-1,"normalTurns":0,"shield":false,"square":1}],"purchased":[],"qualifications":[],"ranking":[],"result":"none","rng":{"ai":0,"board":8,"coin":0,"dice":0},"ruleset":"planning-v1","schema":"BR-STATE-1","seed":"0000000000000000000000000000000000000000000000000000000000000000","trees":[{"live":true,"respawnAt":-1,"square":15},{"live":true,"respawnAt":-1,"square":35},{"live":true,"respawnAt":-1,"square":65},{"live":true,"respawnAt":-1,"square":85}],"version":1}
```

## Event

SHA-256: `b4f9548ae12cb9784956e7117d54de74f9c3edf788d0eb103018769865f045d6`.

```json
{"actionId":1,"actor":0,"afterHash":"82f88de95c5a57134018439674608c8adef536685dab77a936d565be831a2f3c","beforeHash":"89a657e2b56467b5ab3a5ddb2f649cabfc0c6439cd946bfe6fdcf0784584b6a7","cursor":1,"kind":"transition","outcome":"ok"}
```

## Boundary fixture cases

F-002 canonical byte check: an object with key x and a one-byte LF string value
serializes to the 14 ASCII bytes `{"x":"\u000a"}` (the literal escape, not a newline).
Short `\n` serialization is noncanonical. Hashes are computed over canonical output,
not arbitrary input whitespace. Parser may accept valid equivalent JSON then canonicalize.

F-003: replay with config seatCount1 rejects before board generation.
F-004: F-001 action repeated identically returns its original receipt, with no state/event change.
F-005: F-001 actionId1 reused with typeRoll rejects Conflict, with no changes.
F-006: F-001 initial state with seed changed but board retained rejects initial-state comparison.
F-007: F-001 result event with afterHash changed rejects event comparison.
F-008: any state with duplicate schema keys rejects parsing before execution.
F-009: new actionId3 against F-001 result rejects sequence gap without RNG consumption.
F-010: actor1 action at F-001 initial choice rejects out-of-turn; unchanged state.
F-011: canonical config hash uses the exact three keys above; unknown parameter keys reject.

## Exhaustive action-phase expectation

For every persisted phase and every action type in REPLAY_SCHEMA.md, default is rejection
unless the R-003 table lists it or it is active-seat Quit at an external choice. Timeout
requires authenticated host-timer origin. Resolve duplicate receipts before new-action
terminal/phase checks. Thus all unlisted pairs, including actions at internal or terminal
phases, have an explicit unchanged-state expected result. Each legal family also needs
its T-001 through T-026 concrete runtime cases; this small pack does not claim full coverage.
