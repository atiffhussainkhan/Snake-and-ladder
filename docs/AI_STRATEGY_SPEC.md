# AI Strategy Specification

Status: Phase 2 merged planning specification; AI runtime validation pending
Authority: `SNAKES_AND_LADDERS_ENGINEERING_PLAN.md`

## Boundaries

AI selects only legal actions through the same observation/action contract as human players. It cannot inspect hidden randomness, bypass costs, alter authoritative state, or receive extra timing.

## Profiles

Selected candidate policies for [planning-v1](RULESET_CONTRACT.md), tested by T-025:

- Racer buys movement whenever legal/affordable, otherwise skips.
- Saver never purchases, declines coin, accepts portal, and stops immediately in chase.
- Attacker buys a legal snake with head nearest above the leading opponent's square;
  ties choose lowest head then lowest tail. Leader means highest square, then lowest
  seat index. If no such head or purchase is legal/affordable, skip.
- Balanced buys movement when legal and bank >=30; otherwise uses Attacker placement
  when legal and bank >=60; otherwise skips.

All except Saver buy affordable chase attempts until capped/qualified. All accept
eligible portals and request normal rolls immediately. Only Attacker accepts coin.
Priority ties above consume no RNG. Easy maps to Saver, standard to Balanced; hard
is deferred until measured stronger play. Adaptive/exploit bots remain test opponents.
These are explicit experiment inputs, not claims of achieved balance or difficulty.

The priorities above are the selected policy definitions. Hard difficulty and adaptive/exploit
policies require separately versioned definitions before their experiment cells can run.
Attacker considers opponents only, excluding its own seat; candidate heads must be
strictly greater than the selected leader square. Enumerate legal placements first,
then apply the stated distance/head/tail ordering. No legal candidate means Skip.
Balanced tries movement first and sabotage second; it never buys twice in one turn.

## Determinism and fairness

Given the same permitted observation, ruleset and profile, the decision is reproducible.
These four policies consume no AI randomness. The ai stream is reserved for separately
versioned future policies. AI and human players receive equivalent butterfly opportunities, accessibility alternatives, and information.

## Acceptance

Policy tests cover legal actions, deterministic replay, profile separation, exploit resistance, seat fairness, and adaptive-opponent performance. Setup decisions DEC-002 and DEC-008 are recorded; measured difficulty and coverage
acceptance remain pending under GATE-09. See [transition cases](TRANSITION_CASES.md).
