namespace SnakesAndLadders.Domain.Rng
{
    public interface IRng
    {
        uint DrawUInt32(string label, ref ulong counter);
        RandomnessFailure? Uniform(string label, ref ulong counter, uint n, out uint result);
        RandomnessFailure? RollDice(ref ulong diceCounter, out int result);
    }
}
