using System.Collections.Generic;
using SnakesAndLadders.Domain.Rng;
using SnakesAndLadders.Domain.State;

namespace SnakesAndLadders.Domain.Rules
{
    // R-007 board generation: four snakes + four ladders, regenerated before
    // every normal/chase die. Distinct endpoint pairs from squares 2-94 excluding
    // 50. Snakes head>tail, ladders foot<top. Endpoints unique across all dynamic
    // objects, live purchased snakes and fixed tree squares. 95-100 and 50 are
    // forbidden endpoints. Bounded: enumerate legal pairs in ascending
    // (start,end) order for each object, choose one uniformly using the board
    // RNG, reserve its endpoints, repeat in snake/ladder order.
    public static class BoardGen
    {
        public static readonly long[] TreeSquares = { 15, 35, 65, 85 };
        public const long ForbiddenSquare = 50;
        public const long MinSquare = 2;
        public const long MaxSquare = 94;

        // Returns the generated transports (in generation order snake-0..3, ladder-0..3)
        // and the final board RNG counter. On exhaustion, returns null and leaves
        // RNG counter incremented as appropriate.
        public static GenerationResult Generate(
            IRng rng,
            ref ulong boardCounter,
            List<PurchasedTransport> livePurchased)
        {
            var reserved = new HashSet<long>();
            foreach (var t in TreeSquares) reserved.Add(t);
            reserved.Add(ForbiddenSquare);
            foreach (var p in livePurchased)
            {
                reserved.Add(p.Start);
                reserved.Add(p.End);
            }

            var result = new List<Transport>();

            // snakes 0..3
            for (int i = 0; i < 4; i++)
            {
                if (!TryPickPair(rng, ref boardCounter, reserved, snakeHeadGreater: true, out var start, out var end))
                {
                    return new GenerationResult { Exhausted = true };
                }
                var t = new Transport
                {
                    Id = "snake-" + i,
                    Start = start,
                    End = end,
                    Kind = TransportKind.snake,
                };
                result.Add(t);
                reserved.Add(start);
                reserved.Add(end);
            }

            // ladders 0..3
            for (int i = 0; i < 4; i++)
            {
                if (!TryPickPair(rng, ref boardCounter, reserved, snakeHeadGreater: false, out var start, out var end))
                {
                    return new GenerationResult { Exhausted = true };
                }
                var t = new Transport
                {
                    Id = "ladder-" + i,
                    Start = start,
                    End = end,
                    Kind = TransportKind.ladder,
                };
                result.Add(t);
                reserved.Add(start);
                reserved.Add(end);
            }

            return new GenerationResult { Transports = result };
        }

        private static bool TryPickPair(
            IRng rng,
            ref ulong boardCounter,
            HashSet<long> reserved,
            bool snakeHeadGreater,
            out long start,
            out long end)
        {
            // Enumerate legal pairs in ascending (start,end) order.
            var pairs = new List<(long start, long end)>();
            if (snakeHeadGreater)
            {
                for (long s = MinSquare; s <= MaxSquare; s++)
                {
                    if (reserved.Contains(s)) continue;
                    for (long e = MinSquare; e < s; e++)
                    {
                        if (reserved.Contains(e)) continue;
                        pairs.Add((s, e));
                    }
                }
            }
            else
            {
                for (long s = MinSquare; s <= MaxSquare; s++)
                {
                    if (reserved.Contains(s)) continue;
                    for (long e = s + 1; e <= MaxSquare; e++)
                    {
                        if (reserved.Contains(e)) continue;
                        pairs.Add((s, e));
                    }
                }
            }
            if (pairs.Count == 0)
            {
                start = 0; end = 0;
                return false;
            }
            var fail = rng.Uniform("board", ref boardCounter, (uint)pairs.Count, out uint pick);
            if (fail.HasValue)
            {
                start = 0; end = 0;
                return false;
            }
            var (ps, pe) = pairs[(int)pick];
            start = ps;
            end = pe;
            return true;
        }
    }

    public sealed class GenerationResult
    {
        public List<Transport> Transports = new List<Transport>();
        public bool Exhausted;
    }
}
