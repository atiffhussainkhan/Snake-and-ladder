using System;
using System.Collections.Generic;
using SnakesAndLadders.Domain.Actions;
using SnakesAndLadders.Domain.Rng;
using SnakesAndLadders.Domain.State;

namespace SnakesAndLadders.Domain.Rules
{
    // Pure deterministic reducer: Apply(MatchState, Intent) -> ReducerResult.
    // Implements R-001..R-020 end-to-end with R-020 failure transaction on
    // RNG exhaustion or generation failure. The reducer never reads wall
    // clock, environment variables, RNG other than the IRng passed in,
    // file system, or network. Host provides a stable IRng (deterministic
    // BrSha256CtrV1 per REPLAY_SCHEMA.md).
    public sealed class Reducer
    {
        private readonly IRng _rng;

        public Reducer(IRng rng) { _rng = rng; }

        public ReducerResult Apply(MatchState s, Intent intent)
        {
            if (IsTerminal(s.Phase)) return ReducerResult.Reject(s, "match-terminal");
            if (s.MatchId != intent.MatchId) return ReducerResult.Reject(s, "match-id-mismatch");
            if (intent.Actor < 0 || intent.Actor >= s.Players.Count) return ReducerResult.Reject(s, "unknown-actor");

            if (intent.IsHostTimeout)
            {
                if (intent.Type != ActionType.Timeout) return ReducerResult.Reject(s, "host-timeout-must-be-timeout-action");
                if (intent.Actor != s.ActiveSeat) return ReducerResult.Reject(s, "timeout-not-from-active-seat");
                if (s.Players[intent.Actor].ProfileKind != ProfileId.human) return ReducerResult.Reject(s, "timeout-not-from-human");
            }
            else if (intent.Type == ActionType.Timeout)
            {
                return ReducerResult.Reject(s, "timeout-not-from-host");
            }

            long lastAccepted = LastActionIdFor(s, intent.Actor);
            if (intent.ActionId <= lastAccepted)
            {
                var existing = FindAccepted(s, intent.Actor, intent.ActionId);
                if (existing != null)
                {
                    if (PayloadEqual(existing.Payload, intent.Payload) && existing.Type == intent.Type)
                    {
                        return new ReducerResult { State = s, Event = null, Rejection = null, Accepted = true, IsRetry = true, IsTerminalFailure = false };
                    }
                    return ReducerResult.Reject(s, "conflict-on-replay");
                }
                if (intent.ActionId == lastAccepted) return ReducerResult.Reject(s, "id-reused");
                return ReducerResult.Reject(s, "sequence-gap");
            }
            if (intent.ActionId != lastAccepted + 1) return ReducerResult.Reject(s, "sequence-gap");
            if (intent.ExpectedVersion != s.Version) return ReducerResult.Reject(s, "version-stale");

            var effectiveType = intent.Type;
            Dictionary<string, object> effectivePayload = intent.Payload;
            if (intent.Type == ActionType.Timeout)
            {
                if (!TryDefaultForPhase(s, out effectiveType)) return ReducerResult.Reject(s, "no-timeout-default-for-phase");
                effectivePayload = new Dictionary<string, object>();
            }

            if (!IsActionValidInPhase(s.Phase, effectiveType))
            {
                if (effectiveType != ActionType.Quit) return ReducerResult.Reject(s, "action-invalid-for-phase");
                if (!IsOpenChoice(s.Phase)) return ReducerResult.Reject(s, "no-quit-in-terminal-or-internal");
            }

            var before = StateHasher.Hash(s);
            var processing = Process(s, intent.Actor, effectiveType, effectivePayload);
            if (processing.FailureOutcome != Outcome.ok)
            {
                var failState = processing.State;
                failState.Phase = Phase.FAILED;
                failState.Result = processing.FailureOutcome == Outcome.invalid_configuration ? MatchResult.invalid_configuration : MatchResult.randomness_failure;
                failState.Choice = Choice.none;
                failState.AttemptsRemaining = 0;
                failState.Version = s.Version + 1;
                failState.EventCursor = s.EventCursor + 1;
                BuildRanking(failState);
                failState.Accepted.Add(new AcceptedAction { Actor = intent.Actor, ActionId = intent.ActionId, ExpectedVersion = intent.ExpectedVersion, Type = intent.Type, Payload = ClonePayload(intent.Payload), Outcome = processing.FailureOutcome, Version = failState.Version });
                var ev = new DomainEvent { Cursor = failState.EventCursor, ActionId = intent.ActionId, Actor = intent.Actor, Kind = "terminal-failure", BeforeHash = before, AfterHash = StateHasher.Hash(failState), Outcome = processing.FailureOutcome == Outcome.invalid_configuration ? "invalid-configuration" : "randomness-failure" };
                return new ReducerResult { State = failState, Event = ev, Rejection = null, Accepted = true, IsRetry = false, IsTerminalFailure = true };
            }

            var newState = processing.State;
            newState.Version = s.Version + 1;
            newState.EventCursor = s.EventCursor + 1;
            newState.Accepted.Add(new AcceptedAction { Actor = intent.Actor, ActionId = intent.ActionId, ExpectedVersion = intent.ExpectedVersion, Type = intent.Type, Payload = ClonePayload(intent.Payload), Outcome = Outcome.ok, Version = newState.Version });
            var domainEvent = new DomainEvent { Cursor = newState.EventCursor, ActionId = intent.ActionId, Actor = intent.Actor, Kind = "transition", BeforeHash = before, AfterHash = StateHasher.Hash(newState), Outcome = "ok" };
            return new ReducerResult { State = newState, Event = domainEvent, Rejection = null, Accepted = true, IsRetry = false, IsTerminalFailure = false };
        }

