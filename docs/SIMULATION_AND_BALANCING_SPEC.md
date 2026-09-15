# Simulation and Balancing Specification

Status: Phase 2 merged planning specification; simulator execution pending
Authority: `SNAKES_AND_LADDERS_ENGINEERING_PLAN.md`

## Purpose

The simulator must use the same deterministic C# domain assembly as the future server and Unity adapters. It evaluates duration, fairness, economy, strategy, exploitability, termination, and seat advantage.

## Experiment identity

Record ruleset version, configuration hash, source revision, simulator revision, seed set, player count, mode, and strategy profiles. Paired experiments reuse identical seed sets.

## Strategies and metrics

Model Racer, Balanced, Attacker, Saver, exploit bots, and adaptive opponents with no hidden information. Report win rate by seat and strategy, duration distribution, first-finisher and Final Chase outcomes, economy sources/sinks, purchase rates, special-mechanic rates, illegal states, loops, and non-termination. Close-match, blowout and strategy thresholds are defined below. Kingmaker sensitivity
is the fraction of paired continuations in which an alternative legal sabotage choice
by the last unqualified seat changes the winner; report eligible denominators separately.
If no such choice exists, report not applicable, not zero observed impact.

## Methodology

Screen broadly, then increase samples for unstable configurations. Use confidence intervals and effect sizes. Preserve failing and extreme seeds as regression fixtures. Run at least 1,000,000 matches across accepted configurations before economy lock, subject to bounded resource controls.

## Acceptance

Selected experiment policy for [planning-v1](RULESET_CONTRACT.md), T-026:

- Integer bank units; guaranteed income 2 per completed normal seat turn. Measure tree
  and coin income separately; purchases/chase fees are sinks. R-009 parameters stay candidates.
- Predetermine seed corpus: 1,000 smoke matches, then 10,000 per candidate, then at least
  1,000,000 total across shortlisted configurations with per-cell counts. Rotate profiles
  through seats for every paired seed, with 2/3/4 seats and separate exploit/adaptive cells.
- Require zero illegal states, negative banks, unbounded operations and replay mismatches.
  DrawTurnLimit <0.1% in every cell. In four-seat mixed-profile cells only, maximum
  profile win share <=40% and Attacker share >=half the Racer/Balanced mean. Measure
  seat advantage <=5 percentage points in separate identical-policy cells or fully
  balanced seat rotations. First qualifier wins 50-80% in settled four-seat matches.
  Never apply the 40% limit to two-seat cells. Report each cell, not only pooled averages.
- Use Wilson 95% intervals for proportions and paired bootstrap intervals for differences.
  Pass only if the full relevant interval lies inside the accepted region; otherwise
  increase samples or report inconclusive. Do not stop selectively on good results.
- Close match means top-two qualified banks differ by <=10; blowout means >40. Report
  winner sensitivity to alternative legal last-unqualified-seat sabotage on paired seeds.
- Report turn counts separately from duration; the 8-12 minute median and <18 minute
  p95 require measured interaction timing, not headless CPU time.
- Bound each job to 2 workers, 60 minutes and 100,000 matches, with a 2 GiB memory target.
  Checkpoint completed seed IDs and config hashes; resume without duplicate samples.
  Large sweeps partition deterministically. Stop/save trace on first invariant failure.
  Partial or resource-aborted sweeps are incomplete, never PASS.

No simulator results exist; candidate values cannot be production-locked yet.

Every result is replayable from configuration plus seed; invariants pass; paired comparisons and seat analysis are reported; product and simulation reviewers approve thresholds.

## Build order and report ownership

After implementation authorization, build the domain and replay/conformance harness,
then legal-action AI policies, then the simulator adapter, then smoke/candidate/large
sweeps. Simulation owns experiment manifests, paired seed IDs and statistical reports;
QA owns invariant/replay failure triage; economy/product own candidate acceptance.
The same domain source runs in the simulator and client. No parallel rules engine is allowed.

Each report includes completed/requested counts by cell, seed corpus identity, seat/profile
assignments, configuration/ruleset/source hashes, elapsed resources, abort reason, replay
failure IDs, every metric denominator, uncertainty intervals and acceptance disposition.
Unqualified or abandoned matches are counted separately, not silently dropped. Qualification
bank-gap metrics use matches with at least two qualifiers and disclose that denominator.

Exploit/adaptive cells remain BLOCKED until their observation-only policies and versions
are specified. Million-match results, measured play duration and empirical balance lock
are NOT RUN; planning targets do not constitute observed performance.
