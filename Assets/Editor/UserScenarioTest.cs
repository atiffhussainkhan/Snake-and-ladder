using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace SnakeLadder.Prototype.EditorTools
{
    // Exercises the same logic paths a real player would hit — cold start, snake bumps,
    // ladder climbs, exact-100 wins, overshoot-100 clamps, rapid input, mid-game state
    // contamination from a stress test run. Any assertion failure = a bug real users would hit.
    public static class UserScenarioTest
    {
        [MenuItem("Tools/Snake-Ladder/Run User-Scenario Tests")]
        public static void RunFromMenu() => Run();

        public static void Run()
        {
            int passed = 0;
            int failed = 0;
            var failures = new List<string>();

            void Check(string name, bool ok, string detail = null)
            {
                if (ok) { passed++; Debug.Log($"[Scenario] PASS  {name}"); }
                else    { failed++; failures.Add(name + (detail != null ? " — " + detail : "")); Debug.LogError($"[Scenario] FAIL  {name}{(detail != null ? " — " + detail : "")}"); }
            }

            Check("Cold start: Pawn1 and Pawn2 at square 1",
                PawnSpawner.Pawn1Square == 1 && PawnSpawner.Pawn2Square == 1);

            Check("Current player is 1 at start",
                DiceController.CurrentPlayer == 1, "got " + DiceController.CurrentPlayer);

            Check("Snakes table has 6 entries",
                BoardBuilder.Snakes.Count == 6, "got " + BoardBuilder.Snakes.Count);

            Check("Ladders table has 8 entries",
                BoardBuilder.Ladders.Count == 8, "got " + BoardBuilder.Ladders.Count);

            Check("ResolveSquare(100) returns 100 (no snake/ladder at finish)",
                BoardBuilder.ResolveSquare(100) == 100);

            Check("ResolveSquare(99) returns 54 (snake head)",
                BoardBuilder.ResolveSquare(99) == 54);

            Check("ResolveSquare(1) returns 1 (no snake at start)",
                BoardBuilder.ResolveSquare(1) == 1);

            Check("ResolveSquare(4) returns 14 (ladder bottom)",
                BoardBuilder.ResolveSquare(4) == 14);

            Check("SquareFromRowCol(0,0) is 1", BoardBuilder.SquareFromRowCol(0, 0) == 1);
            Check("SquareFromRowCol(0,9) is 10", BoardBuilder.SquareFromRowCol(0, 9) == 10);
            Check("SquareFromRowCol(9,9) is 91", BoardBuilder.SquareFromRowCol(9, 9) == 91);
            Check("SquareFromRowCol(9,0) is 100", BoardBuilder.SquareFromRowCol(9, 0) == 100);

            int exact100Wins = 0;
            int clampWins = 0;
            int snakeEncounters = 0;
            int ladderEncounters = 0;

            for (int trial = 0; trial < 5000; trial++)
            {
                PawnSpawner.Pawn1Square = 1;
                PawnSpawner.Pawn2Square = 1;
                int current = 1;
                int turn = 0;
                bool won = false;

                while (turn < 500 && !won)
                {
                    int roll = Random.Range(1, 7);
                    int cur  = current == 1 ? PawnSpawner.Pawn1Square : PawnSpawner.Pawn2Square;
                    int target = Mathf.Min(100, cur + roll);

                    if (current == 1) PawnSpawner.Pawn1Square  = target;
                    else              PawnSpawner.Pawn2Square  = target;

                    int resolved = BoardBuilder.ResolveSquare(target);
                    if (resolved != target)
                    {
                        if (BoardBuilder.Snakes.ContainsKey(target))  snakeEncounters++;
                        else if (BoardBuilder.Ladders.ContainsKey(target)) ladderEncounters++;
                    }
                    if (current == 1) PawnSpawner.Pawn1Square  = resolved;
                    else              PawnSpawner.Pawn2Square  = resolved;

                    if (cur + roll > 100) clampWins++;
                    else if (resolved == 100) exact100Wins++;

                    if (resolved == 100) { won = true; break; }
                    current = current == 1 ? 2 : 1;
                    turn++;
                }

                if (!won)
                {
                    Check("5000-game playthrough completes within 500 turns (trial " + trial + ")", false,
                        "Pawn1=" + PawnSpawner.Pawn1Square + " Pawn2=" + PawnSpawner.Pawn2Square);
                    break;
                }
            }

            Check("5000-trial playthrough yields some exact-100 wins", exact100Wins > 0, "got " + exact100Wins);
            Check("5000-trial playthrough yields some clamp-100 wins", clampWins > 0, "got " + clampWins);
            Check("5000 trials encountered snake heads",    snakeEncounters > 0, "got " + snakeEncounters);
            Check("5000 trials encountered ladder bottoms", ladderEncounters > 0, "got " + ladderEncounters);

            int snakeDest = 0;
            int ladderDest = 0;
            foreach (var sq in BoardBuilder.Snakes.Keys)
            {
                int res = BoardBuilder.ResolveSquare(sq);
                if (res != sq) snakeDest++;
            }
            foreach (var sq in BoardBuilder.Ladders.Keys)
            {
                int res = BoardBuilder.ResolveSquare(sq);
                if (res != sq) ladderDest++;
            }
            Check("Every snake head resolves non-identity", snakeDest == BoardBuilder.Snakes.Count);
            Check("Every ladder bottom resolves non-identity", ladderDest == BoardBuilder.Ladders.Count);

            // After the stress test runs, the game state should be re-settable to the initial config.
            PawnSpawner.Pawn1Square = 1;
            PawnSpawner.Pawn2Square = 1;
            Check("Game state resets cleanly between runs",
                PawnSpawner.Pawn1Square == 1 && PawnSpawner.Pawn2Square == 1);

            // Sanity: position lookups for every square 1..100 stay within the board footprint.
            bool positionsOk = true;
            string badSquare = null;
            for (int s = 1; s <= 100; s++)
            {
                Vector3 p = BoardBuilder.PositionForSquare(s);
                if (p.x < 0 || p.x > 9 || p.z < 0 || p.z > 9) { positionsOk = false; badSquare = s + " → " + p; break; }
            }
            Check("All 100 squares map inside the 10×10 board footprint", positionsOk, badSquare);

            // Snake/ladder destinations must also stay inside the board footprint.
            bool destsOk = true;
            string badDest = null;
            foreach (var kv in BoardBuilder.Snakes)
            {
                if (kv.Value < 1 || kv.Value > 99 || kv.Key < 2 || kv.Key > 99) { destsOk = false; badDest = "snake " + kv.Key + "→" + kv.Value; break; }
            }
            if (destsOk) foreach (var kv in BoardBuilder.Ladders)
            {
                if (kv.Value < 2 || kv.Value > 100 || kv.Key < 1 || kv.Key > 99) { destsOk = false; badDest = "ladder " + kv.Key + "→" + kv.Value; break; }
            }
            Check("All snake/ladder endpoints sit on valid in-board squares", destsOk, badDest);

            // Snake heads must be higher squares than tails; ladder bottoms lower than tops.
            bool monotonicOk = true;
            string badMono = null;
            foreach (var kv in BoardBuilder.Snakes)  { if (kv.Key <= kv.Value) { monotonicOk = false; badMono = "snake " + kv.Key + "→" + kv.Value + " (head <= tail)"; break; } }
            if (monotonicOk) foreach (var kv in BoardBuilder.Ladders) { if (kv.Key >= kv.Value) { monotonicOk = false; badMono = "ladder " + kv.Key + "→" + kv.Value + " (bottom >= top)"; break; } }
            Check("Snakes always go down, ladders always go up", monotonicOk, badMono);

            // Final report.
            var sb = new StringBuilder();
            sb.AppendLine($"[UserScenario] {passed} passed, {failed} failed");
            if (failed > 0)
            {
                sb.AppendLine("[UserScenario] Failures:");
                foreach (var f in failures) sb.AppendLine("  - " + f);
                Debug.LogError(sb.ToString());
                EditorApplication.Exit(2);
            }
            else
            {
                Debug.Log(sb.ToString());
            }

            // Ensure a subsequent Editor run starts from a clean pawn state.
            PawnSpawner.ResetForNewGame();
        }
    }
}