        private struct ProcessingResult { public MatchState State; public Outcome FailureOutcome; }

        private ProcessingResult Process(MatchState s, int actor, ActionType type, Dictionary<string, object> payload)
        {
            switch (type)
            {
                case ActionType.BuyMovement: return ProcessBuyMovement(s, actor, payload);
                case ActionType.BuySnake: return ProcessBuySnake(s, actor, payload);
                case ActionType.Skip: return ProcessSkip(s, actor);
                case ActionType.Roll: return ProcessRoll(s, actor);
                case ActionType.AcceptCoin: return ProcessAcceptCoin(s, actor);
                case ActionType.DeclineCoin: return ProcessDeclineCoin(s, actor);
                case ActionType.AcceptPortal: return ProcessAcceptPortal(s, actor);
                case ActionType.DeclinePortal: return ProcessDeclinePortal(s, actor);
                case ActionType.BuyAttempt: return ProcessBuyAttempt(s, actor);
                case ActionType.Stop: return ProcessStopChase(s, actor);
                case ActionType.Quit: return ProcessQuit(s, actor);
                default: return new ProcessingResult { State = s, FailureOutcome = Outcome.invalid_configuration };
            }
        }

        private ProcessingResult Fail(MatchState s) => new ProcessingResult { State = s, FailureOutcome = Outcome.invalid_configuration };

        private ProcessingResult ProcessBuyMovement(MatchState s, int actor, Dictionary<string, object> payload)
        {
            if (s.Phase != Phase.PRE_ROLL_CHOICES) return Fail(s);
            var p = s.Players[actor];
            if (p.Shield) return Fail(s);
            if (p.Bank < 10) return Fail(s);
            if (p.LastMoveBuy >= 0 && p.NormalTurns - p.LastMoveBuy < 2) return Fail(s);
            if (IsQualified(s)) return Fail(s);
            p.Bank -= 10; p.LastMoveBuy = p.NormalTurns; p.Shield = true;
            s.Players[actor] = p;
            s.Phase = Phase.DICE_ROLL_REQUESTED; s.Choice = Choice.roll;
            return new ProcessingResult { State = s, FailureOutcome = Outcome.ok };
        }

