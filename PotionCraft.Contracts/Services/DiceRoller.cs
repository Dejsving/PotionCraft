using PotionCraft.Contracts.DiceRolls;
using PotionCraft.Contracts.Interfaces;

namespace PotionCraft.Contracts.Services;

/// <summary>
/// Реализация сервиса бросков кубиков на основе Random.Shared.
/// </summary>
public class DiceRoller : IDiceRoller
{
    /// <inheritdoc />
    public int Roll(DiceRoll dice)
    {
        int sum = 0;
        for (int i = 0; i < dice.Count; i++)
        {
            sum += Random.Shared.Next(1, dice.Sides + 1);
        }
        return sum;
    }

    /// <inheritdoc />
    public DiceRollResult RollDetailed(DiceRoll dice)
    {
        var rolls = new int[dice.Count];
        int sum = 0;

        for (int i = 0; i < dice.Count; i++)
        {
            int roll = Random.Shared.Next(1, dice.Sides + 1);
            rolls[i] = roll;
            sum += roll;
        }

        return new DiceRollResult(sum, rolls);
    }

    /// <inheritdoc />
    public double NextDouble()
    {
        return Random.Shared.NextDouble();
    }

    /// <inheritdoc />
    public int Next(int minValue, int maxValue)
    {
        return Random.Shared.Next(minValue, maxValue);
    }
}
