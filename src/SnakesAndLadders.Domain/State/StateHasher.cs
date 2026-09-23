using System.Security.Cryptography;
using SnakesAndLadders.Domain.Canonical;

namespace SnakesAndLadders.Domain.State
{
    // SHA-256 hash of canonical state bytes. R-018 mandates UTF-8 canonical JSON
    // sorted by ordinal ASCII key, lowercase 64-hex representation.
    public static class StateHasher
    {
        public static string Hash(MatchState s)
        {
            byte[] canonical = CanonicalJson.EncodeObjectSorted(StateEncoder.ToCanonical(s));
            byte[] digest;
            using (var sha = SHA256.Create())
            {
                digest = sha.ComputeHash(canonical);
            }
            return ToLowerHex(digest);
        }

        private static string ToLowerHex(byte[] data)
        {
            var sb = new System.Text.StringBuilder(data.Length * 2);
            for (int i = 0; i < data.Length; i++)
            {
                sb.Append(data[i].ToString("x2", System.Globalization.CultureInfo.InvariantCulture));
            }
            return sb.ToString();
        }
    }
}
