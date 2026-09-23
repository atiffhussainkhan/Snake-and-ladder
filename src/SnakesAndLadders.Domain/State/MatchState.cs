using System.Collections.Generic;

namespace SnakesAndLadders.Domain.State
{
    // Top-level authoritative match state. R-002 + REPLAY_SCHEMA.md.
    // All integer values are 64-bit signed unless the schema constrains smaller.
    public sealed class MatchState
    {
        // Identity
        public string Schema = "BR-STATE-1";
        public string Ruleset = "planning-v1";
        public string ConfigHash = ""; // 64 lowercase hex chars
        public string MatchId = "";
        public string Seed = ""; // 64 lowercase hex chars

        // RNG counters (per stream).
        public ulong RngAi;
        public ulong RngBoard;
        public ulong RngCoin;
        public ulong RngDice;

        // Phases & versions
        public Phase Phase = Phase.PRE_ROLL_CHOICES;
        public long Version; // starts at 0
        public long GlobalTurn; // starts at 0
        public int ActiveSeat;

        public List<PlayerState> Players = new List<PlayerState>();
        public List<Transport> Dynamic = new List<Transport>();
        public List<PurchasedTransport> Purchased = new List<PurchasedTransport>();
        public List<Tree> Trees = new List<Tree>();

        public Choice Choice = Choice.purchase;
        public List<int> Qualifications = new List<int>();
        public List<int> ChaseQueue = new List<int>();
        public long ChaseIndex = -1; // -1 before chase, queue index during chase, queue length after settlement
        public long AttemptsRemaining; // 0 before chase/terminal, 0..3 in chase

        public List<AcceptedAction> Accepted = new List<AcceptedAction>();
        public long EventCursor; // starts at 0; one per accepted external action

        public MatchResult Result = MatchResult.none;
        public List<int> Ranking = new List<int>();
    }

    public sealed class PlayerState
    {
        public long Bank;
        public long Energy;
        public long LastMoveBuy; // -1 or normal-turn index
        public long LastSnakeBuy; // -1 or normal-turn index
        public long NormalTurns; // increments at TURN_START, first turn = 1
        public bool Shield;
        public long Square; // 1..100

        public ProfileId ProfileKind = ProfileId.human;

        public static PlayerState NewHuman(int seatIndex)
        {
            return new PlayerState
            {
                Bank = 40,
                Energy = 0,
                LastMoveBuy = -1,
                LastSnakeBuy = -1,
                NormalTurns = 0, // TURN_START increments before exposing PRE_ROLL_CHOICES
                Shield = false,
                Square = 1,
                ProfileKind = ProfileId.human,
            };
        }
    }

    public enum ProfileId
    {
        human = 0,
        Racer = 1,
        Saver = 2,
        Attacker = 3,
        Balanced = 4,
    }

    public sealed class Transport
    {
        public string Id = ""; // "snake-0".."snake-3" or "ladder-0".."ladder-3"
        public long Start;
        public long End;
        public TransportKind Kind;
    }

    public sealed class PurchasedTransport
    {
        public int Owner;
        public long Start;
        public long End;
        public long ExpiresAt; // nonnegative global turn index
    }

    public sealed class Tree
    {
        public long Square;
        public bool Live;
        public long RespawnAt = -1; // -1 while live, otherwise nonnegative
    }

    public sealed class AcceptedAction
    {
        public int Actor;
        public long ActionId;
        public long ExpectedVersion;
        public ActionType Type;
        public Dictionary<string, object> Payload = new Dictionary<string, object>();
        public Outcome Outcome;
        public long Version; // resulting version
    }
}
