namespace PotionCraft.Contracts.Enums;

/// <summary>
/// Редкость травы, определяет сложность поиска (DC) и стоимость.
/// </summary>
public enum HerbRarity
{
    /// <summary>Обычная.</summary>
    Common = 1,

    /// <summary>Необычная.</summary>
    Uncommon = 2,

    /// <summary>Редкая.</summary>
    Rare = 3,

    /// <summary>Очень редкая.</summary>
    VeryRare = 4,

    /// <summary>Легендарная.</summary>
    Legendary = 5
}
