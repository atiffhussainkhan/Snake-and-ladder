using SnakesAndLadders.Domain.State;

namespace SnakesAndLadders.Domain.Actions
{
    public sealed class ReducerResult
    {
        public MatchState State = null!;       // new state on accept/retry, original on reject
        public DomainEvent? Event;             // null on reject/identical retry
        public string? Rejection;              // null on accept/retry
        public bool Accepted;                  // true on accept (including terminal failure) or identical retry
        public bool IsRetry;                   // true on identical retry (no new event)
        public bool IsTerminalFailure;         // true when committing R-020 failure

        public static ReducerResult Reject(MatchState s, string reason)
        {
            return new ReducerResult
            {
                State = s,
                Rejection = reason,
                Accepted = false,
                IsRetry = false,
                IsTerminalFailure = false,
            };
        }
    }
}
