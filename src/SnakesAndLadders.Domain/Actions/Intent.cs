using System.Collections.Generic;
using SnakesAndLadders.Domain.State;

namespace SnakesAndLadders.Domain.Actions
{
    // External intent (action envelope from REPLAY_SCHEMA.md). The host is
    // responsible for assigning monotonically increasing per-actor actionIds.
    // Timeout origins are authenticated by host context, not by a payload flag,
    // but we still expose IsHostTimeout for the host adapter to set.
    public sealed class Intent
    {
        public string MatchId = "";
        public int Actor;
        public long ActionId;
        public long ExpectedVersion;
        public ActionType Type;
        public Dictionary<string, object> Payload = new Dictionary<string, object>();
        public bool IsHostTimeout;

        public static Intent Timeout(string matchId, int actor, long actionId, long expectedVersion)
        {
            return new Intent
            {
                MatchId = matchId,
                Actor = actor,
                ActionId = actionId,
                ExpectedVersion = expectedVersion,
                Type = ActionType.Timeout,
                IsHostTimeout = true,
            };
        }

        public static Intent Quit(string matchId, int actor, long actionId, long expectedVersion)
        {
            return new Intent
            {
                MatchId = matchId,
                Actor = actor,
                ActionId = actionId,
                ExpectedVersion = expectedVersion,
                Type = ActionType.Quit,
            };
        }
    }
}