        private ProcessingResult ProcessBuySnake(MatchState s, int actor, Dictionary<string, object> payload)
        {
            if (s.Phase != Phase.PRE_ROLL_CHOICES) return Fail(s);
            var p = s.Players[actor];
            if (p.Bank < 20) return Fail(s);
            if (p.LastSnakeBuy >= 0 && p.NormalTurns - p.LastSnakeBuy < 2) return Fail(s);
            if (IsQualified(s)) return Fail(s);
            foreach (var ex in s.Purchased) if (ex.Owner == actor) return Fail(s);

            long start = 0, end = 0;
            var pickFail = PickPurchaseSnakeTarget(s, out start, out end);
            if (pickFail != Outcome.ok) return new ProcessingResult { State = s, FailureOutcome = pickFail };
            p.Bank -= 20; p.LastSnakeBuy = p.NormalTurns;
            s.Players[actor] = p;
            s.Purchased.Add(new PurchasedTransport { Owner = actor, Start = start, End = end, ExpiresAt = s.GlobalTurn + 2L * s.Players.Count });
            s.Phase = Phase.DICE_ROLL_REQUESTED; s.Choice = Choice.roll;
            return new ProcessingResult { State = s, FailureOutcome = Outcome.ok };
        }

        private Outcome PickPurchaseSnakeTarget(MatchState s, out long start, out long end)
        {
            var reserved = new HashSet<long>();
            foreach (var t in BoardGen.TreeSquares) reserved.Add(t);
            reserved.Add(50);
            foreach (var d in s.Dynamic) { reserved.Add(d.Start); reserved.Add(d.End); }
            foreach (var pp in s.Purchased) { reserved.Add(pp.Start); reserved.Add(pp.End); }
            for (long k = 95; k <= 100; k++) reserved.Add(k);

            var pairs = new List<(long start, long end)>();
            for (long a = 2; a <= 94; a++)
            {
                if (reserved.Contains(a)) continue;
                for (long b = 2; b < a; b++)
                {
                    if (reserved.Contains(b)) continue;
                    pairs.Add((a, b));
                }
            }
            if (pairs.Count == 0) { start = 0; end = 0; return Outcome.invalid_configuration; }
            var f = _rng.Uniform("board", ref s.RngBoard, (uint)pairs.Count, out uint idx);
            if (f.HasValue) { start = 0; end = 0; return Outcome.randomness_failure; }
            start = pairs[(int)idx].start;
            end = pairs[(int)idx].end;
            return Outcome.ok;
        }

        private ProcessingResult ProcessSkip(MatchState s, int actor)
        {
            if (s.Phase != Phase.PRE_ROLL_CHOICES) return Fail(s);
            if (actor != s.ActiveSeat) return Fail(s);
            s.Phase = Phase.DICE_ROLL_REQUESTED; s.Choice = Choice.roll;
            return new ProcessingResult { State = s, FailureOutcome = Outcome.ok };
        }

        private ProcessingResult ProcessRoll(MatchState s, int actor)
        {
            if (s.Phase != Phase.DICE_ROLL_REQUESTED || actor != s.ActiveSeat) return Fail(s);
            s.Phase = Phase.BOARD_RESHUFFLE;
            var gen = BoardGen.Generate(_rng, ref s.RngBoard, s.Purchased);
            if (gen.Exhausted) return new ProcessingResult { State = s, FailureOutcome = Outcome.invalid_configuration };
            s.Dynamic = gen.Transports;
            s.Phase = Phase.LAYOUT_LOCKED;
            var diceFail = _rng.RollDice(ref s.RngDice, out int die);
            if (diceFail.HasValue) return new ProcessingResult { State = s, FailureOutcome = Outcome.randomness_failure };
            s.Phase = Phase.DICE_RESOLVED;
            return FinishNormalTurn(s, actor, die);
        }

