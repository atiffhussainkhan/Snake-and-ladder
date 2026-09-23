using System.Collections.Generic;
using System.Globalization;

namespace SnakesAndLadders.Domain.State
{
    // Serializes a MatchState into the canonical Dictionary<string,object> layout
    // that CanonicalJson then sorts/encodes deterministically per R-018.
    public static class StateEncoder
    {
        public static Dictionary<string, object> ToCanonical(MatchState s)
        {
            var dict = new Dictionary<string, object>
            {
                ["accepted"] = EncodeAccepted(s.Accepted),
                ["activeSeat"] = s.ActiveSeat,
                ["attemptsRemaining"] = s.AttemptsRemaining,
                ["chaseIndex"] = s.ChaseIndex,
                ["chaseQueue"] = EncodeIntArray(s.ChaseQueue),
                ["choice"] = s.Choice.ToString(),
                ["configHash"] = s.ConfigHash,
                ["dynamic"] = EncodeDynamic(s.Dynamic),
                ["eventCursor"] = s.EventCursor,
                ["globalTurn"] = s.GlobalTurn,
                ["matchId"] = s.MatchId,
                ["phase"] = s.Phase.ToString(),
                ["players"] = EncodePlayers(s.Players),
                ["purchased"] = EncodePurchased(s.Purchased),
                ["qualifications"] = EncodeIntArray(s.Qualifications),
                ["ranking"] = EncodeIntArray(s.Ranking),
                ["result"] = ResultToWire(s.Result),
                ["rng"] = new Dictionary<string, object>
                {
                    ["ai"] = s.RngAi,
                    ["board"] = s.RngBoard,
                    ["coin"] = s.RngCoin,
                    ["dice"] = s.RngDice,
                },
                ["ruleset"] = s.Ruleset,
                ["schema"] = s.Schema,
                ["seed"] = s.Seed,
                ["trees"] = EncodeTrees(s.Trees),
                ["version"] = s.Version,
            };
            return dict;
        }

        private static string ResultToWire(MatchResult r)
        {
            // Wire format strings used in REPLAY_FIXTURES.md:
            switch (r)
            {
                case MatchResult.none: return "none";
                case MatchResult.winner: return "winner";
                case MatchResult.draw_turn_limit: return "draw-turn-limit";
                case MatchResult.abandoned: return "abandoned";
                case MatchResult.invalid_configuration: return "invalid-configuration";
                case MatchResult.randomness_failure: return "randomness-failure";
                default: return "none";
            }
        }

        private static List<object> EncodeIntArray(List<int> list)
        {
            var arr = new List<object>(list.Count);
            for (int i = 0; i < list.Count; i++) arr.Add((long)list[i]);
            return arr;
        }

        private static List<object> EncodeDynamic(List<Transport> list)
        {
            // Serialize sorted by ordinal ASCII id; the engine stores in generation
            // order but the canonical wire order sorts IDs.
            var sorted = new List<Transport>(list);
            sorted.Sort((a, b) => string.CompareOrdinal(a.Id, b.Id));
            var arr = new List<object>(sorted.Count);
            foreach (var t in sorted)
            {
                arr.Add(new Dictionary<string, object>
                {
                    ["end"] = t.End,
                    ["id"] = t.Id,
                    ["kind"] = t.Kind.ToString(),
                    ["start"] = t.Start,
                });
            }
            return arr;
        }

        private static List<object> EncodePlayers(List<PlayerState> list)
        {
            var arr = new List<object>(list.Count);
            foreach (var p in list)
            {
                arr.Add(new Dictionary<string, object>
                {
                    ["bank"] = p.Bank,
                    ["energy"] = p.Energy,
                    ["lastMoveBuy"] = p.LastMoveBuy,
                    ["lastSnakeBuy"] = p.LastSnakeBuy,
                    ["normalTurns"] = p.NormalTurns,
                    ["shield"] = p.Shield,
                    ["square"] = p.Square,
                });
            }
            return arr;
        }

        private static List<object> EncodePurchased(List<PurchasedTransport> list)
        {
            // Sorted by owner per schema.
            var sorted = new List<PurchasedTransport>(list);
            sorted.Sort((a, b) => a.Owner.CompareTo(b.Owner));
            var arr = new List<object>(sorted.Count);
            foreach (var p in sorted)
            {
                arr.Add(new Dictionary<string, object>
                {
                    ["owner"] = p.Owner,
                    ["start"] = p.Start,
                    ["end"] = p.End,
                    ["expiresAt"] = p.ExpiresAt,
                });
            }
            return arr;
        }

        private static List<object> EncodeTrees(List<Tree> list)
        {
            // Sorted by square.
            var sorted = new List<Tree>(list);
            sorted.Sort((a, b) => a.Square.CompareTo(b.Square));
            var arr = new List<object>(sorted.Count);
            foreach (var t in sorted)
            {
                arr.Add(new Dictionary<string, object>
                {
                    ["live"] = t.Live,
                    ["respawnAt"] = t.RespawnAt,
                    ["square"] = t.Square,
                });
            }
            return arr;
        }

        private static List<object> EncodeAccepted(List<AcceptedAction> list)
        {
            // Sorted by resulting version.
            var sorted = new List<AcceptedAction>(list);
            sorted.Sort((a, b) => a.Version.CompareTo(b.Version));
            var arr = new List<object>(sorted.Count);
            foreach (var a in sorted)
            {
                arr.Add(new Dictionary<string, object>
                {
                    ["actionId"] = a.ActionId,
                    ["actor"] = a.Actor,
                    ["expectedVersion"] = a.ExpectedVersion,
                    ["outcome"] = OutcomeToWire(a.Outcome),
                    ["payload"] = new Dictionary<string, object>(a.Payload),
                    ["type"] = a.Type.ToString(),
                    ["version"] = a.Version,
                });
            }
            return arr;
        }

        private static string OutcomeToWire(Outcome o)
        {
            switch (o)
            {
                case Outcome.ok: return "ok";
                case Outcome.invalid_configuration: return "invalid-configuration";
                case Outcome.randomness_failure: return "randomness-failure";
                default: return "ok";
            }
        }
    }
}
