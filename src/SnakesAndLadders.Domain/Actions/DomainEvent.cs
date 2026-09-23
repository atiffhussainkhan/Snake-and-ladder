namespace SnakesAndLadders.Domain.Actions
{
    // Event emitted exactly once per accepted external action, per REPLAY_SCHEMA.md.
    // Receipt hashes (beforeHash/afterHash) are derived outputs outside hashed state.
    public sealed class DomainEvent
    {
        public long Cursor; // equals resulting state's eventCursor
        public long ActionId;
        public int Actor;
        public string Kind = ""; // "transition" or "terminal-failure"
        public string BeforeHash = "";
        public string AfterHash = "";
        public string Outcome = "ok"; // "ok", "invalid-configuration", "randomness-failure"
    }
}