        private ProcessingResult FinishNormalTurn(MatchState s, int actor, int die)
        {
            s.Phase = Phase.NORMAL_MOVEMENT;
            var p = s.Players[actor];
            long fromSq = p.Square;
            long q = fromSq + die;
            if (q > 100) q = 200 - q;

            if (fromSq + die == 100)
            {
                p.Square = 100; s.Players[actor] = p;
                s.Phase = Phase.LANDING_RESOLUTION;
                s.Phase = Phase.ECONOMY_AND_COOLDOWN_UPDATE;
                EconomyUpdate(s, actor);
                s.Phase = Phase.WIN_CHECK;
                if (!EnterFinalChaseIfQualified(s, actor)) AdvanceTurn(s);
                return new ProcessingResult { State = s, FailureOutcome = Outcome.ok };
            }

            p.Square = q;
            var hit = FindTransportAt(s.Dynamic, q);
            if (hit != null)
            {
                if (hit.Kind == TransportKind.snake && p.Shield) p.Shield = false;
                else p.Square = hit.End;
            }
            s.Players[actor] = p;
            s.Phase = Phase.LANDING_RESOLUTION;

            if (p.Square == 50)
            {
                s.Phase = Phase.OPTIONAL_LANDING_CHOICE; s.Choice = Choice.coin;
                return new ProcessingResult { State = s, FailureOutcome = Outcome.ok };
            }
            return FinishLanding(s, actor, eligiblePortal: true);
        }

        private ProcessingResult FinishLanding(MatchState s, int actor, bool eligiblePortal)
        {
            var p = s.Players[actor];
            var tree = FindLiveTreeAt(s, p.Square);
            if (tree != null)
            {
                tree.Live = false;
                tree.RespawnAt = s.GlobalTurn + 2L * s.Players.Count;
                p.Bank += 10;
                s.Players[actor] = p;
            }

            if (eligiblePortal && p.Square < 95 && !IsQualified(s) && p.NormalTurns % 10 == 0)
            {
                s.Phase = Phase.POST_CHOICE_RESOLUTION; s.Choice = Choice.portal;
                return new ProcessingResult { State = s, FailureOutcome = Outcome.ok };
            }

            s.Phase = Phase.ECONOMY_AND_COOLDOWN_UPDATE;
            EconomyUpdate(s, actor);
            s.Phase = Phase.WIN_CHECK;
            if (!EnterFinalChaseIfQualified(s, actor)) AdvanceTurn(s);
            return new ProcessingResult { State = s, FailureOutcome = Outcome.ok };
        }

        private ProcessingResult ProcessAcceptCoin(MatchState s, int actor)
        {
            if (s.Phase != Phase.OPTIONAL_LANDING_CHOICE || s.Choice != Choice.coin || actor != s.ActiveSeat) return Fail(s);
            // R-013 coin stream: lower bit of next 32-bit draw. DrawUInt32 has no
            // failure mode (consumes one counter increment unconditionally).
            uint raw32 = _rng.DrawUInt32("coin", ref s.RngCoin);
            bool heads = (raw32 & 1u) == 0u;
            var p = s.Players[actor];
            if (heads)
            {
                long toSq = Math.Min(100L, p.Square + 10L);
                p.Square = toSq;
                s.Players[actor] = p;
            }
            s.Phase = Phase.POST_CHOICE_RESOLUTION; s.Choice = Choice.none;
            return FinishLanding(s, actor, eligiblePortal: true);
        }

        private ProcessingResult ProcessDeclineCoin(MatchState s, int actor)
        {
            if (s.Phase != Phase.OPTIONAL_LANDING_CHOICE || s.Choice != Choice.coin || actor != s.ActiveSeat) return Fail(s);
            s.Choice = Choice.none; s.Phase = Phase.POST_CHOICE_RESOLUTION;
            return FinishLanding(s, actor, eligiblePortal: true);
        }

        private ProcessingResult ProcessAcceptPortal(MatchState s, int actor)
        {
            if (s.Phase != Phase.POST_CHOICE_RESOLUTION || s.Choice != Choice.portal || actor != s.ActiveSeat) return Fail(s);
            var p = s.Players[actor];
            p.Square = 50; s.Players[actor] = p;
            s.Phase = Phase.ECONOMY_AND_COOLDOWN_UPDATE;
            EconomyUpdate(s, actor);
            s.Phase = Phase.WIN_CHECK;
            if (!EnterFinalChaseIfQualified(s, actor)) AdvanceTurn(s);
            return new ProcessingResult { State = s, FailureOutcome = Outcome.ok };
        }

