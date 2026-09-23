using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace SnakesAndLadders.Domain.Rng
{
    // Implements R-017: algorithm BR-SHA256-CTR-v1.
    // Stream label L, counter c starting at zero. Hash UTF-8
    // "BR1:" + L + ":" + 64 lowercase seed hex digits + ":" + decimal c.
    // Increment c per block. For Uniform(n), 1<=n<=1,000,000,
    // reject draws >= floor(2^32/n)*n. After 128 rejects return terminal
    // RandomnessFailure.
    public sealed class BrSha256CtrV1 : IRng
    {
        public const int MaxRejectionsBeforeFailure = 128;
        public const uint MaxN = 1_000_000;
        private readonly byte[] _seed; // 32 bytes
        private readonly string _seedHexLower; // 64 lowercase hex digits

        public BrSha256CtrV1(byte[] seed32)
        {
            if (seed32 == null) throw new ArgumentNullException(nameof(seed32));
            if (seed32.Length != 32) throw new ArgumentException("seed must be 32 bytes", nameof(seed32));
            _seed = (byte[])seed32.Clone();
            _seedHexLower = ToLowerHex(_seed);
        }

        // First four digest bytes interpreted as unsigned big-endian uint32.
        public uint DrawUInt32(string label, ref ulong counter)
        {
            if (label == null) throw new ArgumentNullException(nameof(label));
            string s = "BR1:" + label + ":" + _seedHexLower + ":" +
                counter.ToString(CultureInfo.InvariantCulture);
            byte[] bytes = Encoding.UTF8.GetBytes(s);
            byte[] digest;
            using (var sha = SHA256.Create())
            {
                digest = sha.ComputeHash(bytes);
            }
            counter++;
            return ((uint)digest[0] << 24) | ((uint)digest[1] << 16) | ((uint)digest[2] << 8) | digest[3];
        }

        public RandomnessFailure? Uniform(string label, ref ulong counter, uint n, out uint result)
        {
            if (n < 1 || n > MaxN)
            {
                result = 0;
                return RandomnessFailure.OutOfRange;
            }
            uint bound = (uint)((1UL << 32) / n) * n;
            int rejections = 0;
            while (true)
            {
                uint draw = DrawUInt32(label, ref counter);
                if (draw < bound)
                {
                    result = draw % n;
                    return null;
                }
                rejections++;
                if (rejections > MaxRejectionsBeforeFailure)
                {
                    result = 0;
                    return RandomnessFailure.RejectionLimit;
                }
            }
        }

        // 1..6 result.
        public RandomnessFailure? RollDice(ref ulong diceCounter, out int result)
        {
            var fail = Uniform("dice", ref diceCounter, 6, out uint u);
            if (fail.HasValue) { result = 0; return fail; }
            result = (int)u + 1;
            return null;
        }

        private static string ToLowerHex(byte[] data)
        {
            var sb = new StringBuilder(data.Length * 2);
            for (int i = 0; i < data.Length; i++) sb.Append(data[i].ToString("x2", CultureInfo.InvariantCulture));
            return sb.ToString();
        }
    }

    public enum RandomnessFailure
    {
        OutOfRange,
        RejectionLimit,
    }
}
