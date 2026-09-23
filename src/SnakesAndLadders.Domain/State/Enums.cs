namespace SnakesAndLadders.Domain.State
{
    public enum Phase
    {
        PRE_ROLL_CHOICES,
        DICE_ROLL_REQUESTED,
        BOARD_RESHUFFLE,
        LAYOUT_LOCKED,
        DICE_RESOLVED,
        NORMAL_MOVEMENT,
        LANDING_RESOLUTION,
        OPTIONAL_LANDING_CHOICE,
        POST_CHOICE_RESOLUTION,
        ECONOMY_AND_COOLDOWN_UPDATE,
        WIN_CHECK,
        TURN_END,
        CHASE_CHOICE,
        SETTLED,
        ABANDONED,
        FAILED,
        DRAW,
    }

    public enum Choice
    {
        none,
        purchase,
        roll,
        coin,
        portal,
        chase,
    }

    public enum TransportKind
    {
        snake,
        ladder,
    }

    public enum ActionType
    {
        BuyMovement,
        BuySnake,
        Skip,
        Roll,
        AcceptCoin,
        DeclineCoin,
        AcceptPortal,
        DeclinePortal,
        BuyAttempt,
        Stop,
        Timeout,
        Quit,
    }

    public enum Outcome
    {
        ok,
        invalid_configuration,
        randomness_failure,
    }

    public enum MatchResult
    {
        none,
        winner,
        draw_turn_limit,
        abandoned,
        invalid_configuration,
        randomness_failure,
    }

    public enum SeatProfile
    {
        human,
        Racer,
        Saver,
        Attacker,
        Balanced,
    }
}