        private ProcessingResult ProcessDeclinePortal(MatchState s, int actor)
        {
            if (s.Phase != Phase.POST_CHOICE_RESOLUTION || s.Choice != Choice.portal || actor != s.ActiveSeat) return Fail(s);
            s.Choice = Choice.none;
            s.Phase = Phase.ECONOMY_AND_COOLDOWN_UPDATE;
            EconomyUpdate(s, actor);
            s.Phase = Phase.WIN_CHECK;
            if (!EnterFinalChaseIfQualified(s, actor)) AdvanceTurn(s);
            return new ProcessingResult { State = s, FailureOutcome = Outcome.ok };
        }

        private ProcessingResult ProcessBuyAttempt(MatchState s, int actor)
        {
            int curChase = ReducerHelpers.CurrentChaseSeat(s);
            if (s.Phase != Phase.CHASE_CHOICE || actor != curChase) return Fail(s);
            var p = s.Players[actor];
            if (p.Bank < 5) return Fail(s);
            if (IsQualified(s)) return Fail(s);
            p.Bank -= 5;
            s.Players[actor] = p;
            s.Choice = Choice.chase;
            s.AttemptsRemaining++;
            return new ProcessingResult { State = s, FailureOutcome = Outcome.ok };
        }

        private ProcessingResult ProcessStopChase(MatchState s, int actor)
        {
            int curChase = ReducerHelpers.CurrentChaseSeat(s);
            if (s.Phase != Phase.CHASE_CHOICE || actor != curChase) return Fail(s);
            return ResolveChaseStep(s, actor, boughtAttempt: s.AttemptsRemaining > 0);
        }

        private ProcessingResult ProcessQuit(MatchState s, int actor)
        {
            if (actor != s.ActiveSeat) return Fail(s);
            if (s.Players[actor].ProfileKind != ProfileId.human) return Fail(s);
            if (!IsOpenChoice(s.Phase)) return Fail(s);
            s.Phase = Phase.ABANDONED; s.Result = MatchResult.abandoned;
            s.Choice = Choice.none; s.AttemptsRemaining = 0;
            BuildRanking(s);
            return new ProcessingResult { State = s, FailureOutcome = Outcome.ok };
        }

        // ---------- Helpers ----------

        private static bool IsTerminal(Phase ph) => ph == Phase.SETTLED || ph == Phase.ABANDONED || ph == Phase.FAILED || ph == Phase.DRAW;

        private static bool IsOpenChoice(Phase ph) => ph == Phase.PRE_ROLL_CHOICES || ph == Phase.DICE_ROLL_REQUESTED || ph == Phase.OPTIONAL_LANDING_CHOICE || ph == Phase.POST_CHOICE_RESOLUTION || ph == Phase.CHASE_CHOICE;

        private static bool IsActionValidInPhase(Phase ph, ActionType t)
        {
            switch (ph)
            {
                case Phase.PRE_ROLL_CHOICES: return t == ActionType.BuyMovement || t == ActionType.BuySnake || t == ActionType.Skip;
                case Phase.DICE_ROLL_REQUESTED: return t == ActionType.Roll;
                case Phase.OPTIONAL_LANDING_CHOICE: return t == ActionType.AcceptCoin || t == ActionType.DeclineCoin;
                case Phase.POST_CHOICE_RESOLUTION: return t == ActionType.AcceptPortal || t == ActionType.DeclinePortal;
                case Phase.CHASE_CHOICE: return t == ActionType.BuyAttempt || t == ActionType.Stop;
                default: return false;
            }
        }

