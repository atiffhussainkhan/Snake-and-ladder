using System;
using UnityEngine;

namespace SnakeLadder.Prototype
{
    public struct StressStats
    {
        public int gamesPlayed;
        public int gamesWon;
        public int errors;
        public int maxRollObserved;
        public int minRollObserved;
        public int maxSquareObserved;
        public int snakesHit;
        public int laddersHit;
        public int totalTurns;
        public int gamesCompleted;
    }

    // Pure-static simulator — callable from Editor batchmode without entering Play mode.
    public static class StressSimulator
    {
        public static StressStats RunBatch(int games, int maxTurnsPerGame)
        {
            var stats = new StressStats
            {
                minRollObserved = 7,
                maxRollObserved = 0,
                maxSquareObserved = 0
            };

            for (int g = 0; g < games; g++)
            {
                stats.gamesPlayed++;
                try
                {
                    SimulateOneGame(maxTurnsPerGame, ref stats);
                }
                catch (Exception e)
                {
                    stats.errors++;
                    Debug.LogError($"[StressTest] Game {g} threw: {e.GetType().Name}: {e.Message}");
                }
            }

            return stats;
        }

        private static void SimulateOneGame(int maxTurnsPerGame, ref StressStats stats)
        {
            PawnSpawner.Pawn1Square = 1;
            PawnSpawner.Pawn2Square = 1;
            int turn = 1;
            int turns = 0;

            while (turns < maxTurnsPerGame)
            {
                int roll = UnityEngine.Random.Range(1, 7);
                if (roll > stats.maxRollObserved) stats.maxRollObserved = roll;
                if (roll < stats.minRollObserved) stats.minRollObserved = roll;

                int currentSquare = turn == 1 ? PawnSpawner.Pawn1Square : PawnSpawner.Pawn2Square;
                int target = Mathf.Min(100, currentSquare + roll);

                if (turn == 1) PawnSpawner.Pawn1Square = target;
                else           PawnSpawner.Pawn2Square = target;

                int resolved = BoardBuilder.ResolveSquare(target);
                if (resolved != target)
                {
                    if (BoardBuilder.Snakes.ContainsKey(target))  stats.snakesHit++;
                    else if (BoardBuilder.Ladders.ContainsKey(target)) stats.laddersHit++;
                }
                if (turn == 1) PawnSpawner.Pawn1Square = resolved;
                else           PawnSpawner.Pawn2Square = resolved;

                int finalSquare = Mathf.Max(PawnSpawner.Pawn1Square, PawnSpawner.Pawn2Square);
                if (finalSquare > stats.maxSquareObserved) stats.maxSquareObserved = finalSquare;

                if (resolved == 100)
                {
                    stats.gamesWon++;
                    IncrementGamesCompleted(ref stats);
                    return;
                }

                turn = turn == 1 ? 2 : 1;
                turns++;
                stats.totalTurns++;
            }
            IncrementGamesCompleted(ref stats);
        }

        private static void IncrementGamesCompleted(ref StressStats stats)
        {
            stats.gamesCompleted++;
        }
    }

    // MonoBehaviour wrapper — attaches to the Dice so Shift+T in the Game view triggers the run.
    public class StressTest : MonoBehaviour
    {
        public int gamesToPlay = 1000;
        public int maxTurnsPerGame = 500;

        private bool _running;

        private void Update()
        {
            if (_running) return;
            if (Input.GetKeyDown(KeyCode.T) &&
                (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
            {
                RunStressTest();
            }
        }

        public void NotifyGameEnded()
        {
            // Disable the component entirely — Unity stops dispatching Update on disabled Behaviours,
            // so the input poll no longer costs a single per-frame call after the player wins.
            enabled = false;
        }

        public void RunStressTest()
        {
            _running = true;
            try
            {
                var stats = StressSimulator.RunBatch(gamesToPlay, maxTurnsPerGame);
                LogStats(stats, gamesToPlay);
                ResetVisibleGameState();
            }
            finally
            {
                _running = false;
            }
        }

        // The 1000-game sim leaves the static square counters at random final values and
        // doesn't touch the visual pawns. Restore both so the player can keep playing.
        private static void ResetVisibleGameState()
        {
            PawnSpawner.ResetForNewGame();
            DiceController.ResetForNewGame();
            UIHud.ResetForNewGame();
        }

        public static void LogStats(StressStats stats, int requestedGames)
        {
            float avgTurns = stats.totalTurns / (float)stats.gamesCompleted;
            Debug.Log($"[StressTest] Ran {stats.gamesPlayed}/{requestedGames} games, {stats.errors} errors, {stats.gamesWon} reached square 100");
            Debug.Log($"[StressTest] Roll range observed: {stats.minRollObserved}..{stats.maxRollObserved}");
            Debug.Log($"[StressTest] Max square reached: {stats.maxSquareObserved}");
            Debug.Log($"[StressTest] Snakes hit: {stats.snakesHit}, Ladders hit: {stats.laddersHit}");
            Debug.Log($"[StressTest] Avg turns/game: {avgTurns:F1}");
        }
    }
}
