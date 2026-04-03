namespace PotionCraft.Contracts.DiceRolls;

/// <summary>
/// Представляет контракт броска N-гранных кубиков (только описание, без логики генерации).
/// </summary>
public class DiceRoll
{
    /// <summary>
    /// Количество граней у кубика (N).
    /// </summary>
    public int Sides { get; }

    /// <summary>
    /// Количество бросаемых кубиков.
    /// </summary>
    public int Count { get; }

    // Основные предустановленные броски
    public static DiceRoll D20 => new(20, 1);
    public static DiceRoll D4 => new(4, 1);
    public static DiceRoll TwoD6 => new(6, 2);
    public static DiceRoll D100 => new(100, 1);

    /// <summary>
    /// Создает новый бросок.
    /// </summary>
    /// <param name="sides">Количество граней (N).</param>
    /// <param name="count">Количество кубиков (по умолчанию 1).</param>
    public DiceRoll(int sides, int count = 1)
    {
        if (sides < 2)
            throw new ArgumentOutOfRangeException(nameof(sides), "Кубик должен иметь минимум 2 грани.");
        if (count < 1)
            throw new ArgumentOutOfRangeException(nameof(count), "Количество кубиков должно быть не меньше 1.");

        Sides = sides;
        Count = count;
    }
}

/// <summary>
/// Контракт результата броска.
/// Record идеально подходит для иммутабельных данных.
/// </summary>
public record DiceRollResult(int TotalSum, IReadOnlyList<int> IndividualRolls);