        private static bool TryDefaultForPhase(MatchState s, out ActionType def)
        {
            switch (s.Phase)
            {
                case Phase.PRE_ROLL_CHOICES: def = ActionType.Skip; return true;
                case Phase.DICE_ROLL_REQUESTED: def = ActionType.Roll; return true;
                case Phase.OPTIONAL_LANDING_CHOICE: def = ActionType.DeclineCoin; return true;
                case Phase.POST_CHOICE_RESOLUTION: def = ActionType.DeclinePortal; return true;
                case Phase.CHASE_CHOICE: def = ActionType.Stop; return true;
                default: def = ActionType.Quit; return false;
            }
        }

        private static Transport? FindTransportAt(List<Transport> list, long sq)
        {
            for (int i = 0; i < list.Count; i++) if (list[i].Start == sq) return list[i];
            return null;
        }

        private static Tree? FindLiveTreeAt(MatchState s, long sq)
        {
            for (int i = 0; i < s.Trees.Count; i++) if (s.Trees[i].Square == sq && s.Trees[i].Live) return s.Trees[i];
            return null;
        }

        private static bool IsQualified(MatchState s)
        {
            for (int i = 0; i < s.Qualifications.Count; i++) if (s.Qualifications[i] == s.ActiveSeat) return true;
            return false;
        }

        private void EconomyUpdate(MatchState s, int actor)
        {
            var p = s.Players[actor];
            if (p.Bank < 1_000_000L) p.Bank = Math.Min(1_000_000L, p.Bank + 2L);
            if (p.NormalTurns > 0 && p.NormalTurns % 3 == 0)
            {
                if (p.Energy < 3) p.Energy++;
                if (p.Energy >= 3 && !p.Shield)
                {
                    p.Energy = 0;
                    p.Shield = true;
                }
            }
            s.Players[actor] = p;
        }

        private bool EnterFinalChaseIfQualified(MatchState s, int actor)
        {
            var p = s.Players[actor];
            if (p.Square != 100) return false;
            if (!s.Qualifications.Contains(actor)) s.Qualifications.Add(actor);
            if (s.ChaseQueue.Count == 0 && s.Qualifications.Count == 1)
            {
                for (int i = 0; i < s.Players.Count; i++) if (i != actor) s.ChaseQueue.Add(i);
                s.ChaseIndex = 0; s.Phase = Phase.CHASE_CHOICE; s.Choice = Choice.chase; s.AttemptsRemaining = 0;
                return true;
            }
            bool allQualified = true;
            for (int i = 0; i < s.Players.Count; i++) if (!s.Qualifications.Contains(i)) { allQualified = false; break; }
            if (allQualified)
            {
                BuildRanking(s);
                s.Result = MatchResult.winner; s.Phase = Phase.SETTLED; s.Choice = Choice.none; s.ChaseIndex = s.ChaseQueue.Count;
                return false;
            }
            BuildRanking(s);
            s.Result = MatchResult.winner; s.Phase = Phase.SETTLED; s.Choice = Choice.none; s.ChaseIndex = s.ChaseQueue.Count;
            return false;
        }

        private void AdvanceTurn(MatchState s)
        {
            s.GlobalTurn++;
            for (int i = s.Purchased.Count - 1; i >= 0; i--)
                if (s.Purchased[i].ExpiresAt == s.GlobalTurn) s.Purchased.RemoveAt(i);
            for (int i = 0; i < s.Trees.Count; i++)
            {
                if (!s.Trees[i].Live && s.Trees[i].RespawnAt >= 0 && s.Trees[i].RespawnAt <= s.GlobalTurn)
                {
                    s.Trees[i].Live = true; s.Trees[i].RespawnAt = -1;
                }
            }
            int nextSeat = (s.ActiveSeat + 1) % s.Players.Count;
            s.ActiveSeat = nextSeat;
            s.Players[nextSeat].NormalTurns++;
            s.Phase = Phase.PRE_ROLL_CHOICES; s.Choice = Choice.purchase;
            long rounds = s.GlobalTurn / s.Players.Count;
            if (rounds >= 400 && s.Result == MatchResult.none)
            {
                s.Phase = Phase.DRAW; s.Result = MatchResult.draw_turn_limit; s.Choice = Choice.none;
                BuildRanking(s);
            }
        }

