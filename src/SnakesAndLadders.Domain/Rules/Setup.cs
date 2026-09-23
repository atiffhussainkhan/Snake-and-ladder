using System.Collections.Generic;
using SnakesAndLadders.Domain.Rng;
using SnakesAndLadders.Domain.State;

namespace SnakesAndLadders.Domain.Rules
{
    // Initial state construction. R-003: validate config, generate board,
    // start seat 0, expose PRE_ROLL_CHOICES at version 0.
    public static class Setup
    {
        public static MatchState Build(
            Configuration cfg,
            string matchId,
            byte[] seed32,
            string seedHexLower,
            IRng rng,
            ulong boardCounter)
        {
            var s = new MatchState
            {
                Schema = "BR-STATE-1",
                Ruleset = cfg.Ruleset,
                ConfigHash = cfg.Hash(),
                MatchId = matchId,
                Seed = seedHexLower,
                RngBoard = boardCounter,
                Phase = Phase.PRE_ROLL_CHOICES,
                Version = 0,
                GlobalTurn = 0,
                ActiveSeat = 0,
                Choice = Choice.purchase,
                ChaseIndex = -1,
                AttemptsRemaining = 0,
                EventCursor = 0,
                Result = MatchResult.none,
            };
            for (int i = 0; i < cfg.SeatCount; i++)
            {
                var p = PlayerState.NewHuman(i);
                p.ProfileKind = cfg.Profiles[i];
                s.Players.Add(p);
            }
            foreach (var sq in BoardGen.TreeSquares)
            {
                s.Trees.Add(new Tree { Square = sq, Live = true, RespawnAt = -1 });
            }

            var gen = BoardGen.Generate(rng, ref s.RngBoard, s.Purchased);
            if (gen.Exhausted)
            {
                // R-007: terminate InvalidConfiguration before a die is consumed.
                s.Result = MatchResult.invalid_configuration;
                s.Phase = Phase.FAILED;
                s.Choice = Choice.none;
                s.Dynamic.Clear();
                s.Ranking.Clear();
                s.Players.Clear();
                s.Trees.Clear();
                return s;
            }
            s.Dynamic = gen.Transports;

            // TURN_START for seat 0: increment normalTurns.
            s.Players[0].NormalTurns = 1;
            return s;
        }
    }
}
