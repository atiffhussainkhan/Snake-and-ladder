using System.Collections.Generic;
using SnakesAndLadders.Domain.Canonical;
using SnakesAndLadders.Domain.State;

namespace SnakesAndLadders.Domain.State
{
    // R-001 + REPLAY_SCHEMA.md configuration. Profiles list is seatCount long.
    public sealed class Configuration
    {
        public string Ruleset = "planning-v1";
        public int SeatCount;
        public List<ProfileId> Profiles = new List<ProfileId>();

        public Dictionary<string, object> ToCanonical()
        {
            var profiles = new List<object>(Profiles.Count);
            for (int i = 0; i < Profiles.Count; i++)
            {
                profiles.Add(Profiles[i].ToString());
            }
            return new Dictionary<string, object>
            {
                ["profiles"] = profiles,
                ["ruleset"] = Ruleset,
                ["seatCount"] = (long)SeatCount,
            };
        }

        public byte[] CanonicalBytes()
        {
            return CanonicalJson.EncodeObjectSorted(ToCanonical());
        }

        public string Hash()
        {
            byte[] bytes = CanonicalBytes();
            byte[] digest;
            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                digest = sha.ComputeHash(bytes);
            }
            var sb = new System.Text.StringBuilder(64);
            for (int i = 0; i < digest.Length; i++) sb.Append(digest[i].ToString("x2", System.Globalization.CultureInfo.InvariantCulture));
            return sb.ToString();
        }

        // Validates the configuration per R-001.
        public bool IsValid(out string? error)
        {
            if (Ruleset != "planning-v1") { error = "ruleset must be planning-v1"; return false; }
            if (SeatCount < 2 || SeatCount > 4) { error = "seatCount must be 2..4"; return false; }
            if (Profiles.Count != SeatCount) { error = "profiles length must equal seatCount"; return false; }
            foreach (var p in Profiles)
            {
                if (!IsKnownProfile(p)) { error = "unknown profile"; return false; }
            }
            error = null;
            return true;
        }

        public static bool IsKnownProfile(ProfileId p)
        {
            return p == ProfileId.human || p == ProfileId.Racer || p == ProfileId.Saver
                || p == ProfileId.Attacker || p == ProfileId.Balanced;
        }
    }
}