        private ProcessingResult ResolveChaseStep(MatchState s, int actor, bool boughtAttempt)
        {
            var gen = BoardGen.Generate(_rng, ref s.RngBoard, s.Purchased);
            if (gen.Exhausted) return new ProcessingResult { State = s, FailureOutcome = Outcome.invalid_configuration };
            s.Dynamic = gen.Transports;
            var diceFail = _rng.RollDice(ref s.RngDice, out int die);
            if (diceFail.HasValue) return new ProcessingResult { State = s, FailureOutcome = Outcome.randomness_failure };

            var p = s.Players[actor];
            long q = p.Square + die;
            if (q > 100) q = 200 - q;
            long finalSq = q;
            var hit = FindTransportAt(s.Dynamic, q);
            if (hit != null)
            {
                if (hit.Kind == TransportKind.snake && p.Shield) p.Shield = false;
                else finalSq = hit.End;
            }
            p.Square = finalSq;
            s.Players[actor] = p;
            if (finalSq == 100 && !s.Qualifications.Contains(actor)) s.Qualifications.Add(actor);
            s.ChaseIndex++;
            if (s.ChaseIndex >= s.ChaseQueue.Count)
            {
                BuildRanking(s);
                s.Result = MatchResult.winner; s.Phase = Phase.SETTLED; s.Choice = Choice.none;
                return new ProcessingResult { State = s, FailureOutcome = Outcome.ok };
            }
            s.Phase = Phase.CHASE_CHOICE; s.Choice = Choice.chase;
            return new ProcessingResult { State = s, FailureOutcome = Outcome.ok };
        }

        private void BuildRanking(MatchState s)
        {
            s.Ranking.Clear();
            foreach (var q in s.Qualifications) if (!s.Ranking.Contains(q)) s.Ranking.Add(q);
            var rest = new List<int>();
            for (int i = 0; i < s.Players.Count; i++) if (!s.Ranking.Contains(i)) rest.Add(i);
            rest.Sort((a, b) =>
            {
                int cmp = s.Players[b].Square.CompareTo(s.Players[a].Square);
                if (cmp != 0) return cmp;
                return s.Players[b].Bank.CompareTo(s.Players[a].Bank);
            });
            foreach (var id in rest) s.Ranking.Add(id);
        }

        private static long LastActionIdFor(MatchState s, int actor)
        {
            long max = -1;
            for (int i = 0; i < s.Accepted.Count; i++) if (s.Accepted[i].Actor == actor && s.Accepted[i].ActionId > max) max = s.Accepted[i].ActionId;
            return max;
        }

        private static AcceptedAction? FindAccepted(MatchState s, int actor, long actionId)
        {
            for (int i = 0; i < s.Accepted.Count; i++) if (s.Accepted[i].Actor == actor && s.Accepted[i].ActionId == actionId) return s.Accepted[i];
            return null;
        }

        private static bool PayloadEqual(Dictionary<string, object> a, Dictionary<string, object> b)
        {
            if (a.Count != b.Count) return false;
            foreach (var kv in a) { if (!b.TryGetValue(kv.Key, out var v)) return false; if (!ValueEquals(v, kv.Value)) return false; }
            return true;
        }

        private static bool ValueEquals(object a, object b)
        {
            if (a == null && b == null) return true;
            if (a == null || b == null) return false;
            if (a is long la && b is long lb) return la == lb;
            if (a is int ia && b is int ib) return ia == ib;
            if (a is string sa && b is string sb) return sa == sb;
            if (a is bool ba && b is bool bb) return ba == bb;
            return false;
        }

        private static Dictionary<string, object> ClonePayload(Dictionary<string, object> p)
        {
            var d = new Dictionary<string, object>(p.Count);
            foreach (var kv in p) d[kv.Key] = kv.Value;
            return d;
        }
    }

    public static class ReducerHelpers
    {
        public static int CurrentChaseSeat(MatchState s)
        {
            if (s.ChaseIndex < 0 || s.ChaseIndex >= s.ChaseQueue.Count) return -1;
            return s.ChaseQueue[(int)s.ChaseIndex];
        }
    }
}
