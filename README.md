# Snakes and Ladders

Unity + C# game in pre-production planning. **CODE WORK NOT APPROVED**.

- [Master engineering plan](docs/SNAKES_AND_LADDERS_ENGINEERING_PLAN.md): source of truth and gates.
- [Phase 2 completion report](docs/PHASE_2_REVIEW.md): Phase 1 re-review, merged specs and remaining technical blockers.
- [Phase 3 validation authorization](docs/PHASE_3_VALIDATION_AUTHORIZATION.md): bounded validation scope; production implementation remains blocked.
- [Phase 3 prototype authorization](docs/PHASE_3_PROTOTYPE_AUTHORIZATION.md): owner-authorized non-production graphical prototype.
- [Transition cases](docs/TRANSITION_CASES.md): semantic scenarios across all 26 test families.
- [Prior gate review](docs/PLANNING_GATE_REVIEW.md): owner approval, decisions and remaining evidence.
- [Setup ADR](docs/SETUP_ADR.md)
- [Ruleset contract](docs/RULESET_CONTRACT.md)
- [Test traceability](docs/TEST_TRACEABILITY.md)
- [Replay schema](docs/REPLAY_SCHEMA.md) and [fixtures](docs/REPLAY_FIXTURES.md)
- [Source and approval](docs/SOURCE_AND_APPROVAL_RECORD.md)
- [Historical Phase 1 review](docs/PHASE_1_REVIEW.md): repository evidence, corrections, and setup blockers.
- [Game rules](docs/GAME_RULES_SPEC.md)
- [State machine](docs/STATE_MACHINE_SPEC.md)
- [Simulation and balancing](docs/SIMULATION_AND_BALANCING_SPEC.md)
- [AI strategy](docs/AI_STRATEGY_SPEC.md)
- [Test plan](docs/TEST_PLAN.md)
- [Agent orchestration](docs/AGENT_ORCHESTRATION_PLAN.md)
- [Repository instructions](AGENTS.md)

Phase 1 re-review and Phase 2 planning merge are complete under owner approval.
Phase 2 here means the master plan's specification merge; bounded Phase 3 validation and
a non-production graphical prototype are authorized, but C# domain implementation
remains gated. The isolated Phase 3
replay-fixture check is published; technical freeze evidence, Unity project creation
and runtime cross-checks remain pending. No Unity game project or gameplay code exists
yet. Local `.env` files must remain
untracked; `.env.example` contains documentation defaults and blank secret fields.

## Validation files

- [Phase 3 validation report](docs/PHASE_3_VALIDATION_REPORT.md)
- [Fixture harness project](validation/Phase3FixtureHarness/Phase3FixtureHarness.csproj)
- [Fixture harness source](validation/Phase3FixtureHarness/Program.cs)

## Repository configuration

- [Environment template](.env.example) — placeholder values only; keep real `.env` files local.
- [Git ignore rules](.gitignore)
- [Editor settings](.editorconfig)
